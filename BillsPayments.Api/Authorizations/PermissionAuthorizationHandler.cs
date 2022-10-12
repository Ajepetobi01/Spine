using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Spine.Common.Helpers;
using Spine.Data;

namespace BillsPayments.Api.Authorizations
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; private set; }
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly SpineContext _dbContext;
        private readonly IDistributedCache _distributedCache;

        public PermissionAuthorizationHandler(SpineContext dbContext, IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _distributedCache = distributedCache;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var user = context.User;
            if (!user.Claims.Any())
            {
                return;
            }

            List<string> permissions;
            var userRoleId = Guid.Parse(user.Claims.FirstOrDefault(x=>x.Type == "RoleId").Value);
            var cacheKey = userRoleId + Constants.PermissionCache;
            string serializedPermissions;
            
            var permissionsCache = await _distributedCache.GetAsync(cacheKey);
            if (permissionsCache != null)
            {
                serializedPermissions = Encoding.UTF8.GetString(permissionsCache);
                permissions = JsonConvert.DeserializeObject<List<string>>(serializedPermissions);
            }
            else
            {
                permissions = _dbContext.RoleClaims.Where(x =>
                        x.ClaimType == Constants.PermissionClaim && x.RoleId == userRoleId)
                    .Select(x => x.ClaimValue).ToList();
                
                serializedPermissions = JsonConvert.SerializeObject(permissions);
                permissionsCache = Encoding.UTF8.GetBytes(serializedPermissions);

                var options = new DistributedCacheEntryOptions();
                    //.SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                   // .SetSlidingExpiration(TimeSpan.FromMinutes(2));
                
                await _distributedCache.SetAsync(cacheKey, permissionsCache, options);
            }
            
            if (permissions.Any(x => x == requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}
