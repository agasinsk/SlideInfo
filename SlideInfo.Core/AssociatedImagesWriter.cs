using System;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using OpenSlide.Cs.Helpers;

namespace OpenSlide.Cs
{
	public class AssociatedImagesWriter
	{
		private OpenSlide osr;
		private string fileName;
		private string fileNameWithoutExtension;

		private string format;
		private int quality;
		private string outDirectory;

		StringBuilder pathBuilder;

		public AssociatedImagesWriter(string fileName, string outDirectory = "out", string format = "bmp", int quality = 75)
		{
			this.fileName = fileName;
			fileNameWithoutExtension = FileName.GetFileNameWithoutExtension(fileName);
			this.outDirectory = outDirectory;

			this.format = format;
			this.quality = quality;
			osr = new OpenSlide(fileName);
			pathBuilder = new StringBuilder();
		}

		public void Run()
		{
			WriteAllAssociatedImages();
		}

		public void WriteAllAssociatedImages()
		{
			if (osr.AssociatedImages.Count > 0)
			{
				CreateDirectoryForAssociatedImages();

				foreach (var imageName in osr.AssociatedImages.Keys)
				{
					WriteToFile(imageName);
				}
			}
		}

		private string CreateDirectoryForAssociatedImages()
		{
			pathBuilder.Clear();
			pathBuilder.Append(outDirectory).Append("/").Append(fileNameWithoutExtension).Append("_files");
			pathBuilder.Append("/").Append("associated");

			outDirectory = pathBuilder.ToString();
			if (!Directory.Exists(outDirectory))
				Directory.CreateDirectory(outDirectory);

			return outDirectory;
		}

		public void WriteToFile(string name)
		{
			string imagePath = CreateDirectoryForAssociatedImage(name);

			try
			{
				using (var associatedBmp = osr.GetAssociatedImage(name))
				{
					EncoderParameters qualityParameter = GetQualityEncoderParameter();

					ImageCodecInfo formatEncoder;
					switch (format)
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

					DebugTrace.Leave("File " + name + " succesfully saved.");
				}
			}
			catch (Exception)
			{
				DebugTrace.Leave("There was a problem saving the file. \nCheck the file permissions.");
			}
		}

		private string CreateDirectoryForAssociatedImage(string imageName)
		{
			pathBuilder.Clear();
			pathBuilder.Append(outDirectory).Append("/").Append(imageName).Append(".").Append(format);
			DebugTrace.Leave(pathBuilder.ToString());
			return pathBuilder.ToString();
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
	}
}
