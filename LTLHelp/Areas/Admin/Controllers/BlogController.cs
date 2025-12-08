using LTLHelp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LTLHelp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogController : Controller
    {
        private readonly LtlhelpContext _context;

        public BlogController(LtlhelpContext context)
        {
            _context = context;
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _context.BlogCategories
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "BlogCategoryId", "Name");
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _context.BlogPosts
                .Include(p => p.BlogCategory)
                .OrderByDescending(p => p.PublishedAt)
                .ToListAsync();

            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();
            return View(new BlogPost());
        }

        [HttpPost]
        public async Task<IActionResult> Create(BlogPost model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(model);
            }

            model.PublishedAt = DateTime.Now;
            model.PublishedAt = model.IsPublished == true ? DateTime.Now : null;

            _context.BlogPosts.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();

            await LoadCategoriesAsync();
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BlogPost model)
        {
            var post = await _context.BlogPosts.FindAsync(model.BlogPostId);
            if (post == null) return NotFound();

            post.Title = model.Title;
            post.Content = model.Content;
            post.BlogCategoryId = model.BlogCategoryId;
            post.IsPublished = model.IsPublished;
            post.PublishedAt = model.IsPublished == true ? DateTime.Now : null;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();

            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
