using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Spine.Core.Subscription.ViewModel
{
    public class PromoCodeViewModel
    {
        public Guid CompanyId { get; set; }
        public int PlanId { get; set; }
        public string PromoCode { get; set; }
    }
    public class Response
    {
        public Guid CompanyId { get; set; }
        public int PlanId { get; set; }
        public decimal Amount { get; set; }
        public Guid? ReferralCodeId { get; set; }
        public Guid? PromoCodeId { get; set; }
        [JsonIgnore]
        public bool IsSaved { get; set; }
    }
    public class ReferralViewModel
    {
        public Guid CompanyId { get; set; }
        public int PlanId { get; set; }
        public string ReferralCode { get; set; }
    }
}
