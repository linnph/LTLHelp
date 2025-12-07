using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTLHelp.Models;

namespace LTLHelp.Controllers;

public class CampaignController : Controller
{
    private readonly LtlhelpContext _context;
    private readonly ILogger<CampaignController> _logger;

    public CampaignController(LtlhelpContext context, ILogger<CampaignController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Campaign
    public async Task<IActionResult> Index()
    {
        try
        {
            // Lấy toàn bộ danh sách chiến dịch, sắp xếp theo ngày tạo giảm dần
            var campaigns = await _context.Campaigns
                .Include(c => c.Category)
                .OrderByDescending(c => c.CreatedAt)
                .ThenByDescending(c => c.CampaignId)
                .ToListAsync();

            return View(campaigns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading campaigns");
            return View(new List<Campaign>());
        }
    }

    // GET: Campaign/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        try
        {
            // Lấy chiến dịch theo id, kèm các cập nhật và hình ảnh
            var campaign = await _context.Campaigns
                .Include(c => c.Category)
                .Include(c => c.CampaignUpdates)
                .Include(c => c.CampaignGalleries)
                .Include(c => c.Donations)
                .FirstOrDefaultAsync(m => m.CampaignId == id);

            if (campaign == null)
            {
                return NotFound();
            }

            // Sắp xếp CampaignUpdates và CampaignGalleries sau khi load
            if (campaign.CampaignUpdates != null)
            {
                campaign.CampaignUpdates = campaign.CampaignUpdates.OrderByDescending(u => u.CreatedAt).ToList();
            }
            if (campaign.CampaignGalleries != null)
            {
                campaign.CampaignGalleries = campaign.CampaignGalleries.OrderBy(g => g.CreatedAt).ToList();
            }

            return View(campaign);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading campaign details for id {CampaignId}", id);
            return NotFound();
        }
    }

    // GET: Campaign/Progress
    public async Task<IActionResult> Progress()
    {
        try
        {
            // Lấy toàn bộ chiến dịch để hiển thị bảng thống kê tiến độ
            var campaigns = await _context.Campaigns
                .Include(c => c.Category)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(campaigns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading campaign progress");
            return View(new List<Campaign>());
        }
    }
}

