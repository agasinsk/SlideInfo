using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SlideInfo.App.Data;
using SlideInfo.App.Models;
using SlideInfo.Helpers;

namespace SlideInfo.App.Controllers
{
    public class MessengerController : Controller
    {
        private readonly SignInManager<AppUser> signInManager;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager;
        private readonly SlideInfoDbContext context;

        public MessengerController(Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
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
                    }),
                CurrentConversation = currentConversation
            };

            return View(viewModel);
        }

        [Route("[controller]/Users")]
        public string GetAppUsers()
        {
            var userId = context.AppUsers.FirstOrDefault(u => u.UserName == userManager.GetUserName(User))?.Id;
            var users = context.AppUsers.Where(u => u.UserName != userManager.GetUserName(User)).AsEnumerable()
                .Select(u => new MessengerUser()
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    PrivateConversationSubject = Conversation.GenerateConversationSubject(u.Id, userId)
                });

            return JsonConvert.SerializeObject(users);
        }

        [Route("[controller]/CurrentUser")]
        public string GetCurrentUser()
        {
            var currentUser = context.AppUsers.FirstOrDefault(u => u.UserName == userManager.GetUserName(User));
            var messengerUser = new MessengerUser
            {
                Id = currentUser?.Id,
                UserName = currentUser?.UserName,
                FullName = currentUser?.FullName
            };

            return JsonConvert.SerializeObject(messengerUser);
        }

        [Route("[controller]/Conversations/")]
        public string GetConversations()
        {
            var userId = context.AppUsers.FirstOrDefault(u => u.UserName == userManager.GetUserName(User))?.Id;
            var conversations = context.Messages.Where(m => m.ToId == userId || m.FromId == userId)
                .Select(m => m.Id).Distinct();
            return JsonConvert.SerializeObject(conversations);
        }

        [Route("[controller]/Conversation/{conversationSubject}")]
        public string GetConversation(string conversationSubject)
        {
            if (conversationSubject == null) return "";

            var dbMessages = context.Messages.Where(c => c.Subject == conversationSubject);
            var dbConversation = new Conversation { Messages = dbMessages, Subject = conversationSubject };

            if (!dbMessages.Any())
            {
                dbConversation.UnreadMessagesCount = 0;
                var users = context.Users.Where(u => u.Id.ContainsAny(conversationSubject.Split('-')));
                dbConversation.Users = users.Select(u => u.Id);
                var receivers = users.Where(u => u.UserName != userManager.GetUserName(User));
                dbConversation.ReceiverId = receivers.First().Id;
            }

            return JsonConvert.SerializeObject(dbConversation);
        }


    }
}
