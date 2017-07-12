using System;
using SlideInfo.Core;

namespace SlideInfo.Test
{
	class TestOpenSlideTiler
	{
		static OpenSlideTiler ost;
		private static string testMIRAX = "Mirax2.2-4-PNG.mrxs";

		static void Main()
		{
			SetUp();
			ost.Run();

			Console.WriteLine("\nPress any key to quit...");
			Console.ReadKey();
		}

		static void SetUp()
		{
			ost = new OpenSlideTiler(testMIRAX, "out", "jpeg");
		}

	}

}
