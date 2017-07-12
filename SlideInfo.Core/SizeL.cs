using System.Drawing;

namespace SlideInfo.Core
{
	public class SizeL
	{
		public long Width { get; set; }
		public long Height { get; set; }

		public SizeL(long width, long height)
		{
			Width = width;
			Height = height;
		}

		public SizeL(SizeL otherSize)
		{
			Width = otherSize.Width;
			Height = otherSize.Height;
		}

		public SizeL(Size otherSize)
		{
			Width = otherSize.Width;
			Height = otherSize.Height;
		}

		public Size ToSize()
		{
			return new Size((int)Width, (int)Height);
		}

		public SizeF ToSizeF()
		{
			return new SizeF(Width, Height);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			var ls = (SizeL) obj;
			return Width.Equals(ls.Width) && Height.Equals(ls.Height);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return "w:" + Width + " h:" + Height;
		}
	}
}