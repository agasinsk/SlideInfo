using System.Collections.Generic;

namespace SlideInfo.Core
{
	public class AssociatedImageDictionary : SlideDictionary<AssociatedImage>
	{
		public override ICollection<string> Keys
		{
			get
			{
				var osr = Osr as OpenSlide;
				return osr.GetAssociatedImageNames() as ICollection<string>;
			}
		}

		public override AssociatedImage this[string key]
		{
			get
			{
				var osr = Osr as OpenSlide;
				var value = osr.ReadAssociatedImage(key);
				if (value == null) throw new OpenSlideException($"Associated image '{key}' could not be found.");
				var associatedImage = new AssociatedImage(key, value, osr.QuickHash1);
				InternalDict[key] = associatedImage;

				return InternalDict[key];
			}
			set => InternalDict[key] = value;
		}

		public AssociatedImageDictionary(AbstractSlide osr) : base(osr)
		{
		}

	}
}
