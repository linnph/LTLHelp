using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTLHelp.Models;

namespace LTLHelp.Controllers;

public class ContactController : Controller
{
    private readonly LtlhelpContext _context;
    private readonly ILogger<ContactController> _logger;

    public ContactController(LtlhelpContext context, ILogger<ContactController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Contact/Index
    public IActionResult Index()
    {
        return View();
    }

    // POST: Contact/Index
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string name, string email, string phone, string subject, string message)
    {
        try
        {
            // Kiểm tra validation cơ bản
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin bắt buộc (Họ tên, Email, Nội dung).");
                return View();
            }

            // Tạo ContactMessage mới
            var contactMessage = new ContactMessage
            {
                Name = name,
                Email = email,
                Phone = phone,
                Subject = subject,
                Message = message,
                IsHandled = false,
                CreatedAt = DateTime.Now
            };

            _context.Add(contactMessage);
            await _context.SaveChangesAsync();

            // Chuyển hướng với thông báo thành công
            TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting contact message");
            ModelState.AddModelError("", "Đã xảy ra lỗi khi gửi tin nhắn. Vui lòng thử lại.");
            return View();
        }
    }
}

