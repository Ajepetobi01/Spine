using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Vendor
{
    [Index(nameof(CompanyId))]
    public class Vendor : IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }
        [MaxLength(256)]
        public string BusinessName { get; set; }
        [MaxLength(256)]
        public string Email { get; set; }
        [MaxLength(30)]
        public string PhoneNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountReceived { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountOwed { get; set; }

        [MaxLength(256)]
        public string Gender { get; set; }

        public DateTime? LastTransactionDate { get; set; }

        public string DisplayName { get; set; }
        public string OperatingSector { get; set; }
        public string RcNumber { get; set; }
        public string Website { get; set; }
        public string TIN { get; set; }
        
        public TypeOfVendor VendorType { get; set; }
        public Status Status { get; set; }
        
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
