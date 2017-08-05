using System;
using System.ComponentModel.DataAnnotations;

namespace SlideInfo.App.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [Required]
        public int SlideId { get; set; }
        [Required]
        public string AppUserId { get; set; }
        [Required]
        public string Text { get; set; }
        public DateTime Date { get; set; }

        public Slide Slide { get; set; }
        public AppUser AppUser { get; set; }

        public Comment()
        {
            Date = DateTime.Now;
        }
    }
}
