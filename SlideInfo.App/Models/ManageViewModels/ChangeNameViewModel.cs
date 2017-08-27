using System.ComponentModel.DataAnnotations;

namespace SlideInfo.App.Models.ManageViewModels
{
    public class ChangeNameViewModel
    {
        [Display(Name = "Current first name")]
        public string OldFirstName { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [Display(Name = "New first name")]
        public string NewFirstName { get; set; }

        [Display(Name = "Current last name")]
        public string OldLastName { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        [Display(Name = "New last name")]
        public string NewLastName { get; set; }
    }
}
