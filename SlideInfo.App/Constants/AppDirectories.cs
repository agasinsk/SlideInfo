using System.IO;

namespace SlideInfo.App.Constants
{
    public static class AppDirectories
    {
        public static string MainPath = Directory.GetCurrentDirectory();
        public static string AssociatedImagesThumbs = MainPath + @"\wwwroot\images\associatedThumbs\";
        public static string SlideThumbs = MainPath + @"\wwwroot\images\slideThumbs\";
        public static string SlideStorage = "C:\\Users\\artur\\Documents\\Visual Studio 2017\\Projects\\OpenSlideExample\\data";
    }
}
