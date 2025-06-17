using Microsoft.AspNetCore.Mvc;

namespace TaskManagementSystem.MVC.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
