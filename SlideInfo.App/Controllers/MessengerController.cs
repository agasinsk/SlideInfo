using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SlideInfo.App.Data;
using SlideInfo.App.Helpers;
using SlideInfo.App.Hubs;
using SlideInfo.App.Models;
using SlideInfo.Helpers;

namespace SlideInfo.App.Controllers
{
    public class MessengerController : Controller
    {
        private IServiceProvider serviceProvider;
        private readonly SignInManager<AppUser> signInManager;
        private readonly Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager;

        public MessengerController(Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager, IServiceProvider serviceProvider)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.serviceProvider = serviceProvider;
        }

        public IActionResult Messenger(string conversationSubject)
        {
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [Route("[controller]/Users")]
        public string GetAppUsers()
        {
            using (var context = serviceProvider.GetService<SlideInfoDbContext>())
            {
                var userId = context.AppUsers.FirstOrDefault(u => u.UserName == userManager.GetUserName(User))?.Id;
                var users = context.AppUsers.Where(u => u.UserName != userManager.GetUserName(User)).ToList()
                    .Select(u => new MessengerUser()
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        FullName = u.FullName,
                        Email = u.Email,
                        PrivateConversationSubject = Conversation.GenerateConversationSubject(u.Id, userId),
                        UnreadMessagesCount = context.Messages.Count(m =>
                            !m.IsRead() && m.FromId == u.Id && m.ToId == userManager.GetUserId(User))
                    });

                return JsonConvert.SerializeObject(users);
            }
        }

        [Route("[controller]/CurrentUser")]
        public string GetCurrentUser()
        {
            using (var context = serviceProvider.GetService<SlideInfoDbContext>())
            {
                var currentUser = context.AppUsers.FirstOrDefault(u => u.UserName == userManager.GetUserName(User));
                var messengerUser = new MessengerUser
                {
                    Id = currentUser?.Id,
                    FullName = currentUser?.FullName,
                    Email = currentUser?.Email
                };

                return JsonConvert.SerializeObject(messengerUser);
            }
        }

        [Route("[controller]/Conversations/")]
        public string GetConversations()
        {
            using (var context = serviceProvider.GetService<SlideInfoDbContext>())
            {
                var userId = context.AppUsers.FirstOrDefault(u => u.UserName == userManager.GetUserName(User))?.Id;
                var conversations = context.Messages.Where(m => m.ToId == userId || m.FromId == userId)
                    .Select(m => m.Id).Distinct();
                return JsonConvert.SerializeObject(conversations);
            }
        }

        [Route("[controller]/Conversation/{conversationSubject}/{pageNumber}")]
        public string GetConversation(string conversationSubject, int? pageNumber)
        {
            if (conversationSubject == null) return "";
            const int pageSize = 10;

            using (var context = serviceProvider.GetService<SlideInfoDbContext>())
            {
                var dbMessages = context.Messages.Where(m => m.Subject == conversationSubject)
                    .OrderByDescending(m => m.DateSent);
                var dbMessagesPage = dbMessages.AsNoTracking().Skip((pageNumber ?? 1 - 1) * pageSize).Take(pageSize);

                //setting messages as read
                if (dbMessagesPage.Any())
                {
                    var unreadMessages =
                        dbMessagesPage.Where(m => !m.IsRead() && m.ToId == userManager.GetUserId(User));
                    if (unreadMessages.Any())
                    {
                        var dateNow = DateTime.Now;
                        foreach (var unreadMessage in unreadMessages)
                        {
                            unreadMessage.DateRead = dateNow;
                            context.Update(unreadMessage);
                        }
                        context.SaveChanges();
                    }
                }

                var dbConversation = new Conversation
                {
                    Messages = dbMessagesPage,
                    Subject = conversationSubject,
                    MessagesCount = dbMessages.Count()
                };

                return JsonConvert.SerializeObject(dbConversation);
            }
        }

        [Route("[controller]/Conversation/Count{conversationSubject}")]
        public string GetConversationCount(string conversationSubject)
        {
            if (conversationSubject == null) return "";
            using (var context = serviceProvider.GetService<SlideInfoDbContext>())
            {
                var dbMessages = context.Messages.Where(m => m.Subject == conversationSubject)
                    .OrderByDescending(m => m.DateSent);
                return JsonConvert.SerializeObject(dbMessages.Count());
            }
        }

        [Route("[controller]/Save")]
        [HttpPost]
        public bool SaveMessage([FromBody] Message message)
        {
            if (message == null) return false;
            using (var context = serviceProvider.GetService<SlideInfoDbContext>())
            {
                context.Messages.Add(message);
                if (context.Conversations.Find(message.Subject) == null)
                {
                    context.Conversations.Add(new Conversation { Subject = message.Subject });
                }
                var savedCount = context.SaveChanges();
                return savedCount == 1;
            }
        }
    }
}
