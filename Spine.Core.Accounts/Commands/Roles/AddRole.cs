using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Services;

namespace Spine.Core.Accounts.Commands.Roles
{
    public static class AddRole
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required(ErrorMessage = "Name is required")]
            public string Name { get; set; }

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
            private readonly RoleManager<ApplicationRole> _roleManager;
            private readonly IMapper _mapper;

            public Handler(SpineContext context, IMapper mapper, IAuditLogHelper auditHelper, RoleManager<ApplicationRole> roleManager)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _mapper = mapper;
                _roleManager = roleManager;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                request.Name = $"{request.Name}_{request.CompanyId}";

                if (await _roleManager.RoleExistsAsync(request.Name))
                {
                    return new Response("Role name exists");
                }

                var newRole = _mapper.Map<ApplicationRole>(request);
                await _roleManager.CreateAsync(newRole);

                foreach (var item in request.Permissions)
                {
                    await _roleManager.AddClaimAsync(newRole, new Claim(Constants.PermissionClaim, item.GetStringValue()));
                }

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                  new AuditModel
                  {
                      EntityType = (int)AuditLogEntityType.Role,
                      Action = (int)AuditLogRoleAction.Create,
                      Description = $"Added new role  {newRole.Id} with  {request.Permissions.Count} permissions",
                      UserId = request.UserId
                  });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response(HttpStatusCode.Created)
                    : new Response(HttpStatusCode.BadRequest);
            }
        }
    }
}