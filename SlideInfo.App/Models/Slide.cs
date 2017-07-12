using System;
using System.Collections.Generic;
using System.IO;
using SlideInfo.App.Helpers;
using SlideInfo.Core;

namespace SlideInfo.App.Models
{
	public class Slide
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string FilePath { get; set; }
		public string SlideUrl { get; set; }
		public string SlideDziUrl { get; set; }
		public double SlideMpp { get; set; }
		public int QuickHash { get; set; }

		public virtual ICollection<Comment> Comments { get; set; }

		public Slide()
		{

		}

		public Slide(string pathToSlide)
		{
			FilePath = pathToSlide;
			Name = Path.GetFileName(pathToSlide);
			SlideUrl = UrlFormatter.UrlFor(Name);
			SlideDziUrl = SlideUrl + ".dzi";

			using (var osr = new OpenSlide(pathToSlide))
			{
				try
				{
					double.TryParse(osr.Properties[OpenSlide.PROPERTY_NAME_MPP_X], out double mppX);
					double.TryParse(osr.Properties[OpenSlide.PROPERTY_NAME_MPP_Y], out double mppY);
					SlideMpp = (mppX + mppY) / 2;
				}
				catch (Exception)
				{
					SlideMpp = 0;
				}

				QuickHash = osr.QuickHash1;
			}
		}
	}
}
