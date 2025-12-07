using Microsoft.AspNetCore.Mvc;
using LTLHelp.Areas.Admin.Models;
using LTLHelp.Models; 
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace LTLHelp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly LtlhelpContext _context;

        public HomeController(LtlhelpContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            model.TotalDonationAmount = await _context.Donations.SumAsync(d => d.Amount);

            model.PendingDonationsCount = (int)await _context.Donations
                .CountAsync(d => d.Status == "Pending"); 

            model.TotalActiveCampaigns = (int)await _context.Donations
                .CountAsync(c => c.IsAnonymous == true);

            model.TotalDonorCount = (int)await _context.Donations.CountAsync();

            ViewData["Title"] = "Dashboard Tổng quan";
            return View(model);
        }
    }
}

