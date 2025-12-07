using LTLHelp.Models;
using Harmic.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTLHelp.Utilities;

namespace Harmic.Areas.Admin.Controllers
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
        public IActionResult Index(TbAccount account)
        {
            if (account == null) { return NotFound(); }

            var check = _context.TbAccounts.Where(m => m.Email == account.Email).FirstOrDefault();
            if (check != null)
            {
                Function._Message = "Trùng tài khoản";
                return RedirectToAction("Index", "Register");
            }
            Function._Message = string.Empty;
            account.PasswordHash = HashMD5.GetMD5(account.PasswordHash != null ? account.PasswordHash : "");
            account.RoleId = 3;
            account.IsActive = true;

            _context.Add(account);
            _context.SaveChanges();
            return RedirectToAction("Index", "Login");
        }

    }
}