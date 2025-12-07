using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTLHelp.Models;

namespace LTLHelp.Controllers;

public class VolunteerController : Controller
{
    private readonly LtlhelpContext _context;
    private readonly ILogger<VolunteerController> _logger;

    public VolunteerController(LtlhelpContext context, ILogger<VolunteerController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Volunteer/Index
    public async Task<IActionResult> Index()
    {
        try
        {
            // Lấy danh sách từ bảng TeamMembers (đội nhóm)
            var teamMembers = await _context.TeamMembers
                .Where(t => t.IsActive == true)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            return View(teamMembers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading team members");
            return View(new List<TeamMember>());
        }
    }

    // GET: Volunteer/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        try
        {
            if (id == null)
            {
                return NotFound();
            }

            // Lấy chi tiết 1 thành viên đội nhóm
            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(t => t.TeamMemberId == id && t.IsActive == true);

            if (teamMember == null)
            {
                return NotFound();
            }

            return View(teamMember);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading team member details");
            return NotFound();
        }
    }

    // GET: Volunteer/Apply
    public IActionResult Apply()
    {
        return View();
    }

    // POST: Volunteer/Apply
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(string name, string email, string phone, string address, string occupation, string message)
    {
        try
        {
            // Kiểm tra validation cơ bản
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(phone))
            {
                ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin bắt buộc (Họ tên, Email, Số điện thoại).");
                return View();
            }

            // Tạo VolunteerApplication mới
            var application = new VolunteerApplication
            {
                FullName = name,
                Email = email,
                Phone = phone,
                Address = address,
                Occupation = occupation,
                Message = message,
                Status = "Chờ duyệt",
                CreatedAt = DateTime.Now
            };

            _context.Add(application);
            await _context.SaveChangesAsync();

            // Chuyển hướng với thông báo thành công
            TempData["SuccessMessage"] = "Đơn đăng ký tình nguyện viên của bạn đã được gửi thành công! Chúng tôi sẽ liên hệ với bạn sớm nhất có thể.";
            return RedirectToAction(nameof(Apply));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting volunteer application");
            ModelState.AddModelError("", "Đã xảy ra lỗi khi gửi đơn đăng ký. Vui lòng thử lại.");
            return View();
        }
    }
}

