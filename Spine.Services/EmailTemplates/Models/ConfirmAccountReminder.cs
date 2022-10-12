namespace Spine.Services.EmailTemplates.Models
{
    public class ConfirmAccountReminder : BaseClass, ITemplateModel
    {
        public string Date { get; set; }
        public string ActionLink { get; set; }
    }
}
