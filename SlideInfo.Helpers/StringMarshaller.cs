using System.Collections.Generic;
using System.Text;

namespace SlideInfo.Helpers
{
	public static unsafe class StringMarshaller
	{
		public static string[] Marshal(byte** nativeStrings, int stringCount)
		{
			var strings = new string[stringCount];

			for (var x = 0; x < stringCount; ++x)
			{
				if (nativeStrings[x] == null) 
					continue;

				var length = GetStringLength(nativeStrings[x]);
				strings[x] = length == 0
						? string.Empty
						: Encoding.UTF8.GetString(nativeStrings[x], length);
			}
			
			return strings;
		}

		public static string[] Marshal(byte** nativeStrings)
		{
			int stringCount = 1, x;

			var strings = new List<string>();

			for (x = 0; x < stringCount; x++)
			{
				try
				{
					if (nativeStrings[x] == null)
						continue;
				}
				catch (System.AccessViolationException)
				{
					break;
				}

				var length = GetStringLength(nativeStrings[x]);
				strings.Add(length == 0
						? string.Empty
						: Encoding.UTF8.GetString(nativeStrings[x], length));
				stringCount++;
			}

			return strings.ToArray();
		}

		public static int GetStringLength(byte* nativeString)
		{
			var length = 0;

			while (*nativeString != '\0')
			{
				++length;
				++nativeString;
			}

			return length;
		}
	}
}
