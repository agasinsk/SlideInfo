
using System.Collections.Generic;

namespace SlideInfo.App.Models
{
    public class MessengerViewModel
    {
        public string UserName { get; set; }
        public IEnumerable<MessengerUser> Users { get; set; }
        public IEnumerable<Message> CurrentConversation { get; set; }
    }
}
