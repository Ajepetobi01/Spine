using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spine.Data.Documents;
using Spine.Data.Documents.Service;
using Spine.Data.Documents.Service.Interfaces;

namespace Spine.DocumentService
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
            services.AddControllers()
                 .ConfigureApiBehaviorOptions(options =>
                 {
                     options.SuppressModelStateInvalidFilter = true;
                 });

            services.AddDbContext<UploadsDbContext>(x =>
           x.UseSqlServer(Configuration.GetConnectionString("DocumentConnection"))); // reads connection string from config file

            services.AddCors(c =>
            {
                c.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration["RedisServerUrl"];
            });
            
            services.RegisterJwtTokenAuthentication(Configuration);
            services.RegisterSwagger();

            services.AddScoped<IUploadService, UploadService>();
            services.AddScoped<IInvoiceCustomizationService, InvoiceCustomizationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            if (!env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "Spine Upload Service v1"));
            }

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
