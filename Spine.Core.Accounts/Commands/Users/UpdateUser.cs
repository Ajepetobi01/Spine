using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Accounts.Commands.Users
{
    public static class UpdateUser
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid Id { get; set; }

            [Required(ErrorMessage = "First name is required")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Phone number is required")]
            public string PhoneNumber { get; set; }

            //[MinLength(6, ErrorMessage = "Password cannot be less than 6 characters")]
            //[Required(ErrorMessage = "Password is required")]
            //public string Password { get; set; }

            public bool EnableTwoFactorAuth { get; set; }
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
                var user = await _dbContext.Users.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted);

                user.PhoneNumber = request.PhoneNumber;
                user.FirstName = request.FirstName.ToTitleCase();
                user.LastName = request.LastName.ToTitleCase();
                user.FullName = $"{user.FirstName} {user.LastName}";
                user.TwoFactorEnabled = request.EnableTwoFactorAuth;

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.User,
                        Action = (int)AuditLogUserAction.Update,
                        Description = $"Update user {user.Email} Information",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response(HttpStatusCode.NoContent)
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
