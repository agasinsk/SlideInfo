using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SlideInfo.Core;

namespace SlideInfo.App.Models.SlideViewModels
{
    public class AssociatedImagesViewModel
    {
        public string Name { get; set; }
        public IDictionary<string, AssociatedImage> AssociatedImages { get; set; }

        public AssociatedImagesViewModel(string name, IDictionary<string, AssociatedImage> associated)
        {
            Name = name;
            AssociatedImages = associated;
        }

    }
}
