using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTLHelp.Models;

namespace LTLHelp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly LtlhelpContext _context;

    public HomeController(ILogger<HomeController> logger, LtlhelpContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var viewModel = new HomeViewModel();

            // 1. Lấy 6 chiến dịch mới nhất từ bảng Campaigns
            viewModel.Campaigns = await _context.Campaigns
                .OrderByDescending(c => c.CreatedAt)
                .Take(6)
                .ToListAsync();

            // 2. Lấy 4 tình nguyện viên từ bảng TeamMembers (hoặc Volunteers nếu cần)
            viewModel.TeamMembers = await _context.TeamMembers
                .Where(t => t.IsActive == true)
                .OrderByDescending(t => t.CreatedAt)
                .Take(4)
                .ToListAsync();

            // 3. Lấy 3 bài blog mới nhất từ bảng BlogPosts
            viewModel.BlogPosts = await _context.BlogPosts
                .Include(b => b.BlogCategory)
                .Where(b => b.IsPublished == true)
                .OrderByDescending(b => b.PublishedAt)
                .Take(3)
                .ToListAsync();

            // 4. Lấy 3 lời chứng thực (Testimonials)
            // Lưu ý: Bảng Testimonials chưa có trong database, 
            // có thể tạo bảng sau hoặc lấy từ BlogComments/ContactMessages
            // Tạm thời để danh sách rỗng, có thể thêm sau
            viewModel.Testimonials = new List<Testimonial>();

            // 5. Lấy 4 câu hỏi FAQ
            // Lưu ý: Bảng FAQ chưa có trong database, có thể tạo bảng sau
            // Tạm thời để danh sách rỗng, có thể thêm sau
            viewModel.FAQs = new List<FAQ>();

            // 6. Lấy số liệu thống kê
            viewModel.Statistics = new HomeStatistics
            {
                TotalCampaigns = await _context.Campaigns.CountAsync(),
                TotalDonations = await _context.Donations
                    .Where(d => d.Status == "Thành công" || d.Status == "Đã thanh toán")
                    .SumAsync(d => (decimal?)d.Amount) ?? 0,
                TotalVolunteers = await _context.Volunteers
                    .Where(v => v.Status == "Hoạt động")
                    .CountAsync(),
                TotalDonationCount = await _context.Donations
                    .Where(d => d.Status == "Thành công" || d.Status == "Đã thanh toán")
                    .CountAsync()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading home page data");
            // Trả về ViewModel rỗng nếu có lỗi
            return View(new HomeViewModel());
        }
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
