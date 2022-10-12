using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Spine.Common.Extensions;
using Spine.Common.Helpers;
using Spine.Common.Models;
using Spine.Data.Entities;

namespace Spine.Core.Accounts.Helpers
{
    public static class JwtHelper
    {
        public static async Task<JwtSecurityToken> GenerateJWToken(JwtSettings jwtSettings, ApplicationUser user)
        {
            string authMethod = user.TwoFactorEnabled ? "mfa" : "pwd";

            var username = user.FullName.Split(" ").FirstOrDefault();
            if (username.IsNullOrWhiteSpace()) username = user.FullName;

            var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.AuthenticationMethod, authMethod),
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("RoleId", user.RoleId.ToString()),
                    new Claim("CompanyId", user.CompanyId.ToString()),
                };

            var audiences = jwtSettings.Audiences.Split(',').ToList();
            claims.AddRange(audiences.Select(audience => new Claim("aud", audience.Trim())));

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: Constants.GetCurrentDateTime().AddMinutes(jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials);
            return jwtSecurityToken;
        }

        public static RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using(var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                var randomBytes = new byte[64];
                rngCryptoServiceProvider.GetBytes(randomBytes);
                return new RefreshToken
                {
                    Token = Convert.ToBase64String(randomBytes),
                    Expires = DateTime.UtcNow.AddDays(7),
                    Created = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };
            }
        }
    }
}
