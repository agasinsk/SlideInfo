using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SlideInfo.App.Constants;
using SlideInfo.App.Helpers;
using SlideInfo.App.Models;
using SlideInfo.App.Models.ContactViewModels;
using SlideInfo.App.Services;

namespace SlideInfo.App.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<AppUser> signInManager;
        private UserManager<AppUser> userManager;

        public HomeController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }


        //GET Contact
        public IActionResult Contact()
        {
            var viewModel = new ContactForm();
            if (signInManager.IsSignedIn(User))
            {
                var user = userManager.GetUserAsync(User).Result;
                viewModel.Name = user.FullName;
                viewModel.Email = user.Email;
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactForm contactForm)
        {
            try
            {
                EmailSender.SendEmailAsync(contactForm.Email, MessageConstants.AppEmail, contactForm.Subject.GetDisplayName(), contactForm.Message);

                new AlertFactory(HttpContext).CreateAlert(AlertType.Success, ViewDataConstants.EMAIL_SENT);
            }
            catch (Exception)
            {
                new AlertFactory(HttpContext).CreateAlert(AlertType.Danger, ViewDataConstants.EMAIL_ERROR);
            }

            return RedirectToAction("Contact", "Home");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
