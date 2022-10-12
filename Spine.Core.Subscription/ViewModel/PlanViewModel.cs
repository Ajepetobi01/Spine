using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.Subscription.ViewModel
{
    public class PlanViewModel
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public decimal Amount { get; set; }
        public int? PlanDuration { get; set; }
        public bool IsFreePlan { get; set; }
        public string Description { get; set; }
        public string IncludePromotion { get; set; }
        public string Status { get; set; }
    }
}
