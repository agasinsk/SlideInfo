using System.Collections.Generic;
using System.Drawing;

namespace SlideInfo.Core
{
	public abstract class AbstractSlide
	{
		public abstract int LevelCount { get; }

		/*A list of (width, height) tuples, one for each level of the image.
		level_dimensions[n] contains the dimensions of level n.*/
		public abstract IList<SizeL> LevelDimensions { get; }

		//A (width, height) tuple for level 0 of the image.
		public SizeL Dimensions => LevelDimensions[0];

		/*A list of downsampling factors for each level of the image.
		level_downsample[n] contains the downsample factor of level n.*/
		public abstract IList<double> LevelDownsamples { get; }

		public abstract SlideDictionary<string> Properties { get; set;  }
		public abstract SlideDictionary<AssociatedImage> AssociatedImages { get; set; }

		public abstract int GetBestLevelForDownsample(double downsample);
		
		//If the file format is not recognized, returns null.
		public abstract string DetectFormat(string fileName);
		
		public abstract void Close();
		
		/*Returns an Image containing the contents of the region.
		location: (x, y) tuple giving the top left pixel in the level 0 reference frame.
		level:    the level number.
		size:     (width, height) tuple giving the region size.*/
		public abstract Image ReadRegion(SizeL location, int level, SizeL size);
		
		//Returns a thumbnail of the image of the specified maximum size
		public abstract Image GetThumbnail(SizeL size);

		public static bool ThumbnailCallback()
		{
			return true;
		}
	}
}