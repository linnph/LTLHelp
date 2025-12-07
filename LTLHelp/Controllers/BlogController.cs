using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTLHelp.Models;

namespace LTLHelp.Controllers;

public class BlogController : Controller
{
    private readonly LtlhelpContext _context;
    private readonly ILogger<BlogController> _logger;

    public BlogController(LtlhelpContext context, ILogger<BlogController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Blog/Index
    public async Task<IActionResult> Index()
    {
        try
        {
            // Lấy danh sách bài viết từ bảng BlogPosts, sắp xếp theo ngày đăng mới nhất
            var blogPosts = await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.BlogCategory)
                .Include(b => b.BlogComments)
                .Include(b => b.Tags)
                .Where(b => b.IsPublished == true)
                .OrderByDescending(b => b.PublishedAt)
                .ToListAsync();

            return View(blogPosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading blog posts");
            return View(new List<BlogPost>());
        }
    }

    // GET: Blog/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        try
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy 1 bài viết theo id, kèm theo danh sách comment nếu có
            var blogPost = await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.BlogCategory)
                .Include(b => b.Tags)
                .Include(b => b.BlogComments)
                    .ThenInclude(c => c.User)
                .Where(b => b.IsPublished == true)
                .FirstOrDefaultAsync(b => b.BlogPostId == id);

            if (blogPost == null)
            {
                return NotFound();
            }

            // Lấy các bài viết liên quan (cùng category, loại trừ bài hiện tại)
            var relatedPosts = await _context.BlogPosts
                .Include(b => b.Author)
                .Include(b => b.BlogCategory)
                .Where(b => b.BlogCategoryId == blogPost.BlogCategoryId 
                    && b.BlogPostId != blogPost.BlogPostId 
                    && b.IsPublished == true)
                .OrderByDescending(b => b.PublishedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.RelatedPosts = relatedPosts;

            return View(blogPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading blog post details");
            return NotFound();
        }
    }
}

