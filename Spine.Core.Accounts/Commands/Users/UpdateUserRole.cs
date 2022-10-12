using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Services;

namespace Spine.Core.Accounts.Commands.Users
{
    public static class UpdateUserRole
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UpdatedBy { get; set; }

            [JsonIgnore]
            public Guid RoleId { get; set; }

            [Required(ErrorMessage = "UserId is required")]
            public Guid? UserId { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.OK;
            }

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
            private readonly UserManager<ApplicationUser> _userManager;

            public Handler(SpineContext context, IAuditLogHelper auditHelper, UserManager<ApplicationUser> userManager)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _userManager = userManager;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var user = await _userManager.FindByIdAsync(request.UserId.Value.ToString());

                if (user == null || user.IsDeleted)
                {
                    return new Response("User not found");
                }

                var exisitngRoles = await _userManager.GetRolesAsync(user);
                var existingRole = exisitngRoles.First();
                var role = await _dbContext.Roles.Where(x => x.Id == request.RoleId && !x.IsDeleted)
                                                                    .Select(x => new { x.Id, x.Name }).SingleAsync();

                await _userManager.RemoveFromRoleAsync(user, existingRole);
                await _userManager.AddToRoleAsync(user, role.Name);
                user.RoleId = role.Id;

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.User,
                        Action = (int)AuditLogUserAction.UpdateRole,
                        Description = $"Changed  the role of user  with Email {user.Email} from {existingRole.GetFirstPart()} to {role.Name.GetFirstPart()}",
                        UserId = request.UpdatedBy
                    });

                return await _dbContext.SaveChangesAsync() > 0
                                  ? new Response(HttpStatusCode.Created)
                                  : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
