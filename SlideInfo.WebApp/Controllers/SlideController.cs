using System;
using System.IO;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SlideInfo.Core;
using SlideInfo.WebApp.Models;

namespace SlideInfo.WebApp.Controllers
{
    public class SlideController : Controller
    {
	    private readonly Slide viewModel;

	    public SlideController(string pathToSlide)
	    {
		    viewModel = new Slide(pathToSlide);
	    }
		
        public IActionResult ViewSlide()
        {
            return View(viewModel);
        }

	    // GET: slug.dzi
	    [Produces("application/xml")]
	    [Route("[controller]/{slug}.dzi")]
	    public string Dzi(string slug)
	    {
		    try
		    {
			    return viewModel.DeepZoomGenerator.GetDziMetadataString(DefaultOptions.DEEPZOOM_FORMAT);
		    }
		    catch (Exception)
		    {
			    HttpContext.Session.SetString("AlertText", "Nie znaleziono pliku.");
			    RedirectToAction("Index","SlideSet");
			    return "";
		    }
	    }

	    [Route("[controller]/{slug}_files/{level:int}/{col:int}_{row:int}.jpeg")]
	    public IActionResult Tile(string slug, int level, int col, int row)
	    {
		    try
		    {
			    var slide = viewModel;
			    var tile = slide.DeepZoomGenerator.GetTile(level, new SizeL(col, row));
			    var formatEncoder = slide.FormatEncoder;
			    var qualityParameter = slide.QualityParameter;

			    using (var stream = new MemoryStream())
			    {
				    tile.Save(stream, formatEncoder, qualityParameter);
				    tile.Dispose();
				    return File(stream.ToArray(), "image/jpeg");
			    }
		    }
		    catch (OpenSlideException)
		    {
			    throw new HttpException(404, "Wrong level or coordinates");
		    }
	    }

	}
}