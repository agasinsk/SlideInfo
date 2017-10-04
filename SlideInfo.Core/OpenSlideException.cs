using System;

namespace SlideInfo.Core
{
	[Serializable]
	public class OpenSlideException : Exception
	{
		private const string Msg = "An exception produced by the OpenSlide library";

		public OpenSlideException() : base(Msg)
		{
		}

		public OpenSlideException(string msg) : base(Msg + ": " + msg)
		{
		}
	}
}