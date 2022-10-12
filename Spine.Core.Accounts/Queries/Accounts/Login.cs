using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Helpers;
using Spine.Common.Models;
using Spine.Core.Accounts.Helpers;
using Spine.Core.Accounts.Jobs;
using Spine.Core.Subscription.ViewModel;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Services;
using Spine.Services.EmailTemplates.Models;

namespace Spine.Core.Accounts.Queries.Accounts
{
    public static class Login
    {
        public class Query : IRequest<Response>
        {
            [Required(ErrorMessage = "Email address is required")]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Password is required")]
            public string Password { get; set; }
            
            public string DeviceId { get; set; }
            
            [JsonIgnore]
            public string IpAddress { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Guid UserId { get; set; }
            public Guid CompanyId { get; set; }
            public string Username { get; set; }
            public string CompanyLogoId { get; set; }
            public bool EmailConfirmed { get; set; }
            public bool Require2FA { get; set; }
            public CompanySubscriptionDTO Subscription { get; set; }//CompanySubscriptionDTO
            public string Referralcode { get; set; }
            public string Token { get; set; }
            
           // [JsonIgnore] // refresh token is returned in http only cookie
            public string RefreshToken { get; set; }
            
            public DateTime? RefreshTokenExpiration { get; set; }

            public Response()
            {
                Status = HttpStatusCode.OK;
            }

            public Response(string message)
            {
                ErrorMessage = message;
                Status = HttpStatusCode.BadRequest;
            }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly JwtSettings _jwtSettings;
            private readonly IEmailSender _emailSender;
            private readonly SpineContext _dbContext;
            private readonly CommandsScheduler _scheduler;

            public Handler(IOptions<JwtSettings> jwtSettings, UserManager<ApplicationUser> userManager, SpineContext dbContext,
                                            IEmailSender emailSender, CommandsScheduler scheduler)
            {
                _emailSender = emailSender;
                _jwtSettings = jwtSettings.Value;
                _userManager = userManager;
                _dbContext = dbContext;
                _scheduler = scheduler;
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email.Trim() || x.PhoneNumber == request.Email);
               // user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null) return new Response($"Invalid credentials");

                if (await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    if (user.IsDeleted) return new Response("Your user account has been deleted, Contact your administrator");
                    if (!user.EmailConfirmed) return new Response($"Your user account has not been verified. Click on the verify link sent to your mail on {user.CreatedOn}");

                    var company = await _dbContext.Companies.Where(x => x.Id == user.CompanyId)
                        .Select(x => new
                        {
                            x.IsDeleted,
                            x.LogoId,
                            x.Ref_ReferralCode
                        }).SingleOrDefaultAsync();

                    if (company == null || company.IsDeleted)
                        return new Response("Your business account has been disabled");

                   

                    var subscription = await _dbContext.CompanySubscriptions.Where(x => x.ID_Company == user.CompanyId && x.ExpiredDate >= DateTime.Now && x.PaymentStatus)
                        .Select(x => new CompanySubscriptionDTO
                        {
                            Id_Subscription = x.ID_Subscription,
                            Id_Plan = x.ID_Plan,
                            SubscriptionDate = x.TransactionDate == null ? null : x.TransactionDate.Value.ToString("dd/MM/yyyy"),
                            ExpiredDate = x.ExpiredDate == null ? null : x.ExpiredDate.Value.ToString("dd/MM/yyyy"),
                            Amount = x.Amount,
                            IsActive = x.IsActive,
                            PaymentStatus = x.PaymentStatus,
                        }).FirstOrDefaultAsync();

                    if (subscription != null)
                    {
                        if (subscription.ExpiredDate != null)
                        {
                            var dt = Convert.ToDateTime( subscription.ExpiredDate);
                            //if (dt < DateTime.Now)
                                if (Convert.ToDateTime(CovertDate(subscription.ExpiredDate)) < DateTime.Now)
                                {
                                //Update CompanySubscriptions set IsActive to false
                                var updateSubscriberStatus = _dbContext.CompanySubscriptions
                                    .Where(x => x.ID_Subscription == subscription.Id_Subscription).FirstOrDefault();
                                updateSubscriberStatus.IsActive = false;
                                await _dbContext.SaveChangesAsync();
                                return new Response
                                {
                                    UserId = user.Id,
                                    CompanyId = user.CompanyId,
                                    EmailConfirmed = user.EmailConfirmed,
                                    Require2FA = user.TwoFactorEnabled,
                                    Username = user.UserName,
                                    CompanyLogoId = company.LogoId,
                                    Referralcode = company.Ref_ReferralCode == null ? string.Empty : company.Ref_ReferralCode,
                                    Subscription = subscription,
                                    Token = "",
                                    RefreshToken = "",
                                    RefreshTokenExpiration = null,
                                };
                            }
                        }
                        if (!subscription.IsActive)
                        {
                            return new Response
                            {
                                UserId = user.Id,
                                CompanyId = user.CompanyId,
                                EmailConfirmed = user.EmailConfirmed,
                                Require2FA = user.TwoFactorEnabled,
                                Username = user.UserName,
                                CompanyLogoId = company.LogoId,
                                Referralcode = company.Ref_ReferralCode == null ? string.Empty : company.Ref_ReferralCode,
                                Subscription = subscription,
                                Token = "",
                                RefreshToken = "",
                                RefreshTokenExpiration = null,
                            };
                        }
                    }

                    var token = "";
                    RefreshToken refreshToken = null;
                    //  var twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
                    if (user.TwoFactorEnabled)
                    {
                        // var tokenProviders = await _userManager.GetValidTwoFactorProvidersAsync(user);
                        //    var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                        var code = await _userManager.GenerateTwoFactorTokenAsync(user, Constants.OtpProvider);

                        var emailModel = new TwoFactorAuthentication
                        {
                            Name = user.FullName,
                            Date = Constants.GetCurrentDateTime().ToLongDateString(),
                            OTP = code
                        };

                        var emailSent = await _emailSender.SendTemplateEmail(user.Email, $"{emailModel.AppName} - Login OTP", EmailTemplateEnum.LoginOTP, emailModel);
                        if (!emailSent)
                        {
                            return new Response("Unable to send login OTP at this time. Please try again..");
                        }
                    }
                    else
                    {
                        var jwtSecurityToken = await JwtHelper.GenerateJWToken(_jwtSettings, user);
                        token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

                        refreshToken = JwtHelper.GenerateRefreshToken(request.IpAddress);
                        refreshToken.UserId = user.Id;
                        _dbContext.RefreshTokens.Add(refreshToken);

                        if (!await _dbContext.DeviceTokens.AnyAsync(x =>
                            x.UserId == user.Id && x.Token == request.DeviceId))
                        {
                            _dbContext.DeviceTokens.Add(new DeviceToken
                            {
                                UserId = user.Id, DateCreated = DateTime.Now,
                                Token = request.DeviceId
                            });
                        }
                        
                        await _dbContext.SaveChangesAsync();
                    }

                    // run job to setup account if it was created from admin
                    if (!await _dbContext.LedgerAccounts.AnyAsync(x => x.CompanyId == user.CompanyId))
                    {
                        _scheduler.SendNow(new SetupAccountCommand()
                            {
                                CompanyId = user.CompanyId,
                            }, $"Set up Accounts for Company {user.CompanyId}");
                    }

                    return new Response
                    {
                        UserId = user.Id,
                        CompanyId = user.CompanyId,
                        EmailConfirmed = user.EmailConfirmed,
                        Require2FA = user.TwoFactorEnabled,
                        Username = user.UserName,
                        CompanyLogoId = company.LogoId,
                        Referralcode = company.Ref_ReferralCode == null ? string.Empty : company.Ref_ReferralCode,
                        Subscription = subscription,
                        Token = token,
                        RefreshToken = refreshToken?.Token,
                        RefreshTokenExpiration = refreshToken?.Expires,
                    };
                }

                return new Response($"Incorrect credentials");
            }
        }

        public static string CovertDate(string getdate)
        {
            try
            {
                string[] dateTokens = getdate.Split('/', '-');
                string strDay = dateTokens[0];
                string strMonth = dateTokens[1];
                string strYear = dateTokens[2];
                string date = strMonth + "/" + strDay + "/" + strYear;

                return date;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}


