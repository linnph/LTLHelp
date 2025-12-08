using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTLHelp.Models;

namespace LTLHelp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : AdminBaseController
    {
        private readonly LtlhelpContext _context;

        public HomeController(LtlhelpContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Tổng quan
            ViewBag.TotalCampaigns = await _context.Campaigns.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalDonations = await _context.Donations.CountAsync();

            ViewBag.TotalMoney = await _context.Donations
                .Where(d => d.Status == "Thành công" || d.Status == "Đã thanh toán")
                .SumAsync(d => (decimal?)d.Amount ?? 0);

            ViewBag.ActiveCampaigns = await _context.Campaigns
                .Where(c => c.EndDate >= DateOnly.FromDateTime(DateTime.Now))
                .CountAsync();

            ViewBag.EndedCampaigns = ViewBag.TotalCampaigns - ViewBag.ActiveCampaigns;

            // Dữ liệu biểu đồ: Quyên góp theo tháng
            ViewBag.DonationMonths = await _context.Donations
                .GroupBy(d => d.CreatedAt.Value.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(x => x.Amount) })
                .OrderBy(x => x.Month)
                .ToListAsync();

            // Chiến dịch sắp hết hạn
            ViewBag.ExpiringCampaigns = await _context.Campaigns
                .Where(c => c.EndDate < DateOnly.FromDateTime(DateTime.Now.AddDays(7)))
                .OrderBy(c => c.EndDate)
                .Take(5)
                .ToListAsync();

            // Top chiến dịch mạnh nhất
            ViewBag.TopCampaigns = await _context.Campaigns
                .OrderByDescending(c => c.RaisedAmount)
                .Take(5)
                .ToListAsync();

            // Chiến dịch yếu
            ViewBag.WeakCampaigns = await _context.Campaigns
                .OrderBy(c => c.RaisedAmount)
                .Take(5)
                .ToListAsync();

            // Top người dùng quyên góp nhiều nhất
            ViewBag.TopDonors = await _context.Donations
                .GroupBy(g => g.UserId)
                .Select(g => new {
                    User = g.First().User.UserName,
                    Total = g.Sum(x => x.Amount)
                })
                .OrderByDescending(g => g.Total)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}
