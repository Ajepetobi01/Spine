using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Accounts.Api.Authorizations;
using Accounts.Api.Providers;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Spine.Common.Helpers;
using Spine.Common.Models;
using Spine.Core.Accounts;
using Spine.Core.Accounts.Helpers;
using Spine.Data;
using Spine.Services;
using static Accounts.Api.SwaggerSetup;

namespace Accounts.Api
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
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IPermissionHelper, PermissionHelper>();

            services.AddScoped<CommandsExecutor>();
            services.AddScoped<CommandsScheduler>();


            services.AddSignalR();
            services.AddTransient<IUserIdProvider, CustomUserIdProvider>();
            services.AddTransient<INotificationHelper, NotificationHelper>();
            services.AddTransient<INotificationService, NotificationServices>();

            services.AddMediatR(AppDomain.CurrentDomain.Load("Spine.Core.Accounts"));
            services.AddAutoMapper(AppDomain.CurrentDomain.Load("Spine.Core.Accounts"));  //AppDomain.CurrentDomain.GetAssemblies()
            services.RegisterFluentEmail(configuration);

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["RedisServerUrl"]; //"54.155.204.222:6379";
            });
            
            //Register the Permission policy handlers
            //    services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        }

        public static IServiceCollection RegisterJwtTokenAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JWT"));
            var audiences = (configuration["JWT:Audiences"] ?? "").Split(new string[] { "," }, StringSplitOptions.None).ToList();
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
                             ValidateAudience = true,
                             ValidateLifetime = true,
                             ValidIssuer = configuration["JWT:Issuer"],
                             ValidAudiences = audiences,
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
                    Title = "Accounts Microservice",
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

        public static void ConfigureIdentityOptions(this IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = false;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                //  options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = Constants.EmailConfirmationProvider;

            });

            //services.Configure<DataProtectionTokenProviderOptions>(opt =>
            //   opt.TokenLifespan = TimeSpan.FromHours(1));

            //services.Configure<ConfirmEmailDataProtectionTokenProviderOptions>(opt =>
            //    opt.TokenLifespan = TimeSpan.FromMinutes(7));

        }

        public static IdentityBuilder AddLoginOtpTokenProviderUsingTOTP(this IdentityBuilder builder)
        {
            var userType = builder.UserType;
            var totpProvider = typeof(OtpTokenProvider<>).MakeGenericType(userType);
            return builder.AddTokenProvider(Constants.OtpProvider, totpProvider);
        }

        public static IdentityBuilder AddLoginOtpTokenProvider(this IdentityBuilder builder)
        {
            var userType = builder.UserType;
            var provider = typeof(LoginOtpTokenProvider<>).MakeGenericType(userType);
            return builder.AddTokenProvider(Constants.OtpProvider, provider);
        }
    }
}
