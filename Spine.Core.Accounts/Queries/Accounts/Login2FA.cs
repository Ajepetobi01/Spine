using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Spine.Common.ActionResults;
using Spine.Common.Helpers;
using Spine.Common.Models;
using Spine.Core.Accounts.Helpers;
using Spine.Data;
using Spine.Data.Entities;

namespace Spine.Core.Accounts.Queries.Accounts
{
    public static class Login2FA
    {
        public class Query : IRequest<Response>
        {
            [Required]
            public string Email { get; set; }
            [Required]
            public string OTP { get; set; }
            
            public string DeviceId { get; set; }
            
            [JsonIgnore]
            public string IpAddress { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Guid UserId { get; set; }
            public string Username { get; set; }
            public bool EmailConfirmed { get; set; }
            public bool Require2FA { get; set; }
            public string Token { get; set; }
            
           // [JsonIgnore] // refresh token is returned in http only cookie
            public string RefreshToken { get; set; }
            
            public DateTime RefreshTokenExpiration { get; set; }

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
            private readonly SpineContext _dbContext;

            public Handler(SpineContext context, IOptions<JwtSettings> jwtSettings, UserManager<ApplicationUser> userManager)
            {
                _jwtSettings = jwtSettings.Value;
                _userManager = userManager;
                _dbContext = context;
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null) return new Response($"Invalid Email address");

                if (user.IsDeleted) return new Response("Your user account has been disabled, Contact your administrator");

                var company = await _dbContext.Companies.Where(x => x.Id == user.CompanyId)
                                                            .Select(x => new { x.IsDeleted, x.LogoId }).SingleOrDefaultAsync();

                if (company == null || company.IsDeleted)
                    return new Response("Your business account has been disabled");

                var verifyCode = await _userManager.VerifyTwoFactorTokenAsync(user, Constants.OtpProvider, request.OTP);
                if (verifyCode)
                {
                    var jwtSecurityToken = await JwtHelper.GenerateJWToken(_jwtSettings, user);
                    var refreshToken = JwtHelper.GenerateRefreshToken(request.IpAddress);
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
                    
                    return new Response
                    {
                        UserId = user.Id,
                        EmailConfirmed = user.EmailConfirmed,
                        Require2FA = user.TwoFactorEnabled,
                        Username = user.UserName,
                        Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                        RefreshToken = refreshToken.Token,
                        RefreshTokenExpiration = refreshToken.Expires
                    };
                }

                return new Response($"OTP is invalid or has expired. Begin login process again");
            }
        }
    }
}


