using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SlideInfo.App.Controllers
{
    public class AssociatedImageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}