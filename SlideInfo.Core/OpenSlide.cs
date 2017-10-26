using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SlideInfo.Helpers;

namespace SlideInfo.Core
{
    public sealed unsafe partial class OpenSlide : AbstractSlide, IDisposable
    {
        private readonly object slideLock;
        private bool disposed;

        public int* Osr { get; private set; }
        public string FilePath { get; set; }
        public int QuickHash1 { get; private set; }
        public override int LevelCount => ReadLevelCount();
        public override IList<SizeL> LevelDimensions => ReadLevelDimensions();
        public override IList<double> LevelDownsamples => ReadLevelDownsamples();

        private SlideDictionary<string> properties;
        public override SlideDictionary<string> Properties
        {
            get => properties ?? (properties = new PropertiesDictionary(this));
            set => properties = value;
        }

        private SlideDictionary<AssociatedImage> associated;
        public override SlideDictionary<AssociatedImage> AssociatedImages
        {
            get => associated ?? (associated = new AssociatedImageDictionary(this));
            set => associated = value;
        }

        public OpenSlide()
        {
            slideLock = new object();
        }

        public OpenSlide(string filePath)
        {
            slideLock = new object();
            CheckIfFileExists(filePath);
            FilePath = filePath;

            ReadOpenSlide();
            ReadQuickHash();
        }

        private void CheckIfFileExists(string fileName)
        {
            if (!File.Exists(fileName))
                throw new OpenSlideException($"File '{fileName}' cannot be opened");
        }

        private void ReadOpenSlide()
        {
            Osr = OpenSlideDll.openslide_open(FilePath);
            if (Osr == null || Osr[0] == 0)
                CheckVendorIsValid(FilePath);

            // dispose on error, we are in the constructor
            try
            {
                if (Osr != null)
                    CheckError();
            }
            catch (OpenSlideException)
            {
                Close();
                throw;
            }
        }

        private void CheckVendorIsValid(string fileName)
        {
            var vendor = OpenSlideDll.openslide_detect_vendor(fileName);
            if (vendor.ToInt32() != 0)
                throw new OpenSlideException($"Vendor {Marshal.PtrToStringAnsi(vendor)} unsupported");
        }

        private void CheckError()
        {
            var errorMessage = OpenSlideDll.openslide_get_error(Osr);
            if (errorMessage.ToInt64() != 0)
                throw new OpenSlideException($"openslide error: {Marshal.PtrToStringAnsi(errorMessage)}");
        }

        private void ReadQuickHash()
        {
            var quickhash1 = GetPropertyValue(PROPERTY_NAME_QUICKHASH1);

            if (quickhash1 == null)
            {
                QuickHash1 = FilePath.GetHashCode();
            }
            else
            {
                int.TryParse(quickhash1.Substring(0, 8), out int quickHash);
                QuickHash1 = quickHash;
            }
        }

        public int ReadLevelCount()
        {
            var levelCount = OpenSlideDll.openslide_get_level_count(Osr);
            if (levelCount == -1)
                CheckError();
            return levelCount;
        }

        private IList<SizeL> ReadLevelDimensions()
        {
            var dimensions = new List<SizeL>();

            for (var level = 0; level < LevelCount; level++)
            {
                OpenSlideDll.openslide_get_level_dimensions(Osr, level, out long width, out long height);
                if (width == -1 || height == -1)
                    CheckError();

                dimensions.Add(new SizeL(width, height));
            }
            return dimensions;
        }

        private IList<double> ReadLevelDownsamples()
        {
            var downsamples = new List<double>();

            for (var level = 0; level < LevelCount; level++)
            {
                var downsample = OpenSlideDll.openslide_get_level_downsample(Osr, level);
                if (Math.Abs(downsample - (-1.0)) < 1E-9)
                    CheckError();
                downsamples.Add(downsample);
            }
            return downsamples;
        }

        public SlideDictionary<string> ReadProperties()
        {
            var propertyNames = GetPropertyNames();
            var props = new PropertiesDictionary(this);
            foreach (var property in propertyNames)
            {
                props[property] = GetPropertyValue(property);
            }
            AddMissingStandardProperties(props);
            return props;
        }

        public IEnumerable<string> GetPropertyNames()
        {
            var propertyNames = StringMarshaller.Marshal(OpenSlideDll.openslide_get_property_names(Osr));
            return propertyNames;
        }

