using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTLHelp.Models;

namespace LTLHelp.Controllers;

public class TestimonialController : Controller
{
    private readonly LtlhelpContext _context;
    private readonly ILogger<TestimonialController> _logger;

    public TestimonialController(LtlhelpContext context, ILogger<TestimonialController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Testimonial/Index
    public async Task<IActionResult> Index()
    {
        try
        {
            // Lấy toàn bộ danh sách Testimonials từ database, sắp xếp theo CreatedAt giảm dần
            var testimonials = await _context.Testimonials
                .Where(t => t.IsActive == true)
                .OrderByDescending(t => t.CreatedAt ?? DateTime.MinValue)
                .ThenByDescending(t => t.TestimonialId)
                .ToListAsync();

            // Convert từ entity model sang ViewModel
            var testimonialViewModels = testimonials.Select(t => new TestimonialViewModel
            {
                Name = t.FullName ?? "Khách hàng",
                Position = t.Position ?? "",
                Content = t.Message ?? "",
                ImageUrl = !string.IsNullOrEmpty(t.AvatarUrl) 
                    ? (t.AvatarUrl.StartsWith("http://") || t.AvatarUrl.StartsWith("https://")
                        ? t.AvatarUrl
                        : (t.AvatarUrl.StartsWith("~/")
                            ? t.AvatarUrl
                            : (t.AvatarUrl.StartsWith("/")
                                ? $"~{t.AvatarUrl}" // Chuyển /assets/ thành ~/assets/
                                : $"~/assets/img/testimonial/{t.AvatarUrl.TrimStart('/')}")))
                    : "~/assets/img/testimonial/testi_3_1.png",
                Rating = t.Rating
            }).ToList();

            return View(testimonialViewModels);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading testimonials");
            return View(new List<TestimonialViewModel>());
        }
    }
}

