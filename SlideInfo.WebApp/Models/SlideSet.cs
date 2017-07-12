using System.Collections.Generic;
using System.IO;

namespace SlideInfo.WebApp.Models
{
	public class SlideSet
	{
		private readonly object Lock;
		public Dictionary<string, string> FileNames { get; set; }
		public Dictionary<string, string> FilePaths { get; set; }
		public string CurrentDirectory { get; set; }
		public IDictionary<string, Slide> Slides { get; set; }

		public SlideSet()
		{
		}

		public SlideSet(IEnumerable<string> dirs)
		{
			Lock = new object();
			var fileNames = new Dictionary<string, string>();
			var filePaths = new Dictionary<string, string>();

			foreach (var filePath in dirs)
			{
				var fileName = Path.GetFileName(filePath);
				var fileUrl = UrlFormatter.UrlFor(Path.GetFileName(filePath));
				fileNames[fileUrl] = fileName;
				filePaths[fileUrl] = filePath;
			}

			FileNames = fileNames;
			FilePaths = filePaths;
			Slides = new Dictionary<string, Slide>();
		}

		public Slide Get(string slideUrl)
		{
			var contains = Slides.ContainsKey(slideUrl);
			if (contains)
				return Slides[slideUrl];

			var pathToSlide = FilePaths[slideUrl];

			Slides[slideUrl] = new Slide(pathToSlide);

			return Slides[slideUrl];
		}

		public void Add(string slideUrl, Slide slide)
		{
			if (!Slides.ContainsKey(slideUrl))
			{
				Slides[slideUrl] = slide;
			}
		}
	}
}
