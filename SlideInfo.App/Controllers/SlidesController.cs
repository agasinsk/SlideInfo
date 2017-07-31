using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SlideInfo.App.Data;
using SlideInfo.App.Helpers;
using SlideInfo.App.Models;
using SlideInfo.App.Models.SlideViewModels;
using SlideInfo.App.Repositories;
using SlideInfo.Core;
using static SlideInfo.App.Helpers.ViewDataConstants;

namespace SlideInfo.App.Controllers
{
    public partial class SlidesController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly SlideInfoDbContext context;
        private readonly AsyncRepository<Slide> slideRepository;
        private readonly ILogger logger;

        public SlidesController(ILogger<SlidesController> logger, SlideInfoDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.context = context;
            slideRepository = new AsyncRepository<Slide>(context);
        }

        // GET: Slides
        public async Task<IActionResult> Index(int? id, string sortOrder,
            string currentFilter, string searchString)
        {
            if (signInManager.IsSignedIn(User))
            {
                logger.LogInformation("Getting all slides...");

                ViewData[NAME_SORT_PARAM] = String.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";
                ViewData[VENDOR_SORT_PARAM] = sortOrder == "Vendor" ? "Vendor_desc" : "Vendor";
                ViewData[WIDTH_SORT_PARAM] = sortOrder == "Width" ? "Width_desc" : "Width";
                ViewData[HEIGHT_SORT_PARAM] = sortOrder == "Height" ? "Height_desc" : "Height";
                ViewData[CURRENT_FILTER] = searchString;
                ViewData[SLIDE_ID] = null;
                ViewData[HAS_ASSOCIATED_IMAGES] = null;
                ViewData[HAS_COMMENTS] = null;

                HttpContext.Session.Remove(SessionConstants.CURRENT_SLIDE);

                var slides = from s in context.Slides
                             select s;

                if (!String.IsNullOrEmpty(searchString))
                {
                    slides = slides.Where(s => s.Name.Contains(searchString)
                                                   || s.Vendor.Contains(searchString));
                }

                if (string.IsNullOrEmpty(sortOrder))
                {
                    sortOrder = "Name";
                }

                var descending = false;
                if (sortOrder.EndsWith("_desc"))
                {
                    sortOrder = sortOrder.Substring(0, sortOrder.Length - 5);
                    descending = true;
                }

                slides = @descending ?
                    slides.OrderByDescending(e => EF.Property<object>(e, sortOrder))
                    : slides.OrderBy(e => EF.Property<object>(e, sortOrder));

                var finalSlides = await slides.AsNoTracking().ToListAsync();
                GetSlideThumbnails(finalSlides);
                return View(finalSlides);
            }
            return RedirectToAction("Login", "Account");
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

            HttpContext.Session.Set(SessionConstants.CURRENT_SLIDE, slide);
            ViewData[SLIDE_ID] = slide.Id.ToString();
            ViewData[SLIDE_NAME] = slide.Name;
            ViewData[HAS_ASSOCIATED_IMAGES] = slide.HasAssociatedImages;
            ViewData[HAS_COMMENTS] = slide.Comments != null && slide.Comments.Any();

            var osr = new OpenSlide(slide.FilePath);
            var viewModel = new DisplayViewModel(slide.Name, slide.DziUrl, slide.Mpp, osr);

            return View(viewModel);
        }

        // GET: Slides/Properties/5
        public async Task<IActionResult> Properties(int? id, string sortOrder,
            string currentFilter, string searchString, int? pageSize, int? page)
        {
            ViewData[CURRENT_SORT] = sortOrder;
            ViewData[NAME_SORT_PARAM] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData[CURRENT_FILTER] = searchString;

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
            ViewData[SLIDE_ID] = slide.Id.ToString();
            ViewData[SLIDE_NAME] = slide.Name;
            ViewData[HAS_ASSOCIATED_IMAGES] = slide.HasAssociatedImages;
            ViewData[HAS_COMMENTS] = slide.Comments != null && slide.Comments.Any();

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


            var defaultPageSize = 15;

            var paginatedProperties = await PaginatedList<Property>.
                CreateAsync(properties.AsNoTracking(), page ?? 1, pageSize ?? defaultPageSize);
            var viewModel = new PropertiesViewModel(slide.Name, paginatedProperties);
            return View(viewModel);
        }

