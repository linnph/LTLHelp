using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTLHelp.Models;
using LTLHelp.Services.Vnpay;

namespace LTLHelp.Controllers;

public class PaymentController : Controller
{
    private readonly LtlhelpContext _context;
    private readonly ILogger<PaymentController> _logger;
    private readonly IVnPayService _vnPayService;

    public PaymentController(LtlhelpContext context, ILogger<PaymentController> logger, IVnPayService vnPayService)
    {
        _context = context;
        _logger = logger;
        _vnPayService = vnPayService;
    }

    // GET: Payment/PaymentCallback
    // Callback từ VNPay sau khi thanh toán
    public async Task<IActionResult> PaymentCallback()
    {
        try
        {
            // Lấy dữ liệu từ query string
            var response = _vnPayService.PaymentExecute(Request.Query);

            _logger.LogInformation("=== VNPAY CALLBACK RECEIVED ===");
            _logger.LogInformation("OrderId: {OrderId}, ResponseCode: {ResponseCode}, Success: {Success}", 
                response.OrderId, response.VnPayResponseCode, response.Success);

            // Lấy DonationId từ OrderId
            // OrderId có thể là:
            // - Số đơn giản: "123"
            // - Format mới: "123_20231213143000" (DonationId_yyyyMMddHHmmss)
            int donationId;
            if (string.IsNullOrEmpty(response.OrderId))
            {
                _logger.LogError("OrderId is null or empty from VNPay");
                TempData["PaymentError"] = "Mã đơn hàng không hợp lệ.";
                return RedirectToAction("Checkout", "Donation");
            }

            // Thử parse OrderId - có thể là số đơn giản hoặc format DonationId_yyyyMMddHHmmss
            if (response.OrderId.Contains('_'))
            {
                // Format mới: DonationId_yyyyMMddHHmmss
                var parts = response.OrderId.Split('_');
                if (parts.Length > 0 && int.TryParse(parts[0], out donationId))
                {
                    _logger.LogInformation("Parsed DonationId from format DonationId_yyyyMMddHHmmss: {DonationId}", donationId);
                }
                else
                {
                    _logger.LogError("Invalid OrderId format from VNPay: {OrderId}", response.OrderId);
                    TempData["PaymentError"] = "Mã đơn hàng không hợp lệ.";
                    return RedirectToAction("Checkout", "Donation");
                }
            }
            else
            {
                // Format cũ: chỉ là số
                if (!int.TryParse(response.OrderId, out donationId))
                {
                    _logger.LogError("Invalid OrderId from VNPay: {OrderId}", response.OrderId);
                    TempData["PaymentError"] = "Mã đơn hàng không hợp lệ.";
                    return RedirectToAction("Checkout", "Donation");
                }
            }

            // Lấy donation từ database
            var donation = await _context.Donations
                .Include(d => d.Campaign)
                .FirstOrDefaultAsync(d => d.DonationId == donationId);

            if (donation == null)
            {
                _logger.LogError("Donation not found: {DonationId}", donationId);
                return RedirectToAction("Checkout", "Donation", new { error = "Không tìm thấy thông tin quyên góp" });
            }

            // Lấy PaymentMethod VNPay
            var vnpayMethod = await _context.PaymentMethods
                .FirstOrDefaultAsync(p => p.Code != null && p.Code.ToUpper() == "VNPAY");

            if (vnpayMethod == null)
            {
                _logger.LogError("VNPay payment method not found in database");
                return RedirectToAction("Checkout", "Donation", new { error = "Phương thức thanh toán không hợp lệ" });
            }

            // Log chi tiết response
            _logger.LogInformation("Payment response - Success: {Success}, VnPayResponseCode: {ResponseCode}, OrderId: {OrderId}", 
                response.Success, response.VnPayResponseCode, response.OrderId);

            // Kiểm tra kết quả thanh toán
            if (response.Success && response.VnPayResponseCode == "00")
            {
                _logger.LogInformation("Payment successful, processing donation {DonationId}", donationId);
                // Kiểm tra xem donation đã được xử lý chưa (tránh xử lý trùng lặp)
                if (donation.Status == "Xác nhận" || donation.Status == "Thành công")
                {
                    // Đã xử lý rồi, chỉ redirect về Success
                    return RedirectToAction("Success", "Donation", new { donationId = donation.DonationId });
                }

                // Thanh toán thành công
                donation.Status = "Xác nhận";

                // Tạo hoặc cập nhật Transaction
                var existingTransaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.DonationId == donationId && t.PaymentMethodId == vnpayMethod.PaymentMethodId);

                Transaction? newTransaction = null;
                if (existingTransaction == null)
                {
                    newTransaction = new Transaction
                    {
                        DonationId = donation.DonationId,
                        PaymentMethodId = vnpayMethod.PaymentMethodId,
                        TransactionCode = response.TransactionId,
                        Amount = donation.Amount,
                        Currency = "VND",
                        Status = "Thành công",
                        PaidAt = DateTime.Now,
                        RawResponse = $"VNPay Response: {response.VnPayResponseCode}"
                    };
                    _context.Add(newTransaction);
                }
                else
                {
                    // Cập nhật transaction nếu đã tồn tại
                    existingTransaction.TransactionCode = response.TransactionId;
                    existingTransaction.Status = "Thành công";
                    existingTransaction.PaidAt = DateTime.Now;
                    existingTransaction.RawResponse = $"VNPay Response: {response.VnPayResponseCode}";
                }

                // Lưu transaction trước để có TransactionId
                await _context.SaveChangesAsync();

                // Cập nhật RaisedAmount của Campaign (chỉ khi có CampaignId > 0)
                if (donation.CampaignId > 0)
                {
                    var campaign = await _context.Campaigns.FindAsync(donation.CampaignId);
                    if (campaign != null)
                    {
                        // Chỉ cập nhật nếu donation chưa được tính vào RaisedAmount
                        // Kiểm tra xem đã có transaction thành công trước đó chưa
                        var transactionToCheck = newTransaction ?? existingTransaction;
                        var previousSuccessfulTransaction = await _context.Transactions
                            .AnyAsync(t => t.DonationId == donationId && 
                                         t.Status == "Thành công" && 
                                         t.TransactionId != transactionToCheck!.TransactionId);
                        
                        if (!previousSuccessfulTransaction)
                        {
                            campaign.RaisedAmount = (campaign.RaisedAmount ?? 0) + donation.Amount;
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                // Redirect sang trang Success
                return RedirectToAction("Success", "Donation", new { donationId = donation.DonationId });
            }
            else
            {
                // Thanh toán thất bại hoặc bị hủy
                _logger.LogWarning("Payment failed or cancelled - Success: {Success}, VnPayResponseCode: {ResponseCode}, DonationId: {DonationId}", 
                    response.Success, response.VnPayResponseCode, donationId);
                
                donation.Status = "Thất bại";

                // Tạo Transaction với status thất bại
                var existingTransaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.DonationId == donationId && t.PaymentMethodId == vnpayMethod.PaymentMethodId);

                if (existingTransaction == null)
                {
                    var transaction = new Transaction
                    {
                        DonationId = donation.DonationId,
                        PaymentMethodId = vnpayMethod.PaymentMethodId,
                        TransactionCode = response.TransactionId ?? "N/A",
                        Amount = donation.Amount,
                        Currency = "VND",
                        Status = "Thất bại",
                        PaidAt = DateTime.Now,
                        RawResponse = $"VNPay Response: {response.VnPayResponseCode}"
                    };
                    _context.Add(transaction);
                }
                else
                {
                    existingTransaction.Status = "Thất bại";
                    existingTransaction.PaidAt = DateTime.Now;
                    existingTransaction.RawResponse = $"VNPay Response: {response.VnPayResponseCode}";
                }

                await _context.SaveChangesAsync();

                // Redirect về Checkout với thông báo lỗi
                TempData["PaymentError"] = "Thanh toán không thành công. Vui lòng thử lại.";
                return RedirectToAction("Checkout", "Donation", new { campaignId = donation.CampaignId });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay callback");
            _logger.LogError(ex, "Exception details: {Exception}", ex.ToString());
            _logger.LogError("Query string: {QueryString}", Request.QueryString);
            
            // Cố gắng lấy DonationId từ query string nếu có
            var vnpTxnRef = Request.Query["vnp_TxnRef"].FirstOrDefault();
            if (!string.IsNullOrEmpty(vnpTxnRef))
            {
                int? donationIdFromQuery = null;
                if (vnpTxnRef.Contains('_'))
                {
                    var parts = vnpTxnRef.Split('_');
                    if (parts.Length > 0 && int.TryParse(parts[0], out int parsedId))
                    {
                        donationIdFromQuery = parsedId;
                    }
                }
                else if (int.TryParse(vnpTxnRef, out int parsedId2))
                {
                    donationIdFromQuery = parsedId2;
                }

                if (donationIdFromQuery.HasValue)
                {
                    // Kiểm tra xem donation có tồn tại không
                    var donation = await _context.Donations.FindAsync(donationIdFromQuery.Value);
                    if (donation != null)
                    {
                        _logger.LogInformation("Found donation {DonationId} from query string, redirecting to Checkout with campaignId", donationIdFromQuery.Value);
                        TempData["PaymentError"] = "Đã xảy ra lỗi khi xử lý thanh toán. Vui lòng liên hệ hỗ trợ.";
                        return RedirectToAction("Checkout", "Donation", new { campaignId = donation.CampaignId });
                    }
                }
            }

            TempData["PaymentError"] = "Đã xảy ra lỗi khi xử lý thanh toán. Vui lòng liên hệ hỗ trợ.";
            return RedirectToAction("Checkout", "Donation");
        }
    }
}

