using System.Linq;
using SlideInfo.App.Helpers;
using SlideInfo.App.Models;
using SlideInfo.Core;

namespace SlideInfo.App.Data
{
    public static class DbInitializer
    {
        public static void Initialize(SlideInfoDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any users.
            if (context.Slides.Any())
            {
                return;   // DB has been seeded
            }

            var openSlideSupportedExtensions = new[]
            {
                "svs", "tif", "vms", "vmu", "ndpi", "scn", "mrxs",
                "tiff", "svslide", "bif"
            };
            const string mainPath = @"C:\Users\artur\Documents\Visual Studio 2017\Projects\OpenSlideExample\data";

            var dirs = FileFilter.FilterFiles(mainPath, openSlideSupportedExtensions);

            foreach (var path in dirs)
            {
                var osr = new OpenSlide(path);

                var slide = new Slide(osr);
                context.Add(slide);
                context.SaveChanges();

                var properties = osr.ReadProperties();

                foreach (var slideProp in properties)
                {
                    var property = new Property(slide.Id, slideProp.Key, slideProp.Value);
                    context.Add(property);
                }
            }

            context.SaveChanges();

        }

        public static void Update(SlideInfoDbContext context)
        {
            context.Database.EnsureCreated();

            var dirs = FileFilter.FilterFiles(AppDirectories.SlideStorage, FileFilter.OpenSlideExtensions);

            foreach (var path in dirs)
            {
                var osr = new OpenSlide(path);

                var newSlide = new Slide(osr);

                var existingSlide = context.Slides.First(s => s.FilePath == path);
                
                if (existingSlide != null)
                {
                    newSlide.Id = existingSlide.Id;
                    context.Entry(existingSlide).CurrentValues.SetValues(newSlide);
                }
                else
                {
                    context.Add(newSlide);

                    var properties = osr.ReadProperties();
                    foreach (var slideProp in properties)
                    {
                        var property = new Property(newSlide.Id, slideProp.Key, slideProp.Value);
                        context.Add(property);
                    }
                }

            }

            context.SaveChanges();
        }
    }
}
