using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementSystem.API.Models
{
    /// <summary>
    /// نموذج الإشعار - Notification Model
    /// يمثل الإشعارات المرسلة للمستخدمين
    /// Represents notifications sent to users
    /// </summary>
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان الإشعار مطلوب - Notification title is required")]
        [StringLength(200, ErrorMessage = "عنوان الإشعار لا يجب أن يتجاوز 200 حرف - Notification title should not exceed 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "محتوى الإشعار مطلوب - Notification content is required")]
        [StringLength(1000, ErrorMessage = "محتوى الإشعار لا يجب أن يتجاوز 1000 حرف - Notification content should not exceed 1000 characters")]
        public string Content { get; set; }

        [Required(ErrorMessage = "نوع الإشعار مطلوب - Notification type is required")]
        public NotificationType Type { get; set; }

        [Required(ErrorMessage = "تاريخ الإنشاء مطلوب - Creation date is required")]
        public DateTime CreatedDate { get; set; }

        public DateTime? ReadDate { get; set; }

        public bool IsRead { get; set; }


        public int? TaskId { get; set; }
        public int? UserId { get; set; }

        [ForeignKey(nameof(TaskId))]
        public virtual TaskItem Task { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        public Notification()
        {
            CreatedDate = DateTime.Now;
            IsRead = false;
        }
    }

    /// <summary>
    /// تعداد نوع الإشعار - Notification Type Enumeration
    /// </summary>
    public enum NotificationType
    {
        TaskAssigned = 1,    // مهمة مُعيَّنة
        TaskUpdated = 2,     // مهمة محدثة
        TaskCompleted = 3,   // مهمة مكتملة
        TaskOverdue = 4,     // مهمة متأخرة
        TaskComment = 5,     // تعليق على مهمة
        System = 6           // إشعار نظام
    }
}

