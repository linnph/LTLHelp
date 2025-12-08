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
            // Cho phép CampaignId = 0 hoặc null (quyên góp lẻ không theo chiến dịch)
            var donation = new Donation
            {
                CampaignId = campaignId ?? 0, // 0 = quyên góp lẻ, không theo chiến dịch
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
            
            // Validate CampaignId: nếu có campaignId thì phải tồn tại trong database
            if (donation.CampaignId > 0)
            {
                var campaignExists = await _context.Campaigns.AnyAsync(c => c.CampaignId == donation.CampaignId);
                if (!campaignExists)
                {
                    ModelState.AddModelError("CampaignId", "Chiến dịch không tồn tại.");
                }
            }

            if (ModelState.IsValid)
            {
                // Gán thời gian tạo
                donation.CreatedAt = DateTime.Now;

                // Gán trạng thái ban đầu
                donation.Status = "Chờ xác nhận";

                // Gán UserId từ session nếu user đã đăng nhập
                donation.UserId = HttpContext.Session.GetInt32("UserId");

                // Lấy thông tin user từ session để điền DonorEmail nếu chưa có
                if (donation.UserId.HasValue && string.IsNullOrEmpty(donation.DonorEmail))
                {
                    var user = await _context.Users.FindAsync(donation.UserId.Value);
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        donation.DonorEmail = user.Email;
                    }
                }

                // Lưu vào database (bảng Donations)
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
                        TransactionCode = $"TXN{DateTime.Now:yyyyMMddHHmmss}{donation.DonationId}",
                        PaidAt = DateTime.Now // Gán thời gian thanh toán
                    };
                    _context.Add(transaction);
                    await _context.SaveChangesAsync();

                    // Nếu thanh toán thành công, cập nhật status của donation
                    // (Có thể mở rộng logic này để check status từ payment gateway)
                    // Tạm thời: nếu có transaction thì coi như đã thanh toán, cập nhật status
                    donation.Status = "Xác nhận";
                    await _context.SaveChangesAsync();
                }

                // Cập nhật RaisedAmount của Campaign (chỉ khi có CampaignId > 0)
                if (donation.CampaignId > 0)
                {
                    var campaign = await _context.Campaigns.FindAsync(donation.CampaignId);
                    if (campaign != null)
                    {
                        campaign.RaisedAmount = (campaign.RaisedAmount ?? 0) + donation.Amount;
                        await _context.SaveChangesAsync();
                    }
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
            string? userEmail = HttpContext.Session.GetString("UserEmail");

            // Nếu không có userId từ session và không có từ parameter, redirect về login
            if (!currentUserId.HasValue && string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            // Tự động cập nhật UserId cho các donations cũ có DonorEmail khớp nhưng UserId null
            if (currentUserId.HasValue && !string.IsNullOrEmpty(userEmail))
            {
                var donationsToUpdate = await _context.Donations
                    .Where(d => d.UserId == null && d.DonorEmail != null && d.DonorEmail.ToLower() == userEmail.ToLower())
                    .ToListAsync();
                
                foreach (var donation in donationsToUpdate)
                {
                    donation.UserId = currentUserId.Value;
                }
                
                if (donationsToUpdate.Any())
                {
                    await _context.SaveChangesAsync();
                }
            }

            // Lấy donations của user hiện tại: chỉ lấy theo UserId
            IQueryable<Donation> query = _context.Donations;
            
            if (currentUserId.HasValue)
            {
                // Chỉ lấy donations có UserId khớp với user hiện tại
                query = query.Where(d => d.UserId == currentUserId);
            }
            
            // Include Campaign chỉ cho các donations có CampaignId > 0
            // Sử dụng cách load riêng để tránh lỗi khi CampaignId = 0
            var donations = await query
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
            
            // Load Campaign cho các donations có CampaignId > 0
            var campaignIds = donations.Where(d => d.CampaignId > 0).Select(d => d.CampaignId).Distinct().ToList();
            if (campaignIds.Any())
            {
                var campaigns = await _context.Campaigns
                    .Where(c => campaignIds.Contains(c.CampaignId))
                    .ToDictionaryAsync(c => c.CampaignId);
                
                foreach (var donation in donations.Where(d => d.CampaignId > 0))
                {
                    if (campaigns.TryGetValue(donation.CampaignId, out var campaign))
                    {
                        donation.Campaign = campaign;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(userEmail))
            {
                // Nếu không có UserId nhưng có email, lấy theo DonorEmail
                query = query.Where(d => d.DonorEmail != null && d.DonorEmail.ToLower() == userEmail.ToLower());
            }
            else
            {
                // Không có thông tin user, trả về danh sách rỗng
                query = query.Where(d => false);
            }

            // Tính tổng số liệu
            var totalDonated = donations.Sum(d => d.Amount);
            var totalDonations = donations.Count;
            // Sửa logic: check status "Xác nhận" (theo dữ liệu thực tế trong database)
            var confirmedAmount = donations
                .Where(d => d.Status == "Xác nhận" || d.Status == "Thành công" || d.Status == "Đã thanh toán")
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

