using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Spine.Common.ActionResults;
using Spine.Common.Extensions;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Data.Entities;

namespace Spine.Core.Accounts.Commands
{
    public static class UpdateNewlyAddedThings
    {
        public class Command : IRequest<Response>
        {
        }

        public class Response : BasicActionResult
        {
            public Response(HttpStatusCode statusCode)
            {
                Status = statusCode;
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
            private readonly RoleManager<ApplicationRole> _roleManager;
            private readonly IDistributedCache _distributedCache;

            public Handler(SpineContext context, RoleManager<ApplicationRole> roleManager, IDistributedCache distributedCache)
            {
                _dbContext = context;
                _roleManager = roleManager;
                _distributedCache = distributedCache;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var adminRole = await _dbContext.Roles.Where(x => !x.IsDeleted && x.IsOwnerRole).SingleAsync();
                var allPermissions = Constants.BusinessOwnerPermission().Select(x => x.GetStringValue()).ToList();

                var existingClaims = await _roleManager.GetClaimsAsync(adminRole);
                var existingPermissions = existingClaims.Select(x => x.Value).ToList();
                var newlyAdded = allPermissions.Except(existingPermissions).ToList();
                var removed = existingPermissions.Except(allPermissions).ToList();

                foreach (var item in newlyAdded)
                {
                    await _roleManager.AddClaimAsync(adminRole, new Claim(Constants.PermissionClaim, item));
                }

                foreach (var item in removed)
                {
                    await _roleManager.RemoveClaimAsync(adminRole, existingClaims.Single(x => x.Value == item));
                }

                var cacheKey = adminRole.Id + Constants.PermissionCache;
                await _distributedCache.RemoveAsync(cacheKey);
                
                await _dbContext.SaveChangesAsync();

                return new Response(HttpStatusCode.NoContent);
            }
        }
    }
}