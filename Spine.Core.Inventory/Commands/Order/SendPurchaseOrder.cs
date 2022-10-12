using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Core.Inventories.Helper;
using Spine.Core.Inventories.Jobs;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Order
{
    public static class SendPurchaseOrder
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
            private readonly CommandsScheduler _scheduler;
            
            public Handler(SpineContext context, IAuditLogHelper auditHelper, CommandsScheduler scheduler)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _scheduler = scheduler;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var order = await _dbContext.PurchaseOrders.SingleOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted);
                if (order == null) return new Response("Purchase order not found");
                
                if (order.Status < PurchaseOrderStatus.Issued)
                    order.Status = PurchaseOrderStatus.Issued;
                
                _scheduler.SendNow(new SendOrderJobCommand
                {
                    CompanyId = request.CompanyId,
                    OrderId = order.Id,
                }, $"Send Purchase Order {order.Id}");

                await _dbContext.SaveChangesAsync();
                return new Response();
            }
        }
    }
}
