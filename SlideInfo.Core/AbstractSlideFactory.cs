using System;

namespace SlideInfo.Core
{
    public class AbstractSlideFactory
    {
        public static AbstractSlide GetSlide(string filePath)
        {
            AbstractSlide osr;
            try
            {
                osr = new OpenSlide(filePath);
            }
            catch (Exception)
            {
                osr = new ImageSlide(filePath);
            }

            return osr;
        }
    }
}
