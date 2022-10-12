using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Inventories.Commands
{
    public static class DeleteInventoryLocation
    {
        public class Command : IRequest<Response>
        {
            public Guid CompanyId { get; set; }
            public Guid UserId { get; set; }
            public Guid Id { get; set; }
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

            public Handler(SpineContext context, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var item = await _dbContext.InventoryLocations.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId
                        && x.Id == request.Id && !x.IsDeleted);

                if (item == null) return new Response("Item not found");

                if (await _dbContext.ProductLocations.AnyAsync(x => x.CompanyId == request.CompanyId && x.LocationId == item.Id && x.QuantityInStock > 0))
                    return new Response("Location cannot be deleted. Products have been allocated");

                item.IsDeleted = true;
                item.DeletedBy = request.UserId;

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.Inventory,
                        Action = (int)AuditLogInventoryAction.DeleteInventoryLocation,
                        Description = $"Deleted location  {item.Name}",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response(HttpStatusCode.BadRequest);
            }
        }

    }
}
