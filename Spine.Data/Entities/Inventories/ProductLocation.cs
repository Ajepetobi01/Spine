using System;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities.Inventories
{
    [Index(nameof(CompanyId), nameof(InventoryId), nameof(LocationId))]
    public class ProductLocation : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public Guid InventoryId { get; set; }

        public Guid LocationId { get; set; }
        public int QuantityInStock { get; set; }

        public DateTime DateAdded { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
