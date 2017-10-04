using System.ComponentModel.DataAnnotations;
using SlideInfo.App.Constants;

namespace SlideInfo.App.Models.ContactViewModels
{
    public class ContactForm
    {
        [Required]
        [StringLength(40, MinimumLength = 3)]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public ContactEmailSubject Subject { get; set; }
        [Required]
        public string Message { get; set; }
    }
}
