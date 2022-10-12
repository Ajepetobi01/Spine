using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Vendor
{
    [Index(nameof(CompanyId), nameof(ReceivedGoodId))]
    public class VendorPayment : IEntity, ICompany, IAuditable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public PaymentMode PaymentSource { get; set; }

        public Guid ReceivedGoodId { get; set; }
        public Guid ReceivedGoodItemId { get; set; }
        public Guid? PurchaseOrderId { get; set; }
        
        [MaxLength(100)]
        public string VendorName { get; set; }
        public Guid? VendorId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? RemainingBalance { get; set; }
        
        public DateTime PaymentDate { get; set; }
        
        [MaxLength(50)]
        public string ReferenceNo { get; set; }
        public Guid? BankAccountId { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
    }
}
