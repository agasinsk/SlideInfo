using System.ComponentModel.DataAnnotations;

namespace SlideInfo.App.Helpers
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
