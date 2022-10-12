using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Spine.Services.Hubs;

namespace Spine.Services
{
    //sends the notifications 
    public interface INotificationHelper
    {
        Task SendToAll(string message);
        Task SendToSingleUser(Guid userId, string message);
        Task SendToMultiUser(List<string> userIds, string message);
    }

    public class NotificationHelper : INotificationHelper
    {
        private readonly IHubContext<BroadcastHub> _hubContext;
        public NotificationHelper(IHubContext<BroadcastHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToAll(string message)
        {
            await _hubContext.Clients.All.SendAsync("Notification", message);
        }

        public async Task SendToSingleUser(Guid userId, string message)
        {
            await _hubContext.Clients.User(userId.ToString()).SendAsync("Notification", message);
        }

        public async Task SendToMultiUser(List<string> userIds, string message)
        {
            await _hubContext.Clients.Users(userIds).SendAsync("Notification", message);
        }

    }
}
