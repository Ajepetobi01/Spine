using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.Subscription.ViewModel
{
    public class CompanySubscriptionViewModel
    {
        public int ID_Subscription { get; set; }
        public Guid ID_Company { get; set; }
        public int ID_Plan { get; set; }
        public string TransactionRef { get; set; }
        public decimal Amount { get; set; }
        public bool Status { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }

    
}
