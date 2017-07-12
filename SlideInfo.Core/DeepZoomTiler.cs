using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using OpenSlide.Cs.Helpers;

namespace OpenSlide.Cs
{
	public class DeepZoomTiler
	{
		private DeepZoomGenerator dz;
		private int tileSize;
		private int overlap;
		private bool limitBounds;

		private OpenSlide osr;
		private string fileName;
		private string fileNameWithoutExtension;

		private string format;
		private int quality;
		private string outDirectory;

		private StringBuilder pathBuilder;
		private int processedTilesCount;

		public DeepZoomTiler(string fileName, string outDirectory = "out", string format = "jpeg", int quality = 75, int tileSize = 254, int overlap = 1, bool limitBounds = true)
		{
			this.fileName = fileName;
			fileNameWithoutExtension = FileName.GetFileNameWithoutExtension(fileName);
			this.outDirectory = outDirectory;

			this.format = format;
			this.quality = quality;

			this.tileSize = tileSize;
			this.overlap = overlap;
			this.limitBounds = limitBounds;

			osr = new OpenSlide(fileName);
			dz = new DeepZoomGenerator(osr, tileSize, overlap, limitBounds);
			pathBuilder = new StringBuilder();
			processedTilesCount = 0;
		}

		public void Run()
		{
			CreateDirectoryForDeepZoom();
			GenerateDZIMetadataFile();
			GenerateDeepZoomTiles();
		}

		private string CreateDirectoryForDeepZoom()
		{
			pathBuilder.Clear();
			pathBuilder.Append(outDirectory).Append("/").Append(fileNameWithoutExtension).Append("_files");
			pathBuilder.Append("/").Append("deep_zoom");

			outDirectory = pathBuilder.ToString();
			if (!Directory.Exists(outDirectory))
				Directory.CreateDirectory(outDirectory);

			return outDirectory;
		}

		private void GenerateDZIMetadataFile()
		{
			var dziMetadata = GetDZI();
			var dziMetadataPath = CreateDirectoryForDZIMetadata();
			FileStream dziMetadataFile = new FileStream(dziMetadataPath, FileMode.Create, FileAccess.Write);
			dziMetadata.WriteTo(dziMetadataFile);
			dziMetadataFile.Close();
			dziMetadata.Close();
		}

		MemoryStream GetDZI()
		{
			return dz.GetDZIMetadata(format);
		}

		private string CreateDirectoryForDZIMetadata()
		{
			this.pathBuilder.Clear();

			pathBuilder.Append(outDirectory).Append("/").Append(FileName.GetFileNameWithoutExtension(fileName)).Append(".dzi");

			return pathBuilder.ToString();
		}

		private void GenerateDeepZoomTiles()
		{
			int maxDeepZoomLevel = dz.MaxDeepZoomLevel;

			for (int level = 0; level < maxDeepZoomLevel; level++)
			{
				GenerateDZITilesAtLevel(level);
			}
		}

		private void GenerateDZITilesAtLevel(int level)
		{
			var levelPath = CreateDirectoryForLevel(level);

			if (!Directory.Exists(levelPath))
				Directory.CreateDirectory(levelPath);

			var maxWidth = dz.TileDimensions[level].Width;
			var maxHeight = dz.TileDimensions[level].Height;

			for (int row = 0; row < maxHeight; row++)
				for (int col = 0; col < maxWidth; col++)
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
			string tilePath = CreateDirectoryForTile(levelPath, row, col);
			using (var tileStream = GetTile(level, row, col))
			{

				EncoderParameters qualityParameter = GetQualityEncoderParameter();

				ImageCodecInfo formatEncoder;
				if (format == "png")
					formatEncoder = GetEncoder(ImageFormat.Png);
				else
					formatEncoder = GetEncoder(ImageFormat.Jpeg);

				tileStream.Save(tilePath, formatEncoder, qualityParameter);
			}
		}

		private string CreateDirectoryForTile(string levelPath, int col, int row)
		{
			pathBuilder.Clear();
			pathBuilder.Append(levelPath).Append("/").Append(col.ToString()).Append("_").Append(row.ToString()).Append(".").Append(format);

			return pathBuilder.ToString();
		}

		Bitmap GetTile(int level, int col, int row)
		{
			return dz.GetTile(level, new LevelSize(col, row));
		}

		private EncoderParameters GetQualityEncoderParameter()
		{
			System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
			EncoderParameters myEncoderParameters = new EncoderParameters(1);
			myEncoderParameters.Param[0] = new EncoderParameter(myEncoder, this.quality);
			return myEncoderParameters;
		}

		private ImageCodecInfo GetEncoder(ImageFormat format)
		{
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
			foreach (ImageCodecInfo codec in codecs)
			{
				if (codec.FormatID == format.Guid)
				{
					return codec;
				}
			}
			return null;
		}

		private void PrintTilesDone()
		{
			processedTilesCount++;
			int count = processedTilesCount;
			int total = dz.DeepZoomTilesCount;

			Console.Write("Tiling " + fileName + ": wrote " + count + "/" + total + " tiles\r");
		}

	}
}
