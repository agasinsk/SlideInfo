using SlideInfo.App.Helpers;

namespace SlideInfo.App.Models.SlideViewModels
{
    public class CommentsViewModel
    {
        public string Name { get; set; }
        public PaginatedList<Comment> Comments { get; set; }
        public CommentsViewModel(string name, PaginatedList<Comment> comments)
        {
            Name = name;
            Comments = comments;
        }
    }
}
