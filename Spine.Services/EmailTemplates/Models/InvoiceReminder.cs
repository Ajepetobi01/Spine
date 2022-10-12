using System;
using Spine.Common.Models;

namespace Spine.Services.EmailTemplates.Models
{
    public class InvoiceReminder : BaseClass, ITemplateModel
    {
        public CurrencyModel Currency { get; set; }
        public string Subject { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceDue { get; set; }
    }

}
