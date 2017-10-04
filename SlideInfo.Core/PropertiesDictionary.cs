using System.Collections.Generic;

namespace SlideInfo.Core
{
	public class PropertiesDictionary : SlideDictionary<string>
	{
		public override ICollection<string> Keys
		{
			get
			{
				var osr = Osr as OpenSlide;
				return osr.GetPropertyNames() as ICollection<string>;
			}
		}

		public override string this[string key]
		{
			get
			{
				if (InternalDict.ContainsKey(key))
					return InternalDict[key];
                if (Osr is OpenSlide osr)
                {
                    var value = osr.GetPropertyValue(key);
                    InternalDict[key] = value ?? throw new OpenSlideException($"Property {key} was not found.");
                }
                else
                {
                    return null;
                }

                return InternalDict[key];
			}
			set => InternalDict[key] = value;
		}

		public PropertiesDictionary(AbstractSlide osr) : base(osr)
		{
		}
	}
}
