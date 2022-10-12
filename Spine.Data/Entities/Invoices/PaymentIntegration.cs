using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Invoices
{
    [Index(nameof(CompanyId))]
    public class PaymentIntegration : IEntity, ICompany, IAuditable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public PaymentIntegrationProvider IntegrationProvider { get; set; }
        public PaymentIntegrationType IntegrationType { get; set; }

        [MaxLength(256)]
        public string SubaccountCode { get; set; }
        [MaxLength(256)]
        public string RecipientCode { get; set; }
        [MaxLength(256)]
        public string BusinessName { get; set; }
        [MaxLength(2000)]
        public string Description { get; set; }
        [MaxLength(256)]
        public string PrimaryContactName { get; set; }
        [MaxLength(256)]
        public string PrimaryContactEmail { get; set; }
        [MaxLength(256)]
        public string PrimaryContactPhone { get; set; }
        [MaxLength(20)]
        public string SettlementBankCode { get; set; }
        [MaxLength(256)]
        public string SettlementBankName { get; set; }
        [MaxLength(10)]
        public string SettlementBankCurrency { get; set; }
        [MaxLength(256)]
        public string SettlementAccountNumber { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }

    }
}
