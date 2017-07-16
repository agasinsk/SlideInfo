using System;
using System.Drawing;
using SlideInfo.Helpers;

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

	    public Image GetThumbnail(SizeL size)
	    {
	        return GetThumbnail(size.ToSize());
	    }

        public Image GetThumbnail(Size size)
	    {
	        var thumbSize = Image.GetProportionateResize(size);
            var callback = new Image.GetThumbnailImageAbort(AbstractSlide.ThumbnailCallback);
	        var thumbnail = Image.GetThumbnailImage(thumbSize.Width, thumbSize.Height, callback, new IntPtr());
	        return thumbnail;
	    }

	    public ImageSlide ToImageSlide()
	    {
	        return new ImageSlide(Image);
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