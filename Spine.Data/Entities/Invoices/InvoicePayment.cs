using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Invoices
{
    [Index(nameof(CompanyId), nameof(InvoiceId))]
    public class InvoicePayment : IEntity, ICompany, IAuditable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public PaymentMode PaymentSource { get; set; }

        public Guid InvoiceId { get; set; }
        public bool IsPartPayment { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }

        public Guid? PaymentIntegrationId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal BankCharges { get; set; }
        [MaxLength(50)]
        public string ReferenceNo { get; set; }
        [MaxLength(50)]
        public string UserReferenceNo { get; set; }
        
        [MaxLength(2000)]
        public string Notes { get; set; }
        public Guid? BankAccountId { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
    }
}