        public string GetPropertyValue(string propertyName)
        {
            return Osr != null ? Marshal.PtrToStringAnsi(OpenSlideDll.openslide_get_property_value(Osr, propertyName)) : "";
        }

        private void AddMissingStandardProperties(IDictionary<string, string> properties)
        {
            if (!properties.ContainsKey(PROPERTY_NAME_BOUNDS_WIDTH))
                properties[PROPERTY_NAME_BOUNDS_WIDTH] = Dimensions.Width.ToString();
            if (!properties.ContainsKey(PROPERTY_NAME_BOUNDS_HEIGHT))
                properties[PROPERTY_NAME_BOUNDS_HEIGHT] = Dimensions.Height.ToString();
            if (!properties.ContainsKey(PROPERTY_NAME_BOUNDS_X))
                properties[PROPERTY_NAME_BOUNDS_X] = "0";
            if (!properties.ContainsKey(PROPERTY_NAME_BOUNDS_Y))
                properties[PROPERTY_NAME_BOUNDS_Y] = "0";
            if (!properties.ContainsKey(PROPERTY_NAME_MPP_X))
                properties[PROPERTY_NAME_MPP_X] = "0";
            if (!properties.ContainsKey(PROPERTY_NAME_MPP_Y))
                properties[PROPERTY_NAME_MPP_Y] = "0";
            if (!properties.ContainsKey(PROPERTY_NAME_BACKGROUND_COLOR))
                properties[PROPERTY_NAME_BACKGROUND_COLOR] = "ffffff";
        }

        public SlideDictionary<AssociatedImage> ReadAssociatedImages()
        {
            var associatedImageNames = GetAssociatedImageNames();
            var images = new AssociatedImageDictionary(this);
            foreach (var s in associatedImageNames)
            {
                try
                {
                    var image = ReadAssociatedImage(s);
                    images.Add(s, new AssociatedImage(s, image, QuickHash1));
                }
                catch
                {
                    images.Add(s, new AssociatedImage(s, QuickHash1));
                }
            }

            return images;
        }

        public IEnumerable<string> GetAssociatedImageNames()
        {
            var imageNames = StringMarshaller.Marshal(OpenSlideDll.openslide_get_associated_image_names(Osr));
            return imageNames;
        }

        public Image ReadAssociatedImage(string name)
        {
            lock (slideLock)
            {
                CheckDisposed();
                OpenSlideDll.openslide_get_associated_image_dimensions(Osr, name, out long width, out long height);
                CheckError();
                if (width == -1)
                    throw new OpenSlideException("Failure reading associated image");

                var bmp = new Bitmap((int)width, (int)height);
                var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite,
                    PixelFormat.Format32bppArgb);

                var bmpPtr = bmpData.Scan0.ToPointer();
                OpenSlideDll.openslide_read_associated_image(Osr, name, bmpPtr);
                CheckError();
                bmp.UnlockBits(bmpData);
                return bmp;
            }
        }

        public override int GetBestLevelForDownsample(double downsample)
        {
            // too small, return first
            if (downsample < LevelDownsamples[0])
                return 0;

            // find where we are in the middle
            for (var i = 1; i < LevelCount; i++)
                if (downsample < LevelDownsamples[i])
                    return i - 1;

            // too big, return last
            return LevelCount - 1;
        }

        public override Image ReadRegion(SizeL location, int level, SizeL size)
        {
            if (level < 0)
                throw new OpenSlideException("Invalid level");
            if (size.Width < 0 || size.Height < 0)
                throw new OpenSlideException($"Size {size} must be non-negative");

            var bmp = new Bitmap((int)size.Width, (int)size.Height);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            var bmpPtr = bmpData.Scan0.ToPointer();

            CheckDisposed();
            OpenSlideDll.openslide_read_region(Osr, bmpPtr, location.Width, location.Height, level, size.Width, size.Height);
            if (bmpPtr == null)
                throw new OpenSlideException($"error reading region location:{location}, level:{level}, maxSize:{size}");

            bmp.UnlockBits(bmpData);

            CheckError();
            return bmp;

        }

        public override Image GetThumbnail(Size size)
        {
            return GetThumbnail(size.ToSizeL());
        }

