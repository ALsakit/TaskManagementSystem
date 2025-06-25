using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using TaskManagementSystem.MVC.Helpers;
using TaskManagementSystem.MVC.Models;

namespace TaskManagementSystem.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApiClientFactory _apiClientFactory;

        public HomeController(ILogger<HomeController> logger, ApiClientFactory apiClientFactory)
        {
            _logger = logger;
            _apiClientFactory = apiClientFactory;
        }
        [RequireLogin]
        public async Task<IActionResult> Index()
        {
            var client = _apiClientFactory.CreateClient();

            // المهام حسب الحالة
            var statusResp = await client.GetAsync("Task/stats/status-counts");
            List<StatusCount> statusCounts = new();
            if (statusResp.IsSuccessStatusCode)
            {
                var json = await statusResp.Content.ReadAsStringAsync();
                statusCounts = JsonConvert.DeserializeObject<List<StatusCount>>(json);
            }

            // متوسط الزمن
            var avgResp = await client.GetAsync("Task/stats/average-duration");
            AverageDuration avgDur = new();
            if (avgResp.IsSuccessStatusCode)
            {
                var json = await avgResp.Content.ReadAsStringAsync();
                avgDur = JsonConvert.DeserializeObject<AverageDuration>(json);
            }

            // أداء الموظفين
            var perfResp = await client.GetAsync("Task/stats/user-performance");
            List<UserPerformance> userPerformances = new();
            if (perfResp.IsSuccessStatusCode)
            {
                var json = await perfResp.Content.ReadAsStringAsync();
                userPerformances = JsonConvert.DeserializeObject<List<UserPerformance>>(json);
            }

            // المهام الأخيرة
            var recentResp = await client.GetAsync("Task/stats/recent-tasks");
            //            List<TaskItem> recentTasks = new List<TaskItem>
            //{
            //    new TaskItem { Title = "تطوير الواجهة", AssignedTo = "سارة أحمد", AssignedAt = DateTime.Now.AddHours(-2), Status = "Pending" },
            //    new TaskItem { Title = "إصلاح الخادم", AssignedTo = "محمد علي", AssignedAt = DateTime.Now.AddDays(-1), Status = "InProgress" }
            //};

            List<TaskItem> recentTasks = new();
            if (recentResp.IsSuccessStatusCode)
            {
                var json = await recentResp.Content.ReadAsStringAsync();
                recentTasks = JsonConvert.DeserializeObject<List<TaskItem>>(json);
            }

            var model = new DashboardViewModel
            {
                StatusCounts = statusCounts,
                AverageDuration = avgDur,
                UserPerformances = userPerformances,
                RecentTasks = recentTasks
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
