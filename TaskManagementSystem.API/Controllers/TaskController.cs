using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.API.Data;
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
        public TaskController(AppDbContext context, ILogger<TaskController> logger)
        {
            _context = context;
            _logger = logger;
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

        // GET: api/Task
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        //{
        //    return await _context.TaskItems
        //        .Include(t => t.AssignedTo)
        //        .Include(t => t.Comments)
        //        .ToListAsync();
        //}

        //// GET: api/Task/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<TaskItem>> GetTaskItem(int id)
        //{
        //    var taskItem = await _context.TaskItems
        //        .Include(t => t.AssignedTo)
        //        .Include(t => t.Comments)
        //        .FirstOrDefaultAsync(t => t.Id == id);

        //    if (taskItem == null)
        //        return NotFound();

        //    return taskItem;
        //}
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

        // 2. عدّل الميثود POST في TaskController لاستخدام CreateTaskModel بدلاً من TaskItem
        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask([FromBody] CreateTaskModel model)
        {
            _logger.LogInformation("CreateTask called with: {@model}", model);

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

            return CreatedAtAction(nameof(GetTaskItem), new { id = taskItem.Id }, taskItem);
        }


        //[HttpPost]
        //public async Task<ActionResult<TaskItem>> CreateTask([FromBody] TaskItem taskItem)
        //{
        //    _logger.LogInformation("CreateTask called with: {@taskItem}", taskItem);
        //    taskItem.CreatedAt = DateTime.UtcNow;
        //    taskItem.Status = "Pending";
        //    _context.TaskItems.Add(taskItem);
        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //        _logger.LogInformation("Task saved with Id {Id}", taskItem.Id);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error saving Task");
        //        return BadRequest("خطأ في حفظ المهمة: " + ex.Message);
        //    }
        //    return CreatedAtAction(nameof(GetTaskItem), new { id = taskItem.Id }, taskItem);
        //}

        // POST: api/Task
        //[HttpPost]
        //public async Task<ActionResult<TaskItem>> CreateTask([FromBody] TaskItem taskItem)
        //{
        //    taskItem.CreatedAt = DateTime.UtcNow;
        //    taskItem.Status = "Pending";
        //    _context.TaskItems.Add(taskItem);
        //    await _context.SaveChangesAsync();
        //    return CreatedAtAction(nameof(GetTaskItem), new { id = taskItem.Id }, taskItem);
        //}
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

        // PUT: api/Task/5
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateTask1(int id, [FromBody] TaskItem updatedTask)
        //{
        //    if (id != updatedTask.Id)
        //        return BadRequest();

        //    var existing = await _context.TaskItems.FindAsync(id);
        //    if (existing == null) return NotFound();

        //    // تحديث الحقول المسموح بها
        //    existing.Title = updatedTask.Title;
        //    existing.Description = updatedTask.Description;
        //    existing.Priority = updatedTask.Priority;
        //    existing.System = updatedTask.System;
        //    existing.DueDate = updatedTask.DueDate;

        //    // عند تغيير الحالة:
        //    if (existing.Status != updatedTask.Status)
        //    {
        //        existing.Status = updatedTask.Status;
        //        if (updatedTask.Status == "Completed")
        //        {
        //            existing.CompletedAt = DateTime.UtcNow;
        //        }
        //        // إذا أردت تغيير إلى Overdue من الواجهة يدوياً، أو الخدمة الخلفية تفعل ذلك.
        //    }

        //    existing.AssignedToUserId = updatedTask.AssignedToUserId;

        //    _context.Entry(existing).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();
        //    return NoContent();
        //}

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

    }
}



