using LTLHelp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LTLHelp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CampaignsController : Controller
    {
        private readonly LtlhelpContext _context;

        public CampaignsController(LtlhelpContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.Campaigns.ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Create()
        {
            await LoadCategories();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Campaign model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(model);
            }

            _context.Campaigns.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null) return NotFound();

            await LoadCategories();
            return View(campaign);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Campaign model)
        {
            if (id != model.CampaignId)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(model);
            }

            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
                return NotFound();

            // Cập nhật các field được phép sửa
            campaign.Title = model.Title;
            campaign.GoalAmount = model.GoalAmount;
            campaign.StartDate = model.StartDate;
            campaign.EndDate = model.EndDate;
            campaign.RaisedAmount = model.RaisedAmount;

            // ❗ Không đụng đến RaisedAmount → GIỮ NGUYÊN GIÁ TRỊ CŨ

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }




        public async Task<IActionResult> Delete(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null) return NotFound();

            bool hasDonations = await _context.Donations.AnyAsync(d => d.CampaignId == id);
            if (hasDonations)
            {
                TempData["Error"] = "Không thể xóa vì có quyên góp liên quan.";
                return RedirectToAction(nameof(Index));
            }

            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategories()
        {
            ViewBag.Categories = new SelectList(
                await _context.CampaignCategories.ToListAsync(),
                "CategoryId",
                "Name"
            );
        }

    }
}
