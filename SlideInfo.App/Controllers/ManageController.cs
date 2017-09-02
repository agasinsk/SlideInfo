using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlideInfo.App.Constants;
using SlideInfo.App.Data;
using SlideInfo.App.Helpers;
using SlideInfo.App.Models;
using SlideInfo.App.Models.ManageViewModels;
using SlideInfo.App.Services;
using static SlideInfo.App.Constants.ViewDataConstants;

namespace SlideInfo.App.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly SlideInfoDbContext context;
        private readonly string externalCookieScheme;
        private readonly IEmailSender emailSender;
        private readonly ISmsSender smsSender;
        private readonly ILogger logger;

        public ManageController(
          UserManager<AppUser> userManager,
          SignInManager<AppUser> signInManager,
          IOptions<IdentityCookieOptions> identityCookieOptions,
          IEmailSender emailSender,
          ISmsSender smsSender,
          ILoggerFactory loggerFactory,
          SlideInfoDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            this.emailSender = emailSender;
            this.smsSender = smsSender;
            this.context = context;
            logger = loggerFactory.CreateLogger<ManageController>();
        }

        // GET: /Manage/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }

            var model = new IndexViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                UnconfirmedEmail = user.UnconfirmedEmail,
                EmailConfirmed = user.EmailConfirmed,
                HasPassword = await userManager.HasPasswordAsync(user),
                Logins = await userManager.GetLoginsAsync(user),
                BrowserRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user),
                CommentsCount = context.Comments.Count(c => c.AppUserId == user.Id)
            };
            return View(model);
        }

        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account)
        {

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);

                    new AlertFactory(HttpContext).CreateAlert(AlertType.Success, SessionConstants.RemoveLoginSuccess);
                }
            }
            else
            {
                new AlertFactory(HttpContext).CreateAlert(AlertType.Danger, SessionConstants.Error);
            }
            return RedirectToAction(nameof(ManageLogins));
        }

        // GET: /Manage/Comments
        public async Task<IActionResult> Comments(string sortOrder,
            string currentFilter, string searchString, int? pageSize, int? page)
        {
            ViewData[CURRENT_SORT] = sortOrder;
            ViewData[DATE_SORT_PARAM] = string.IsNullOrEmpty(sortOrder) ? "Date_desc" : "";
            ViewData[TEXT_SORT_PARAM] = sortOrder == "Text" ? "Text_desc" : "Text";
            ViewData[SLIDE_SORT_PARAM] = sortOrder == "SlideId" ? "SlideId_desc" : "SlideId";
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
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            logger.LogInformation("Getting comments of user {ID}", user.Id);

            var comments = context.Comments.Where(c => c.AppUserId == user.Id);


            //filtering
            if (!string.IsNullOrEmpty(searchString))
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
                CreateAsync(comments.Include(c => c.Slide).AsNoTracking(), page ?? 1, pageSize ?? defaultPageSize);
            var viewModel = new UserCommentsViewModel(user.FullName, paginatedComments);
            return View(viewModel);
        }

        // GET: /Manage/ChangeEmail
        [HttpGet]
        public async Task<IActionResult> ChangeEmail()
        {
            var user = await GetCurrentUserAsync();
            var model = new ChangeEmailViewModel
            {
                OldEmail = await userManager.GetEmailAsync(user)
            };
            return View(model);
        }

        //
        // POST: /Manage/ChangeEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                //doing a quick swap so we can send the appropriate confirmation email
                user.UnconfirmedEmail = user.Email;
                user.Email = model.NewEmail;
                user.EmailConfirmed = false;
                var result = await userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("ConfirmEmail", "Manage",
                        new { userId = user.Id, code = token }, protocol: HttpContext.Request.Scheme);
                    var confirmationEmailBody =
                        MessageConstants.ConfirmationEmailBodyTemplate.Replace("callbackUrl", callbackUrl);
                    await emailSender.SendEmailAsync(model.NewEmail, "Confirm your account", confirmationEmailBody);

                    logger.LogInformation(3, "User was sent a new confirmation link.");
                    return View("ConfirmationEmailSent");
                }
            }
            new AlertFactory(HttpContext).CreateAlert(AlertType.Danger, SessionConstants.Error);
            return RedirectToAction(nameof(Index));
        }


        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }
            var result = await userManager.ChangeEmailAsync(user, user.UnconfirmedEmail, code);
            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(user.UnconfirmedEmail))
                {
                    user.UserName = user.UnconfirmedEmail;
                    user.UnconfirmedEmail = "";

                    await userManager.UpdateAsync(user);
                }
            }
            return RedirectToAction("Index", "Manage");
        }

        public async Task<ActionResult> CancelUnconfirmedEmail(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user != null)
            {
                user.UnconfirmedEmail = "";
                user.EmailConfirmed = true;
                await userManager.UpdateAsync(user);
            }
            else
            {
                return View("Error");
            }

            return RedirectToAction("Index", "Manage");
        }

        [HttpGet]
        public async Task<IActionResult> ChangeName()
        {
            var user = await GetCurrentUserAsync();
            var model = new ChangeNameViewModel()
            {
                OldFirstName = user.FirstMidName,
                OldLastName = user.LastName
            };
            return View(model);
        }

        //
        // POST: /Manage/ChangeEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeName(ChangeNameViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                new AlertFactory(HttpContext).CreateAlert(AlertType.Danger, SessionConstants.Error);
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(model.NewFirstName))
            {
                user.FirstMidName = model.NewFirstName;
            }
            if (!string.IsNullOrEmpty(model.NewLastName))
            {
                user.LastName = model.NewLastName;
            }

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                logger.LogInformation(3, "User changed their name successfully.");
                new AlertFactory(HttpContext).CreateAlert(AlertType.Success, SessionConstants.ChangeNameSuccess);
                return RedirectToAction(nameof(Index));
            }
            AddErrors(result);
            return View(model);
        }

        // GET: /Manage/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);
                    logger.LogInformation(3, "User changed their password successfully.");
                    new AlertFactory(HttpContext).CreateAlert(AlertType.Success, SessionConstants.ChangePasswordSuccess);
                    return RedirectToAction(nameof(Index));
                }
                AddErrors(result);
                return View(model);
            }
            new AlertFactory(HttpContext).CreateAlert(AlertType.Danger, SessionConstants.Error);
            return RedirectToAction(nameof(Index));
        }

        //
        // GET: /Manage/SetPassword
        [HttpGet]
        public IActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await userManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);

                    new AlertFactory(HttpContext).CreateAlert(AlertType.Success, SessionConstants.SetPasswordSuccess);
                    return RedirectToAction(nameof(Index));
                }
                AddErrors(result);
                return View(model);
            }
            new AlertFactory(HttpContext).CreateAlert(AlertType.Danger, SessionConstants.Error);
            return RedirectToAction(nameof(Index));
        }

        //GET: /Manage/ManageLogins
        [HttpGet]
        public async Task<IActionResult> ManageLogins()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await userManager.GetLoginsAsync(user);
            var otherLogins = signInManager.GetExternalAuthenticationSchemes().Where(auth => userLogins.All(ul => auth.AuthenticationScheme != ul.LoginProvider)).ToList();
            ViewData["ShowRemoveButton"] = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkLogin(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.Authentication.SignOutAsync(externalCookieScheme);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Action(nameof(LinkLoginCallback), "Manage");
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userManager.GetUserId(User));
            return Challenge(properties, provider);
        }

        //
        // GET: /Manage/LinkLoginCallback
        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var info = await signInManager.GetExternalLoginInfoAsync(await userManager.GetUserIdAsync(user));
            if (info == null)
            {
                new AlertFactory(HttpContext).CreateAlert(AlertType.Danger, SessionConstants.Error);
                return RedirectToAction(nameof(ManageLogins));
            }
            var result = await userManager.AddLoginAsync(user, info);
            if (result.Succeeded)
            {
                new AlertFactory(HttpContext).CreateAlert(AlertType.Success, SessionConstants.AddLoginSuccess);
                // Clear the existing external cookie to ensure a clean login process
                await HttpContext.Authentication.SignOutAsync(externalCookieScheme);

            }
            else
            {
                new AlertFactory(HttpContext).CreateAlert(AlertType.Danger, SessionConstants.Error);
            }
            return RedirectToAction(nameof(ManageLogins));
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private Task<AppUser> GetCurrentUserAsync()
        {
            return userManager.GetUserAsync(HttpContext.User);
        }

        #endregion
    }
}
