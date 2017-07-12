using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace SlideInfo.Core
{
	public class AssociatedImage
	{
		public string Name { get; }
		public Image Image { get; }
		public int SlideHashCode { get; }

		public AssociatedImage() { }

		public AssociatedImage(string name, int slideHashCode)
		{
			Name = name ?? throw new NullReferenceException("Arguments cannot be null");
			SlideHashCode = slideHashCode;
		}

		public AssociatedImage(string name, Image image)
		{
			Name = name ?? throw new NullReferenceException("Arguments cannot be null");
			if (image != null) Image = image;
		}

		public AssociatedImage(string name, Image image, int slideHashCode)
		{
			Name = name;
			Image = image;
			SlideHashCode = slideHashCode;
		}

		public void WriteToFile(string writePath)
		{
			var exactPathToWrite = CreatePathToWriteBitmap(writePath);

			Directory.CreateDirectory(writePath);
			Image?.Save(exactPathToWrite);
		}

		private string CreatePathToWriteBitmap(string writePath)
		{
			var exactWritePath = new StringBuilder();

			exactWritePath.Append(writePath).Append("/").Append(Name).Append(".bmp");
			return exactWritePath.ToString();
		}

		public override int GetHashCode()
		{
			return Image.GetHashCode() + Name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			var ai2 = (AssociatedImage)obj;
			return Image.Equals(ai2.Image) && Name.Equals(ai2.Name);
		}
	}
}