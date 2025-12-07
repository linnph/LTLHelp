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
            // Lấy danh sách chiến dịch mới nhất từ database
            var campaigns = await _context.Campaigns
                .OrderByDescending(c => c.CreatedAt)
                .Take(6)
                .ToListAsync();

            // Đảm bảo luôn trả về một list, không bao giờ null
            return View(campaigns ?? new List<Campaign>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading campaigns");
            // Trả về list rỗng nếu có lỗi
            return View(new List<Campaign>());
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
