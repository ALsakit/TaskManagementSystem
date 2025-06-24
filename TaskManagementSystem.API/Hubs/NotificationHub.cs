using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace TaskManagementSystem.API.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext.Request.Query["userId"];

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }

            await base.OnConnectedAsync();
        }
        // يمكن إضافة methods إذا أردت من العميل يرسل.
        // مثلاً: عند الاتصال، يمكن تسجيل المستخدم في مجموعة حسب ID:
        //public override Task OnConnectedAsync()
        //{
        // يفترض أن العميل يرسل اليوزر آي دي بعد الاتصال لتسجيل نفسه في مجموعة:
        //return base.OnConnectedAsync();
        //}

        // مثال: إرسال إشعار إلى مستخدم محدد:
        // سيتم استخدام IHubContext<NotificationHub> في الخدمة الخلفية.
    }
}
