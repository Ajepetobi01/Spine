using System;

namespace Spine.Services.EmailTemplates.Models
{
    public class SendPurchaseOrder : BaseClass, ITemplateModel
    {
        public string Recipient { get; set; }
        public string CompanyEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpecedDtae { get; set; }
    }

}
