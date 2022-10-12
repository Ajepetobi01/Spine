using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities.Inventories
{
    [Index(nameof(CompanyId), nameof(GoodReceivedId))]
    public class ReceivedGoodsLineItem : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        
        public Guid GoodReceivedId { get; set; }
        public Guid? OrderLineItemId { get; set; }

        public Guid InventoryId { get; set; }

        [MaxLength(256)]
        public string Item { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Rate { get; set; }

        /// <summary>
        /// final line item amount, after tax has been applied
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
        public decimal TaxAmount { get; set; }

        public int ReturnedQuantity { get; set; }
        public DateTime CreatedOn { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }
    }
}
