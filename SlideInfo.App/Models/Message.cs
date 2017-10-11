using System;

namespace SlideInfo.App.Models
{
    public class Message
    {
        public int Id { get; set; }

        public string FromId { get; set; }// -- Foreign key AppUser.Id
        public string ToId { get; set; }// -- Foreign key AppUser.Id
        public string Subject { get; set; }
        public string Content { get; set; }

        public Attachment Attachment { get; set; }//, -- can be null or default to a 0

        public DateTime DateReceived { get; set; }// -- can be null or default to 1901 or sumin'
        public DateTime DateRead { get; set; }

        public AppUser From { get; set; }
        public AppUser To { get; set; }
    }
}
