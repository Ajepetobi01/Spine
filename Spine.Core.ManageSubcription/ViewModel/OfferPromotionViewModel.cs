using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.ViewModel
{
    public class OfferPromotionViewModel
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string Percentage { get; set; }
        public string PromotionCode { get; set; }
        public string DateCreated { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
    public class CreatePromotionViewModel
    {
        public bool Status { get; set; }
        public decimal Percentage { get; set; }
        public string PromotionCode { get; set; }
    }

    public class PromoViewModel
    {
        public Guid Id { get; set; }
        public string PromotionCode { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedOn { get; set; }
        public string DateCreated { get; set; }
    }
    public class CreatePromoViewModel
    {
        public string PromoCode { get; set; }
    }
}
