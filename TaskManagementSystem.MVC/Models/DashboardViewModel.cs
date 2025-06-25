using Newtonsoft.Json;
using System.Collections.Generic;

namespace TaskManagementSystem.MVC.Models
{
    public class StatusCount
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }

    public class AverageDuration
    {
        public double AverageMinutes { get; set; }
    }

    public class UserPerformance
    {
        public string UserName { get; set; }
        public int Total { get; set; }
        public int CompletedOnTime { get; set; }
        public int OverdueCount { get; set; }
    }

    public class DashboardViewModel
    {
        public List<StatusCount> StatusCounts { get; set; }
        public AverageDuration AverageDuration { get; set; }
        public List<UserPerformance> UserPerformances { get; set; }

        public List<TaskItem> RecentTasks { get; set; }

        // إضافات لسهولة الربط في الصفحة الرئيسية
        public int NewTasks => StatusCounts?.FirstOrDefault(s => s.Status == "Pending")?.Count ?? 0;
        public int InProgressTasks => StatusCounts?.FirstOrDefault(s => s.Status == "InProgress")?.Count ?? 0;
        public int CompletedTasks => StatusCounts?.FirstOrDefault(s => s.Status == "Completed")?.Count ?? 0;
        public int OverdueTasks => StatusCounts?.FirstOrDefault(s => s.Status == "Overdue")?.Count ?? 0;

    }
    public class TaskItem
    {
        public string Title { get; set; }
        [JsonProperty("AssignedToName")]
        public string AssignedTo { get; set; }
        [JsonProperty("CreatedAt")]
        public DateTime AssignedAt { get; set; }
        public string Status { get; set; }
    }

}
