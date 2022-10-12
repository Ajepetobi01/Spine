using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spine.Data.Entities.Admin
{
    [Table("OfferPromotion")]
    public class OfferPromotion
    {
        [Key]
        public Guid Id { get; set; }
        public bool EnablePromotion { get; set; }
        [Column(TypeName = "decimal(18,8)")]
        public decimal Percentage { get; set; }
        [MaxLength(20)]
        public string PromotionCode { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
