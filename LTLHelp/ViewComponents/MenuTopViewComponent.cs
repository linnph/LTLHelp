using LTLHelp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LTLHelp.ViewComponents
{
    public class MenuTopViewComponent : ViewComponent
    {
        private readonly LtlhelpContext _context;

        public MenuTopViewComponent(LtlhelpContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var menus = _context.TbMenus
                .Where(m => m.IsActive == true && m.IsDeleted == false)
                .OrderBy(m => m.OrderIndex)
                .ToList();

            return View(menus);
        }
    }
}