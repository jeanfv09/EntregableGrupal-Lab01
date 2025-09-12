using Microsoft.AspNetCore.Mvc;

namespace Lab01_Grupo1.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Powerbi()
            {
                return View();
            }

    }
}
