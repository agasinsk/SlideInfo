using System.ComponentModel.DataAnnotations;

namespace SlideInfo.App.Constants
{
    public enum ContactEmailSubject
    {
        [Display(Name = "Bug report")]
        BugReport,
        [Display(Name = "Feature request")]
        FeatureRequest,
        [Display(Name = "User opinion")]
        UserOpinion,
        [Display(Name = "Other")]
        Other
    }
}
