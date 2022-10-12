using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Invoices.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spine.Core.Inventories.Jobs;

namespace Inventory.Api
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

            services.AddControllers()
                // .AddNewtonsoftJson(c =>
                // {
                //     c.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                //     c.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                //     c.SerializerSettings.DateFormatString = "dd-MM-yyyyTHH:mm:ss";
                // })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });

            services.AddHttpContextAccessor();
            services.RegisterJwtTokenAuthentication(Configuration);
            services.RegisterSwagger();
            services.RegisterOtherServices(Configuration);
            
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
            app.UseMiddleware<ExceptionHandlerMiddleware>(); // use custom exceptionhandler middleware
            if (!env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "Inventory Api v1"));
            }
            app.UseHangfireDashboard("/jobs", new DashboardOptions
            {
                Authorization = new[] { new HangfireCustomBasicAuthenticationFilter { User = "dev", Pass = "Spine" } }
            });

            app.UseHangfireServer();

            var every6Hours = "0 0 0/6 1/1 * ? *";
            RecurringJob.AddOrUpdate<StockLevelNotificationHandler>(x => x.Handle(new StockLevelNotificationCommand(), CancellationToken.None), Cron.Daily(7, 0), TimeZoneInfo.Local);

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
