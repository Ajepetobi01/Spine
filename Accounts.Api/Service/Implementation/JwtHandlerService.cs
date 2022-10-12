using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Spine.Data.Entities;
using Accounts.Api.Service.Contract;
using Accounts.Api.Service.DTO;
using Spine.Common.Models;
using System.Linq;
using Spine.Common.Helpers;
using Spine.Common.Helper;
using Spine.Data;
using Microsoft.EntityFrameworkCore;
using Spine.Core.Accounts.Helpers;
using Spine.Core.Subscription.ViewModel;

namespace Accounts.Api.Service.Implementation
{
    public class JwtHandlerService : IJwtHandlerService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly SpineContext _dbContext;

        public JwtHandlerService(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor, IOptions<JwtSettings> jwtSettings, SpineContext context)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _jwtSettings = jwtSettings;
            _dbContext = context;
        }

        public async Task<TokenDto> GenerateToken(ApplicationUser user)
        {
            var jwtSecurityToken = await JwtHelper.GenerateJWToken(_jwtSettings.Value, user);
            return new TokenDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Expiration = jwtSecurityToken.ValidTo
            };
        }

        public async Task<ApplicationUser> GetOrCreateExternalUserAccount(string provider, string key, string email, string firstName, string lastName, string role)
        {
            // Login already linked to a user
            var user = await _userManager.FindByLoginAsync(provider, key);
            if (user != null)
                return user;

            user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var currencyId = _dbContext.Currencies.First(x => x.Code == Constants.NigerianCurrencyCode).Id;
                // No user exists with this email address, we create a new one
                var company = new Company
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    CreatedOn = DateTime.Now,
                    BaseCurrencyId = currencyId,
                    ReferralCode = EnumExtensionHelper.GenerateRandomString(8, true, false)
                };
                _dbContext.Companies.Add(company);

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    user = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = company.Id,
                        Email = email,
                        UserName = email,
                        FullName = $"{firstName} {lastName}",
                        FirstName = firstName,
                        LastName = lastName,
                    };
                    await _userManager.CreateAsync(user);
                    //if (!await _roleManager.RoleExistsAsync(role))
                    //    await _userManager.AddToRoleAsync(user, role);
                }

            }

            var QueryCompany = await _dbContext.Companies.Where(x => x.Id == user.CompanyId)
                                                                .Select(x => new
                                                                {
                                                                    x.IsDeleted,
                                                                    x.LogoId
                                                                }).SingleOrDefaultAsync();

            if (QueryCompany == null || QueryCompany.IsDeleted)
                throw new Exception("Your business account has been disabled");


            var subscription = await _dbContext.CompanySubscriptions.Where(x => x.ID_Company == user.CompanyId && x.ExpiredDate >= DateTime.Now && x.PaymentStatus)
                        .Select(x => new CompanySubscriptionDTO
                        {
                            Id_Subscription = x.ID_Subscription,
                            Id_Plan = x.ID_Plan,
                            SubscriptionDate = x.TransactionDate == null ? null : x.TransactionDate.Value.ToString("dd/MM/yyyy"),
                            ExpiredDate = x.ExpiredDate == null ? null : x.ExpiredDate.Value.ToString("dd/MM/yyyy"),
                            Amount = x.Amount,
                            IsActive = x.IsActive,
                            PaymentStatus = x.PaymentStatus
                        }).FirstOrDefaultAsync();

            if (subscription != null)
            {
                if (subscription.ExpiredDate != null)
                {
                    if (Convert.ToDateTime(CovertDate(subscription.ExpiredDate)) < DateTime.Now)
                    {
                        //Update CompanySubscriptions set IsActive to false
                        var updateSubscriberStatus = _dbContext.CompanySubscriptions
                                   .Where(x => x.ID_Subscription == subscription.Id_Subscription).FirstOrDefault();
                        updateSubscriberStatus.IsActive = false;
                        await _dbContext.SaveChangesAsync();
                        throw new Exception("Your subscription has expired");
                    }
                }
                if (!subscription.IsActive)
                {
                    throw new Exception("Your subscription has been disabled");
                }
            }

            // Link the user to this login
            var info = new UserLoginInfo(provider, key, provider.ToUpperInvariant());
            var result = await _userManager.AddLoginAsync(user, info);
            if (result.Succeeded)
                return user;

            //_logger.LogError("Failed add a user linked to a login.");
            //_logger.LogError(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
            return null;
        }

        public string GetLoggedInUserId()
        {
            if (_httpContextAccessor.HttpContext.User.Identity is ClaimsIdentity identity)
                return identity.FindFirst(ClaimTypes.NameIdentifier).Value;
            throw new Exception(); // todo: handle exception properly
        }

        public async Task<ApplicationUser> GetLoggedInUser()
        {
            var user = await _userManager.FindByIdAsync(GetLoggedInUserId());
            if (user == null)
                throw new Exception(); // todo: handle exception properly
            return user;
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
