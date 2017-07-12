using System;
using OpenSlide.Cs;

namespace OpenSlide.Cs.Test
{
	class TestDeepZoomTiler
	{
		static DeepZoomTiler dzTiler;
		private static string testSVS = "JP2K-33003-1.svs";
		private static string testMIRAX = "Mirax2.2-4-PNG.mrxs";
		private static string testSCN = "Leica-1.scn";

		public static void Main()
		{
			SetUp();

			Console.WriteLine("\nPress any key to quit...");
			Console.ReadKey();
		}

		static void SetUp()
		{
			dzTiler = new DeepZoomTiler(testSCN, "out");
			dzTiler.Run();
		}


	}
}
