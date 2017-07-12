using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using OpenSlide.Cs.Helpers;

namespace OpenSlide.Cs
{
	public unsafe class OpenSlideLite : AbstractSlide, IDisposable
	{
		private readonly object slideLock;
		private bool disposed;
		private int* osr;

		public string FileName { get; set; }
		public int HashCodeVal { get; private set; }

		public OpenSlideLite()
		{
			slideLock = new object();
		}

		public OpenSlideLite(string fileName)
		{
			CheckIfFileExists(fileName);
			FileName = fileName;
			slideLock = new object();

			ReadOpenSlide();
			ReadHashCodeVal();
			/*ReadLevelCount();
			ReadLevelDimensionsAndDownsamples();

			ReadProperties();
			ReadAssociatedImages();*/
		}

		public override string DetectFormat(string fileName)
		{
			return Marshal.PtrToStringAnsi(OpenSlideDll.openslide_detect_vendor(fileName));
		}

		private void CheckIfFileExists(string fileName)
		{
			if (File.Exists(fileName))
				return;
			throw new OpenSlideException($"File '{fileName}' can't be opened");
		}

		private void ReadOpenSlide()
		{
			osr = OpenSlideDll.openslide_open(FileName);
			if (osr == null || osr[0] == 0)
				CheckVendorIsValid(FileName);
			// dispose on error, we are in the constructor
			try
			{
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
				throw new OpenSlideUnsupportedFormatException($"Vendor {Marshal.PtrToStringAnsi(vendor)} unsupported");
		}

		public void CheckError()
		{
			var errorMessage = OpenSlideDll.openslide_get_error(osr);
			if (errorMessage.ToInt32() != 0)
				throw new OpenSlideException($"openslide error: {Marshal.PtrToStringAnsi(errorMessage)}");
		}

		public void ReadLevelCount()
		{
			LevelCount = OpenSlideDll.openslide_get_level_count(osr);
			if (LevelCount == -1)
				CheckError();
		}

		public void ReadLevelDimensionsAndDownsamples()
		{
			LevelDimensions = new List<LevelSize>();
			LevelDownsamples = new List<double>();

			for (var level = 0; level < LevelCount; level++)
			{
				GetDimensionsAtLevel(level);
				GetDownsampleAtLevel(level);
			}
		}

		private void GetDimensionsAtLevel(int level)
		{
			OpenSlideDll.openslide_get_level_dimensions(osr, level, out long width, out long height);
			if (width == -1 || height == -1)
				CheckError();

			LevelDimensions.Add(new LevelSize(width, height));
		}

		private void GetDownsampleAtLevel(int level)
		{
			var downsample = OpenSlideDll.openslide_get_level_downsample(osr, level);
			if (downsample == -1.0)
				CheckError();
			LevelDownsamples.Add(downsample);
		}

		public void ReadProperties()
		{
			var propertyNames = GetPropertyNames();
			Properties = new Dictionary<string, string>();
			foreach (var s in propertyNames)
				Properties.Add(s, GetPropertyValue(s));
			AddMissingStandardProperties();
		}

		private IEnumerable<string> GetPropertyNames()
		{
			var propertyNames = StringMarshaller.Marshal(OpenSlideDll.openslide_get_property_names(osr));
			return propertyNames;
		}

		private string GetPropertyValue(string propertyName)
		{
			return Marshal.PtrToStringAnsi(OpenSlideDll.openslide_get_property_value(osr, propertyName));
		}

		private void AddMissingStandardProperties()
		{
			if (!Properties.ContainsKey(OpenSlide.PROPERTY_NAME_BOUNDS_WIDTH))
				Properties[OpenSlide.PROPERTY_NAME_BOUNDS_WIDTH] = Dimensions.Width.ToString();
			if (!Properties.ContainsKey(OpenSlide.PROPERTY_NAME_BOUNDS_HEIGHT))
				Properties[OpenSlide.PROPERTY_NAME_BOUNDS_HEIGHT] = Dimensions.Height.ToString();
			if (!Properties.ContainsKey(OpenSlide.PROPERTY_NAME_BOUNDS_X))
				Properties[OpenSlide.PROPERTY_NAME_BOUNDS_X] = "0";
			if (!Properties.ContainsKey(OpenSlide.PROPERTY_NAME_BOUNDS_Y))
				Properties[OpenSlide.PROPERTY_NAME_BOUNDS_Y] = "0";
			if (!Properties.ContainsKey(OpenSlide.PROPERTY_NAME_MPP_X))
				Properties[OpenSlide.PROPERTY_NAME_MPP_X] = "0";
			if (!Properties.ContainsKey(OpenSlide.PROPERTY_NAME_MPP_Y))
				Properties[OpenSlide.PROPERTY_NAME_MPP_Y] = "0";
			if (!Properties.ContainsKey(OpenSlide.PROPERTY_NAME_BACKGROUND_COLOR))
				Properties[OpenSlide.PROPERTY_NAME_BACKGROUND_COLOR] = "ffffff";
		}

		private void ReadHashCodeVal()
		{
			var quickhash1 = GetPropertyValue(OpenSlide.PROPERTY_NAME_QUICKHASH1);

			if (quickhash1 == null)
			{
				HashCodeVal = FileName.GetHashCode();
			}
			else
			{
				int.TryParse(quickhash1.Substring(0, 8), out int hashCodeVal);
				HashCodeVal = hashCodeVal;
			}
		}

		public void ReadAssociatedImages()
		{
			var associatedImageNames = GetAssociatedImageNames();
			AssociatedImages = new Dictionary<string, AssociatedImage>();
			foreach (var s in associatedImageNames)
			{
				try
				{
					var image = ReadAssociatedImage(s);
					AssociatedImages.Add(s, new AssociatedImage(s, image, HashCodeVal));
				}
				catch
				{
					AssociatedImages.Add(s, new AssociatedImage(s, HashCodeVal));
				}
			}
		}

		private IEnumerable<string> GetAssociatedImageNames()
		{
			var imageNames = StringMarshaller.Marshal(OpenSlideDll.openslide_get_associated_image_names(osr));
			return imageNames;
		}

		public Image ReadAssociatedImage(string name)
		{
			lock (slideLock)
			{
				CheckDisposed();

				OpenSlideDll.openslide_get_associated_image_dimensions(osr, name, out long width, out long height);
				CheckError();
				if (width == -1)
					throw new OpenSlideException("Failure reading associated image");

				var bmp = new Bitmap((int)width, (int)height);
				var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite,
					PixelFormat.Format32bppArgb);

				var bmpPtr = bmpData.Scan0.ToPointer();
				OpenSlideDll.openslide_read_associated_image(osr, name, bmpPtr);
				CheckError();

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

		public override Image ReadRegion(LevelSize location, int level, LevelSize size)
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
			OpenSlideDll.openslide_read_region(osr, bmpPtr, location.Width, location.Height, level, size.Width, size.Height);
			if (bmpPtr == null)
				throw new OpenSlideException($"error reading region location:{location}, level:{level}, size:{size}");

			bmp.UnlockBits(bmpData);

			CheckError();
			return bmp;

		}

		public override Image GetThumbnail(LevelSize size)
		{
			var downsample = Math.Max(Dimensions.Width / size.Width, Dimensions.Height / size.Height);
			var level = GetBestLevelForDownsample(downsample);
			var thumbnailSize = LevelDimensions[level];

			var thumbnail = ReadRegion(new LevelSize(0, 0), level, thumbnailSize);
			if (size.Equals(thumbnailSize))
				return thumbnail;

			var thumbSize = thumbnail.GetProportionateResize(size.ToSize());
			var callback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
			var thumb = thumbnail.GetThumbnailImage(thumbSize.Width, thumbSize.Height, callback, new IntPtr());

			return thumb;
		}

		// call with the reader lock held
		private void CheckDisposed()
		{
			if (osr == null)
				throw new OpenSlideDisposedException();
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

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;
			if (disposing) { }
			if (osr != null && osr[0] != 0)
			{
				OpenSlideDll.openslide_close(osr);
			}

			disposed = true;
		}

		public string GetLibraryVersion()
		{
			return Marshal.PtrToStringAnsi(OpenSlideDll.openslide_get_version());
		}

		public override string ToString()
		{
			return $"OpenSlide({FileName})";
		}

		public override int GetHashCode()
		{
			return HashCodeVal;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			var os2 = (OpenSlide)obj;
			var quickhash1 = Properties[OpenSlide.PROPERTY_NAME_QUICKHASH1];
			var os2Quickhash1 = os2.Properties[OpenSlide.PROPERTY_NAME_QUICKHASH1];

			if (quickhash1 != null && os2Quickhash1 != null)
				return quickhash1.Equals(os2Quickhash1);
			if (quickhash1 == null && os2Quickhash1 == null)
				return FileName.Equals(os2.FileName);
			return false;
		}
	}
}
