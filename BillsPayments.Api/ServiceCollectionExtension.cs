using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper.Internal;
using BillsPayments.Api.Authorizations;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Spine.Common.Models;
using Spine.Data;
using Spine.Services;
using Spine.Services.HttpClients;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BillsPayments.Api
{
    public static class ServiceCollectionExtension
    {

        public static void RegisterOtherServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SpineContext>(x =>
           x.UseSqlServer(configuration.GetConnectionString("SpineConnection"))); // reads connection string from config file

            services.AddCors(c =>
            {
                c.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            services.AddScoped<IAuditLogHelper, AuditLogHelper>();

            services.RegisterFluentEmail(configuration);
            services.AddScoped<IEmailSender, EmailSender>();

            services.AddScoped<InterswitchClient>();
            services.AddHttpClient<InterswitchClient>();

            services.AddMediatR(AppDomain.CurrentDomain.Load("Spine.Core.BillsPayments"));
            services.AddAutoMapper(AppDomain.CurrentDomain.Load("Spine.Core.BillsPayments"));
            
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["RedisServerUrl"];
            });
            
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        }

        public static IServiceCollection RegisterJwtTokenAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JWT"));

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(x =>
                {
                    x.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"])),
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidIssuer = configuration["JWT:Issuer"],
                        ValidAudience = configuration["JWT:Audience"],
                        RequireExpirationTime = true,
                        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                        ClockSkew = TimeSpan.Zero,
                    };
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.Events = new JwtBearerEvents()
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";

                            // Ensure we always have an error and error description.
                            if (string.IsNullOrEmpty(context.Error))
                                context.Error = "invalid_token";
                            if (string.IsNullOrEmpty(context.ErrorDescription))
                                context.ErrorDescription = "This request requires a valid JWT access token to be provided";

                            // Add some extra context for expired tokens.
                            var isExpired = false;
                            if (context.AuthenticateFailure != null && context.AuthenticateFailure.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                var authenticationException = context.AuthenticateFailure as SecurityTokenExpiredException;
                                context.Response.Headers.Add("x-token-expired", authenticationException.Expires.ToString("o"));
                                context.ErrorDescription = $"The token expired on {authenticationException.Expires.ToString("o")}";
                                isExpired = true;
                            }

                            return context.Response.WriteAsync(JsonSerializer.Serialize(new
                            {
                                ErrorMessage = context.ErrorDescription,
                                IsExpired = isExpired
                            }));
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = 403;
                            context.Response.ContentType = "application/json";
                            return context.Response.WriteAsync(JsonSerializer.Serialize(new
                            {
                                ErrorMessage = "You do not have the permission to access this resource"
                            }));
                        },
                    };
                });

            return services;
        }

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
                    Title = "Bills Payments Microservice",
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

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

    }

    public class RemoveJsonIgnoreFromQueryOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription == null || operation.Parameters == null)
                return;

            if (!context.ApiDescription.ParameterDescriptions.Any())
                return;

            context.ApiDescription.ParameterDescriptions.Where(p => p.Source.Equals(BindingSource.Form)
                        && p.CustomAttributes().Any(p => p.GetType().Equals(typeof(JsonIgnoreAttribute))))
                .ForAll(p => operation.RequestBody.Content.Values.Single(v => v.Schema.Properties.Remove(p.Name)));

            context.ApiDescription.ParameterDescriptions.Where(p => p.Source.Equals(BindingSource.Query)
                          && p.CustomAttributes().Any(p => p.GetType().Equals(typeof(JsonIgnoreAttribute))))
                .ForAll(p => operation.Parameters.Remove(operation.Parameters.Single(w => w.Name.Equals(p.Name))));

        }
    }

}
