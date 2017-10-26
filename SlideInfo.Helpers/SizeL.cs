using System.Drawing;

namespace SlideInfo.Helpers
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

		public SizeL(Size size)
		{
			Width = size.Width;
			Height = size.Height;
		}

		public Size ToSize()
		{
			return new Size((int)Width, (int)Height);
		}

		public SizeF ToSizeF()
		{
			return new SizeF(Width, Height);
		}

	    public static SizeL operator +(SizeL s1, SizeL s2)
	    {
	        return new SizeL(s1.Width + s2.Width, s1.Height + s2.Height);
	    }

	    public static SizeL operator -(SizeL s1, SizeL s2)
	    {
	        return new SizeL(s1.Width - s2.Width, s1.Height - s2.Height);
	    }

        public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
				return false;

			var ls = (SizeL) obj;
			return Width.Equals(ls.Width) && Height.Equals(ls.Height);
		}

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
		{
			return "w " + Width + " h " + Height;
		}
	}
}