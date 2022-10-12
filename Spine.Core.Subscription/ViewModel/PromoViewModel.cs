using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.Subscription.ViewModel
{
    public class PromoViewModel
    {
        public Guid Id { get; set; }
        public string PromotionCode { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedOn { get; set; }
        public string DateCreated { get; set; }
    }
}
