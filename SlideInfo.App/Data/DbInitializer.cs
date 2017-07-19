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

                var updatedSlide = new Slide(osr);

                var existingSlide = context.Slides.Find(updatedSlide.FilePath);
                Slide slide;
                if (existingSlide != null)
                {
                    context.Entry(existingSlide).CurrentValues.SetValues(updatedSlide);
                    slide = existingSlide;
                }
                else
                {
                    context.Add(updatedSlide);
                    slide = updatedSlide;
                }

                context.SaveChanges();

                var properties = osr.ReadProperties();

                foreach (var slideProp in properties)
                {
                    var property = new Property(slide.Id, slideProp.Key, slideProp.Value);
                    context.Update(property);
                }
            }

            context.SaveChanges();
        }
    }
}
