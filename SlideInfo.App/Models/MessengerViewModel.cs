
using System.Collections.Generic;

namespace SlideInfo.App.Models
{
    public class MessengerViewModel
    {
        public IEnumerable<AppUser> Users { get; set; }
        public IEnumerable<Message> CurrentConversation { get; set; }
    }
}
