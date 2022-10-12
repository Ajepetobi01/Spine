using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Services;

namespace Spine.Core.Accounts.Commands.Roles
{
    public static class UpdateRolePermission
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required(ErrorMessage = "Role Id is required")]
            public Guid? RoleId { get; set; }

            public List<Permissions> Permissions { get; set; }

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
            private readonly IAuditLogHelper _auditHelper;
            private readonly IDistributedCache _distributedCache;
            private readonly RoleManager<ApplicationRole> _roleManager;

            public Handler(SpineContext context, IAuditLogHelper auditHelper, RoleManager<ApplicationRole> roleManager, IDistributedCache distributedCache)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _roleManager = roleManager;
                _distributedCache = distributedCache;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
                if (role == null)
                {
                    return new Response("Role does not exist");
                }

                if (role.IsSystemDefined || !role.CompanyId.HasValue)
                {
                    return new Response("You cannot update permissions for system defined roles");
                }

                var existingClaims = await _roleManager.GetClaimsAsync(role);
                var newPermissions = request.Permissions.Select(x => x.GetStringValue());
                var existingPermissions = existingClaims.Select(x => x.Value).ToList();
                var newlyAdded = newPermissions.Except(existingPermissions).ToList();
                var removed = existingPermissions.Except(newPermissions).ToList();

                foreach (var item in newlyAdded)
                {
                    await _roleManager.AddClaimAsync(role, new Claim(Constants.PermissionClaim, item));
                }

                foreach (var item in removed)
                {
                    await _roleManager.RemoveClaimAsync(role, existingClaims.Single(x => x.Value == item));
                }

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                  new AuditModel
                  {
                      EntityType = (int)AuditLogEntityType.Role,
                      Action = (int)AuditLogRoleAction.UpdatePermission,
                      Description = $"Updated permissions for role with id  {request.RoleId}",
                      UserId = request.UserId
                  });

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    var cacheKey = role.Id + Constants.PermissionCache;
                    await _distributedCache.RemoveAsync(cacheKey);
                    return new Response(HttpStatusCode.NoContent);
                }
                return  new Response(HttpStatusCode.BadRequest);
            }
        }
    }
}