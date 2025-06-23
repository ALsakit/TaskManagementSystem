using Microsoft.AspNetCore.Mvc;

namespace TaskManagementSystem.MVC.Controllers
{
    public class NotificationsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
