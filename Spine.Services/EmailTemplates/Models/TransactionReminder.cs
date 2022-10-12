using System;

namespace Spine.Services.EmailTemplates.Models
{
    public class TransactionReminder : BaseClass, ITemplateModel
    {
        public string Description { get; set; }
        public string TransRef { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransDate { get; set; }
        public DateTime ReminderDate { get; set; }
    }

}
