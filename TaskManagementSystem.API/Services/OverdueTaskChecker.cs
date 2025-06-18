using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.API.Hubs;
using TaskManagementSystem.API.Models;

namespace TaskManagementSystem.API.Services
{
    public class OverdueTaskChecker : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OverdueTaskChecker> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public OverdueTaskChecker(IServiceScopeFactory scopeFactory, ILogger<OverdueTaskChecker> logger, IHubContext<NotificationHub> hubContext)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _hubContext = hubContext;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // فحص كل ساعة بدءًا فوريًا
            _timer = new Timer(CheckOverdueTasks, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        private void CheckOverdueTasks(object state)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var now = DateTime.UtcNow;
            var overdueTasks = context.TaskItems
                .Where(t => t.DueDate.HasValue
                            && t.DueDate.Value < now
                            && t.Status != "Completed"
                            && t.Status != "Overdue")
                .ToList();

            if (overdueTasks.Any())
            {
                foreach (var task in overdueTasks)
                {
                    task.Status = "Overdue";
                    context.Entry(task).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _logger.LogInformation($"Task {task.Id} marked as Overdue.");

                    // إرسال إشعار عبر SignalR:
                    // نفترض أن العميل قد سجّل نفسه في مجموعة باسم "User_{AssignedToUserId}"
                    var groupName = $"User_{task.AssignedToUserId}";
                    _hubContext.Clients.Group(groupName)
                        .SendAsync("ReceiveNotification", new
                        {
                            taskId = task.Id,
                            message = $"المهمة \"{task.Title}\" أصبحت متأخرة."
                        });
                }
                context.SaveChanges();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
