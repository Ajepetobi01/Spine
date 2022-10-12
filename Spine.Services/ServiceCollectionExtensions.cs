using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Spine.Services
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterFluentEmail(this IServiceCollection services, IConfiguration configuration)
        {
            var apiKey = configuration["Email:SendGridApiKey"];
            var displayName = configuration["Email:DisplayName"];
            var senderEmail = configuration["Email:SenderEmail"];

            // Using Razor templating package
            //    Email.DefaultRenderer = new RazorRenderer();

            services.AddFluentEmail(senderEmail, displayName)
             .AddRazorRenderer()
             .AddSendGridSender(apiKey);
            //.AddSmtpSender(new System.Net.Mail.SmtpClient
            //{
            //    Host = "smtp.sendgrid.net",
            //    Port = 587,
            //    //UseDefaultCredentials = true,
            //    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
            //    Credentials = new NetworkCredential("apiKey", apiKey)
            //});
        }
    }
}
