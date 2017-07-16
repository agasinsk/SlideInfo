using System.IO;

namespace SlideInfo.App.Helpers
{
    public static class AppDirectories
    {
        public static string MainPath = Directory.GetCurrentDirectory();
        public static string AssociatedImagesThumbs = MainPath + @"\wwwroot\images\associatedThumbs\";
        public static string SlidesThumbs = MainPath + @"\wwwroot\images\slideThumbs\";
    }
}
