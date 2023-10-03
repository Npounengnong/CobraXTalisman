using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    public class DemoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
