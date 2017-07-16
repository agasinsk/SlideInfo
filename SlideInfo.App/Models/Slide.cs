using System;
using System.Collections.Generic;
using System.IO;
using SlideInfo.App.Helpers;
using SlideInfo.Core;

namespace SlideInfo.App.Models
{
    [Serializable]
    public class Slide
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Url { get; set; }
        public string DziUrl => Url + ".dzi";
        public double Mpp { get; set; }
        public string Vendor { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Property> Properties { get; set; }

        public Slide()
        {

        }

        public Slide(string pathToSlide)
        {
            FilePath = pathToSlide;
            Name = Path.GetFileName(pathToSlide);
            Url = UrlFormatter.UrlFor(Name);
            Vendor = OpenSlide.DetectVendor(pathToSlide);

            using (var osr = new OpenSlide(pathToSlide))
            {
                try
                {
                    double.TryParse(osr.Properties[OpenSlide.PROPERTY_NAME_MPP_X], out double mppX);
                    double.TryParse(osr.Properties[OpenSlide.PROPERTY_NAME_MPP_Y], out double mppY);
                    Mpp = (mppX + mppY) / 2;
                }
                catch (Exception)
                {
                    Mpp = 0;
                }
            }
        }

        public Slide(OpenSlide osr)
        {
            FilePath = osr.FilePath;
            Name = Path.GetFileName(FilePath);
            Url = UrlFormatter.UrlFor(Name);

            try
            {
                double.TryParse(osr.Properties[OpenSlide.PROPERTY_NAME_MPP_X], out double mppX);
                double.TryParse(osr.Properties[OpenSlide.PROPERTY_NAME_MPP_Y], out double mppY);
                Mpp = (mppX + mppY) / 2;
            }
            catch (Exception)
            {
                Mpp = 0;
            }
        }
    }
}
