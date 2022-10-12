using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities
{
    [Index(nameof(CompanyId))]
    public class Customer : IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(256)]
        public string Email { get; set; }
        [MaxLength(30)]
        public string PhoneNumber { get; set; }

        [MaxLength(256)]
        public string BusinessName { get; set; }
        [MaxLength(50)]
        public string BusinessType { get; set; }
        [MaxLength(100)]
        public string OperatingSector { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountReceived { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountOwed { get; set; }

        [MaxLength(256)]
        public string Gender { get; set; }

        [MaxLength(256)]
        public string TIN { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPurchases { get; set; }

        public DateTime? LastTransactionDate { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
