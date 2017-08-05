using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlideInfo.App.Data;
using SlideInfo.App.Models;

namespace SlideInfo.App.Controllers
{
    public class CommentsController : Controller
    {
        private readonly SlideInfoDbContext context;
        private readonly UserManager<AppUser> userManager;

        public CommentsController(SlideInfoDbContext context, UserManager<AppUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            return View(await context.Comments.ToListAsync());
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await context.Comments
                .SingleOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Comments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Comment comment)
        { 
            if (!ModelState.IsValid)
                throw new ArgumentNullException();

            context.Add(comment);
            await context.SaveChangesAsync();
            return RedirectToAction("Index", "Slides");
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await context.Comments.SingleOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,SlideId,Text,Date")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(comment);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(comment);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await context.Comments
                .SingleOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await context.Comments.SingleOrDefaultAsync(m => m.Id == id);
            context.Comments.Remove(comment);
            await context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool CommentExists(int id)
        {
            return context.Comments.Any(e => e.Id == id);
        }
    }
}
