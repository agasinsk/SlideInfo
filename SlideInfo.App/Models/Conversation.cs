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
        public int MessagesCount { get; set; }

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
