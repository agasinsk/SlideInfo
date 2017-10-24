using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace SlideInfo.App.Models
{
    public class Conversation
    {
        [Key]
        public string Subject { get; set; }
        public IEnumerable<Message> Messages { get; set; }

        [NotMapped]
        public bool AllMessagesFetched { get; set; }

        [NotMapped]
        public string ReceiverId { get; set; }

        [NotMapped]
        public int UnreadMessagesCount
        {
            get => GetUnreadMessagesCount();
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        private int GetUnreadMessagesCount()
        {
            var unreadMessages = Messages.Where(m => !m.IsRead()).AsQueryable();
            return unreadMessages.Count();
        }

        [NotMapped]
        public IEnumerable<string> Users
        {
            get => GetConversationUserNames();
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
            }
        }

        public IEnumerable<string> GetConversationUserNames()
        {
            var userNames = new List<string>();
            foreach (var message in Messages)
            {
                if (!userNames.Contains(message.FromId))
                {
                    userNames.Add(message.FromId);
                }
            }
            return userNames;
        }

        public static IEnumerable<string> GetConversationUserNames(IEnumerable<Message> messageList)
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

        public static int GetUnreadMessagesCount(IEnumerable<Message> messageList)
        {
            var unreadMessages = messageList.Where(m => !m.IsRead()).AsQueryable();
            return unreadMessages.Count();
        }

        public static string GenerateConversationSubject(params string[] userIds)
        {
            var subjectBuilder = new StringBuilder();
            var sortedIds = userIds.OrderBy(item => item);
            foreach (var userId in sortedIds)
            {
                var userIdTruncated = userId.Split('-')[0];
                subjectBuilder.Append(userIdTruncated).Append("-");
            }
            return subjectBuilder.ToString();
        }
    }
}
