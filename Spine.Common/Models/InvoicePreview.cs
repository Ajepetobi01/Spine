using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Spine.Common.Enums;

namespace Spine.Common.Models
{
    public class InvoicePreview
    {
        public string Recipient { get; set; }
        public string RecipientAddress1 { get; set; }
        public string RecipientAddress2 { get; set; }
        public string RecipientState { get; set; }
        public string RecipientCountry { get; set; }

        public string Subject { get; set; }
        public string InvoiceNo { get; set; }
        public string Notes { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }

        public decimal SubTotal { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceDue { get; set; }

        public DiscountType DiscountType { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountRate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        public string TaxLabel { get; set; }
        public decimal TaxRate { get; set; }
        public string PaymentLink { get; set; }
        public CompanyModel Business { get; set; }
        public PreferenceModel Preference { get; set; }
        public CustomizationModel Customization { get; set; }
        public List<NewLineItem> LineItems { get; set; }
    }
    public class PreferenceModel
    {
        public DiscountSettings Discount { get; set; }
        public TaxSettings Tax { get; set; }
        public ApplyTaxSettings ApplyTax { get; set; }
        public CurrencyModel Currency { get; set; }
        public bool EnableDueDate { get; set; }
        public string PaymentTerms { get; set; }
        public string ShareMessage { get; set; }
        public bool PaymentLinkEnabled { get; set; }
    }

    public class CustomizationModel
    {
        public bool LogoEnabled { get; set; }
        public bool SignatureEnabled { get; set; }

        public string Banner { get; set; }
        public string CompanyLogo { get; set; }
        public string Signature { get; set; }
        public string Theme { get; set; }
        public string SignatureName { get; set; }
    }

    public class CompanyModel
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
    }

    public class NewLineItem
    {
        public string Item { get; set; }
        public string Description { get; set; }

        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Rate { get; set; }
        public DiscountType DiscountType { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountRate { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string TaxLabel { get; set; }
        public decimal TaxRate { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }
    }
}