        // GET: Slides/AssociatedImages/5
        public async Task<IActionResult> AssociatedImages(int? id, string imageName)
        {
            logger.LogInformation("Getting associated images of slide {ID}...", id);

            if (id == null)
            {
                return NotFound();
            }

            var slide = await slideRepository.GetByIdAsync(id.Value);

            if (slide == null)
            {
                return NotFound();
            }
            ViewData[SLIDE_NAME] = slide.Name;
            ViewData[SLIDE_ID] = id.ToString();
            ViewData[HAS_ASSOCIATED_IMAGES] = slide.HasAssociatedImages;
            ViewData[HAS_COMMENTS] = slide.Comments != null && slide.Comments.Any();
            var osr = new OpenSlide(slide.FilePath);

            if (!String.IsNullOrEmpty(imageName))
            {
                logger.LogInformation("Getting associated image {name} of slide {ID}...", imageName, id);
                var associatedSlide = osr.AssociatedImages[imageName].ToImageSlide();
                var imageUrl = slide.Id + "/" + imageName + ".dzi";

                var displayViewModel = new DisplayViewModel(imageName, imageUrl, 0, associatedSlide);

                return View("Display", displayViewModel);
            }

            var associated = osr.ReadAssociatedImages();

            GetAssociatedImagesThumbnails(id.Value, associated);

            var viewModel = new AssociatedImagesViewModel(slide.Name, associated);

            return View(viewModel);
        }

        // GET: Slides/Comments/5
        public async Task<IActionResult> Comments(int? id, string sortOrder,
            string currentFilter, string searchString, int? pageSize, int? page)
        {
            ViewData[CURRENT_SORT] = sortOrder;
            ViewData[NAME_SORT_PARAM] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData[CURRENT_FILTER] = searchString;

            logger.LogInformation("Getting comments of slide {ID}", id);
            if (id == null)
            {
                return NotFound();
            }

            var slide = await slideRepository.GetByIdAsync(id.Value);
            if (slide == null)
            {
                return NotFound();
            }
            ViewData[SLIDE_ID] = slide.Id.ToString();
            ViewData[SLIDE_NAME] = slide.Name;
            ViewData[HAS_ASSOCIATED_IMAGES] = slide.HasAssociatedImages;
            ViewData[HAS_COMMENTS] = slide.Comments != null && slide.Comments.Any();

            var comments = context.Comments.Where(c => c.SlideId == id);

            //filtering
            if (!String.IsNullOrEmpty(searchString))
            {
                logger.LogInformation("Searching for properties containing {searchString}", searchString);
                comments = comments.Where(s => s.Text.Contains(searchString));
            }

            //sorting
            switch (sortOrder)
            {
                case "name_desc":
                    comments = comments.OrderByDescending(s => s.Date);
                    logger.LogInformation("Sorting properties of slide {ID} by name descending", id);
                    break;
                default:
                    logger.LogInformation("Sorting properties of slide {ID} by name", id);
                    comments = comments.OrderBy(s => s.Date);
                    break;
            }


            var defaultPageSize = 15;

            var paginatedProperties = await PaginatedList<Comment>.
                CreateAsync(comments.AsNoTracking(), page ?? 1, pageSize ?? defaultPageSize);
            var viewModel = new CommentsViewModel(slide.Name, paginatedProperties);
            return View(viewModel);
        }

        public IActionResult CreateComment()
        {
            var slideId = ViewData[SLIDE_ID];
            HttpContext.Session.SetInt32(ViewDataConstants.SLIDE_ID, (int)slideId);
            return RedirectToAction("Create", "Comments");
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
