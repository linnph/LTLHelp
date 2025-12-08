using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTLHelp.Models;

namespace LTLHelp.Controllers;

public class AccountController : Controller
{
    private readonly LtlhelpContext _context;
    private readonly ILogger<AccountController> _logger;

    public AccountController(LtlhelpContext context, ILogger<AccountController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Account/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin email và mật khẩu.");
                return View();
            }

            // Kiểm tra username/email có tồn tại trong bảng Users hay không, bao gồm Roles
            var user = await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => (u.Email == email || u.UserName == email) && u.IsActive != false);

            if (user == null)
            {
                ModelState.AddModelError("", "Email/Username hoặc mật khẩu không đúng.");
                return View();
            }

            // Tạm thời chỉ kiểm tra user tồn tại, chưa kiểm tra password (sẽ làm sau)
            // Trong thực tế, cần hash password và so sánh với PasswordHash

            // Cập nhật LastLoginAt
            user.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Lấy RoleId đầu tiên của user (thường user chỉ có 1 role)
            var firstRole = user.Roles?.FirstOrDefault();
            var roleId = firstRole?.RoleId ?? 2; // Mặc định là role 2 (user thường) nếu không có role

            // Lưu thông tin user vào session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetInt32("RoleId", roleId);

            // Chuyển hướng theo RoleId
            if (roleId == 1)
            {
                // RoleId = 1: Admin -> chuyển sang trang Admin
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            else
            {
                // RoleId = 2 hoặc khác: User thường -> chuyển về trang chủ
                return RedirectToAction("Index", "Home");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            ModelState.AddModelError("", "Đã xảy ra lỗi khi đăng nhập. Vui lòng thử lại.");
            return View();
        }
    }

    // GET: Account/Register
    public IActionResult Register()
    {
        return View();
    }

    // POST: Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string name, string email, string phone, string password, string confirm_password)
    {
        try
        {
            // Kiểm tra validation cơ bản
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || 
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirm_password))
            {
                ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin.");
                return View();
            }

            if (password != confirm_password)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                return View();
            }

            // Kiểm tra email đã tồn tại chưa
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email || u.UserName == email);

            if (existingUser != null)
            {
                ModelState.AddModelError("", "Email hoặc Username đã được sử dụng.");
                return View();
            }

            // Tạo User mới
            var newUser = new User
            {
                UserName = email.Split('@')[0], // Tạm thời dùng phần trước @ làm username
                Email = email,
                PasswordHash = password, // Tạm thời lưu plain text (sẽ hash sau)
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Add(newUser);
            await _context.SaveChangesAsync();

            // Gán role mặc định (RoleId = 2) cho user mới
            var defaultRole = await _context.Roles.FindAsync(2);
            if (defaultRole != null)
            {
                newUser.Roles.Add(defaultRole);
                await _context.SaveChangesAsync();
            }

            // Tạo UserProfile nếu cần
            if (!string.IsNullOrWhiteSpace(phone))
            {
                var userProfile = new UserProfile
                {
                    UserId = newUser.UserId,
                    FirstName = name,
                    Phone = phone
                };
                _context.Add(userProfile);
                await _context.SaveChangesAsync();
            }

            // Chuyển hướng sang trang Login
            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            ModelState.AddModelError("", "Đã xảy ra lỗi khi đăng ký. Vui lòng thử lại.");
            return View();
        }
    }

    // GET: Account/MyAccount
    public async Task<IActionResult> MyAccount()
    {
        try
        {
            // Lấy userId từ session
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                // Nếu chưa đăng nhập, redirect về login
                return RedirectToAction(nameof(Login));
            }

            var user = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                // Nếu không tìm thấy user, redirect về login
                return RedirectToAction(nameof(Login));
            }

            // Lấy donations trực tiếp từ bảng Donations (đồng bộ với DonationController.History)
            // Lấy donations có UserId khớp HOẶC DonorEmail khớp với email của user
            string? userEmail = user.Email;
            
            // Tự động cập nhật UserId cho các donations cũ có DonorEmail khớp nhưng UserId null
            if (!string.IsNullOrEmpty(userEmail))
            {
                var donationsToUpdate = await _context.Donations
                    .Where(d => d.UserId == null && d.DonorEmail != null && d.DonorEmail.ToLower() == userEmail.ToLower())
                    .ToListAsync();
                
                foreach (var donation in donationsToUpdate)
                {
                    donation.UserId = userId.Value;
                }
                
                if (donationsToUpdate.Any())
                {
                    await _context.SaveChangesAsync();
                }
            }
            
            // Chỉ lấy donations có UserId khớp với user hiện tại (đồng bộ với DonationController.History)
            var donations = await _context.Donations
                .Where(d => d.UserId == userId.Value)
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

            // Tính tổng số liệu donation
            var totalDonated = donations.Sum(d => d.Amount);
            var totalDonations = donations.Count;
            // Sửa logic: check status "Xác nhận" (theo dữ liệu thực tế trong database)
            var confirmedAmount = donations
                .Where(d => d.Status == "Xác nhận" || d.Status == "Thành công" || d.Status == "Đã thanh toán")
                .Sum(d => d.Amount);

            ViewBag.TotalDonated = totalDonated;
            ViewBag.TotalDonations = totalDonations;
            ViewBag.ConfirmedAmount = confirmedAmount;
            ViewBag.Donations = donations; // Truyền donations qua ViewBag để view sử dụng

            return View(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading my account");
            return RedirectToAction(nameof(Login));
        }
    }

    // GET: Account/Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}

