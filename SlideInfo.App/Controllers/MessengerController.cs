﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
                    Email = u.Email,
                    PrivateConversationSubject = Conversation.GenerateConversationSubject(u.Id, userId),
                    UnreadMessagesCount = context.Messages.Count(m => !m.IsRead() && m.FromId == u.Id && m.ToId == userManager.GetUserId(User))
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
                FullName = currentUser?.FullName,
                Email = currentUser?.Email
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

        [Route("[controller]/Conversation/{conversationSubject}/{pageNumber}")]
        public string GetConversation(string conversationSubject, int? pageNumber)
        {
            if (conversationSubject == null) return "";
            const int pageSize = 10;

            var dbMessages = context.Messages.Where(m => m.Subject == conversationSubject).OrderByDescending(m => m.DateSent);
            var dbMessagesPage = dbMessages.AsNoTracking().Skip((pageNumber ?? 1 - 1) * pageSize).Take(pageSize);

            //setting messages as read
            if (dbMessagesPage.Any())
            {
                var unreadMessages = dbMessagesPage.Where(m => !m.IsRead() && m.ToId == userManager.GetUserId(User));
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

            var dbConversation = new Conversation { Messages = dbMessagesPage, Subject = conversationSubject, MessagesCount = dbMessages.Count() };

            return JsonConvert.SerializeObject(dbConversation);
        }

        [Route("[controller]/Conversation/Count{conversationSubject}")]
        public string GetConversationCount(string conversationSubject)
        {
            if (conversationSubject == null) return "";

            var dbMessages = context.Messages.Where(m => m.Subject == conversationSubject).OrderByDescending(m => m.DateSent);
            return JsonConvert.SerializeObject(dbMessages.Count());
        }

        [Route("[controller]/Save")]
        [HttpPost]
        public bool SaveMessage([FromBody] Message message)
        {
            if (message == null) return false;

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
