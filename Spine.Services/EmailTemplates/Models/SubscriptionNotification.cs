using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Services.EmailTemplates.Models
{
    public class SubscriptionNotification : BaseClass, ITemplateModel
    {
        public string Description
        {
            get; set;
        }
    }
}
