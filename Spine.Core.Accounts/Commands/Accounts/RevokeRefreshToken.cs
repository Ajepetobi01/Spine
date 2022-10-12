using System;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Extensions;
using Spine.Data;

namespace Spine.Core.Accounts.Commands.Accounts
{
    public static class RevokeRefreshToken
    {
        public class Command : IRequest<Response>
        {
            public string RefreshToken { get; set; }
            [JsonIgnore]
            public string IpAddress { get; set; }

            public string DeviceId { get; set; }
        }

        public class Response : BasicActionResult
        {
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
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var detail = await (from token in _dbContext.RefreshTokens.Where(y => y.Token == request.RefreshToken)
                        join user in _dbContext.Users on token.UserId equals user.Id
                        select new { token, user}).SingleOrDefaultAsync();
                
                if (detail == null) return new Response($"Invalid token");
                
                var refreshToken = detail.token;
                
                refreshToken.Revoked = DateTime.UtcNow;
                refreshToken.RevokedByIp = request.IpAddress;

                if (!request.DeviceId.IsNullOrEmpty())
                {
                    var deviceToken = await _dbContext.DeviceTokens
                        .Where(x => x.UserId == detail.user.Id && x.Token == request.DeviceId).ToListAsync();
                    
                    _dbContext.DeviceTokens.RemoveRange(deviceToken);
                }
                
                return await _dbContext.SaveChangesAsync() > 0 
                    ? new Response() 
                    : new Response("Could not log out user");
            }
        }
    }

}
