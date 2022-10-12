namespace Spine.Services.EmailTemplates.Models
{
    public class AcceptInvite : BaseClass, ITemplateModel
    {
        public string BusinessName { get; set; }
        public string Date { get; set; }
        public string ActionLink { get; set; }
    }
}
