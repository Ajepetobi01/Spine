using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities.Invoices
{
    [Index(nameof(CompanyId))]
    public class InvoiceNoSetting : IEntity, ICompany, IAuditable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        [MaxLength(5)]
        public string Prefix { get; set; }
        [MaxLength(2)]
        public string Separator { get; set; }

        public int LastGenerated { get; set; }
        public DateTime LastGeneratedDate { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }

    }
}
