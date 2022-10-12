using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities.Inventories
{
    [Index(nameof(CompanyId), nameof(InventoryId))]
    public class InventoryPriceHistory : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public Guid InventoryId { get; set; }
        
        public DateTime RestockDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCostPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitSalesPrice { get; set; }
        [MaxLength(500)]
        public string Description { get; set; }
        [MaxLength(256)]
        public string Reason { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }

        public Guid? GoodsReceivedId { get; set; }

    }
}
