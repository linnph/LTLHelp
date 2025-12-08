using LTLHelp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LTLHelp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportController : AdminBaseController
    {
        private readonly LtlhelpContext _context;

        public ReportController(LtlhelpContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Tổng số chiến dịch
            int totalCampaigns = await _context.Campaigns.CountAsync();

            // Tổng số người dùng
            int totalUsers = await _context.Users.CountAsync();

            // Tổng số khoản quyên góp
            int totalDonations = await _context.Donations.CountAsync();

            // Tổng số tiền quyên góp
            decimal totalMoney = await _context.Donations
                .Where(d => d.Status == "Thành công" || d.Status == "Đã thanh toán")
                .SumAsync(d => (decimal?)d.Amount ?? 0);

            // Chiến dịch đang hoạt động
            int activeCampaigns = await _context.Campaigns
                .Where(c => c.EndDate >= DateOnly.FromDateTime(DateTime.Now)).CountAsync();

            // Biểu đồ quyên góp theo tháng
            var donationByMonth = await _context.Donations
                .GroupBy(d => d.CreatedAt.Value.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(x => x.Amount) })
                .ToListAsync();

            // tỷ lệ trạng thái chiến dịch
            int endedCampaigns = totalCampaigns - activeCampaigns;

            // Top 5 chiến dịch quyên góp cao nhất
            var topCampaigns = await _context.Campaigns
                .OrderByDescending(c => c.RaisedAmount)
                .Take(5)
                .ToListAsync();

            // Top 5 donor quyên góp nhiều nhất
            var topDonors = await _context.Donations
                .GroupBy(d => d.UserId)
                .Select(g => new {
                    User = g.First().User.UserName,
                    Total = g.Sum(x => x.Amount)
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalCampaigns = totalCampaigns;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalDonations = totalDonations;
            ViewBag.TotalMoney = totalMoney;
            ViewBag.ActiveCampaigns = activeCampaigns;
            ViewBag.EndedCampaigns = endedCampaigns;

            ViewBag.DonationMonths = donationByMonth;

            ViewBag.TopCampaigns = topCampaigns;
            ViewBag.TopDonors = topDonors;

            return View();
        }
    }
}
