using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.MVC.Models;
using System.Collections.Generic;

namespace TaskManagementSystem.MVC.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            // في تطبيقك الحقيقي هنا يتم استدعاء الـ API
            var model = new DashboardViewModel
            {
                StatusCounts = new List<StatusCount>
                {
                    new StatusCount { Status = "Pending", Count = 5 },
                    new StatusCount { Status = "InProgress", Count = 7 },
                    new StatusCount { Status = "Completed", Count = 12 },
                    new StatusCount { Status = "Overdue", Count = 3 }
                },
                AverageDuration = new AverageDuration
                {
                    AverageMinutes = 35.7
                },
                UserPerformances = new List<UserPerformance>
                {
                    new UserPerformance { UserName = "أحمد", Total = 10, CompletedOnTime = 7, OverdueCount = 3 },
                    new UserPerformance { UserName = "سارة", Total = 8, CompletedOnTime = 8, OverdueCount = 0 }
                }
            };

            return View(model);
        }
    }
}
