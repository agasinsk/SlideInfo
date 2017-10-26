using System;
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

        public static Size GetProportionateResize(this Image i, Size maxSize)
        {
            var ratioX = (double)maxSize.Width / i.Width;
            var ratioY = (double)maxSize.Height / i.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)Math.Ceiling(i.Width * ratio);
            var newHeight = (int)(i.Height * ratio);

            return new Size(newWidth, newHeight);
        }

        public static Size GetProportionateResize(this Image i, SizeL maxSize)
        {
            return i.GetProportionateResize(maxSize.ToSize());
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

        public static Image ApplyOnBackgroundColor(this Image i, Color color)
        {
            using (var src = new Bitmap(i))
            {
                i.Dispose();
                i = new Bitmap(src.Width, src.Height);
                using (var g1 = Graphics.FromImage(i))
                {
                    g1.Clear(color);
                    g1.DrawImage(src, 0, 0);
                }
            }
            return i;
        }

        public static Image GetThumbnail(this Image i, SizeL maxSize)
        {
            return GetThumbnail(i, maxSize.ToSize());
        }

        public static Image GetThumbnail(this Image i, Size maxSize)
        {
            var thumbSize = i.GetProportionateResize(maxSize);
            var callback = new Image.GetThumbnailImageAbort(ThumbnailCallback);

            return i.GetThumbnailImage(thumbSize.Width, thumbSize.Height, callback, new IntPtr());
        }

        public static bool ThumbnailCallback()
        {
            return true;
        }
    }
}
