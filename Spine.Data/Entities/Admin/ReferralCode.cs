using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spine.Data.Entities.Admin
{
    [Table("ReferralCode")]
    public class ReferralCode
    {
        [Key]
        public Guid Id { get; set; }
        public bool Status { get; set; }
        [Column(TypeName = "decimal(18,8)")]
        public decimal Percentage { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
