using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities.Invoices
{
    [Index(nameof(CompanyId), 
        nameof(BannerImageId), nameof(ColorThemeId), nameof(LogoImageId), nameof(SignatureImageId))]
    public class InvoiceCustomization : IEntity, ICompany, IAuditable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public bool LogoEnabled { get; set; }
        public bool SignatureEnabled { get; set; }

        public Guid? BannerImageId { get; set; }
        public Guid? LogoImageId { get; set; }
        public Guid? SignatureImageId { get; set; }
        public Guid? ColorThemeId { get; set; }

        [MaxLength(256)]
        public string SignatureName { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
    }
}
