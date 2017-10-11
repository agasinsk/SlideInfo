using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SlideInfo.App.Data;
using SlideInfo.App.Models;

namespace SlideInfo.App.Controllers
{
    public class MessengerController : Controller
    {
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserManager<AppUser> userManager;
        private readonly SlideInfoDbContext context;

        public MessengerController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, SlideInfoDbContext context)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
        }

        public IActionResult Messenger()
        {
            var viewModel = new MessengerViewModel { Users = context.AppUsers };

            return View(viewModel);
        }
    }
}