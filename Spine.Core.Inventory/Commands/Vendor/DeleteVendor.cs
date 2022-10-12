using System;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Vendor
{
    public static class DeleteVendor
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public Guid Id { get; set; }

        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.NoContent;
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

            public Handler(SpineContext context, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var vendor = await _dbContext.Vendors.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted);

                if (vendor == null) return new Response("Vendor not found");

                vendor.IsDeleted = true;
                vendor.DeletedBy = request.UserId;

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Vendor,
                    Action = (int)AuditLogVendorAction.Delete,
                    UserId = request.UserId,
                    Description = $"Deleted vendor {vendor.Email}"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Vendor could not be deleted");
            }
        }

    }
}
