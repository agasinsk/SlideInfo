using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
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
            var users = context.AppUsers.Where(u => u.UserName != userManager.GetUserName(User)).AsEnumerable()
                .Select(u => new MessengerUser()
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    UnreadMessagesCount = 0,
                });

            return JsonConvert.SerializeObject(users);
        }

        [Route("[controller]/CurrentUser")]
        public string GetCurrentUser(string conversationSubject)
        {
            return userManager.GetUserName(User);
        }

        [Route("[controller]/Conversation/{conversationSubject}")]
        public string GetConversation(string conversationSubject)
        {
            IEnumerable<Message> messageList = null;
            IEnumerable<string> users = null;
            if (conversationSubject != null)
            {
                messageList = context.Messages.Where(m => m.Subject == conversationSubject);
                if (!messageList.Any())
                {
                    users = new List<string>
                    {
                        conversationSubject,
                        IdentityExtensions.GetUserName(Request.HttpContext.User.Identity)
                    };
                }
                else
                {
                    users = GetConversationUserNames(messageList);
                }
            }
            //var messageList = new List<Message>();
            //for (var i = 0; i < 11; i++)
            //{
            //    var message = new Message
            //    {
            //        Id = i,
            //        Content = "content of " + i + "vip",
            //        FromId = "arturgasinski@gmail.com",
            //        ToId = "arturgasinski@hotmail.com",
            //        Subject = "mink",
            //        DateSent = DateTime.Now.AddMinutes(i).AddSeconds(0.5 * i)
            //    };
            //    messageList.Add(message);
            //}
            var conversation = new Conversation { Messages = messageList, Subject = conversationSubject, Users = users };

            return JsonConvert.SerializeObject(conversation);
        }

        public IEnumerable<string> GetConversationUserNames(IEnumerable<Message> messageList)
        {
            var userNames = new List<string>();
            foreach (var message in messageList)
            {
                if (!userNames.Contains(message.FromId))
                {
                    userNames.Add(message.FromId);
                }
            }
            return userNames;
        }


    }
}
