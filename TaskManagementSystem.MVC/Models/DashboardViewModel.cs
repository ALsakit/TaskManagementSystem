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
    }
}
