using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SlideInfo.App.Data;
using SlideInfo.App.Helpers;
using SlideInfo.App.Models;
using SlideInfo.App.Models.SlideViewModels;
using SlideInfo.App.Repositories;
using SlideInfo.Core;
using static SlideInfo.App.Helpers.SessionConstants;

namespace SlideInfo.App.Controllers
{
    public class SlidesController : Controller
    {
        private readonly SlideInfoDbContext context;
        private readonly AsyncRepository<Slide> slideRepository;
        private readonly AsyncRepository<Property> propertyRepository;
        private readonly ILogger logger;

        public SlidesController(ILogger<SlidesController> logger, SlideInfoDbContext context)
        {
            this.logger = logger;
            this.context = context;
            slideRepository = new AsyncRepository<Slide>(context);
            propertyRepository = new AsyncRepository<Property>(context);
        }

        // GET: Slides
        public async Task<IActionResult> Index()
        {
            logger.LogInformation("Getting all slides...");
            var slides = await slideRepository.GetAllAsync();
            GenerateSlidesThumbnails(slides);
            HttpContext.Session.Remove(SLIDE_ENTRY);
            return View(slides);
        }

        private void GenerateSlidesThumbnails(ICollection<Slide> slides)
        {
            var existingThumbs = Directory.EnumerateFiles(AppDirectories.SlidesThumbs);
            var existingThumbsCount = existingThumbs.Count();
            if (slides.Count == existingThumbsCount)
                return;

            logger.LogInformation("Generating thumbnails of slides...");
            foreach (var slide in slides)
            {
                if (existingThumbs.Contains($"{slide.Id}")) continue;
                using (var osr = new OpenSlide(slide.FilePath))
                {
                    var thumb = osr.GetThumbnail(new Size(400, 400));
                    thumb.Save($@"{AppDirectories.SlidesThumbs}{slide.Id}.jpeg", ImageFormat.Jpeg);
                }
            }
        }

        // GET: Slides/Display/5
        public async Task<IActionResult> Display(int? id)
        {
            logger.LogInformation("Getting slide {ID} to display...", id);
            if (id == null)
            {
                logger.LogWarning("Slide id was null");
                return NotFound();
            }

            var slide = await slideRepository.GetByIdAsync(id.Value);

            if (slide == null)
            {
                logger.LogError("GetById({ID}) NOT FOUND", id);
                return NotFound();
            }

            HttpContext.Session.Set(SLIDE_ENTRY, slide);

            var osr = new OpenSlide(slide.FilePath);
            var viewModel = new DisplayViewModel(slide.Name, slide.DziUrl, slide.Mpp, osr);

            return View(viewModel);
        }

        [Produces("application/xml")]
        [Route("[controller]/Display/{slug}.dzi")]
        public string Dzi(string slug)
        {
            try
            {
                logger.LogInformation("Getting {slug}.dzi metadata...", slug);
                var slide = slideRepository.Get(m => m.Url == slug).FirstOrDefault();
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
                HttpContext.Session.SetString(ALERT, NO_ACCESS);
            }
            return "";
        }

        [Route("[controller]/Display/{slug}_files/{level:int}/{col:int}_{row:int}.jpeg")]
        public IActionResult Tile(string slug, int level, int col, int row)
        {
            try
            {
                logger.LogInformation("Getting tile: {level}, col: {col}, row: {row}", level, col, row);

                var slide = slideRepository.Get(m => m.Url == slug).FirstOrDefault();
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
                HttpContext.Session.SetString(ALERT, CANT_LOAD);
            }
            return new FileContentResult(new byte[] { }, "");
        }

        // GET: Slides/Properties/5
        public async Task<IActionResult> Properties(int? id, string sortOrder,
            string currentFilter, string searchString, int? page)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            logger.LogInformation("Getting properties of slide {ID}", id);
            if (id == null)
            {
                return NotFound();
            }

            var slide = await slideRepository.GetByIdAsync(id.Value);

            if (slide == null)
            {
                return NotFound();
            }

            var properties = context.Properties.Where(c => c.SlideId == id);

            //filtering
            if (!String.IsNullOrEmpty(searchString))
            {
                logger.LogInformation("Searching for properties containing {searchString}", searchString);
                properties = properties.Where(s => s.Key.Contains(searchString)
                                               || s.Value.Contains(searchString));
            }

