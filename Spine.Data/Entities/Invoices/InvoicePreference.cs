using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Invoices
{
    [Index(nameof(CompanyId))]
    public class InvoicePreference : IEntity, ICompany, IAuditable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public DiscountSettings Discount { get; set; }
        public TaxSettings Tax { get; set; }
        public ApplyTaxSettings ApplyTax { get; set; }
        public int DueDate { get; set; }

        public int CurrencyId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RateToCompanyBaseCurrency { get; set; }

        public bool RoundAmountToNearestWhole { get; set; }
        public bool EnableDueDate { get; set; }

        public Guid CustomizationId { get; set; }

        public string PaymentTerms { get; set; }
        public string ShareMessage { get; set; }
        public bool PaymentLinkEnabled { get; set; }
        public Guid? PaymentIntegrationId { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }

    }
}
