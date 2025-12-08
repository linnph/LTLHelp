using LTLHelp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace LTLHelp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CampaignsController : AdminBaseController
    {
        private readonly LtlhelpContext _context;
        private readonly string _uploadFolder;

        public CampaignsController(LtlhelpContext context)
        {
            _context = context;

            // Thư mục chứa ảnh upload
            _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/campaigns");
            Directory.CreateDirectory(_uploadFolder);
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.Campaigns
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(list);
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _context.CampaignCategories
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name");
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();
            return View(new Campaign());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Campaign model, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(model);
            }

            // === Upload ảnh ===
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                string filePath = Path.Combine(_uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await ImageFile.CopyToAsync(stream);

                model.ImageUrl = "/uploads/campaigns/" + fileName;
            }

            model.CreatedAt = DateTime.Now;

            _context.Campaigns.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✔ Thêm chiến dịch thành công!";
            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null) return NotFound();

            await LoadCategoriesAsync();
            return View(campaign);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Campaign model, IFormFile ImageFile)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync();
                return View(model);
            }

            // Cập nhật thông tin cơ bản
            campaign.Title = model.Title;
            campaign.GoalAmount = model.GoalAmount;
            campaign.StartDate = model.StartDate;
            campaign.EndDate = model.EndDate;
            campaign.CategoryId = model.CategoryId;

            // === Upload ảnh mới nếu có ===
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                string filePath = Path.Combine(_uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await ImageFile.CopyToAsync(stream);

                // === Xóa ảnh cũ nếu tồn tại ===
                if (!string.IsNullOrWhiteSpace(campaign.ImageUrl))
                {
                    string oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", campaign.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                campaign.ImageUrl = "/uploads/campaigns/" + fileName;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "✔ Cập nhật chiến dịch thành công!";
            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);

            if (campaign == null)
                return NotFound();

            // Check nếu có donation thì không cho xóa
            bool hasDonations = await _context.Donations
                .AnyAsync(d => d.CampaignId == id);

            if (hasDonations)
            {
                TempData["Error"] = "❌ Chiến dịch này đã có khoản quyên góp, không thể xóa!";
                return RedirectToAction(nameof(Index));
            }

            // Xóa ảnh nếu có
            if (!string.IsNullOrWhiteSpace(campaign.ImageUrl))
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", campaign.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();

            TempData["Success"] = "✔ Xóa chiến dịch thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
