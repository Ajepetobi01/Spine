using System;

namespace Spine.Services.EmailTemplates.Models
{
    public class SendInvoice : BaseClass, ITemplateModel
    {
        public string CompanyLogo { get; set; }
        public string Recipient { get; set; }
        public string InvoiceCreator { get; set; }
        public string InvoiceCreatorEmail { get; set; }
        public string Subject { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool EnableDueDate { get; set; }
        public bool EnablePaymentLink { get; set; }
        public string PaymentLink { get; set; }
    }

}
