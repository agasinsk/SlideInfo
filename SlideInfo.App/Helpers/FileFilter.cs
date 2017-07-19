using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SlideInfo.App.Helpers
{
    public static class FileFilter
    {
        public static IEnumerable<string> FilterFiles(string path, params string[] exts)
        {
            return
                Directory
                    .EnumerateFiles(path, "*.*")
                    .Where(file => exts.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
        }

        public static String[] OpenSlideExtensions = 
            {
                "svs", "tif", "vms", "vmu", "ndpi", "scn", "mrxs",
                "tiff", "svslide", "bif"
            };

    }
}