            //sorting
            switch (sortOrder)
            {
                case "name_desc":
                    properties = properties.OrderByDescending(s => s.Key);
                    logger.LogInformation("Sorting properties of slide {ID} by name descending", id);
                    break;
                default:
                    logger.LogInformation("Sorting properties of slide {ID} by name", id);
                    properties = properties.OrderBy(s => s.Key);
                    break;
            }

            int pageSize = 13;
            var paginatedProperties = await PaginatedList<Property>.
                CreateAsync(properties.AsNoTracking(), page ?? 1, pageSize);
            var viewModel = new PropertiesViewModel(slide.Name, paginatedProperties);
            return View(viewModel);
        }

        // GET: Slides/AssociatedImages/5
        public async Task<IActionResult> AssociatedImages(int? id, string imageName)
        {
            logger.LogInformation("Getting associated images of slide {ID}", id);

            if (id == null)
            {
                return NotFound();
            }

            var slide = await slideRepository.GetByIdAsync(id.Value);

            if (slide == null)
            {
                return NotFound();
            }

            var osr = new OpenSlide(slide.FilePath);

            if (!String.IsNullOrEmpty(imageName))
            {
                logger.LogInformation("Getting associated image {name} of slide {ID}", imageName, id);
                var associatedSlide = osr.AssociatedImages[imageName].ToImageSlide();
                var imageUrl = slide.Id + "/" + imageName + ".dzi";

                var displayViewModel = new DisplayViewModel(imageName, imageUrl, 0, associatedSlide);

                return View("Display", displayViewModel);
            }

            var associated = osr.ReadAssociatedImages();

            GenerateAssociatedImagesThumbnails(id.Value, associated);

            var viewModel = new AssociatedImagesViewModel(slide.Name, associated);
            ViewData["SlideId"] = id.Value;

            return View(viewModel);
        }

        private void GenerateAssociatedImagesThumbnails(int id, SlideDictionary<AssociatedImage> associated)
        {
            if (Directory.EnumerateFiles(AppDirectories.AssociatedImagesThumbs, $"{id}*").Any())
                return;

            logger.LogInformation("Generating thumbnails of associated images of slide {ID}", id);
            foreach (var image in associated)
            {
                var thumb = image.Value.GetThumbnail(new Size(400, 400));
                thumb.Save($@"{AppDirectories.AssociatedImagesThumbs}{id}_{image.Key}.jpeg", ImageFormat.Jpeg);
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
                HttpContext.Session.SetString(ALERT, NO_ACCESS);
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
                HttpContext.Session.SetString(ALERT, CANT_LOAD);
            }
            return new FileContentResult(new byte[] { }, "");
        }

        // GET: Slides/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            logger.LogInformation("Getting details of slide {ID}", id);
            if (id == null)
            {
                return NotFound();
            }

            var slide = await slideRepository.GetByIdAsync(id.Value);

            if (slide == null)
            {
                return NotFound();
            }

            return View(slide);
        }

        // GET: Slides/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Slides/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,FilePath,SlideUrl,SlideDziUrl,SlideMpp")] Slide slide)
        {
            if (ModelState.IsValid)
            {
                await slideRepository.InsertAsync(slide);
                return RedirectToAction("Index");
            }
            return View(slide);
        }

        // GET: Slides/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slide = await slideRepository.GetByIdAsync(id.Value);

            if (slide == null)
            {
                return NotFound();
            }
            return View(slide);
        }

        // POST: Slides/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,FilePath,SlideUrl,SlideDziUrl,SlideMpp")] Slide slide)
        {
            if (id != slide.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await slideRepository.UpdateAsync(slide);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SlideExists(slide.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction("Index");
            }
            return View(slide);
        }

        // GET: Slides/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var slide = await slideRepository.GetByIdAsync(id.Value);

            if (slide == null)
            {
                return NotFound();
            }

            return View(slide);
        }

        // POST: Slides/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await slideRepository.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        private bool SlideExists(int id)
        {
            return context.Slides.Any(e => e.Id == id);
        }

        public IActionResult Comments()
        {
            throw new NotImplementedException();
        }
    }
}
