using System;

namespace Spine.Services.EmailTemplates.Models
{
    public class CustomerReminder : BaseClass, ITemplateModel
    {
        public string Description { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime ReminderDate { get; set; }
    }

}
