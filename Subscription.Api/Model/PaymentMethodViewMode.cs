using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Subscription.Api.Model
{
    public class PaymentMethodViewMode
    {
        public Guid CompanyId { get; set; }
        public int PlanId { get; set; }
        public string method { get; set; }
        public string Amount { get; set; }
        public Guid? PromoCodeId { get; set; }
        [JsonIgnore]
        public Guid? ReferralCodeId { get; set; }
    }
}
