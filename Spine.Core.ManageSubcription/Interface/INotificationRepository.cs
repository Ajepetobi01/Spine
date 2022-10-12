using Spine.Core.ManageSubcription.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.Interface
{
    public interface INotificationRepository
    {
        Task<string> ScheduleNotification(NotificationParam model);
        Task<string> AlmostExpirySubscription();
        string DisabledExpirySubscription();
    }
}
