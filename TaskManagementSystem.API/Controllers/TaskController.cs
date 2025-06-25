using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.API.Hubs;
using TaskManagementSystem.API.Models;

namespace TaskManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    [AllowAnonymous]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TaskController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public TaskController(AppDbContext context, ILogger<TaskController> logger, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

       

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks()
        {
            var data = await _context.TaskItems
                .Include(t => t.AssignedTo)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority,
                    System = t.System,
                    CreatedAt = t.CreatedAt,
                    DueDate = t.DueDate,
                    Status = t.Status,
                    AssignedToUserId = t.AssignedToUserId,
                    AssignedToName = t.AssignedTo.Name
                })
                .ToListAsync();
            return Ok(data);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTaskItem(int id)
        {
            var t = await _context.TaskItems.Include(x => x.AssignedTo)
                .FirstOrDefaultAsync(ti => ti.Id == id);
            if (t == null) return NotFound();
            return new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Priority = t.Priority,
                System = t.System,
                CreatedAt = t.CreatedAt,
                DueDate = t.DueDate,
                Status = t.Status,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToName = t.AssignedTo.Name
            };
        }

        

        public class TaskDto
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Priority { get; set; }
            public string System { get; set; }
            public DateTime CreatedAt { get; set; }

            public DateTime? DueDate { get; set; }
            public string Status { get; set; }
            public int AssignedToUserId { get; set; }
            public string AssignedToName { get; set; }
        }


        // 1. أضف DTO جديد لقبول بيانات إنشاء المهمة فقط
        public class CreateTaskModel
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Priority { get; set; }
            public string System { get; set; }
            public DateTime? DueDate { get; set; }
            public int AssignedToUserId { get; set; }
        }
        // POST: api/Tasks
        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskModel model)
        {
            _logger.LogInformation("CreateTask called with: {@model}", model);

            // التحقق من البيانات الأساسية
            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest("Title is required.");
            // يمكن إضافة تحقق ModelState آخر حسب الحاجة

            // تحقق أن المستخدم المُعيّن موجود
            var assignedUser = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == model.AssignedToUserId);
            if (assignedUser == null)
                return BadRequest($"AssignedToUserId {model.AssignedToUserId} not found.");

            // إنشاء الكيان
            var taskItem = new TaskItem
            {
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                System = model.System,
                DueDate = model.DueDate,
                AssignedToUserId = model.AssignedToUserId,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.TaskItems.Add(taskItem);
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Task saved with Id {Id}", taskItem.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Task");
                return BadRequest("خطأ في حفظ المهمة: " + ex.Message);
            }

            // إعداد DTO للإرجاع
            var taskDto = new TaskDto
            {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Description = taskItem.Description,
                Priority = taskItem.Priority,
                System = taskItem.System,
                DueDate = taskItem.DueDate,
                Status = taskItem.Status,
                CreatedAt = taskItem.CreatedAt,
                AssignedToUserId = taskItem.AssignedToUserId,
                AssignedToName = assignedUser.Name
            };

            // إنشاء إشعار للمستخدم المعين
            try
            {
                // مثال: نرسل إشعارًا للمستخدم المعين ومجموعة الإداريين/المدراء
                // أولاً: لمستخدم المعين
                var notifForAssigned = new Notification
                {
                    Title = $"مهمة جديدة: {taskItem.Title}",
                    Content = $"تم تعيين مهمة جديدة لك: {taskItem.Title}",
                    UserId = assignedUser.Id,
                    TaskId = taskItem.Id,
                    Type = NotificationType.TaskAssigned,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Notifications.Add(notifForAssigned);

                // ثم لمجموعة المدراء والـAdmins
                var adminsAndManagers = await _context.Users
                    .Where(u => u.Role == "Manager" || u.Role == "Admin")
                    .ToListAsync();

                foreach (var recipient in adminsAndManagers)
                {
                    var notif = new Notification
                    {
                        Title = $"مهمة جديدة: {taskItem.Title}",
                        Content = $"تم إنشاء مهمة جديدة بعنوان '{taskItem.Title}'",
                        UserId = recipient.Id,
                        TaskId = taskItem.Id,
                        Type = NotificationType.TaskAssigned,
                        CreatedDate = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notif);
                    await _context.SaveChangesAsync();

                    // إرسال فوري عبر SignalR إلى المجموعة المقابلة للمستخدم
                    // نفترض أن الـHub يربط كل مستخدم إلى مجموعة باسمه أو بمعرفه
                    await _hubContext.Clients.Group(recipient.Id.ToString())
                        .SendAsync("ReceiveNotification", new
                        {
                            userId = recipient.Id,
                            title = notif.Title,
                            content = notif.Content,
                            createdAt = notif.CreatedDate.ToString("g")
                        });
                }

                // أيضاً إرسال إشعار للمستخدم المعين عبر SignalR
                await _hubContext.Clients.Group(assignedUser.Id.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        userId = assignedUser.Id,
                        title = notifForAssigned.Title,
                        content = notifForAssigned.Content,
                        createdAt = notifForAssigned.CreatedDate.ToString("g")
                    });

                // حفظ جميع الإشعارات في قاعدة البيانات
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating or sending notifications");
                // لا نوقف العملية الأساسية لإرجاع الـTaskDto، بل نسجل الخطأ
            }

            // إرجاع CREATED مع DTO لتجنّب دورات المراجع
            return CreatedAtAction(nameof(GetTaskItem), new { id = taskDto.Id }, taskDto);
        }
       
        public class UpdateTaskModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Priority { get; set; }
            public string System { get; set; }
            public DateTime? DueDate { get; set; }
            public string Status { get; set; }
            public int AssignedToUserId { get; set; }
        }

        //public async Task<IActionResult> UpdateTask(int id,[FromBody] UpdateTaskModel updatedTask)

        [HttpPut("{id}")]
        public async Task<ActionResult<TaskItem>> UpdateTask(int id, [FromBody] UpdateTaskModel updatedTask)
        {
            if (id != updatedTask.Id)
                return BadRequest("المعرف لا يتطابق مع الجسم المرسل.");

            var existing = await _context.TaskItems.FindAsync(id);
            if (existing == null) return NotFound("المهمة غير موجودة.");

            // تحديث الحقول
            existing.Title = updatedTask.Title;
            existing.Description = updatedTask.Description;
            existing.Priority = updatedTask.Priority;
            existing.System = updatedTask.System;
            existing.DueDate = updatedTask.DueDate;

            // تحديث الحالة + حساب وقت الإكمال
            if (existing.Status != updatedTask.Status)
            {
                existing.Status = updatedTask.Status;
                if (updatedTask.Status == "Completed")
                {
                    existing.CompletedAt = DateTime.UtcNow;
                }
            }

            existing.AssignedToUserId = updatedTask.AssignedToUserId;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // DELETE: api/Task/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
                return NotFound();

            _context.TaskItems.Remove(taskItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        // GET: api/Task/stats/status-counts
        [HttpGet("stats/status-counts")]
        public async Task<ActionResult<IEnumerable<object>>> GetStatusCounts()
        {
            var data = await _context.TaskItems
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            return Ok(data);
        }

        // GET: api/Task/stats/average-duration
        [HttpGet("stats/average-duration")]
        public async Task<ActionResult<object>> GetAverageDuration()
        {
            var durations = await _context.TaskItems
                .Where(t => t.Status == "Completed" && t.CompletedAt.HasValue)
                .Select(t => EF.Functions.DateDiffMinute(t.CreatedAt, t.CompletedAt.Value))
                .ToListAsync();

            double avg = durations.Any() ? durations.Average() : 0;
            return Ok(new { AverageMinutes = avg });
        }

        // GET: api/Task/stats/user-performance
        [HttpGet("stats/user-performance")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserPerformance()
        {
            var data = await _context.Users
                .Select(u => new
                {
                    UserName = u.Name,
                    Total = u.Tasks.Count(),
                    CompletedOnTime = u.Tasks
                        .Count(t => t.Status == "Completed" && t.CompletedAt.HasValue
                                    && t.DueDate.HasValue && t.CompletedAt <= t.DueDate),
                    OverdueCount = u.Tasks.Count(t => t.Status == "Overdue")
                })
                .ToListAsync();
            return Ok(data);
        }
        // GET: api/Task/stats/recent-tasks
        [HttpGet("stats/recent-tasks")]
        public async Task<ActionResult<IEnumerable<RecentTaskDto>>> GetRecentTasks()
        {
            var tasks = await _context.TaskItems
                .Include(t => t.AssignedTo)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .Select(t => new RecentTaskDto
                {
                    Title = t.Title,
                    AssignedToName = t.AssignedTo.Name,
                    CreatedAt = t.CreatedAt,
                    Status = t.Status
                })
                .ToListAsync();

            return Ok(tasks);
        }
        // GET: api/Task/{id}/comments
       
        [HttpGet("{id}/comments")]
        [AllowAnonymous] // أو حسب صلاحياتك
        public async Task<ActionResult<IEnumerable<TaskCommentDto>>> GetComments(int id)
        {
            var comments = await _context.TaskComments
               .Where(c => c.TaskItemId == id)
                .Select(c => new TaskCommentDto
                {
                    Id = c.Id,
                    CommentText = c.CommentText,
                    CreatedAt = c.CreatedAt,
                    TaskItemId = c.TaskItemId,
                    UserId = c.UserId,
                    UserName = c.User.Name
                })
                .ToListAsync();

            return Ok(comments);
        }

        // DTO لاستقبال بيانات التعليق الجديد
        public class CreateCommentModel
        {
            public int TaskId { get; set; }
            public string CommentText { get; set; }
            public int UserId { get; set; }
        }

        // POST: api/Task/{id}/comments
        [HttpPost("AddComment")]
        [AllowAnonymous]  // أو حسب صلاحياتك
        public async Task<IActionResult> AddComment([FromBody] CreateCommentModel request)
        {
            if (string.IsNullOrWhiteSpace(request.CommentText))
                return BadRequest("نص التعليق فارغ.");

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return BadRequest("المستخدم غير موجود.");

            var comment = new TaskComment
            {
                TaskItemId = request.TaskId,
                CommentText = request.CommentText.Trim(),
                CreatedAt = DateTime.UtcNow,
                UserId = request.UserId
            };

            _context.TaskComments.Add(comment);
            await _context.SaveChangesAsync();

            // بعد حفظ التعليق بنجاح مثلاً
            var task = await _context.TaskItems
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == request.TaskId);

            var adminsAndManagers = await _context.Users
                .Where(u => u.Role == "Manager" || u.Role == "Admin")
                .ToListAsync();

            foreach (var recipient in adminsAndManagers)
            {
                var notification = new Notification
                {
                    Title = $"تعليق جديد على المهمة: {task.Title}",
                    Content = $"قام {user.Name} بإضافة تعليق: {request.CommentText}",
                    UserId = recipient.Id,
                    TaskId = task.Id,
                    Type = NotificationType.TaskComment
                };

                _context.Notifications.Add(notification);

                // إرسال الإشعار مباشرةً بعد إنشائه User
                await _hubContext.Clients.Group(recipient.Id.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        userId = recipient.Id,
                        title = notification.Title,
                        content = notification.Content,
                        createdAt = notification.CreatedDate.ToString("g")
                    });
            }

            // حفظ كل الإشعارات في النهاية
            await _context.SaveChangesAsync();
            // الخاصة بالاشعارات


            return Ok(new
            {
                id = comment.Id,
                commentText = comment.CommentText,
                createdAt = comment.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                userName = user.Name
            });
        }



    }


    public class TaskCommentDto
    {
        public int Id { get; set; }
        public string CommentText { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TaskItemId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public class RecentTaskDto
    {
        public string Title { get; set; }
        public string AssignedToName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
    }

}



