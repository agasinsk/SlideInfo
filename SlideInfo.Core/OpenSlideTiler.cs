using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Encoder = System.Drawing.Imaging.Encoder;

namespace SlideInfo.Core
{
	public class OpenSlideTiler
	{
		private readonly AssociatedImagesWriter associatedImagesGenerator;
		private readonly DeepZoomWriter deepZoomGenerator;
		private readonly string fileName;
		private readonly string fileNameWithoutExtension;

		private readonly string format;
		private readonly bool limitBounds;
		private readonly string outDirectory;
		private readonly int overlap;
		private readonly int quality;

		private readonly OpenSlide slide;

		private readonly int tileSize;

		public OpenSlideTiler(string fileName, string outDirectory = "out", string format = "jpeg", int quality = 75,
			int tileSize = 254, int overlap = 1, bool limitBounds = true)
		{
			this.fileName = fileName;
			fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
			slide = new OpenSlide(fileName);
			
			this.outDirectory = outDirectory;

			this.format = format;
			this.quality = quality;

			this.tileSize = tileSize;
			this.overlap = overlap;
			this.limitBounds = limitBounds;

			deepZoomGenerator = new DeepZoomWriter(this);
			associatedImagesGenerator = new AssociatedImagesWriter(this);
		}

		public async void Run()
		{
			await GenerateAllAssociatedImages();
			await GenerateDeepZoomTiles();
		}

		public async Task GenerateDeepZoomTiles()
		{
			await Task.Run(() => deepZoomGenerator.Run());
		}

		public async Task GenerateAllAssociatedImages()
		{
			await Task.Run(() => associatedImagesGenerator.Run());
		}

		private EncoderParameters GetQualityEncoderParameter()
		{
			var myEncoder = Encoder.Quality;
			var myEncoderParameters =
				new EncoderParameters(1) {Param = {[0] = new EncoderParameter(myEncoder, quality)}};
			return myEncoderParameters;
		}

		private static ImageCodecInfo GetEncoder(ImageFormat format)
		{
			var codecs = ImageCodecInfo.GetImageDecoders();
			foreach (var codec in codecs)
				if (codec.FormatID == format.Guid)
					return codec;
			return null;
		}

		internal class DeepZoomWriter
		{
			private readonly DeepZoomGenerator dz;
			private readonly OpenSlideTiler ost;
			private readonly StringBuilder pathBuilder;
			private string outDirectory;
			private int processedTilesCount;

			public DeepZoomWriter(OpenSlideTiler o)
			{
				ost = o;
				if (ost != null)
				{
					outDirectory = ost.outDirectory;
					dz = new DeepZoomGenerator(ost.slide, ost.tileSize, ost.overlap, ost.limitBounds);
				}
				pathBuilder = new StringBuilder();
			}

			public void Run()
			{
				CreateDirectoryForDeepZoom();
				GenerateDziMetadataFile();
				GenerateDeepZoomTiles();
			}

			private void CreateDirectoryForDeepZoom()
			{
				pathBuilder.Clear();
				pathBuilder.Append(outDirectory).Append("/").Append(ost.fileNameWithoutExtension).Append("_files");
				pathBuilder.Append("/").Append("deep_zoom");

				outDirectory = pathBuilder.ToString();
				if (!Directory.Exists(outDirectory))
					Directory.CreateDirectory(outDirectory);
			}

			private void GenerateDziMetadataFile()
			{
				var dziMetadata = GetDzi();
				var dziMetadataPath = CreateDirectoryForDziMetadata();
				var dziMetadataFile = new FileStream(dziMetadataPath, FileMode.Create, FileAccess.Write);
				dziMetadata.WriteTo(dziMetadataFile);
				dziMetadataFile.Close();
				dziMetadata.Close();
			}

			private MemoryStream GetDzi()
			{
				return dz.GetDziMetadata(ost.format);
			}

			private string CreateDirectoryForDziMetadata()
			{
				pathBuilder.Clear();

				pathBuilder.Append(outDirectory).Append("/").Append(Path.GetFileNameWithoutExtension(ost.fileName))
					.Append(".dzi");

				return pathBuilder.ToString();
			}

			private void GenerateDeepZoomTiles()
			{
				var maxDeepZoomLevel = dz.LevelCount;

				for (var level = 0; level < maxDeepZoomLevel; level++)
					GenerateDziTilesAtLevel(level);
			}

