using Microsoft.AspNetCore.Mvc;

namespace TodoApp.Controllers
{
    public class ColourThemeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
