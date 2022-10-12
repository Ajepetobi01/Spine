namespace Spine.Common.Models
{
    public class PaymentLinkModel
    {
        public string AuthorizationUrl { get; set; }
        public string AccessCode { get; set; }
        public string Reference { get; set; }
    }
}
