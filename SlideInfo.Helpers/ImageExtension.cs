using System.Drawing;
using System.Drawing.Imaging;

namespace SlideInfo.Helpers
{
	public static class ImageExtension
	{
		public static string GetMimeType(Image i)
		{
			var imgguid = i.RawFormat.Guid;
			foreach (var codec in ImageCodecInfo.GetImageDecoders())
			{
				if (codec.FormatID == imgguid)
					return codec.MimeType;
			}
			return "image/unknown";
		}

		public static string GetFormat(this Image i)
		{
			var imgguid = i.RawFormat.Guid;
			foreach (var codec in ImageCodecInfo.GetImageDecoders())
			{
				if (codec.FormatID == imgguid)
					return codec.MimeType;
			}
			return "image/unknown";
		}

		public static Size GetProportionateResize(this Image i, Size resize)
		{
			var proportionRatio = (double)i.Width / i.Height;
			if (resize.Width >= resize.Height)
			{
				var height = (int) (resize.Width / proportionRatio) ;
				var size = new Size(resize.Width, height);
				return size;
			}
			else
			{
				var width = (int)(resize.Height / proportionRatio);
				var size = new Size(width, resize.Height);
				return size;
			}
		}

	    public static Image CropImage(this Image i, Rectangle section)
	    {
	        // An empty bitmap which will hold the cropped image
	        var bmp = new Bitmap(section.Width, section.Height);

	        var g = Graphics.FromImage(bmp);

	        // Draw the given area (section) of the source image
	        // at location 0,0 on the empty bitmap (bmp)
	        g.DrawImage(i, 0, 0, section, GraphicsUnit.Pixel);

	        return bmp;
        }
	}
}
