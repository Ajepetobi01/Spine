using System;

namespace Spine.Services.EmailTemplates.Models
{
    public class Notification : BaseClass, ITemplateModel
    {
        public string Description { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime ReminderDate { get; set; }
    }
}
