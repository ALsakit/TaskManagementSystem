using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.API.Models;

namespace TaskManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPost("mark-as-read/{userId}")]
        public async Task<IActionResult> MarkAsRead(int userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] Notification model)
        {
            model.CreatedDate = DateTime.Now;
            model.IsRead = false;

            _context.Notifications.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }
        [HttpGet("notifications")]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPost("notifications/readall")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var notifs = _context.Notifications.Where(n => n.UserId == userId && !n.IsRead);

            foreach (var n in notifs)
            {
                n.IsRead = true;
                n.ReadDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
