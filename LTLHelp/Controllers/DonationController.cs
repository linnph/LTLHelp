using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LTLHelp.Models;
using LTLHelp.Services.Vnpay;
using LTLHelp.Models.Vnpay;

namespace LTLHelp.Controllers;

public class DonationController : Controller
{
    private readonly LtlhelpContext _context;
    private readonly ILogger<DonationController> _logger;
    private readonly IVnPayService _vnPayService;
    private readonly IConfiguration _configuration;

    public DonationController(LtlhelpContext context, ILogger<DonationController> logger, IVnPayService vnPayService, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _vnPayService = vnPayService;
        _configuration = configuration;
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

            // Log để debug
            _logger.LogInformation("Available PaymentMethods: {Methods}", 
                string.Join(", ", paymentMethods.Select(p => $"ID={p.PaymentMethodId}, Name={p.Name}, Code={p.Code}")));

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
            // Thử lấy PaymentMethodId từ Request.Form nếu parameter không có
            if (!PaymentMethodId.HasValue)
            {
                var paymentMethodIdStr = Request.Form["PaymentMethodId"].FirstOrDefault();
                if (!string.IsNullOrEmpty(paymentMethodIdStr) && int.TryParse(paymentMethodIdStr, out int parsedId))
                {
                    PaymentMethodId = parsedId;
                    _logger.LogInformation("PaymentMethodId retrieved from Request.Form: {PaymentMethodId}", PaymentMethodId);
                }
            }
            
            _logger.LogInformation("=== CHECKOUT POST START ===");
            _logger.LogInformation("Initial - PaymentMethodId: {PaymentMethodId}, Amount: {Amount}, CampaignId: {CampaignId}, DonorName: {DonorName}, DonorEmail: {DonorEmail}", 
                PaymentMethodId, donation.Amount, donation.CampaignId, donation.DonorName, donation.DonorEmail);
            
            // Debug: Log Request.Form CampaignId
            var formCampaignId = Request.Form["CampaignId"].FirstOrDefault();
            _logger.LogInformation("Request.Form CampaignId: {FormCampaignId}", formCampaignId);
            
            // Lấy CampaignId từ Request.Form nếu donation.CampaignId == 0
            if (donation.CampaignId == 0 && !string.IsNullOrEmpty(formCampaignId))
            {
                if (int.TryParse(formCampaignId, out int parsedCampaignId))
                {
                    donation.CampaignId = parsedCampaignId;
                    _logger.LogInformation("CampaignId retrieved from Request.Form: {CampaignId}", donation.CampaignId);
                }
            }
            
            _logger.LogInformation("After processing - CampaignId: {CampaignId}", donation.CampaignId);
            
            // Đảm bảo IsAnonymous có giá trị (không null)
            // Nếu checkbox không được check, IsAnonymous sẽ là null, cần set thành false
            if (!donation.IsAnonymous.HasValue)
            {
                donation.IsAnonymous = false;
            }
            
            // ⚠️ QUAN TRỌNG: Loại bỏ validation của navigation property Campaign TRƯỚC khi kiểm tra ModelState
            ModelState.Remove("Campaign");
            
            // Validate CampaignId: cho phép CampaignId = 0 (quyên góp lẻ), nếu > 0 thì phải tồn tại trong database
            if (donation.CampaignId > 0)
            {
                var campaignExists = await _context.Campaigns.AnyAsync(c => c.CampaignId == donation.CampaignId);
                if (!campaignExists)
                {
                    _logger.LogWarning("Campaign not found: {CampaignId}", donation.CampaignId);
                    ModelState.AddModelError("CampaignId", "Chiến dịch không tồn tại.");
                }
            }
            // Nếu CampaignId = 0, đó là quyên góp lẻ, không cần validate - xóa lỗi nếu có
            else if (donation.CampaignId == 0)
            {
                ModelState.Remove("CampaignId");
            }

            // Validate PaymentMethodId
            if (!PaymentMethodId.HasValue)
            {
                _logger.LogWarning("PaymentMethodId is missing in form submission. Form data: {FormData}", 
                    string.Join(", ", Request.Form.Select(kv => $"{kv.Key}={string.Join(",", kv.Value.ToArray())}")));
                ModelState.AddModelError("", "Vui lòng chọn phương thức thanh toán.");
            }
            
            // Log ModelState errors và trạng thái
            _logger.LogInformation("ModelState.IsValid: {IsValid}, CampaignId: {CampaignId}, PaymentMethodId: {PaymentMethodId}", 
                ModelState.IsValid, donation.CampaignId, PaymentMethodId);
            
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                _logger.LogWarning("ModelState is invalid. Errors: {Errors}", errors);
                
                // Log chi tiết từng field error
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state?.Errors != null && state.Errors.Count > 0)
                    {
                        _logger.LogWarning("Field '{Key}' has errors: {Errors}", key, 
                            string.Join(", ", state.Errors.Select(e => e.ErrorMessage)));
                    }
                }
            }

            if (ModelState.IsValid)
            {
                // Gán thời gian tạo
                donation.CreatedAt = DateTime.Now;

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

                // Kiểm tra phương thức thanh toán
                PaymentMethod? paymentMethod = null;
                if (PaymentMethodId.HasValue)
                {
                    paymentMethod = await _context.PaymentMethods
                        .FirstOrDefaultAsync(p => p.PaymentMethodId == PaymentMethodId.Value);
                    
                    if (paymentMethod == null)
                    {
                        _logger.LogWarning("PaymentMethod not found: {PaymentMethodId}", PaymentMethodId.Value);
                        ModelState.AddModelError("", "Phương thức thanh toán không tồn tại.");
                    }
                    else
                    {
                        // Log tất cả payment methods để debug
                        var allPaymentMethods = await _context.PaymentMethods.ToListAsync();
                        _logger.LogInformation("All PaymentMethods in DB: {Methods}", 
                            string.Join(", ", allPaymentMethods.Select(p => $"ID={p.PaymentMethodId}, Name={p.Name}, Code={p.Code}")));
                    }
                }

                // Kiểm tra lại ModelState sau khi validate PaymentMethod
                if (!ModelState.IsValid)
                {
                    // Load lại dữ liệu cho View
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

                // Log thông tin payment method để debug
                _logger.LogInformation("PaymentMethod found: {PaymentMethodName}, Code: {Code}, PaymentMethodId: {PaymentMethodId}", 
                    paymentMethod?.Name ?? "NULL", paymentMethod?.Code ?? "NULL", PaymentMethodId);
                
                // Nếu là VNPay, tạo donation với status "Chờ thanh toán" và redirect sang VNPay
                bool isVnPay = paymentMethod != null && 
                              paymentMethod.Code != null && 
                              paymentMethod.Code.Trim().ToUpper() == "VNPAY";
                
                _logger.LogInformation("Is VNPay check: {IsVnPay}, PaymentMethod: {PaymentMethod}, Code: {Code}", 
                    isVnPay, paymentMethod?.Name, paymentMethod?.Code);
                
                if (isVnPay)
                {
                    _logger.LogInformation("=== STARTING VNPAY PAYMENT PROCESS ===");
                    _logger.LogInformation("Donation Amount: {Amount}, PaymentMethodId: {PaymentMethodId}, Code: {Code}", 
                        donation.Amount, PaymentMethodId.Value, paymentMethod.Code);
                    
                    // Gán trạng thái ban đầu cho VNPay
                    donation.Status = "Chờ thanh toán";

                    // Lưu vào database (bảng Donations)
                    _context.Add(donation);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Donation saved with ID: {DonationId}", donation.DonationId);

                    try
                    {
                        // Tạo PaymentInformationModel cho VNPay
                        var paymentInfo = new PaymentInformationModel
                        {
                            Name = donation.DonorName ?? "Người quyên góp",
                            OrderDescription = donation.CampaignId > 0 
                                ? $"Quyen gop chien dich {donation.CampaignId}" 
                                : "Quyen gop tu thien",
                            Amount = donation.Amount,
                            OrderType = "other"
                        };

                        // Tạo orderId (vnp_TxnRef) với format: DonationId_yyyyMMddHHmmss
                        var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"] ?? "SE Asia Standard Time");
                        var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
                        string orderId = $"{donation.DonationId}_{timeNow:yyyyMMddHHmmss}";
                        
                        _logger.LogInformation("Creating VNPay URL with OrderId: {OrderId}, Amount: {Amount}", orderId, paymentInfo.Amount);

                        // Tạo URL thanh toán VNPay
                        var paymentUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext, orderId);
                        
                        _logger.LogInformation("=== VNPAY URL CREATED SUCCESSFULLY ===");
                        _logger.LogInformation("Payment URL: {PaymentUrl}", paymentUrl);
                        _logger.LogInformation("Redirecting to VNPay...");

                        // Redirect sang cổng VNPay
                        return Redirect(paymentUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "=== ERROR CREATING VNPAY URL ===");
                        _logger.LogError(ex, "Exception details: {Exception}", ex.ToString());
                        
                        // Xóa donation đã tạo nếu có lỗi
                        try
                        {
                            _context.Remove(donation);
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogError(deleteEx, "Error deleting donation after VNPay URL creation failure");
                        }
                        
                        TempData["PaymentError"] = $"Đã xảy ra lỗi khi tạo liên kết thanh toán VNPay: {ex.Message}. Vui lòng thử lại hoặc liên hệ hỗ trợ.";
                        
                        // Load lại dữ liệu cho View
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
                }
                else
                {
                    // Các phương thức thanh toán khác (MoMo, Bank...) hoặc không phải VNPay
                    _logger.LogInformation("Processing non-VNPay payment. PaymentMethod: {PaymentMethodName}, Code: {Code}", 
                        paymentMethod?.Name ?? "Unknown", paymentMethod?.Code ?? "Unknown");
                    
                    // Các phương thức thanh toán khác (MoMo, Bank...)
                    // Gán trạng thái ban đầu
                    donation.Status = "Chờ xác nhận";

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
            }

            // Nếu ModelState không hợp lệ, load lại dữ liệu cho View
            var paymentMethodsForView = await _context.PaymentMethods
                .Where(p => p.IsActive == true)
                .ToListAsync();

            if (donation.CampaignId > 0)
            {
                var campaign = await _context.Campaigns
                    .Include(c => c.Category)
                    .FirstOrDefaultAsync(c => c.CampaignId == donation.CampaignId);
                ViewBag.Campaign = campaign;
            }

            ViewBag.PaymentMethods = paymentMethodsForView;
            ViewBag.PaymentMethodSelectList = new SelectList(paymentMethodsForView, "PaymentMethodId", "Name");

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
                .Include(d => d.Transactions)
                    .ThenInclude(t => t.PaymentMethod)
                .FirstOrDefaultAsync(d => d.DonationId == donationId.Value);

            if (donation != null)
            {
                ViewBag.Donation = donation;
                
                // Lấy transaction thành công gần nhất
                var successfulTransaction = donation.Transactions
                    .Where(t => t.Status == "Thành công")
                    .OrderByDescending(t => t.PaidAt)
                    .FirstOrDefault();
                
                ViewBag.Transaction = successfulTransaction;
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
