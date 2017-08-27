using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SlideInfo.App.Models.ManageViewModels
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }
        public string UnconfirmedEmail { get; set; }
        public bool EmailConfirmed { get; set; }


        public IList<UserLoginInfo> Logins { get; set; }

        public bool BrowserRemembered { get; set; }

        public int CommentsCount { get; set; }

        public IEnumerable<Comment> Comments { get; set; }

    }
}
