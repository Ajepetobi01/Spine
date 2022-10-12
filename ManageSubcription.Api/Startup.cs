using Hangfire;
using Hangfire.SqlServer;
using HangfireBasicAuthenticationFilter;
using ManageSubcription.Api.Authorizations;
using ManageSubcription.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Spine.Core.ManageSubcription.Interface;
using Spine.Core.ManageSubcription.Services;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Services;
using System;
using System.Globalization;

namespace ManageSubcription.Api
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
            // Add Hangfire services.
            services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection")));
            services.AddHangfireServer();

            //services.AddControllers();
            services.AddControllers()
                   .AddNewtonsoftJson(c =>
                   {
                       c.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                       c.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                       c.SerializerSettings.DateFormatString = "dd/MM/yyyy hh:mm:ss";
                   })
                   .ConfigureApiBehaviorOptions(options =>
                   {
                       options.SuppressModelStateInvalidFilter = true;
                   });


            //services.AddTransient<JwtSettings>();
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
            })
           .AddEntityFrameworkStores<SpineContext>()
           .AddDefaultTokenProviders();
            //services.AddSingleton<IUriService, UriService>();

            services.AddHttpContextAccessor();
            services.RegisterJwtTokenAuthentication(Configuration);
            services.RegisterSwagger();
            services.RegisterOtherServices(Configuration);

            services.AddSingleton<IUriService>(provider =>
            {
                var accessor = provider.GetRequiredService<IHttpContextAccessor>();
                var request = accessor.HttpContext.Request;
                var absoluteUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), "/");
                return new UriService(absoluteUri);
            });
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var cultureInfo = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "ManageSubcription.Api v1"));
            
            app.UseHangfireDashboard("/jobs", new DashboardOptions
            {
                Authorization = new[] { new HangfireCustomBasicAuthenticationFilter { User = "dev", Pass = "Spine" } }
            });

            app.UseHangfireServer();
            RecurringJob.AddOrUpdate<NotificationRepository>(x => x.AlmostExpirySubscription(), Cron.Daily(), TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate<NotificationRepository>(x => x.DisabledExpirySubscription(), Cron.Daily(), TimeZoneInfo.Local);

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
