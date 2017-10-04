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

	[Serializable]
	public class OpenSlideDisposedException : OpenSlideException
    {
		private const string Msg = "OpenSlide object has been disposed";

		public OpenSlideDisposedException() : base(Msg)
		{
		}
	}

	[Serializable]
	public class OpenSlideUnsupportedFormatException : OpenSlideException
	{
		private const string Msg = "OpenSlide does not support the requested file.";

		public OpenSlideUnsupportedFormatException() : base(Msg)
		{
		}

		public OpenSlideUnsupportedFormatException(string msg) : base(msg)
		{
		}
	}
}