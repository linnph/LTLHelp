using LTLHelp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LTLHelp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly LtlhelpContext _context;

        public AccountController(LtlhelpContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    u.PasswordHash == password &&   // 🔥 trùng với SQL
                    u.IsActive == true);

            if (user == null)
            {
                ViewBag.Error = "Sai email hoặc mật khẩu!";
                return View();
            }

            // Lưu session
            HttpContext.Session.SetInt32("AdminUserId", user.UserId);
            HttpContext.Session.SetString("AdminUserName", user.UserName);

            // cập nhật thời gian đăng nhập
            user.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
    }
}
