using LTLHelp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LTLHelp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RegisterController : Controller
    {
        private readonly LtlhelpContext _context;

        public RegisterController(LtlhelpContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string email, string password)
        {
            // Kiểm tra tồn tại
            bool exists = await _context.Users.AnyAsync(u =>
                u.UserName == username || u.Email == email);

            if (exists)
            {
                ViewBag.Error = "Tên đăng nhập hoặc Email đã tồn tại!";
                return View();
            }

            var newUser = new User
            {
                UserName = username,
                Email = email,
                PasswordHash = password,   // 🔥 đang dùng mật khẩu dạng plain!
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Tự động login sau khi đăng ký
            HttpContext.Session.SetInt32("AdminUserId", newUser.UserId);
            HttpContext.Session.SetString("AdminUserName", newUser.UserName);

            return RedirectToAction("Index", "Home", new { area = "Admin" });
        }
    }
}
