using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using static Subscription.Api.SwaggerSetup;

namespace Subscription.Api
{
    public static class ServiceCollectionExtension
    {
        public static void RegisterSwagger(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSwaggerGen(c =>
            {
                // doing this to ignore JsonIgnored properties from FromQuery. (serialization doesn't take place using FromQuery, so it's not ignored by default
                c.OperationFilter<RemoveJsonIgnoreFromQueryOperationFilter>();

                //use fully qualified object names
                c.CustomSchemaIds(x => x.FullName);

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Subscription Microservice",
                    Description = "",
                    TermsOfService = new Uri("http://tempuri.org/terms"),
                    Contact = new OpenApiContact()
                    {
                        Name = "",
                        Email = "",
                        //  Url = new Uri("")
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Bearer {your token}",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {   new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    new string[] {}}
                });

            });
        }

    }
}
