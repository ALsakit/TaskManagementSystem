namespace TaskManagementSystem.MVC.Models
{
    public class TaskCommentDto
    {
        public int Id { get; set; }
        public string CommentText { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TaskItemId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public class TaskDetailsViewModel
    {
        public TaskViewModel Task { get; set; }
        public List<TaskCommentDto> Comments { get; set; }
        public int CurrentUserId { get; set; }
    }
}
