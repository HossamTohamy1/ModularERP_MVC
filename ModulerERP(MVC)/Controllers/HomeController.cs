using Microsoft.AspNetCore.Mvc;

namespace ModulerERP_MVC_.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Home/About
        public IActionResult About()
        {
            ViewData["Message"] = "This is the About page.";
            return View();
        }

        // GET: /Home/Contact
        public IActionResult Contact()
        {
            return View();
        }
    }
}