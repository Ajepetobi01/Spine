using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities
{
    [Index(nameof(CompanyId), nameof(ParentItemId))]
    public class LineItem : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        /// <summary>
        ///  can be invoiceId, purchaseOrderId
        /// </summary>
        public Guid ParentItemId { get; set; }

        /// <summary>
        /// InventoryId for Invoice and PO
        /// </summary>
        public Guid? ItemId { get; set; }

        [MaxLength(256)]
        public string Item { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Rate { get; set; }

        public DiscountType DiscountType { get; set; }

        [Range(0, 100)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountRate { get; set; }

        /// <summary>
        /// final line item amount, after tax and discount has been applied
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(256)]
        public string TaxLabel { get; set; }
        [Range(0, 100)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxRate { get; set; }
        
        public Guid? TaxId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }
        
        public DateTime CreatedOn { get; set; }
        
    }
}
