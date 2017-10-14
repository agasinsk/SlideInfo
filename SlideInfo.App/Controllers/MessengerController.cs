﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SlideInfo.App.Data;
using SlideInfo.App.Models;

namespace SlideInfo.App.Controllers
{
    public class MessengerController : Controller
    {
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserManager<AppUser> userManager;
        private readonly SlideInfoDbContext context;

        public MessengerController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, SlideInfoDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
        }

        public IActionResult Messenger(string conversationSubject)
        {
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Login", "Account");
            }

            IEnumerable<Message> currentConversation = null;
            if (conversationSubject != null)
            {
                currentConversation = context.Messages.Where(m => m.Subject == conversationSubject);
            }
            var viewModel = new MessengerViewModel
            {
                UserName = userManager.GetUserName(User),
                Users = context.AppUsers.AsEnumerable()
                    .Select(u => new MessengerUser() { UserName = u.UserName, FullName = u.FullName }),
                ReceiverUserName = context.AppUsers.AsEnumerable().Last().UserName,
                CurrentConversation = currentConversation
            };

            return View(viewModel);
        }

        public JsonResult GetUserName()
        {
            var userName = userManager.GetUserName(User);
            return Json(userName);
        }

        public JsonResult GeserName()
        {
            var userName = userManager.GetUserName(User);
            return Json(userName);
        }
    }
}