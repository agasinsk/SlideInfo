using System.ComponentModel.DataAnnotations;

namespace SlideInfo.App.Models
{
    public class MessengerUser
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public int UnreadMessagesCount { get; set; }
    }
}
