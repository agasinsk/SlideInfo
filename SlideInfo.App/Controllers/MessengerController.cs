using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SlideInfo.App.Data;
using SlideInfo.App.Models;

namespace SlideInfo.App.Controllers
{
    public class MessengerController : Controller
    {
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserManager<AppUser> userManager;
        private readonly SlideInfoDbContext context;

        public MessengerController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            SlideInfoDbContext context)
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
                Users = context.AppUsers.Where(u => u.UserName != userManager.GetUserName(User)).AsEnumerable()
                    .Select(u => new MessengerUser()
                    {
                        UserName = u.UserName,
                        FullName = u.FullName,
                        Status = MessengerUserStatus.Unavailable
                    }),
                CurrentConversation = currentConversation
            };

            return View(viewModel);
        }

        [Route("[controller]/Users")]
        public string GetAppUsers()
        {
            var users = context.AppUsers.Where(u => u.UserName != userManager.GetUserName(User)).AsEnumerable()
                .Select(u => new MessengerUser()
                {
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Status = MessengerUserStatus.Unavailable,
                    UnreadMessagesCount = 0,
                });
            var usersJson = JsonConvert.SerializeObject(users);
            return usersJson;
        }

        [Route("[controller]/CurrentUser")]
        public string GetCurrentUser(string conversationSubject)
        {
            return userManager.GetUserName(User);
        }

        [Route("[controller]/Conversation/{conversationSubject}")]
        public string GetConversation(string conversationSubject)
        {
            var mockConversation = new List<Message>();
            for (var i = 0; i < 10; i++)
            {
                var message = new Message
                {
                    Id = i,
                    Content = "content of " + i + "vip",
                    FromId = "arturgasinski@rmail.com",
                    ToId = "arturgasinski@hotmail.com",
                    Subject = "Subject1",
                    DateReceived = DateTime.Now.AddSeconds(i)
                };
                mockConversation.Add(message);
            }

            return JsonConvert.SerializeObject(mockConversation);


            /*
            IEnumerable<Message> currentConversation = null;
            if (conversationSubject != null)
            {
                currentConversation = context.Messages.Where(m => m.Subject == conversationSubject);
            }
            return JsonConvert.SerializeObject(currentConversation);
            */
        }
    }
}
