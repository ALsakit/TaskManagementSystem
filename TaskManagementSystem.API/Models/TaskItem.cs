
using System;
using System.Collections.Generic;

namespace TaskManagementSystem.API.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; } // "Low", "Medium", "High"
        public string System { get; set; }   // النظام المرتبط
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }   // "Pending", "InProgress", "Completed", "Overdue"
        public DateTime? CompletedAt { get; set; } // لحساب المدة عند الإكمال

        // الربط إلى المستخدم المعين
        public int AssignedToUserId { get; set; }
        public User AssignedTo { get; set; }

        public ICollection<TaskComment> Comments { get; set; }
        public ICollection<Notification> Notifications { get; set; }
    }
}
