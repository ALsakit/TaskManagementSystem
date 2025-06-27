using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.API.Models;

namespace TaskManagementSystem.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // علاقة التعليقات بالمستخدم
            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(tc => tc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة التعليقات بالمهمة
            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.TaskItem)
                .WithMany(ti => ti.Comments)
                .HasForeignKey(tc => tc.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة المهمة بالمستخدم المعين
            modelBuilder.Entity<TaskItem>()
                .HasOne(ti => ti.AssignedTo)
                .WithMany(u => u.Tasks)
                .HasForeignKey(ti => ti.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);
            // إصلاح علاقة Notification مع TaskItem
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Task)
                .WithMany(t => t.Notifications)
                .HasForeignKey(n => n.TaskId)
                .OnDelete(DeleteBehavior.SetNull); // ✅ يؤكد أن TaskId يمكن تعيينه إلى null

            // إصلاح علاقة Notification مع User
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }


    }
}
