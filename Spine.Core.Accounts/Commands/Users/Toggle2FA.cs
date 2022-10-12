using System;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Accounts.Commands.Users
{
    public static class Toggle2FA
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            
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
            private readonly IMapper _mapper;

            public Handler(SpineContext context, IMapper mapper, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _mapper = mapper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.UserId && !x.IsDeleted);

                if (!user.TwoFactorEnabled && !user.EmailConfirmed)
                    return new Response(
                        "Email address must be confirmed before you can enable two-factor authentication");

                user.TwoFactorEnabled = !user.TwoFactorEnabled;
                
                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.User,
                        Action = (int)AuditLogUserAction.Update,
                        Description = $"Toggle user {user.Email} 2FA status",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response(HttpStatusCode.NoContent)
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
