using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities.Invoices
{
    [Index(nameof(CompanyId))]
    public class TaxType : IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        [MaxLength(256)]
        public string Tax { get; set; }

        [Range(0, 100)]
        public double TaxRate { get; set; }

        public bool IsVAT { get; set; }
        public bool IsCompound { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public Guid LedgerAccountId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }

    }
}
