using System;
using System.Text.Json;
using System.Threading;
using Accounts.Api.Providers;
using Accounts.Api.Service.Contract;
using Accounts.Api.Service.DTO;
using Accounts.Api.Service.Implementation;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spine.Common.Helpers;
using Spine.Core.Accounts.Jobs;
using Spine.Data;
using Spine.Data.Entities;

namespace Accounts.Api
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
                //options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            })
                 .ConfigureApiBehaviorOptions(options =>
                 {
                     options.SuppressModelStateInvalidFilter = true;
                 });

            //            services.Configure<ApiBehaviorOptions>(options =>
            //            {
            //                options.InvalidModelStateResponseFactory = actionContext =>
            //                {
            //                    var errors = actionContext.ModelState
            //                        .Where(e => e.Value.Errors.Count > 0)
            //                        .Select(e => new Error
            //                        {
            //                            Name = e.Key,
            //                            Message = e.Value.Errors.First().ErrorMessage
            //                        }).ToArray();

            //                    return new BadRequestObjectResult(errors);
            //                }
            //          });

            services.AddHttpContextAccessor();

            services.ConfigureIdentityOptions();
            services.AddIdentity<ApplicationUser, ApplicationRole>(config =>
            {
                config.SignIn.RequireConfirmedEmail = false;
            }).AddEntityFrameworkStores<SpineContext>()
            .AddDefaultTokenProviders()
              //    .AddLoginOtpTokenProviderUsingTOTP();
              .AddLoginOtpTokenProvider()
              .AddTokenProvider<EmailConfirmationTokenProvider<ApplicationUser>>(Constants.EmailConfirmationProvider);

            //I think the normal DataProtectionTokenProvider is a day by default
            //  services.Configure<DataProtectionTokenProviderOptions>(opt => opt.TokenLifespan = TimeSpan.FromHours(2));

            services.Configure<EmailConfirmationTokenProviderOptions>(opt => opt.TokenLifespan = TimeSpan.FromDays(Constants.DaysToDisableAccount));
            
            services.RegisterJwtTokenAuthentication(Configuration);
            services.RegisterSwagger();
            services.RegisterOtherServices(Configuration);

            services.AddHttpClient();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddSingleton<IFacebookAuthService, FacebookAuthService>();

            services.AddScoped<IJwtHandlerService, JwtHandlerService>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
                                IBackgroundJobClient backgroundJob, IRecurringJobManager recurringJob)
        {
            app.UseMiddleware<ExceptionHandlerMiddleware>(); // use custom exceptionhandler middleware
            if (!env.IsProduction())
            {
                //app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "Accounts Api v1"));
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Accounts Api v1"));
            }

            app.UseHangfireDashboard("/jobs", new DashboardOptions
            {
                Authorization = new[] { new HangfireCustomBasicAuthenticationFilter { User = "dev", Pass = "Spine" } }
                //new HangfireAuthorization(new TokenValidationParameters())
            });

            app.UseHangfireServer();

            RecurringJob.AddOrUpdate<DisableDueAccountsHandler>(x => x.Handle(new DisableDueAccountsCommand(), CancellationToken.None), Cron.Daily(), TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate<DailyVerificationReminderHandler>(x => x.Handle(new DailyVerificationReminderCommand(), CancellationToken.None), Cron.Daily(), TimeZoneInfo.Local);
            
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
