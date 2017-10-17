using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlideInfo.App.Models
{
    public class Conversation
    {
        public int Id { get; set; }
        public IEnumerable<Message> Messages { get; set; }
        public int MessagesCount { get; set; }
        public string Subject { get; set; }
    }
}
