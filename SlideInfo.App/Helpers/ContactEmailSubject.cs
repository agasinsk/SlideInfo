
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

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

    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumType)
        {
            return enumType.GetType().GetMember(enumType.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                .Name;
        }
    }
}
