using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using TaskManagementSystem.MVC.Models;

namespace TaskManagementSystem.MVC.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _contextAccessor;

        public NotificationsController(IHttpClientFactory factory, IHttpContextAccessor accessor)
        {
            _httpClientFactory = factory;
            _contextAccessor = accessor;
        }

        public async Task<IActionResult> Index()
        {
            var token = _contextAccessor.HttpContext.Session.GetString("JWToken");
            var userId = int.Parse(_contextAccessor.HttpContext.Session.GetString("UserId"));

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"https://localhost:7172/api/notifications/user/{userId}");
            if (!response.IsSuccessStatusCode) return View(new List<NotificationViewModel>());

            var json = await response.Content.ReadAsStringAsync();
            var notifications = JsonConvert.DeserializeObject<List<NotificationViewModel>>(json);

            // تعليم كمقروءة
            await client.PostAsync($"https://localhost:7172/api/notifications/mark-as-read/{userId}", null);

            return View(notifications);
        }
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userIdStr = _contextAccessor.HttpContext.Session.GetString("UserId");

                if (string.IsNullOrEmpty(userIdStr))
                {
                    return RedirectToAction("Index", "Home"); // أو أي صفحة مناسبة
                }

                var userId = int.Parse(userIdStr);
                var client = _httpClientFactory.CreateClient();

                var response = await client.PostAsync($"https://localhost:7172/api/notifications/mark-as-read/{userId}", null);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "تم تحديد كل الإشعارات كمقروءة.";
                }
                else
                {
                    TempData["Error"] = "حدث خطأ أثناء تحديد الإشعارات كمقروءة.";
                }
            }
            catch
            {
                TempData["Error"] = "خطأ داخلي.";
            }

            return RedirectToAction("Index", "Notifications");
        }
    }
}
