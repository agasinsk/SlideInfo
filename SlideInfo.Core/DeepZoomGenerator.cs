using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace SlideInfo.Core
{
	public class DeepZoomGenerator
	{
		public AbstractSlide Osr { get; }
		public int TileSize { get; set; }
		public int DeepZoomOverlap { get; set; }
		public bool LimitBounds { get; set; }

		public int LevelCount => GetLevelCount();
		public SizeL[] TileDimensions => GetTileDimensions();
		public IList<SizeL> DeepZoomLevelDimensions => GetDeepZoomLevels();
		public int TilesCount => GetTilesCount();
		public Color BackgroundColor => GetBackgroundColor();

		public SizeL Level0Offset => LimitBounds ? GetLimitedLevel0Offset() : GetLevel0Offset();

		internal IList<SizeL> LevelDimensions
		{
			get
			{
				if (!LimitBounds) return GetLevelDimensions();
				try
				{
					return GetLimitedLevelDimensions();
				}
				catch
				{
					return GetLevelDimensions();
				}
			}
		}
		internal SizeL Level0Dimensions => LevelDimensions[0];
		public int[] Level0DeepZoomDownsamples => GetLevel0DeepZoomDownsamples();
		public int[] BestSlideLevelForDeepZoomLevel => GetBestSlideLevelsForDeepZoomLevels();
		public double[] DeepZoomLevelDownsamples => GetDeepZoomDownsamples();
		public string DeepZoomImageTemplate { get; } =
			"<?xml version=\"1.0\" encoding=\"UTF-8\"?><Image xmlns=\"http://schemas.microsoft.com/deepzoom/2008\" Format=\"@Format\"  Overlap=\"@Overlap\"  TileSize=\"@TileSize\">" +
			"<Size Height=\"@Height\" Width=\"@Width\"/></Image>";


		/*Creates a DeepZoomGenerator wrapping a Slide object.
        osr:         a slide object.
        tileSize:    the width and height of a single tile. 
					 For best viewer performance, tile_size+2*overlap should be a power of two.
        overlap:     the number of extra pixels to add to each interior edge of a tile.
        limitBounds: True to render only the non-empty slide region.*/
		public DeepZoomGenerator(AbstractSlide osr, int tileSize = 254, int overlap = 1, bool limitBounds = true)
		{
			Osr = osr;
			TileSize = tileSize;
			DeepZoomOverlap = overlap;
			LimitBounds = limitBounds;
		}

		private IList<SizeL> GetLimitedLevelDimensions()
		{
			//Slide level dimensions scale factor in each axis
			float.TryParse(Osr.Properties[OpenSlide.PROPERTY_NAME_BOUNDS_WIDTH], out float scaledWidth);
			float.TryParse(Osr.Properties[OpenSlide.PROPERTY_NAME_BOUNDS_HEIGHT], out float scaledHeight);

			var level0Limits = Osr.LevelDimensions[0];

			scaledWidth = scaledWidth / level0Limits.Width;
			scaledHeight = scaledHeight / level0Limits.Height;

			var sizeScale = new SizeF(scaledWidth, scaledHeight);

			// Dimensions of active area
			var levelDimensions = new List<SizeL>();
			foreach (var levelSize in Osr.LevelDimensions)
			{
				var newScaledWidth = (long)Math.Ceiling((double)levelSize.Width * sizeScale.Width);
				var newScaledHeight = (long)Math.Ceiling((double)levelSize.Height * sizeScale.Height);
				var scaledLevelSize = new SizeL(newScaledWidth, newScaledHeight);
				levelDimensions.Add(scaledLevelSize);
			}
			return levelDimensions;
		}

		private SizeL GetLimitedLevel0Offset()
		{
			try
			{
				long.TryParse(Osr.Properties[OpenSlide.PROPERTY_NAME_BOUNDS_X], out long width);

				long.TryParse(Osr.Properties[OpenSlide.PROPERTY_NAME_BOUNDS_Y], out long height);
				return new SizeL(width, height);
			}
			catch
			{
				return GetLevel0Offset();
			}
		}

		private SizeL GetLevel0Offset()
		{
			return new SizeL(0, 0);
		}

		private IList<SizeL> GetLevelDimensions()
		{
			return Osr.LevelDimensions.ToList();
		}

		private IList<SizeL> GetDeepZoomLevels()
		{
			var deepZoomSize = Level0Dimensions;
			var dzLevelDimensions = new List<SizeL> { deepZoomSize };

			while (deepZoomSize.Width > 1 || deepZoomSize.Height > 1)
			{
				var width = (long)Math.Max(1, Math.Ceiling(deepZoomSize.Width / 2.0));
				var height = (long)Math.Max(1, Math.Ceiling(deepZoomSize.Height / 2.0));

				deepZoomSize = new SizeL(width, height);
				dzLevelDimensions.Add(deepZoomSize);
			}
			dzLevelDimensions.Reverse();
			return dzLevelDimensions;
		}

		public SizeL[] GetTileDimensions()
		{
			return DeepZoomLevelDimensions
				.Select(x => new SizeL((long)Math.Ceiling(x.Width / (double)TileSize),
						(long)Math.Ceiling(x.Height / (double)TileSize)))
				.ToArray();
		}

		private int GetLevelCount()
		{
			return DeepZoomLevelDimensions.Count;
		}

		private int[] GetLevel0DeepZoomDownsamples()
		{
			return Enumerable.Range(0, LevelCount).Select(x => (int)Math.Pow(2, LevelCount - x - 1))
					.ToArray();
		}

		private int[] GetBestSlideLevelsForDeepZoomLevels()
		{
			return Level0DeepZoomDownsamples.Select(d => Osr.GetBestLevelForDownsample(d)).ToArray();
		}

		private double[] GetDeepZoomDownsamples()
		{
			return Enumerable.Range(0, LevelCount)
				.Select(level => Level0DeepZoomDownsamples[level] /
						 Osr.LevelDownsamples[BestSlideLevelForDeepZoomLevel[level]])
				.ToArray();
		}

		private Color GetBackgroundColor()
		{
			try
			{
				var bgColorString = Osr.Properties[OpenSlide.PROPERTY_NAME_BACKGROUND_COLOR];

				return ColorTranslator.FromHtml("#" + bgColorString);
			}
			catch
			{
				return Color.White;
			}
		}

		private int GetTilesCount()
		{
			var sum = TileDimensions.Sum(tileDim => tileDim.Width * tileDim.Height);
			return (int)sum;
		}

		public Image GetTile(int level, SizeL address)
		{
			// Read tile
			CheckParameters(level, address);
			var col = address.Width;
			var row = address.Height;

			// Get preferred slide level
			var preferedSlideLevel = BestSlideLevelForDeepZoomLevel[level];

			// Calculate top left and bottom right overlap
			var topLeftOverlap = new SizeL(col == 0 ? 0 : DeepZoomOverlap,
				row == 0 ? 0 : DeepZoomOverlap);

			var bottomRightOverlap = new SizeL(col == TileDimensions[level].Width - 1 ? 0 : DeepZoomOverlap,
				row == TileDimensions[level].Height - 1 ? 0 : DeepZoomOverlap);

			// Get final size of the tile
			var finalTileWidth = Math.Min(TileSize, DeepZoomLevelDimensions[level].Width - TileSize * col) +
								 topLeftOverlap.Width + bottomRightOverlap.Width;
			var finalTileHeight = Math.Min(TileSize, DeepZoomLevelDimensions[level].Height - TileSize * row) +
								  topLeftOverlap.Height + bottomRightOverlap.Height;
			var finalTileSize = new SizeL(finalTileWidth, finalTileHeight);

			if (finalTileSize.Width < 0 || finalTileSize.Height < 0)
				throw new ArgumentException($"out of bounds level {level}, row {row}, col {col}");

			// Obtain the region coordinates
			var deepZoomLocation = new SizeL(TileSize * col, TileSize * row);

			var levelLocation = new SizeF(
				(float)DeepZoomLevelDownsamples[level] * (deepZoomLocation.Width - topLeftOverlap.Width),
				(float)DeepZoomLevelDownsamples[level] * (deepZoomLocation.Height - topLeftOverlap.Height));

			// Round location down and size up, and add offset of active area
			var level0Location = new SizeL(
				(long)(Osr.LevelDownsamples[preferedSlideLevel] * levelLocation.Width + Level0Offset.Width),
				(long)(Osr.LevelDownsamples[preferedSlideLevel] * levelLocation.Height + Level0Offset.Height));

			var regionWidth = (long)Math.Min(Math.Ceiling(DeepZoomLevelDownsamples[level] * finalTileSize.Width),
				LevelDimensions[preferedSlideLevel].Width - Math.Ceiling(levelLocation.Width));
			var regionHeight = (long)Math.Min(Math.Ceiling(DeepZoomLevelDownsamples[level] * finalTileSize.Height),
				LevelDimensions[preferedSlideLevel].Height - Math.Ceiling(levelLocation.Height));
			var regionSize = new SizeL(regionWidth, regionHeight);

			var tileBmp = Osr.ReadRegion(level0Location, preferedSlideLevel, regionSize);

            //Apply on background color (composite)
            tileBmp = ApplyOnBackgroundColor(tileBmp);

			// Scale to the correct size
			var deepZoomSize = finalTileSize;
			if (regionSize.Width == deepZoomSize.Width &&
				regionSize.Height == deepZoomSize.Height)
				return tileBmp;

		    tileBmp = new Bitmap(tileBmp, deepZoomSize.ToSize());
            return tileBmp;
		}

		private void CheckParameters(int level, SizeL adress)
		{
			var col = adress.Width;
			var row = adress.Height;

			if (level < 0 || level >= LevelCount)
				throw new ArgumentException($"wrong level level {level}, row {row}, col {col}");

			if (TileDimensions[level].Width <= col || TileDimensions[level].Height <= row ||
				0 > col || 0 > row)
				throw new ArgumentException($"wrong address level {level}, row {row}, col {col}");
		}

		private Image ApplyOnBackgroundColor(Image bmp)
		{
			using (var src = new Bitmap(bmp))
			{
				bmp.Dispose();
				bmp = new Bitmap(src.Width, src.Height);
				using (var g1 = Graphics.FromImage(bmp))
				{
					g1.Clear(BackgroundColor);
					g1.DrawImage(src, 0, 0);
				}
			}
			return bmp;
		}

		/*Returns a stream containing the XML metadata for the .dzi file.
		format:    the format of the individual tiles ('png' or 'jpeg')*/
		public MemoryStream GetDziMetadata(string imageFormat = "jpeg")
		{
			var updatedMetadataString = GetDziMetadataString(imageFormat);
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(updatedMetadataString);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		/*Returns a string containing the XML metadata for the .dzi file.
		format:    the format of the individual tiles ('png' or 'jpeg')*/
		public string GetDziMetadataString(string imageFormat = "jpeg")
		{
			if (!imageFormat.Equals("jpeg", StringComparison.OrdinalIgnoreCase) && !imageFormat.Equals("png", StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException($"Invalid image format {imageFormat}");

			var width = Level0Dimensions.Width;
			var height = Level0Dimensions.Height;

			var updatedMetadataTemplate = DeepZoomImageTemplate.Replace("@Width", width.ToString())
				.Replace("@Height", height.ToString());
			updatedMetadataTemplate = updatedMetadataTemplate.Replace("@TileSize", TileSize.ToString())
				.Replace("@Overlap", DeepZoomOverlap.ToString());
			updatedMetadataTemplate = updatedMetadataTemplate.Replace("@Format", imageFormat.ToLower());
			return updatedMetadataTemplate;
		}

		public override string ToString()
		{
			return $"DeepZoomGenerator({Osr}, tile_size = {TileSize}, overlap = {DeepZoomOverlap}, limit_bounds = {LimitBounds})";
		}
	}
}