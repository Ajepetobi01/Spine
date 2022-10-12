namespace Spine.Services.EmailTemplates.Models
{
    public class TwoFactorAuthentication : BaseClass, ITemplateModel
    {
        public string Date { get; set; }
        public string OTP { get; set; }
    }
}
