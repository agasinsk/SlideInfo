using System;


namespace OpenSlide.Cs.Test
{
	class TestAssociatedImagesWriter
	{
		static AssociatedImagesWriter dzTiler;
		private static string testSVS = "JP2K-33003-1.svs";
		private static string testMIRAX = "Mirax2.2-4-PNG.mrxs";

		public static void Main()
		{
			SetUp();

			Console.WriteLine("\nPress any key to quit...");
			Console.ReadKey();
		}

		static void SetUp()
		{
			dzTiler = new AssociatedImagesWriter(testMIRAX, "out", "png", 100);
			dzTiler.Run();
		}

	}
}
