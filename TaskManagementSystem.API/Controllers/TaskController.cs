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

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Task
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            return await _context.TaskItems
                .Include(t => t.AssignedTo)
                .Include(t => t.Comments)
                .ToListAsync();
        }

        // GET: api/Task/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTaskItem(int id)
        {
            var taskItem = await _context.TaskItems
                .Include(t => t.AssignedTo)
                .Include(t => t.Comments)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskItem == null)
                return NotFound();

            return taskItem;
        }

        // POST: api/Task
        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask([FromBody] TaskItem taskItem)
        {
            taskItem.CreatedAt = DateTime.UtcNow;
            taskItem.Status = "Pending";
            _context.TaskItems.Add(taskItem);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTaskItem), new { id = taskItem.Id }, taskItem);
        }

        // PUT: api/Task/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskItem updatedTask)
        {
            if (id != updatedTask.Id)
                return BadRequest();

            var existing = await _context.TaskItems.FindAsync(id);
            if (existing == null) return NotFound();

            // تحديث الحقول المسموح بها
            existing.Title = updatedTask.Title;
            existing.Description = updatedTask.Description;
            existing.Priority = updatedTask.Priority;
            existing.System = updatedTask.System;
            existing.DueDate = updatedTask.DueDate;

            // عند تغيير الحالة:
            if (existing.Status != updatedTask.Status)
            {
                existing.Status = updatedTask.Status;
                if (updatedTask.Status == "Completed")
                {
                    existing.CompletedAt = DateTime.UtcNow;
                }
                // إذا أردت تغيير إلى Overdue من الواجهة يدوياً، أو الخدمة الخلفية تفعل ذلك.
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

    }
}
