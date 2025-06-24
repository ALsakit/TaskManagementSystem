namespace TaskManagementSystem.MVC.Models
{
    public class NotificationViewModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; } // أو NotificationType إذا تستخدم Enum

    }
}
