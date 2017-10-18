using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SlideInfo.App.Models
{
    public class Message
    {
        public int Id { get; set; }

        public string FromId { get; set; }// -- Foreign key AppUser.Id
        public string ToId { get; set; }// -- Foreign key AppUser.Id

        public string Subject { get; set; }
        public string Content { get; set; }

        //public Attachment Attachment { get; set; }//, -- can be null or default to a 0

        public DateTime DateSent { get; set; }// -- can be null or default to 1901 or sumin'
        public DateTime DateRead { get; set; }

        [JsonIgnore]
        public Conversation Conversation { get; set; }
        [JsonIgnore]
        public AppUser From { get; set; }
        [JsonIgnore]
        public AppUser To { get; set; }

        public bool IsRead() => DateRead > DateSent;

    }
}
