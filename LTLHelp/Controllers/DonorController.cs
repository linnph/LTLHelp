using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTLHelp.Models;

namespace LTLHelp.Controllers;

public class DonorController : Controller
{
    private readonly LtlhelpContext _context;
    private readonly ILogger<DonorController> _logger;

    public DonorController(LtlhelpContext context, ILogger<DonorController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Donor
    public async Task<IActionResult> Index()
    {
        try
        {
            // Lấy toàn bộ danh sách giao dịch donate từ bảng Transactions
            // Bao gồm thông tin Donation để lấy tên người donate, ẩn danh, lời nhắn
            // Sắp xếp theo số tiền từ lớn đến nhỏ
            var transactions = await _context.Transactions
                .Include(t => t.Donation)
                .Where(t => (t.Status == "Thành công" || t.Status == "Đã thanh toán") && t.Donation != null)
                .OrderByDescending(t => t.Amount ?? (t.Donation != null ? t.Donation.Amount : 0))
                .ThenByDescending(t => t.PaidAt.HasValue ? t.PaidAt.Value : (t.Donation != null && t.Donation.CreatedAt.HasValue ? t.Donation.CreatedAt.Value : DateTime.MinValue))
                .ThenByDescending(t => t.TransactionId)
                .ToListAsync();

            return View(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading donors");
            return View(new List<Transaction>());
        }
    }
}

