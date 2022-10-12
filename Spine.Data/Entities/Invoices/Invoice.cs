using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Data.Interfaces;
using Spine.Common.Enums;

namespace Spine.Data.Entities.Invoices
{
    [Index(nameof(CompanyId), nameof(CustomerId))]
    public class Invoice : IEntity, ICompany, IAuditable, IDeletable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid CustomerId { get; set; }

        public int CurrencyId { get; set; }
        public int BaseCurrencyId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal RateToBaseCurrency { get; set; }

        public int InvoiceTypeId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }

        [MaxLength(100)]
        public string Subject { get; set; }

        public InvoiceStatus InvoiceStatus { get; set; }

        [MaxLength(50)]
        public string PhoneNo { get; set; }

        [MaxLength(100)]
        public string InvoiceNoString { get; set; }

        [MaxLength(256)]
        public string CustomerName { get; set; }

        [MaxLength(256)]
        public string CustomerEmail { get; set; }
        [MaxLength(2000)]
        public string CustomerNotes { get; set; }

        public DiscountType DiscountType { get; set; }

        [Range(0, 100)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// this is the sub total of the invoice line items (after the line items discount and tax has been applied) without the invoice discount or tax rate
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal InvoiceAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal InvoiceTotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal InvoiceBalance { get; set; }

        public bool IsRetainer { get; set; }

        public bool IsRecurring { get; set; }
        public InvoiceFrequency RecurringFrequency { get; set; }
        public InvoiceFrequency CustomerReminder { get; set; }
        public DateTime? ReminderTime { get; set; }
        [MaxLength(256)]
        public string TaxLabel { get; set; }
        [Range(0, 100)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxRate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }
        
        public Guid? TaxId { get; set; }
        
        [MaxLength(256)]
        public string BillingAddressLine1 { get; set; }
        [MaxLength(256)]
        public string BillingAddressLine2 { get; set; }
        [MaxLength(256)]
        public string BillingState { get; set; }
        [MaxLength(256)]
        public string BillingCountry { get; set; }
        [MaxLength(20)]
        public string BillingPostalCode { get; set; }

        [MaxLength(256)]
        public string ShippingAddressLine1 { get; set; }
        [MaxLength(256)]
        public string ShippingAddressLine2 { get; set; }
        [MaxLength(256)]
        public string ShippingState { get; set; }
        [MaxLength(256)]
        public string ShippingCountry { get; set; }
        [MaxLength(20)]
        public string ShippingPostalCode { get; set; }

        public DateTime CreatedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? DeletedBy { get; set; }
    }
}
