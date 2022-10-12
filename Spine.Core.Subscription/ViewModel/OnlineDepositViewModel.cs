using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.Subscription.ViewModel
{
    public class OnlineDepositViewModel
    {
        public Guid CompanyId { get; set; }
        public int PlanId { get; set; }
        public string TransactionRef { get; set; }
        public string PaymentMethod { get; set; }
        public string Amount { get; set; }
    }
}
