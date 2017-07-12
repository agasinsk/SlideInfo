using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using SlideInfo.Core;

namespace SlideInfo.WebApp.Models
{
	public class Slide
	{
		public string FileName { get; set; }
		public string SlideUrl { get; set; }
		public string SlideDziUrl { get; set; }
		public double SlideMpp { get; set; }

		public IDictionary<string, string> Properties { get; set; }
		public IDictionary<string, AssociatedImage> AssociatedImages { get; set; }

		public DeepZoomGenerator DeepZoomGenerator { get; set; }
		public ImageCodecInfo FormatEncoder { get; set; }
		public EncoderParameters QualityParameter { get; set; }

		public Slide() { }

		public Slide(string pathToSlide)
		{
			var osr = new OpenSlide(pathToSlide);

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

			var fileName = Path.GetFileName(pathToSlide);
			FileName = fileName;
			SlideUrl = UrlFormatter.UrlFor(fileName);
			SlideDziUrl = SlideUrl + ".dzi";

			Properties = osr.ReadProperties();
			//AssociatedImages = osr.ReadAssociatedImages();

			DeepZoomGenerator = new DeepZoomGenerator(osr, DefaultOptions.DEEPZOOM_TILE_SIZE, DefaultOptions.DEEPZOOM_OVERLAP);
			QualityParameter = DefaultOptions.GetQualityEncoderParameter();
			FormatEncoder = DefaultOptions.GetEncoder(DefaultOptions.DEEPZOOM_FORMAT == "png" ? ImageFormat.Png : ImageFormat.Jpeg);
		}

	}
}
