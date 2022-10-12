using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spine.Data.Entities.Subscription
{
    [Table("CompanySubscription")]
    public class CompanySubscription
    {
        [Key]
        public int ID_Subscription { get; set; }
        public Guid ID_Company { get; set; }
        public int ID_Plan { get; set; }
        [MaxLength(50)]
        public string PlanType { get; set; }
        [MaxLength(50)]
        public string TransactionRef { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public bool PaymentStatus { get; set; }
        public bool IsActive { get; set; }
        [MaxLength(20)]
        public string PaymentMethod { get; set; }
        public DateTime? TransactionDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }
}
