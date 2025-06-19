//using Microsoft.AspNetCore.Mvc;
//using TaskManagementSystem.MVC.Models;
//using System.Collections.Generic;

//namespace TaskManagementSystem.MVC.Controllers
//{
//    public class DashboardController : Controller
//    {
//        public IActionResult Index()
//        {
//            // في تطبيقك الحقيقي هنا يتم استدعاء الـ API
//            var model = new DashboardViewModel
//            {
//                StatusCounts = new List<StatusCount>
//                {
//                    new StatusCount { Status = "Pending", Count = 5 },
//                    new StatusCount { Status = "InProgress", Count = 7 },
//                    new StatusCount { Status = "Completed", Count = 12 },
//                    new StatusCount { Status = "Overdue", Count = 3 }
//                },
//                AverageDuration = new AverageDuration
//                {
//                    AverageMinutes = 35.7
//                },
//                UserPerformances = new List<UserPerformance>
//                {
//                    new UserPerformance { UserName = "أحمد", Total = 10, CompletedOnTime = 7, OverdueCount = 3 },
//                    new UserPerformance { UserName = "سارة", Total = 8, CompletedOnTime = 8, OverdueCount = 0 }
//                }
//            };

//            return View(model);
//        }
//    }
//}

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TaskManagementSystem.MVC.Helpers;
using TaskManagementSystem.MVC.Models;

namespace TaskManagementSystem.MVC.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApiClientFactory _apiClientFactory;

        public DashboardController(ApiClientFactory apiClientFactory)
        {
            _apiClientFactory = apiClientFactory;
        }
        [RequireLogin]
        public async Task<IActionResult> Index()
        {
            var client = _apiClientFactory.CreateClient();

            // جلب عدد المهام حسب الحالة
            var statusResp = await client.GetAsync("Task/stats/status-counts");
            List<StatusCount> statusCounts = new List<StatusCount>();
            if (statusResp.IsSuccessStatusCode)
            {
                var json = await statusResp.Content.ReadAsStringAsync();
                statusCounts = JsonConvert.DeserializeObject<List<StatusCount>>(json);
            }

            // جلب متوسط زمن التنفيذ
            var avgResp = await client.GetAsync("Task/stats/average-duration");
            AverageDuration avgDur = new AverageDuration { AverageMinutes = 0 };
            if (avgResp.IsSuccessStatusCode)
            {
                var json = await avgResp.Content.ReadAsStringAsync();
                avgDur = JsonConvert.DeserializeObject<AverageDuration>(json);
            }

            // جلب أداء الموظفين
            var perfResp = await client.GetAsync("Task/stats/user-performance");
            List<UserPerformance> userPerformances = new List<UserPerformance>();
            if (perfResp.IsSuccessStatusCode)
            {
                var json = await perfResp.Content.ReadAsStringAsync();
                userPerformances = JsonConvert.DeserializeObject<List<UserPerformance>>(json);
            }

            var vm = new DashboardViewModel
            {
                StatusCounts = statusCounts,
                AverageDuration = avgDur,
                UserPerformances = userPerformances
            };
            return View(vm);
        }
    }
}
