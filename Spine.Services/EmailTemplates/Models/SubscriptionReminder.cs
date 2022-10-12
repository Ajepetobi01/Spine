using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Services.EmailTemplates.Models
{
    public class SubscriptionReminder : BaseClass, ITemplateModel
    {
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string Plan { get; set; }
        public string Duration { get; set; }
        public string SubscriptionDate { get; set; }
        public string Message { get; set; }
    }
}
