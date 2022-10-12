using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.ViewModel
{
    public class NotificationParam
    {
        public Guid CompanyId { get; set; }
        public Guid ReminderId { get; set; }
    }

    public class SubscriptionNotifications
    {
        public int IdSubscription { get; set; }
        public Guid IdCompany { get; set; }
    }
}
