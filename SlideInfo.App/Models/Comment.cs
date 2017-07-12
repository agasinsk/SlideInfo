using System;
using System.ComponentModel.DataAnnotations;

namespace SlideInfo.App.Models
{
    public class Comment
    {
	    public int Id { get; set; }
	    [Required]
		public int AppUserId { get; set; }
	    [Required]
		public int SlideId { get; set; }
	    [Required]
		public string Text { get; set; }
		public DateTime Date { get; set; }
	}
}
