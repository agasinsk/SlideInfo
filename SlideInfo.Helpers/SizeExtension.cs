using System;
using System.Drawing;


namespace SlideInfo.Helpers
{
    public static class SizeExtension
    {
        public static SizeL ToSizeL(this Size size)
        {
            return new SizeL(size.Width, size.Height);
        }

        public static Size GetProportionateResize(this Size i, SizeL maxSize)
        {
            return i.GetProportionateResize(maxSize.ToSize());
        }

        public static Size GetProportionateResize(this Size i, Size maxSize)
        {
            var ratioX = (double)maxSize.Width / i.Width;
            var ratioY = (double)maxSize.Height / i.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)Math.Ceiling(i.Width * ratio);
            var newHeight = (int)(i.Height * ratio);

            return new Size(newWidth, newHeight);
        }

        public static Size GetProportionateResize(this SizeL i, SizeL maxSize)
        {
            return i.GetProportionateResize(maxSize.ToSize());
        }

        public static Size GetProportionateResize(this SizeL i, Size maxSize)
        {
            var ratioX = (double)maxSize.Width / i.Width;
            var ratioY = (double)maxSize.Height / i.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)Math.Ceiling(i.Width * ratio);
            var newHeight = (int)(i.Height * ratio);

            return new Size(newWidth, newHeight);
        }
    }
}