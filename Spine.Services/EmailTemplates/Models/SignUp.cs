namespace Spine.Services.EmailTemplates.Models
{
    public class SignUp : BaseClass, ITemplateModel
    {
        public string Date { get; set; }
        public string ActionLink { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
