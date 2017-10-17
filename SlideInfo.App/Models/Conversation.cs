
using System.Collections.Generic;

namespace SlideInfo.App.Models
{
    public class Conversation
    {
        public int Id { get; set; }
        public IEnumerable<Message> Messages { get; set; }
        public string Subject { get; set; }
        public IEnumerable<string> Users { get; set; }
    }
}
