using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Inventories
{
    [Index(nameof(CompanyId), nameof(PurchaseOrderId))]
    public class ReceivedGood : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid? PurchaseOrderId { get; set; }
        public Guid? VendorId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime DateReceived { get; set; }
        public Guid ReceivedBy { get; set; }

        [MaxLength(50)]
        public string GoodReceivedNo { get; set; }

        public DateTime CreatedOn { get; set; }

        public PaymentStatus PaymentStatus { get; set; }
        
        public DateTime? PaymentDueDate { get; set; }
        
    }
}
