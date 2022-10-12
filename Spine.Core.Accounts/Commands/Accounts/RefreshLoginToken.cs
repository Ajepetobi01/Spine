using System;
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
using Spine.Common.Models;
using Spine.Core.Accounts.Helpers;
using Spine.Data;
using Spine.Data.Entities;

namespace Spine.Core.Accounts.Commands.Accounts
{
    public static class RefreshLoginToken
    {
        public class Command : IRequest<Response>
        {
            public string RefreshToken { get; set; }
            [JsonIgnore]
            public string IpAddress { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Guid UserId { get; set; }
            public Guid CompanyId { get; set; }
            public string Username { get; set; }
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

        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly SpineContext _dbContext;
            private readonly JwtSettings _jwtSettings;

            public Handler(UserManager<ApplicationUser> userManager, SpineContext dbContext, IOptions<JwtSettings> jwtSettings)
            {
                _userManager = userManager;
                _jwtSettings = jwtSettings.Value;
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var detail = await (from token in _dbContext.RefreshTokens.Where(y => y.Token == request.RefreshToken)
                        join user in _dbContext.Users on token.UserId equals user.Id
                        select new { token, user}).SingleOrDefaultAsync();
                
                if (detail == null) return new Response($"Invalid refresh token");
                
                var refreshToken = detail.token;
                if(!refreshToken.IsActive) return new Response($"Refresh token is expired. Login again");

                var newRefreshToken = JwtHelper.GenerateRefreshToken(request.IpAddress);
                
                refreshToken.Revoked = DateTime.UtcNow;
                refreshToken.RevokedByIp = request.IpAddress;
                refreshToken.ReplacedByToken = newRefreshToken.Token;

                newRefreshToken.UserId = detail.user.Id;
                _dbContext.RefreshTokens.Add(newRefreshToken);
                await _dbContext.SaveChangesAsync();
                
                var tokenHelper  = await JwtHelper.GenerateJWToken(_jwtSettings, detail.user);
                var newLoginToken = new JwtSecurityTokenHandler().WriteToken(tokenHelper);

                return new Response
                {
                    UserId = detail.user.Id,
                    CompanyId = detail.user.CompanyId,
                    Username = detail.user.UserName,
                    Token = newLoginToken,
                    RefreshToken = newRefreshToken.Token,
                    RefreshTokenExpiration = newRefreshToken.Expires
                };

            }
        }
    }

}
