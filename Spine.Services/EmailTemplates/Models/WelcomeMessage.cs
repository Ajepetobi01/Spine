namespace Spine.Services.EmailTemplates.Models
{
    public class WelcomeMessage : BaseClass, ITemplateModel
    {
        public string UserName { get; set; }
    }
}
