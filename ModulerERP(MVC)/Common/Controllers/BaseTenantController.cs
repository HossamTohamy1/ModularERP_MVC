using Microsoft.AspNetCore.Mvc;

namespace ModulerERP_MVC_.Common.Controllers
{
    public class BaseTenantController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
