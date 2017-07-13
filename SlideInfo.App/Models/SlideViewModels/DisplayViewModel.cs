using SlideInfo.Core;

namespace SlideInfo.App.Models.SlideViewModels
{
    public class DisplayViewModel
    {
        public string Name { get; set; }
        public string SlideDziUrl { get; set; }
        public double SlideMpp { get; set; }

        public DeepZoomGenerator DeepZoomGenerator { get; set; }

        public DisplayViewModel(string name, string dziUrl, double mpp, AbstractSlide osr)
        {
            Name = name;
            SlideDziUrl = dziUrl;
            SlideMpp = mpp;
            DeepZoomGenerator = new DeepZoomGenerator(osr);
        }

        public DisplayViewModel(string name, string dziUrl, double mpp, DeepZoomGenerator generator)
        {
            Name = name;
            SlideDziUrl = dziUrl;
            SlideMpp = mpp;
            DeepZoomGenerator = generator;
        }

        public DisplayViewModel()
        {
        }
    }
}