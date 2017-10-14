using System.ComponentModel.DataAnnotations;

namespace SlideInfo.App.Models
{
    public class MessengerUser
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public MessengerUserStatus Status { get; set; }
        public int UnreadMessagesCount { get; set; }
    }

    public enum MessengerUserStatus
    {
        [Display(Name = "available")]
        Available,
        [Display(Name = "unavailable")]
        Unavailable,
        [Display(Name = "away")]
        Away
    }
}
