using Spine.Services.EmailTemplates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.ViewModel
{
    public class SubscriberNotificationViewModel
    {
        public Guid ID_Notification { get; set; }
        public Guid CompanyId { get; set; }
        public string Description { get; set; }
        public bool IsRead { get; set; }
        public string DateCreated { get; set; }
        public string TimeCreated { get; set; }
        public string DateRead { get; set; }
        public string TimeRead { get; set; }
        public string ReminderDate { get; set; }
        public string ReminderTime { get; set; }
    }

    public class NotificationRequest
    {
        public int NotificationId { get; set; }
        public Guid CompanyId { get; set; }
        public string Description { get; set; }
        public string ReminderDate { get; set; }
        public DateTime ReminderTime { get; set; }
    }
}
