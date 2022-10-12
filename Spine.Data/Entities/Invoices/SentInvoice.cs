using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;

namespace Spine.Data.Entities.Invoices
{
    [Index(nameof(CompanyId), nameof(InvoiceId))]
    public class SentInvoice : IEntity, ICompany
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid CustomizationId { get; set; }
        [MaxLength(2000)]
        public string PaymentLink { get; set; }
        [MaxLength(256)]
        public string PaymentLinkReference { get; set; }
        [MaxLength(256)]
        public string PaymentLinkAccessCode { get; set; }
        public DateTime DateSent { get; set; }
    }
}