			private void GenerateDziTilesAtLevel(int level)
			{
				var levelPath = CreateDirectoryForLevel(level);

				if (!Directory.Exists(levelPath))
					Directory.CreateDirectory(levelPath);

				var maxWidth = dz.TileDimensions[level].Width;
				var maxHeight = dz.TileDimensions[level].Height;

				for (var row = 0; row < maxHeight; row++)
				for (var col = 0; col < maxWidth; col++)
				{
					SaveTile(levelPath, level, row, col);
					PrintTilesDone();
				}
			}

			private string CreateDirectoryForLevel(int level)
			{
				pathBuilder.Clear();
				pathBuilder.Append(outDirectory).Append("/").Append(level.ToString());

				return pathBuilder.ToString();
			}

			private void SaveTile(string levelPath, int level, int col, int row)
			{
				var tilePath = CreateDirectoryForTile(levelPath, row, col);
				using (var tileBmp = GetTile(level, row, col))
				{
					var qualityParameter = ost.GetQualityEncoderParameter();

					var formatEncoder = GetEncoder(ost.format == "png" ? ImageFormat.Png : ImageFormat.Jpeg);

					tileBmp.Save(tilePath, formatEncoder, qualityParameter);
				}
			}

			private string CreateDirectoryForTile(string levelPath, int col, int row)
			{
				pathBuilder.Clear();
				pathBuilder.Append(levelPath).Append("/").Append(col.ToString()).Append("_").Append(row.ToString()).Append(".")
					.Append(ost.format);

				return pathBuilder.ToString();
			}

			private Image GetTile(int level, int col, int row)
			{
				return dz.GetTile(level, new SizeL(col, row));
			}

			private void PrintTilesDone()
			{
				processedTilesCount++;
				var count = processedTilesCount;
				var total = dz.TilesCount;

				Console.Write("Tiling " + ost.fileName + ": wrote " + count + "/" + total + " tiles\r");
			}
		}

		internal class AssociatedImagesWriter
		{
			private readonly OpenSlideTiler ost;
			private readonly StringBuilder pathBuilder;
			private string outDirectory;
			private int processedTilesCount;

			public AssociatedImagesWriter(OpenSlideTiler ost)
			{
				this.ost = ost;
				pathBuilder = new StringBuilder();
				outDirectory = ost.outDirectory;
				processedTilesCount = 0;
			}

			public void Run()
			{
				WriteAllAssociatedImages();
			}

			public void WriteAllAssociatedImages()
			{
				if (ost.slide.AssociatedImages.Count <= 0) return;
				CreateDirectoryForAssociatedImages();

				foreach (var imageName in ost.slide.AssociatedImages.Keys)
				{
					WriteToFile(imageName);
					PrintImagesDone();
				}
			}

			private void CreateDirectoryForAssociatedImages()
			{
				pathBuilder.Clear();
				pathBuilder.Append(outDirectory).Append("/").Append(ost.fileNameWithoutExtension).Append("_files");
				pathBuilder.Append("/").Append("associated");

				outDirectory = pathBuilder.ToString();
				if (!Directory.Exists(outDirectory))
					Directory.CreateDirectory(outDirectory);
			}

			public void WriteToFile(string name)
			{
				var imagePath = CreateDirectoryForAssociatedImage(name);

				try
				{
					using (var associatedBmp = ost.slide.ReadAssociatedImage(name))
					{
						var qualityParameter = ost.GetQualityEncoderParameter();

						ImageCodecInfo formatEncoder;
						switch (ost.format)
						{
							case "png":
								formatEncoder = GetEncoder(ImageFormat.Png);
								break;
							case "jpeg":
								formatEncoder = GetEncoder(ImageFormat.Jpeg);
								break;
							default:
								formatEncoder = GetEncoder(ImageFormat.Bmp);
								break;
						}
						associatedBmp.Save(imagePath, formatEncoder, qualityParameter);

					}
				}
				catch (Exception)
				{
					;
				}
			}

			private string CreateDirectoryForAssociatedImage(string imageName)
			{
				pathBuilder.Clear();
				pathBuilder.Append(outDirectory).Append("/").Append(imageName).Append(".").Append(ost.format);
				return pathBuilder.ToString();
			}

			private void PrintImagesDone()
			{
				processedTilesCount++;
				var count = processedTilesCount;
				var total = ost.slide.AssociatedImages.Count;

				Console.Write("\rSaving " + ost.fileName + ": wrote " + count + "/" + total + " associated images");
			}
		}
	}
}