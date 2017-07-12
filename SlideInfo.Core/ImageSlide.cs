using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using SlideInfo.Helpers;

namespace SlideInfo.Core
{
	public class ImageSlide : AbstractSlide
	{
		private Image image;
		private bool closeImage;
		private readonly string fileName;

		public override int LevelCount => 1;
		public override IList<SizeL> LevelDimensions => new List<SizeL> { new SizeL(image.Width, image.Height) };
		public override IList<double> LevelDownsamples => new List<double> { 1.0 };
		public override SlideDictionary<string> Properties { get; set; } = null;
		public override SlideDictionary<AssociatedImage> AssociatedImages { get; set; } = null;

		public ImageSlide()
		{
		}

		public ImageSlide(string fileName)
		{
			this.fileName = fileName;
			image = Image.FromFile(fileName);
			closeImage = true;
		}

		public ImageSlide(Image image)
		{
			this.image = image;
			closeImage = false;
		}

		public override void Close()
		{
			if (closeImage)
			{
				image.Dispose();
				closeImage = false;
			}
			image = null;
		}

		//Returns a string describing the format of the specified file.
		//If the file format is not recognized, returns null.
		public override string DetectFormat(string fileName)
		{
			try
			{
				var img = Image.FromFile(fileName);
				return img.GetFormat();
			}
			catch (IOException)
			{
				return null;
			}
		}

		public override int GetBestLevelForDownsample(double downsample)
		{
			return 0;
		}

		/*Returns an image containing the contents of the region.
			location: (x, y) tuple giving the top left pixel in the level 0 reference frame.
			level:    the level number.
			size:     (width, height) tuple giving the region size.*/
		public override Image ReadRegion(SizeL location, int level, SizeL size)
		{
			if (image == null)
				throw new ArgumentNullException();
			if (level != 0)
				throw new OpenSlideException("Invalid level");
			if (size.Width < 0 || size.Height < 0)
				throw new OpenSlideException($"Size {size} must be non-negative");

			/*	Any corner of the requested region may be outside the bounds of
				the image. Create a transparent tile of the correct size and
				paste the valid part of the region into the correct location.*/
			var imageTopLeft = new SizeL(Math.Max(0, Math.Min(location.Width, size.Width)),
											Math.Max(0, Math.Min(location.Height, size.Height)));
			var imageBottomRight = new SizeL(Math.Max(0, Math.Min(location.Width + size.Width - 1, Dimensions.Width - 1)),
										Math.Max(0, Math.Min(location.Height + size.Height - 1, Dimensions.Height - 1)));

			var tile = new Bitmap((int)size.Width, (int)size.Height);

			// Crop size is greater than zero in both dimensions.
			if (imageBottomRight.Width - imageTopLeft.Width >= 0 &&
				imageBottomRight.Height - imageTopLeft.Height >= 0) return tile;

			var crop = new Rectangle((int)imageTopLeft.Width, (int)imageTopLeft.Height,
				(int)(imageTopLeft.Width + imageBottomRight.Width + 1), (int)(imageTopLeft.Height + imageBottomRight.Height + 1));

			var g = Graphics.FromImage(tile);
			g.DrawImage(image, -crop.X, -crop.Y);
			return tile;
		}

		public override Image GetThumbnail(SizeL size)
		{
			var thumbSize = image.GetProportionateResize(size.ToSize());
			var callback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
			var thumbnail = image.GetThumbnailImage(thumbSize.Width, thumbSize.Height,
								callback, new IntPtr());
			return thumbnail;
		}

		public override string ToString()
		{
			return $"ImageSlide({fileName})";
		}
	}
}
