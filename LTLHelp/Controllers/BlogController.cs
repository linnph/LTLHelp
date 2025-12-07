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

            // Lấy thông tin user nếu đã đăng nhập để điền vào form
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var user = await _context.Users
                    .Include(u => u.UserProfile)
                    .FirstOrDefaultAsync(u => u.UserId == userId.Value);
                
                if (user != null)
                {
                    ViewBag.CurrentUserName = user.UserProfile != null 
                        ? $"{user.UserProfile.FirstName} {user.UserProfile.LastName}".Trim()
                        : user.UserName;
                    ViewBag.CurrentUserEmail = user.Email;
                }
            }

            return View(blogPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading blog post details");
            return NotFound();
        }
    }

    // POST: Blog/AddComment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int blogPostId, string name, string email, string content)
    {
        try
        {
            // Kiểm tra validation
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin (Tên, Email, Nội dung).";
                return RedirectToAction("Details", new { id = blogPostId });
            }

            // Kiểm tra blog post có tồn tại không
            var blogPost = await _context.BlogPosts.FindAsync(blogPostId);
            if (blogPost == null)
            {
                return NotFound();
            }

            // Lấy UserId từ session nếu user đã đăng nhập
            int? userId = HttpContext.Session.GetInt32("UserId");

            // Tạo BlogComment mới
            var comment = new BlogComment
            {
                BlogPostId = blogPostId,
                UserId = userId,
                Name = name,
                Email = email,
                Content = content,
                IsApproved = true, // Tự động approve, có thể thay đổi thành false nếu cần admin duyệt
                CreatedAt = DateTime.Now
            };

            _context.Add(comment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bình luận của bạn đã được gửi thành công!";
            return RedirectToAction("Details", new { id = blogPostId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding blog comment");
            TempData["ErrorMessage"] = "Đã xảy ra lỗi khi gửi bình luận. Vui lòng thử lại.";
            return RedirectToAction("Details", new { id = blogPostId });
        }
    }
}

