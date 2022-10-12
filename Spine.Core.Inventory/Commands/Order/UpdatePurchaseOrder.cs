using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Order
{
    public static class UpdatePurchaseOrder
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public Guid PurchaseOrderId { get; set; }

            [Required]
            public Guid? VendorId { get; set; }
            
            public string AdditionalNote { get; set; }
            [RequiredNonDefault]
            public DateTime? OrderDate { get; set; }
            public DateTime ExpectedDate { get; set; }
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
                var order = await _dbContext.PurchaseOrders.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.PurchaseOrderId && !x.IsDeleted);
                if (order == null) if (order == null) return new Response("Item not found");

                if (order.Status != PurchaseOrderStatus.Draft) return new Response("This purchase order cannot be updated");
                
                if (order.VendorId != request.VendorId)
                {
                    var vendor = await _dbContext.Vendors
                        .Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.VendorId)
                        .Select(x => new {x.Email, x.Name}).SingleOrDefaultAsync();
                    
                    order.VendorId = request.VendorId;
                    order.VendorName = vendor?.Name;
                    order.VendorEmail = vendor?.Email;
                }
                
                order.OrderDate = request.OrderDate.Value;
                order.ExpectedDate = request.ExpectedDate;
                order.AdditionalNote = request.AdditionalNote;

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.PurchaseOrder,
                        Action = (int)AuditLogPurchaseOrderAction.Update,
                        Description = $"Update purchase order with {order.Id}",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response("Unable to update purchase order");
            }
        }

    }
}
