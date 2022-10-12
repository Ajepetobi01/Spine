using System.Text.Json;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spine.Data.Documents;
using Spine.Data.Documents.Service;
using Spine.Data.Documents.Service.Interfaces;

namespace Invoices.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString("JobsConnection")));
            services.AddHangfireServer();
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
            })
                 .ConfigureApiBehaviorOptions(options =>
                 {
                     options.SuppressModelStateInvalidFilter = true;
                 });

            services.AddHttpContextAccessor();
            services.RegisterJwtTokenAuthentication(Configuration);
            services.RegisterSwagger();
            services.RegisterOtherServices(Configuration);
            
            //added this to be able to access teh invoice customization base 64 without another API call
            services.AddDbContext<UploadsDbContext>(x =>
                x.UseSqlServer(Configuration.GetConnectionString("DocumentConnection"))); // reads connection string from config file

            services.AddScoped<IInvoiceCustomizationService, InvoiceCustomizationService>();
            
            var credential = GoogleCredential.FromFile("firebase-spine.json") //;
                .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
            
            FirebaseApp.Create(new AppOptions()  
            {
                Credential = credential  
            }); 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ExceptionHandlerMiddleware>(); // use custom exception handler middleware
            if (!env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "Invoices Api v1"));
            }

            app.UseHangfireDashboard("/jobs", new DashboardOptions
            {
                Authorization = new[] { new HangfireCustomBasicAuthenticationFilter { User = "dev", Pass = "Spine" } }
            });

            app.UseHangfireServer();

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
