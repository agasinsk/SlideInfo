using SlideInfo.App.Helpers;

namespace SlideInfo.App.Models.ManageViewModels

{
    public class UserCommentsViewModel
    {
        public string UserName { get; set; }
        public PaginatedList<Comment> Comments { get; set; }
        public UserCommentsViewModel(string userName, PaginatedList<Comment> comments)
        {
            UserName = userName;
            Comments = comments;
        }
    }
}