        //        downsample = max(*[dim / thumb for dim, thumb in
        //        zip(self.dimensions, maxSize)])
        //        level = self.get_best_level_for_downsample(downsample)
        //            tile = self.read_region((0, 0), level, self.level_dimensions[level])
        //
        //        bg_color = '#' + self.properties.get(PROPERTY_NAME_BACKGROUND_COLOR,
        //            'ffffff')
        //            thumb = Image.new('RGB', tile.maxSize, bg_color)
        //        thumb.paste(tile, None, tile)
        //            thumb.thumbnail(maxSize, Image.ANTIALIAS)
        //            return thumb
        public override Image GetThumbnail(SizeL maxSize)
        {
            var thumbnail = ReadThumbnailRegion(maxSize);
            var bgthumbnail = thumbnail.ApplyOnBackgroundColor(Color.White);
            bgthumbnail.Save(Path.GetFileNameWithoutExtension(FilePath) + ".jpeg", ImageFormat.Jpeg);

            return bgthumbnail.GetThumbnail(maxSize);
        }

        private Image ReadThumbnailRegion(SizeL maxSize)
        {
            double downsample;
            int level;
            SizeL thumbnailSize, startLocation;

            try
            {   //if the slides is not aligned from point (0, 0)
                float.TryParse(Properties[PROPERTY_NAME_BOUNDS_X], out var xBound);
                float.TryParse(Properties[PROPERTY_NAME_BOUNDS_Y], out var yBound);

                //Slide level dimensions scale factor in each axis
                float.TryParse(Properties[PROPERTY_NAME_BOUNDS_WIDTH], out var scaledWidth);
                float.TryParse(Properties[PROPERTY_NAME_BOUNDS_HEIGHT], out var scaledHeight);

                scaledWidth = scaledWidth / LevelDimensions[0].Width;
                scaledHeight = scaledHeight / LevelDimensions[0].Height;

                var sizeScale = new SizeF(scaledWidth, scaledHeight);

                // Dimensions of active area
                var scaledLevelDimensions = (from levelSize in LevelDimensions
                                       let newScaledWidth = (long)Math.Ceiling((double)levelSize.Width * sizeScale.Width)
                                       let newScaledHeight = (long)Math.Ceiling((double)levelSize.Height * sizeScale.Height)
                                       select new SizeL(newScaledWidth, newScaledHeight)).ToList();

                startLocation = new SizeL((long)xBound, (long)yBound);
                downsample = Math.Min(scaledLevelDimensions[0].Width / maxSize.Width, scaledLevelDimensions[0].Height / maxSize.Height);
                level = GetBestLevelForDownsample(downsample);
                thumbnailSize = scaledLevelDimensions[level];
            }
            catch
            {
                startLocation = new SizeL(0, 0);
                downsample = Math.Min(Dimensions.Width / maxSize.Width, Dimensions.Height / maxSize.Height);
                level = GetBestLevelForDownsample(downsample);
                thumbnailSize = LevelDimensions[level];
            }
            var realthumbnail = ReadRegion(startLocation, level, thumbnailSize);
            realthumbnail.Save(Path.GetFileNameWithoutExtension(FilePath) + "_real.jpeg", ImageFormat.Jpeg);
            return realthumbnail;
        }

        public override string DetectFormat(string fileName)
        {
            return DetectVendor(fileName);
        }

        public static string DetectVendor(string fileName)
        {
            return Marshal.PtrToStringAnsi(OpenSlideDll.openslide_detect_vendor(fileName));
        }

        // call with the reader lock held
        private void CheckDisposed()
        {
            if (Osr == null)
                throw new OpenSlideException("OpenSlide object has been disposed");
        }

        public override void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (Osr != null && Osr[0] != 0)
                    {
                        OpenSlideDll.openslide_close(Osr);
                    }
                }
            }
            disposed = true;
        }

        public string GetLibraryVersion()
        {
            LIBRARY_VERSION = Marshal.PtrToStringAnsi(OpenSlideDll.openslide_get_version());
            return LIBRARY_VERSION;
        }

        public override string ToString()
        {
            return $"OpenSlide({FilePath})";
        }

        public override int GetHashCode()
        {
            return QuickHash1;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var os2 = (OpenSlide)obj;
            var quickhash1 = Properties[PROPERTY_NAME_QUICKHASH1];
            var os2Quickhash1 = os2.Properties[PROPERTY_NAME_QUICKHASH1];

            if (quickhash1 != null && os2Quickhash1 != null)
                return quickhash1.Equals(os2Quickhash1);
            if (quickhash1 == null && os2Quickhash1 == null)
                return FilePath.Equals(os2.FilePath);
            return false;
        }
    }
}
