using System.Collections.Generic;
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
            string receiver = null;
            if (conversationSubject != null)
            {
                currentConversation = context.Messages.Where(m => m.Subject == conversationSubject);
                receiver = conversationSubject;
            }
            var viewModel = new MessengerViewModel
            {
                UserName = userManager.GetUserName(User),
                Users = context.AppUsers.Where(u => u.UserName != userManager.GetUserName(User)).AsEnumerable()
                    .Select(u => new MessengerUser() { UserName = u.UserName, FullName = u.FullName, Status = MessengerUserStatus.Unavailable }),
                ReceiverUserName = receiver,
                CurrentConversation = currentConversation
            };

            return View(viewModel);
        }
    }
}