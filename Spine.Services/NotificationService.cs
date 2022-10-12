using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Services.ViewModels;
using Notification = Spine.Data.Entities.Notification;

namespace Spine.Services
{
    //this iwll be used to handle crud for notifications
    public interface INotificationService
    {
        Task Send(NotificationCategory category, Guid companyId, List<Guid> userIds, Guid entityId, string message, Guid sentBy);
        Task MarkAsRead(Guid companyId, Guid notificationId, Guid userId);
        Task Clear(Guid companyId, List<Guid> notificationIds, Guid userId);

        Task<List<NotificationModel>> GetNotifications(Guid companyId, Guid userId);
        //Task<NotificationModel> GetRecentNotification(Guid companyId, Guid userId);

        Task PushNotification(string token, string title, string body);
        Task PushMultiNotification(List<string> tokens, string title, string body);
    }

    public class NotificationServices : INotificationService
    {
        private readonly SpineContext _context;
        private readonly INotificationHelper _notifyHelper;
        public NotificationServices(SpineContext context, INotificationHelper notifyHelper)
        {
            _context = context;
            _notifyHelper = notifyHelper;
        }

        public async Task Clear(Guid companyId, List<Guid> notificationIds, Guid userId)
        {
            var notifications = await _context.Notifications.Where(x => x.CompanyId == companyId && notificationIds.Contains(x.Id) && !x.IsCleared)
                                                            .ToListAsync();
            if (!notifications.IsNullOrEmpty())
            {
                foreach (var item in notifications)
                {
                    if (item.UserId == userId)
                    {
                        //   item.IsRead = true;
                        item.IsCleared = true;
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<NotificationModel>> GetNotifications(Guid companyId, Guid userId)
        {
            var notifications = await _context.Notifications.Where(x => x.CompanyId == companyId && x.UserId == userId && !x.IsCleared)
                  .Select(x => new NotificationModel
                  {
                      Id = x.Id,
                      EntityId = x.EntityId,
                      IsRead = x.IsRead,
                      Message = x.Message,
                      CreatedOn = x.CreatedOn,
                      Category = x.Category.GetDescription()
                  })
                  .OrderByDescending(x => x.CreatedOn).ToListAsync();

            return notifications;
        }

        public async Task MarkAsRead(Guid companyId, Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications.SingleOrDefaultAsync(x => x.CompanyId == companyId && x.Id == notificationId && !x.IsCleared && !x.IsRead);
            if (notification != null && notification.UserId == userId)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task Send(NotificationCategory category, Guid companyId, List<Guid> userIds, Guid entityId, string message, Guid sentBy)
        {
            var notifications = new List<Notification>();
            foreach (var userId in userIds)
            {
                notifications.Add(new Notification
                {
                    CompanyId = companyId,
                    Id = SequentialGuid.Create(),
                    EntityId = entityId,
                    CreatedBy = sentBy,
                    CreatedOn = Constants.GetCurrentDateTime(),
                    Message = message,
                    UserId = userId,
                    Category = category
                });
            }

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            if (userIds.Count == 1)
            {
                await _notifyHelper.SendToSingleUser(userIds.First(), message);
            }
            else
            {
                var stringList = userIds.ConvertAll(x => x.ToString());
                await _notifyHelper.SendToMultiUser(stringList, message);
            }
        }


        #region Push Notifications

        public Task PushNotification(string token, string title, string body)
        {
            var pusher = FirebaseMessaging.DefaultInstance;
            if (pusher != null)
            {
                return pusher.SendAsync(new Message
                {
                    Token = token,
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Body = body,
                        Title = title
                    }
                });
            }
            
            return Task.CompletedTask;
        }
        
        public Task PushMultiNotification(List<string> tokens, string title, string body)
        {
            var pusher = FirebaseMessaging.DefaultInstance;
            if (pusher != null)
            {
                return pusher.SendMulticastAsync(new MulticastMessage
                {
                    Tokens = tokens,
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Body = body,
                        Title = title
                    }
                });
            }
            
            return Task.CompletedTask;
        }

        #endregion
    }
}
