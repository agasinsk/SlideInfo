using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SlideInfo.App.Helpers;
using SlideInfo.App.Models.ContactViewModels;
using SlideInfo.App.Services;

namespace SlideInfo.App.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }


        //GET Contact
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactForm contactForm)
        {
            try { 
                EmailSender.SendEmailAsync(MessageConstants.APP_EMAIL, contactForm.Subject.GetDisplayName(), contactForm.Message);

                new AlertFactory(HttpContext).CreateAlert(AlertType.Success, ViewDataConstants.EMAIL_SENT);
            }
            catch (Exception)
            {
                new AlertFactory(HttpContext).CreateAlert(AlertType.Danger, ViewDataConstants.EMAIL_ERROR);
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
