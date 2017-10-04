using System.Drawing;


namespace SlideInfo.Helpers
{
    public static class SizeExtension
    {
        public static SizeL ToSizeL(this Size size)
        {
            return new SizeL(size.Width, size.Height);
        }
    }
}