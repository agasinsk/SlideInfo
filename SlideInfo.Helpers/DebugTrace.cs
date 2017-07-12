using System;

namespace OpenSlide.Cs.Helpers
{
	public class DebugTrace
	{
		public static Action<string> trace;//= (message) => Console.WriteLine(message);
		public static Action<string> innerTrace;// = (message) => Console.WriteLine("\t"+message);
		public static Action<string> testTrace;// = (message) => Console.WriteLine(message);
		public static void Leave(string message)
		{
			trace?.Invoke(message);
		}

		public static void Drop(string message)
		{
			innerTrace?.Invoke(message);
		}

		public static void Test(string message)
		{
			testTrace?.Invoke(message);
		}

	}
}
