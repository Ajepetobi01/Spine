using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Inventories
{
    [Index(nameof(CompanyId))]
    public class InventoryCategory : IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }
        public Status Status { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        
        public bool IsServiceCategory { get; set; }
        public Guid? DeletedBy { get; set; }
        
        /// <summary>
        /// credited when product in category is sold
        /// </summary>
        public Guid? SalesAccountId { get; set; }
        /// <summary>
        /// debited when product in category is purchased
        /// </summary>
        public Guid? InventoryAccountId { get; set; }
        /// <summary>
        /// debited when product in category is sold
        /// </summary>
        public Guid? CostOfSalesAccountId { get; set; }
        
        public bool ApplyTaxOnPO { get; set; }
    }
}
