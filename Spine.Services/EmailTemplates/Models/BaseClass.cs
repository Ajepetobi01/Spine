namespace Spine.Services.EmailTemplates.Models
{
    public interface ITemplateModel
    {
    }

    public class BaseClass
    {
        private readonly string appName = "Spine";

        public string AppName => appName;
        public string Name { get; set; }

        public string CompanyName { get; set; }

    }
}
