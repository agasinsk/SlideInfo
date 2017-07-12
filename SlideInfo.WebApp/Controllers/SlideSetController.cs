using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SlideInfo.Core;
using SlideInfo.WebApp.Models;
using Controller = Microsoft.AspNetCore.Mvc.Controller;

namespace SlideInfo.WebApp.Controllers
{
	public class SlideSetController : Controller
	{
		private const string NoAccessAlertText = "No access to data.";
		private readonly SlideSet slideSet;
		
		public SlideSetController()
		{
			var openSlideSupportedExtensions = new[]
			{
				"svs", "tif", "vms", "vmu", "ndpi", "scn", "mrxs",
				"tiff", "svslide", "bif"
			};
			const string mainPath = @"C:\Users\artur\Documents\Visual Studio 2017\Projects\OpenSlideExample\data";

			var dirs = FilterFiles(mainPath, openSlideSupportedExtensions);
			slideSet = new SlideSet(dirs);
		}

		public IEnumerable<string> FilterFiles(string path, params string[] exts)
		{
			return
				Directory
					.EnumerateFiles(path, "*.*")
					.Where(file => exts.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
		}

		public IActionResult Index()
		{
			HttpContext.Session.Remove("Slide");
			return View(slideSet);
		}

		[Microsoft.AspNetCore.Mvc.Route("[controller]/{slug}")]
		public IActionResult ViewSlide(string slug)
		{
			try
			{
				var currentSlide = slideSet.Get(slug);
				HttpContext.Session.SetString("Slide", slug);
				return View(currentSlide);
			}
			catch (Exception)
			{
				HttpContext.Session.SetString("AlertText", NoAccessAlertText);
				return RedirectToAction("Index");
			}
		}

		// GET: slug.dzi
		[Produces("application/xml")]
		[Microsoft.AspNetCore.Mvc.Route("[controller]/{slug}.dzi")]
		[OutputCache(Duration = 120, Location = OutputCacheLocation.Server)]
		public string Dzi(string slug)
		{
			try
			{
				return slideSet.Get(slug).DeepZoomGenerator.GetDziMetadataString(DefaultOptions.DEEPZOOM_FORMAT);
			}
			catch (Exception)
			{
				HttpContext.Session.SetString("AlertText", NoAccessAlertText);
				RedirectToAction("Index");
				return "";
			}
		}

		[Produces("image/jpeg")]
		[Microsoft.AspNetCore.Mvc.Route("[controller]/{slug}_files/{level:int}/{col:int}_{row:int}.jpeg")]
		[OutputCache(Duration = 120, Location = OutputCacheLocation.Server)]
		public IActionResult Tile(string slug, int level, int col, int row)
		{
			try
			{
				var slide = slideSet.Get(slug);
				var tile = slide.DeepZoomGenerator.GetTile(level, new SizeL(col, row));

				using (var stream = new MemoryStream())
				{
					tile.Save(stream, ImageFormat.Jpeg);
					tile.Dispose();
					return File(stream.ToArray(), "image/jpeg");
				}
			}
			catch (OpenSlideException)
			{
				throw new HttpException(404, "Wrong level or coordinates");
			}
		}

		[Microsoft.AspNetCore.Mvc.Route("[controller]/{slug}/Properties")]
		public IActionResult Properties(string slug)
		{
			try
			{
				return View(slideSet.Get(slug));
			}
			catch (Exception)
			{
				HttpContext.Session.SetString("AlertText", NoAccessAlertText);
				return RedirectToAction("Index");
			}
		}

		[Microsoft.AspNetCore.Mvc.Route("[controller]/{slug}/AssociatedImages")]
		public IActionResult AssociatedImages(string slug)
		{
			try
			{
				return View(slideSet.Get(slug));
			}
			catch (Exception)
			{
				HttpContext.Session.SetString("AlertText", NoAccessAlertText);
				return RedirectToAction("Index");
			}
		}

		[Microsoft.AspNetCore.Mvc.Route("[controller]/{slug}/AssociatedImages/{imageName}")]
		[Produces("image/jpeg")]
		public IActionResult GetAssociatedImage(string slug, string imageName)
		{
			try
			{
				var slide = slideSet.Get(slug);
				var associated = slide.AssociatedImages[imageName];
				var associatedBitmap = associated.Image;
				using (var stream = new MemoryStream())
				{
					associatedBitmap.Save(stream, ImageFormat.Jpeg);
					associatedBitmap.Dispose();
					return File(stream.ToArray(), "image/jpeg");
				}
			}
			catch (Exception)
			{
				HttpContext.Session.SetString("AlertText", NoAccessAlertText);
				return RedirectToAction("Index");
			}
		}
	}
}