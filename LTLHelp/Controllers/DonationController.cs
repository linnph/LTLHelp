using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LTLHelp.Models;

namespace LTLHelp.Controllers;

public class DonationController : Controller
{
    private readonly LtlhelpContext _context;
    private readonly ILogger<DonationController> _logger;

    public DonationController(LtlhelpContext context, ILogger<DonationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Donation/Checkout
    public async Task<IActionResult> Checkout(int? campaignId)
    {
        try
        {
            Campaign? campaign = null;
            if (campaignId.HasValue)
            {
                campaign = await _context.Campaigns
                    .Include(c => c.Category)
                    .FirstOrDefaultAsync(c => c.CampaignId == campaignId.Value);
            }

            // Lấy danh sách phương thức thanh toán
            var paymentMethods = await _context.PaymentMethods
                .Where(p => p.IsActive == true)
                .ToListAsync();

            // Tạo model mới với giá trị mặc định
            var donation = new Donation
            {
                CampaignId = campaignId ?? 0,
                IsAnonymous = false, // Set giá trị mặc định là false (non-null)
                Amount = 0
            };

            ViewBag.Campaign = campaign;
            ViewBag.PaymentMethods = paymentMethods;
            ViewBag.PaymentMethodSelectList = new SelectList(paymentMethods, "PaymentMethodId", "Name");

            return View(donation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading checkout page");
            // Trả về model mới ngay cả khi có lỗi
            var donation = new Donation
            {
                CampaignId = campaignId ?? 0,
                IsAnonymous = false, // Set giá trị mặc định là false (non-null)
                Amount = 0
            };
            return View(donation);
        }
    }

    // POST: Donation/Checkout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout([Bind("CampaignId,DonorName,DonorEmail,Amount,IsAnonymous,DonorMessage")] Donation donation, int? PaymentMethodId)
    {
        try
        {
            // Đảm bảo IsAnonymous có giá trị (không null)
            // Nếu checkbox không được check, IsAnonymous sẽ là null, cần set thành false
            if (!donation.IsAnonymous.HasValue)
            {
                donation.IsAnonymous = false;
            }
            
            if (ModelState.IsValid)
            {
                // Gán thời gian tạo
                donation.CreatedAt = DateTime.Now;

                // Gán trạng thái ban đầu
                donation.Status = "Chờ xác nhận";

                // Gán UserId nếu có (tạm thời để null, có thể lấy từ session sau)
                donation.UserId = null;

                // Lưu vào database
                _context.Add(donation);
                await _context.SaveChangesAsync();

                // Tạo Transaction nếu có PaymentMethodId
                if (PaymentMethodId.HasValue)
                {
                    var transaction = new Transaction
                    {
                        DonationId = donation.DonationId,
                        PaymentMethodId = PaymentMethodId.Value,
                        Amount = donation.Amount,
                        Currency = "VND",
                        Status = "Chờ xác nhận",
                        TransactionCode = $"TXN{DateTime.Now:yyyyMMddHHmmss}{donation.DonationId}"
                    };
                    _context.Add(transaction);
                    await _context.SaveChangesAsync();
                }

                // Cập nhật RaisedAmount của Campaign
                var campaign = await _context.Campaigns.FindAsync(donation.CampaignId);
                if (campaign != null)
                {
                    campaign.RaisedAmount = (campaign.RaisedAmount ?? 0) + donation.Amount;
                    await _context.SaveChangesAsync();
                }

                // Chuyển hướng sang trang Success
                return RedirectToAction(nameof(Success), new { donationId = donation.DonationId });
            }

            // Nếu ModelState không hợp lệ, load lại dữ liệu cho View
            var paymentMethods = await _context.PaymentMethods
                .Where(p => p.IsActive == true)
                .ToListAsync();

            if (donation.CampaignId > 0)
            {
                var campaign = await _context.Campaigns
                    .Include(c => c.Category)
                    .FirstOrDefaultAsync(c => c.CampaignId == donation.CampaignId);
                ViewBag.Campaign = campaign;
            }

            ViewBag.PaymentMethods = paymentMethods;
            ViewBag.PaymentMethodSelectList = new SelectList(paymentMethods, "PaymentMethodId", "Name");

            return View(donation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing donation");
            ModelState.AddModelError("", "Đã xảy ra lỗi khi xử lý quyên góp. Vui lòng thử lại.");
            
            var paymentMethods = await _context.PaymentMethods
                .Where(p => p.IsActive == true)
                .ToListAsync();
            ViewBag.PaymentMethods = paymentMethods;
            ViewBag.PaymentMethodSelectList = new SelectList(paymentMethods, "PaymentMethodId", "Name");

            return View(donation);
        }
    }

    // GET: Donation/Success
    public async Task<IActionResult> Success(int? donationId)
    {
        if (donationId.HasValue)
        {
            var donation = await _context.Donations
                .Include(d => d.Campaign)
                .FirstOrDefaultAsync(d => d.DonationId == donationId.Value);

            if (donation != null)
            {
                ViewBag.Donation = donation;
            }
        }

        return View();
    }

    // GET: Donation/History
    public async Task<IActionResult> History(int? userId)
    {
        try
        {
            // Lấy userId từ session (ưu tiên) hoặc từ parameter
            int? currentUserId = userId ?? HttpContext.Session.GetInt32("UserId");

            // Nếu không có userId từ session và không có từ parameter, redirect về login
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            IQueryable<Donation> query = _context.Donations.Include(d => d.Campaign);

            // Chỉ lấy donations của user hiện tại
            query = query.Where(d => d.UserId == currentUserId);

            var donations = await query
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            // Tính tổng số liệu
            var totalDonated = donations.Sum(d => d.Amount);
            var totalDonations = donations.Count;
            var confirmedAmount = donations
                .Where(d => d.Status == "Thành công" || d.Status == "Đã thanh toán")
                .Sum(d => d.Amount);

            ViewBag.TotalDonated = totalDonated;
            ViewBag.TotalDonations = totalDonations;
            ViewBag.ConfirmedAmount = confirmedAmount;

            return View(donations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading donation history");
            return View(new List<Donation>());
        }
    }
}

