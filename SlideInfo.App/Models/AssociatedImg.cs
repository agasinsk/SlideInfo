using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SlideInfo.App.Helpers;

namespace SlideInfo.App.Models
{
    public class AssociatedImg
    {
        public int Id { get; set; }
        public int SlideId { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Url { get; set; }
        public string DziUrl => Url + ".dzi";
        public double Mpp => 0;

        public AssociatedImg()
        { }

        public AssociatedImg(int slideId, string imageName)
        {
            SlideId = slideId;
            Name = imageName;
            Url = UrlFormatter.UrlFor(Name);
        }

        public AssociatedImg(Slide slide, string imageName)
        {
            SlideId = slide.Id;
            Name = slide.Name + imageName;
            Url = UrlFormatter.UrlFor(Name);
        }
    }
}
