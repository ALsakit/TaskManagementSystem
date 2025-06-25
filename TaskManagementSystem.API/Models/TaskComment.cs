
using System;

namespace TaskManagementSystem.API.Models
{
    public class TaskComment
    {
        public int Id { get; set; }
        public string CommentText { get; set; }
        public DateTime CreatedAt { get; set; }

        // الربط إلى المهمة
        public int TaskItemId { get; set; }
        public TaskItem TaskItem { get; set; }

        // من أضاف التعليق
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
