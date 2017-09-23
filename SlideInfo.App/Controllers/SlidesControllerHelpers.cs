using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SlideInfo.App.Constants;
using SlideInfo.App.Data;
using SlideInfo.App.Helpers;
using SlideInfo.App.Models;
using SlideInfo.App.Models.SlideViewModels;
using SlideInfo.Core;

namespace SlideInfo.App.Controllers
{
    public partial class SlidesController
    {
        // Slides
        public IActionResult Update()
        {
            DbInitializer.UpdateSlides(context);
            return RedirectToAction("Index");
        }

        private void GenerateSlideThumbnails(ICollection<Slide> slides)
        {
            Directory.CreateDirectory(AppDirectories.SlideThumbs);
            var existingThumbs = from file in Directory.EnumerateFiles(AppDirectories.SlideThumbs, "*.jpeg")
                                 select file;
            var existingThumbsCount = existingThumbs.Count();

            if (slides.Count == existingThumbsCount)
                return;

            foreach (var slide in slides)
            {
                var existingSlideThumb = from file in existingThumbs
                                         where file.ToLower().Equals($"{slide.Name.ToUrlSlug()}.jpeg")
                                         select file;
                if (existingSlideThumb.Any())
                    continue;

                logger.LogInformation("Generating thumbnail of slide {id}: {name}..", slide.Id, slide.Name);
                using (var osr = new OpenSlide(slide.FilePath))
                {
                    var thumb = osr.GetThumbnail(new Size(400, 400));
                    thumb.Save($@"{AppDirectories.SlideThumbs}{slide.Name.ToUrlSlug()}.jpeg", ImageFormat.Jpeg);
                }
            }
        }

        [Produces("application/xml")]
        [Route("[controller]/Display/{slug}.dzi")]
        public string Dzi(string slug)
        {
            try
            {
                logger.LogInformation("Getting {slug}.dzi metadata...", slug);
                var slide = HttpContext.Session.Get<Slide>(SessionConstants.CURRENT_SLIDE) ?? slideRepository.Get(m => m.Url == slug).FirstOrDefault();

                if (slide != null)
                    using (var osr = new OpenSlide(slide.FilePath))
                    {
                        var viewModel = new DisplayViewModel(slide.Name, slide.DziUrl, slide.Mpp, osr);
                        return viewModel.DeepZoomGenerator.GetDziMetadataString();
                    }
            }
            catch (Exception)
            {
                logger.LogError("Error while getting {slug}.dzi", slug);
                HttpContext.Session.SetString(SessionConstants.ALERT, SessionConstants.NO_ACCESS);
            }
            return "";
        }

        [Route("[controller]/Display/{slug}_files/{level:int}/{col:int}_{row:int}.jpeg")]
        public IActionResult Tile(string slug, int level, int col, int row)
        {
            try
            {
                logger.LogInformation("Getting tile: {level}, col: {col}, row: {row}", level, col, row);

                var slide = HttpContext.Session.Get<Slide>(SessionConstants.CURRENT_SLIDE) ?? slideRepository.Get(m => m.Url == slug).FirstOrDefault();

                if (slide != null)
                    using (var osr = new OpenSlide(slide.FilePath))
                    {
                        var viewModel = new DisplayViewModel(slide.Name, slide.DziUrl, slide.Mpp, osr);
                        var tile = viewModel.DeepZoomGenerator.GetTile(level, new SizeL(col, row));

                        using (var stream = new MemoryStream())
                        {
                            tile.Save(stream, ImageFormat.Jpeg);
                            tile.Dispose();
                            return File(stream.ToArray(), "image/jpeg");
                        }
                    }
            }
            catch (OpenSlideException)
            {
                logger.LogError("Error while getting tile lev: {level}, col: {col}, row: {row}", level, col, row);
                HttpContext.Session.SetString(SessionConstants.ALERT, SessionConstants.CANT_LOAD_DATA);
            }
            return new FileContentResult(new byte[] { }, "");
        }

        private void GenerateAssociatedImagesThumbnails(int slideId, SlideDictionary<AssociatedImage> associated)
        {
            Directory.CreateDirectory(AppDirectories.AssociatedImagesThumbs);
            if (Directory.EnumerateFiles(AppDirectories.AssociatedImagesThumbs, $"{slideId}*").Any())
                return;

            logger.LogInformation("Generating thumbnails of associated images of slide {ID}...", slideId);
            foreach (var image in associated)
            {
                var thumb = image.Value.GetThumbnail(new Size(400, 400));
                thumb.Save($@"{AppDirectories.AssociatedImagesThumbs}{slideId}_{image.Key}.jpeg", ImageFormat.Jpeg);
            }
        }

        [Produces("application/xml")]
        [Route("[controller]/AssociatedImages/{id}/{imageName}.dzi")]
        public string AssociatedDzi(int? id, string imageName)
        {
            logger.LogInformation("Getting {slug}.dzi metadata...", imageName);
            try
            {
                var slide = slideRepository.Get(m => m.Id == id).FirstOrDefault();
                if (slide != null)
                    using (var osr = new OpenSlide(slide.FilePath))
                    {
                        var associated = osr.AssociatedImages[imageName].ToImageSlide();
                        var viewModel = new DisplayViewModel(slide.Name, slide.DziUrl, slide.Mpp, associated);
                        return viewModel.DeepZoomGenerator.GetDziMetadataString();
                    }
            }
            catch (Exception)
            {
                logger.LogError("Error while getting {slug}.dzi", imageName);
                HttpContext.Session.SetString(SessionConstants.ALERT, SessionConstants.NO_ACCESS);
            }
            return "";
        }

        [Route("[controller]/AssociatedImages/{id}/{imageName}_files/{level:int}/{col:int}_{row:int}.jpeg")]
        public IActionResult AssociatedTile(int? id, string imageName, int level, int col, int row)
        {
            logger.LogInformation("Getting tile of {slug} | lev: {level}, col: {col}, row: {row}", imageName, level, col, row);
            try
            {
                var slide = slideRepository.Get(m => m.Id == id).FirstOrDefault();
                if (slide != null)
                    using (var osr = new OpenSlide(slide.FilePath))
                    {
                        var associated = osr.AssociatedImages[imageName].ToImageSlide();
                        var viewModel = new DisplayViewModel(slide.Name, slide.DziUrl, slide.Mpp, associated);
                        var tile = viewModel.DeepZoomGenerator.GetTile(level, new SizeL(col, row));

                        using (var stream = new MemoryStream())
                        {
                            tile.Save(stream, ImageFormat.Jpeg);
                            tile.Dispose();
                            return File(stream.ToArray(), "image/jpeg");
                        }
                    }
            }
            catch (OpenSlideException)
            {
                logger.LogError("Error while getting tile | lev: {level}, col: {col}, row: {row}", level, col, row);
                HttpContext.Session.SetString(SessionConstants.ALERT, SessionConstants.CANT_LOAD_DATA);
            }
            return new FileContentResult(new byte[] { }, "");
        }


    }
}
