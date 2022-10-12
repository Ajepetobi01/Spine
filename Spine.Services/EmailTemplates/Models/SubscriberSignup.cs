using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Services.EmailTemplates.Models
{
    public class SubscriberSignup : BaseClass, ITemplateModel
    {
        public string Date { get; set; }
        public string ActionLink { get; set; }
        public string UserName { get; set; }
        public string ReferralCode { get; set; }
        public string Password { get; set; }
    }
}
