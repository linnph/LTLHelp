using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LTLHelp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminBaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetInt32("AdminUserId") == null)
            {
                context.Result = new RedirectToActionResult(
                    "Login",     // Action
                    "Account",   // Controller
                    new { area = "Admin" }
                );
            }

            base.OnActionExecuting(context);
        }
    }
}
