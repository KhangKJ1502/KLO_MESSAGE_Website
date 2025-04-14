using Microsoft.AspNetCore.Mvc;

namespace MessageWeb1.Controllers
{
    public class ScreenQRController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
