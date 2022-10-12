using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.Subscription.ViewModel
{
    public class CompanySubscriptionDTO
    {
        public int Id_Subscription { get; set; }
        public int Id_Plan { get; set; }
        public string SubscriptionDate { get; set; }
        public string ExpiredDate { get; set; }
        public bool PaymentStatus { get; set; }
        public bool IsActive { get; set; }
        public decimal Amount { get; set; }
    }
}
