using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spine.Data.Entities.Subscription
{
    [Table("Plan")]
    public class Plan
    {
        [Key]
        public int PlanId { get; set; }
        [Required]
        [MaxLength(50)]
        public string PlanName { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public int? PlanDuration { get; set; }
        [MaxLength(100)]
        public string Description { get; set; }
        public bool IsFreePlan { get; set; }
        public bool IncludePromotion { get; set; }
        public bool? Status { get; set; }
    }
}
