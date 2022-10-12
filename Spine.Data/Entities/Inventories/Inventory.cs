using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Inventories
{
    [Index(nameof(CompanyId))]
    public class Inventory : IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        /// <summary>
        /// inventory items that are accessories will have this set
        /// </summary>
        public Guid? ParentInventoryId { get; set; }

        public InventoryType InventoryType { get; set; }


        [MaxLength(256)]
        public string Name { get; set; }
        public Guid? CategoryId { get; set; }
        public int QuantityInStock { get; set; }
        public DateTime InventoryDate { get; set; }
        public DateTime LastRestockDate { get; set; }
        /// <summary>
        /// ISBN
        /// </summary>
        [MaxLength(50)]
        public string SerialNo { get; set; }

        /// <summary>
        /// Stock Keeping Unit
        /// </summary>
        [MaxLength(256)]
        public string SKU { get; set; }

        public int ReorderLevel { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }
        public int? MeasurementUnitId { get; set; }
        public InventoryStatus Status { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCostPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitSalesPrice { get; set; }
        public bool NotifyOutOfStock { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
