using System;

namespace SlideInfo.Helpers
{
	public static class StringExtension
	{
		public static bool Contains(this string source, string toCheck, StringComparison comp)
		{
			return source.IndexOf(toCheck, comp) >= 0;
		}
	}
}

