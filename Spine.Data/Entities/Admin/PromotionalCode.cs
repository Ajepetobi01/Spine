using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Data.Entities.Admin
{
    [Table("PromotionalCode")]
    public class PromotionalCode
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string PromoCode { get; set; }
        public Guid UserId { get; set; }
        public DateTime DateCreated { get; set; }
        public decimal PercentageOffer { get; set; }
        public bool IsUsed { get; set; }
        public int PlanId { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal AmountAfterDistcount { get; set; }
        [MaxLength(50)]
        public string TransactionRef { get; set; }
    }
}
