using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Common.Models;
using Spine.Core.Accounts.Helpers;
using Spine.Core.Accounts.Jobs;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Data.Entities.Subscription;
using Spine.Services;
using Spine.Services.EmailTemplates.Models;

namespace Spine.Core.Accounts.Commands.Accounts
{
    public static class Signup
    {
        public class Command : IRequest<Response>
        {
            [Required(ErrorMessage = "Business name is required")]
            public string BusinessName { get; set; }

            [Required(ErrorMessage = "Firstname is required")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Lastname is required")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Email address is required")]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Please select sector you operate in")]
            public string OperatingSector { get; set; }

            [Required(ErrorMessage = "Business type is required")]
            public string BusinessType { get; set; }

            [MinLength(6, ErrorMessage = "Password cannot be less than 6 characters")]
            [Required(ErrorMessage = "Password is required")]
            public string Password { get; set; }

            [Compare(nameof(Password), ErrorMessage = "Password and confirm password must match")]
            public string ConfirmPassword { get; set; }
            public string Ref_ReferralCode { get; set; }
        }

        public class Response : BasicActionResult
        {
            public string LoginToken { get; set; }

            public Response(HttpStatusCode statusCode, string token)
            {
                Status = statusCode;
                LoginToken = token;
            }

            public Response(string message)
            {
                ErrorMessage = message;
                Status = HttpStatusCode.BadRequest;
            }
        }

        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly SpineContext _dbContext;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IMapper _mapper;
            private readonly IConfiguration _configuration;
            private readonly IEmailSender _emailSender;
            private readonly JwtSettings _jwtSettings;
            private readonly CommandsScheduler _scheduler;

            public Handler(SpineContext context, IMapper mapper, IConfiguration configuration,
                IOptions<JwtSettings> jwtSettings, IEmailSender emailSender, UserManager<ApplicationUser> userManager,
                CommandsScheduler scheduler)
            {
                _dbContext = context;
                _mapper = mapper;
                _userManager = userManager;
                _configuration = configuration;
                _emailSender = emailSender;
                _jwtSettings = jwtSettings.Value;
                _scheduler = scheduler;
            }

            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                if (await _dbContext.Companies.AnyAsync(x => (x.Name.ToLower() == request.BusinessName.ToLower() || x.Email == request.Email.ToLower()), cancellationToken: cancellationToken))
                    return new Response("Business name or email is already taken");

                if (await _userManager.FindByEmailAsync(request.Email) != null)
                {
                    return new Response("Email is already in use");
                }

                if (!EnumExtensionHelper.IsStrongPassword(request.Password))
                {
                    return new Response("The password is very weak");
                }

                if (!string.IsNullOrEmpty(request.Ref_ReferralCode))
                {
                    var isReferralCodeValid = _dbContext.Companies.Where(x => x.ReferralCode.Trim().ToLower() == request.Ref_ReferralCode.Trim().ToLower())
                        .Select(x => x.ReferralCode).FirstOrDefault();
                    if (string.IsNullOrEmpty(isReferralCodeValid))
                    {
                        return new Response("Referral Code not valid");
                    }
                }

                var validators = _userManager.PasswordValidators;
                foreach (var validator in validators)
                {
                    var check = await validator.ValidateAsync(_userManager, null, request.Password);
                    if (!check.Succeeded) return new Response(check.Errors.FirstOrDefault()?.Description);
                }

                var currencyId = _dbContext.Currencies.First(x => x.Code == Constants.NigerianCurrencyCode).Id;

                var company = _mapper.Map<Company>(request);
                company.BaseCurrencyId = currencyId;
                company.ImportRecord = false;
                company.ReferralCode = EnumExtensionHelper.GenerateRandomString(8, true, false);

                _dbContext.Companies.Add(company);

                _dbContext.CompanyCurrencies.Add(new CompanyCurrency
                {
                    Id = SequentialGuid.Create(),
                    CompanyId = company.Id,
                    ActivatedOn = Constants.GetCurrentDateTime(),
                    IsActive = true,
                    OldCurrencyId = currencyId,
                    CurrencyId = currencyId,
                    Rate = 1,
                    ActivatedBy = company.CreatedBy
                });

                var ownerRole = await _dbContext.Roles.FirstOrDefaultAsync(x => x.IsOwnerRole && !x.IsDeleted);
                // add user
                var user = _mapper.Map<ApplicationUser>(request);
                user.CompanyId = company.Id;
                user.IsBusinessOwner = true;
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.RoleId = ownerRole == null ? Guid.Empty : ownerRole.Id;
                user.CreatedBy = user.Id;

                //var userToken = await _userManager.CreateSecurityTokenAsync(user);
                // user.SecurityStamp = userToken;

                var create = await _userManager.CreateAsync(user, request.Password);
                if (!create.Succeeded)
                {
                    return new Response("Unable to complete signup, Please try again");
                }

                if (ownerRole != null)
                {
                    await _userManager.AddToRoleAsync(user, ownerRole.Name);
                }

                // add addresses
                _dbContext.SubscriberBillings.Add(new SubscriberBilling
                {
                    ID_Company = company.Id,
                    Address1 = "",
                    Address2 = "",
                    ID_Country = "",
                    ID_State = "",
                    PostalCode = "",
                    DateCreated = Constants.GetCurrentDateTime(),
                });
                _dbContext.SubscriberShippings.Add(new SubscriberShipping
                {
                    ID_Company = company.Id,
                    Address1 = "",
                    Address2 = "",
                    ID_Country = "",
                    ID_State = "",
                    PostalCode = "",
                    DateCreated = Constants.GetCurrentDateTime(),
                });

                var loginToken = await JwtHelper.GenerateJWToken(_jwtSettings, user);

               var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                _dbContext.AccountConfirmationTokens.Add(new AccountConfirmationToken
                {
                    Email = user.Email,
                    CreatedOn = Constants.GetCurrentDateTime(),
                    Token = code,
                    Id = user.Id
                });

                var webUrl = _configuration["SpineWeb"];
                var emailModel = new SignUp
                {
                    ActionLink = Constants.GetConfirmAccountLink(webUrl, code),
                    Name = request.BusinessName,
                    Date = Constants.GetCurrentDateTime().ToLongDateString(),
                    UserName = request.Email,
                    Password = request.Password
                };

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    _scheduler.SendNow(new SetupAccountCommand()
                    {
                        CompanyId = company.Id,
                    }, $"Set up Accounts for Company {company.Id}");
                    
                    var emailSent = await _emailSender.SendTemplateEmail(user.Email, $"{emailModel.AppName} - User Signup ", EmailTemplateEnum.Signup, emailModel);

                    return new Response(HttpStatusCode.Created, new JwtSecurityTokenHandler().WriteToken(loginToken));
                }

                return new Response("An error occured. Could not complete signup");
            }
        }
    }
}
