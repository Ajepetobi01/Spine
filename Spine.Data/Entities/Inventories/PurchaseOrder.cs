using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Inventories
{
    [Index(nameof(CompanyId))]
    public class PurchaseOrder : IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public Guid? VendorId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDate { get; set; }

        [MaxLength(256)]
        public string VendorName { get; set; }

        [MaxLength(256)]
        public string VendorEmail { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderAmount { get; set; }

        [MaxLength(500)]
        public string AdditionalNote { get; set; }

        public PurchaseOrderStatus Status { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
        
        [MaxLength(50)]
        public string OrderNo { get; set; }
        
        public BilledStatus BilledStatus { get; set; }
    }
}
