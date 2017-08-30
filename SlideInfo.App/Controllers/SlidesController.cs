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
using SlideInfo.App.Constants;
using SlideInfo.App.Data;
using SlideInfo.App.Helpers;
using SlideInfo.App.Models;
using SlideInfo.App.Models.SlideViewModels;
using SlideInfo.App.Repositories;
using SlideInfo.Core;
using static SlideInfo.App.Constants.ViewDataConstants;

namespace SlideInfo.App.Controllers
{
    public partial class SlidesController : Controller
    {
        private readonly SignInManager<AppUser> signInManager;
        private UserManager<AppUser> userManager;
        private readonly SlideInfoDbContext context;
        private readonly AsyncRepository<Slide> slideRepository;
        private readonly AlertFactory alertFactory;
        private readonly ILogger logger;


        public SlidesController(ILogger<SlidesController> logger, SlideInfoDbContext context, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.logger = logger;
            this.context = context;
            alertFactory = new AlertFactory(HttpContext);
            slideRepository = new AsyncRepository<Slide>(context);
        }

        // GET: Slides
        public async Task<IActionResult> Index(int? id, string sortOrder,
            string currentFilter, string searchString)
        {
            if (!signInManager.IsSignedIn(User))
                return RedirectToAction("Login", "Account");

            logger.LogInformation("Getting all slides...");

            ViewData[NAME_SORT_PARAM] = String.IsNullOrEmpty(sortOrder) ? "Name_desc" : "";
            ViewData[VENDOR_SORT_PARAM] = sortOrder == "Vendor" ? "Vendor_desc" : "Vendor";
            ViewData[WIDTH_SORT_PARAM] = sortOrder == "Width" ? "Width_desc" : "Width";
            ViewData[HEIGHT_SORT_PARAM] = sortOrder == "Height" ? "Height_desc" : "Height";
            ViewData[CURRENT_FILTER] = searchString;
            ViewData[SLIDE_ID] = null;
            HttpContext.Session.Remove(SLIDE_ID);
            ViewData[HAS_ASSOCIATED_IMAGES] = null;
            ViewData[HAS_COMMENTS] = null;

            var slides = from s in context.Slides.Include(s => s.Comments)
                         select s;

            if (!string.IsNullOrEmpty(searchString))
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
                @descending = true;
            }

            slides = @descending ?
                slides.OrderByDescending(e => EF.Property<object>(e, sortOrder))
                : slides.OrderBy(e => EF.Property<object>(e, sortOrder));

            var finalSlides = await slides.AsNoTracking().ToListAsync();
            GenerateSlideThumbnails(finalSlides);
            return View(finalSlides);
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

            var slide = await context.Slides.FindAsync(id.Value);

            if (slide == null)
            {
                logger.LogError("GetById({ID}) NOT FOUND", id);
                return NotFound();
            }

            HttpContext.Session.Set(SessionConstants.CURRENT_SLIDE, slide);
            ViewData[SLIDE_ID] = slide.Id.ToString();
            HttpContext.Session.SetString(SLIDE_ID, slide.Id.ToString());
            ViewData[SLIDE_NAME] = slide.Name;
            ViewData[HAS_ASSOCIATED_IMAGES] = slide.HasAssociatedImages;
            var comments = context.Comments.Where(c => c.SlideId == id);
            ViewData[HAS_COMMENTS] = comments.Any();

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
            HttpContext.Session.SetString(SLIDE_ID, slide.Id.ToString());
            ViewData[SLIDE_NAME] = slide.Name;
            ViewData[HAS_ASSOCIATED_IMAGES] = slide.HasAssociatedImages;
            var comments = context.Comments.Where(c => c.SlideId == id);
            ViewData[HAS_COMMENTS] = comments.Any();

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
            HttpContext.Session.SetString(SLIDE_ID, slide.Id.ToString());
            ViewData[HAS_ASSOCIATED_IMAGES] = slide.HasAssociatedImages;
            var comments = context.Comments.Where(c => c.SlideId == id);
            ViewData[HAS_COMMENTS] = comments.Any();
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

            GenerateAssociatedImagesThumbnails(id.Value, associated);

            var viewModel = new AssociatedImagesViewModel(slide.Name, associated);

            return View(viewModel);
        }

        // GET: Slides/Comments/5
        public async Task<IActionResult> Comments(int? id, string sortOrder,
            string currentFilter, string searchString, int? pageSize, int? page)
        {
            ViewData[CURRENT_SORT] = sortOrder;
            ViewData[DATE_SORT_PARAM] = String.IsNullOrEmpty(sortOrder) ? "Date_desc" : "";
            ViewData[TEXT_SORT_PARAM] = sortOrder == "Text" ? "Text_desc" : "Text";
            ViewData[USER_SORT_PARAM] = sortOrder == "AppUserId" ? "AppUserId_desc" : "AppUserId";
            ViewData[CURRENT_FILTER] = searchString;

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
            HttpContext.Session.SetString(SLIDE_ID, slide.Id.ToString());
            ViewData[SLIDE_NAME] = slide.Name;
            ViewData[HAS_ASSOCIATED_IMAGES] = slide.HasAssociatedImages;

            var comments = context.Comments.Where(c => c.SlideId == id);
            ViewData[HAS_COMMENTS] = comments.Any();

            //filtering
            if (!String.IsNullOrEmpty(searchString))
            {
                logger.LogInformation("Searching for comments containing {searchString}", searchString);
                comments = comments.Where(s => s.Text.Contains(searchString) || s.AppUser.FullName.Contains(searchString));
            }

            if (string.IsNullOrEmpty(sortOrder))
            {
                sortOrder = "Date";
            }

            var descending = false;
            if (sortOrder.EndsWith("_desc"))
            {
                sortOrder = sortOrder.Substring(0, sortOrder.Length - 5);
                descending = true;
            }

            comments = descending ?
                comments.OrderByDescending(e => EF.Property<object>(e, sortOrder))
                : comments.OrderBy(e => EF.Property<object>(e, sortOrder));

            var defaultPageSize = 15;

            var paginatedComments = await PaginatedList<Comment>.
                CreateAsync(comments.Include(c => c.AppUser).AsNoTracking(), page ?? 1, pageSize ?? defaultPageSize);
            var viewModel = new CommentsViewModel(slide.Name, paginatedComments);
            return View(viewModel);
        }

        public async Task<IActionResult> CreateComment()
        {
            try
            {
                var username = Request.Form["commentUserName"].ToString();
                var appUser = context.Users.FirstAsync(s => s.Email == username).Result;

                var slideName = Request.Form["commentSlideName"].ToString();
                var slide = context.Slides.FirstAsync(s => s.Name == slideName).Result;

                var commentText = Request.Form["commentText"].ToString();
                //TODO: validation of comment fields

                var comment = new Comment() { AppUserId = appUser.Id, SlideId = slide.Id, Text = commentText };

                context.Add(comment);
                await context.SaveChangesAsync();

                new AlertFactory(HttpContext).CreateAlert(AlertType.Success, INSERT_COMMENT_SUCCESS);
                return RedirectToAction("Comments", "Slides", new { id = slide.Id });
            }
            catch
            {
                new AlertFactory(HttpContext).CreateAlert(AlertType.Danger, INSERT_COMMENT_FAILED);
                return RedirectToAction("Index", "Slides");
            }
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

    }
}
