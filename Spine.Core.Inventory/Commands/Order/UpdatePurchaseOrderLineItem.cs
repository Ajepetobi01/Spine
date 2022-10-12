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
    public static class UpdatePurchaseOrderLineItem
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public Guid Id { get; set; }

            public string Description { get; set; }
            [RequiredNonDefault]
            public int? Quantity { get; set; }
            [RequiredNonDefault]
            public decimal? Rate { get; set; }
            
            public string TaxLabel { get; set; }
            [Range(0, 100)]
            public decimal TaxRate { get; set; }
            
            public Guid? TaxId { get; set; }
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
                var item = await (from lineItem in _dbContext.LineItems.Where(x => x.CompanyId == request.CompanyId && x.Id == request.Id)
                                  join order in _dbContext.PurchaseOrders.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted) on lineItem.ParentItemId equals order.Id
                                  select new { lineItem, order }).SingleOrDefaultAsync();

                if (item == null) return new Response("Item not found");
                if (item.order.Status != PurchaseOrderStatus.Draft) return new Response("This line item cannot be updated");

                item.order.OrderAmount -= item.lineItem.Amount;

                var amount = request.Rate.Value * request.Quantity.Value;
                var taxAmount = (amount * request.TaxRate) / 100;
                item.lineItem.Description = request.Description;
                item.lineItem.Rate = request.Rate.Value;
                item.lineItem.Quantity = request.Quantity.Value;
                item.lineItem.TaxAmount = taxAmount;
                item.lineItem.TaxId = request.TaxId;
                item.lineItem.TaxRate = request.TaxRate;
                item.lineItem.TaxLabel = request.TaxLabel;
                item.lineItem.Amount = amount + taxAmount;

                item.order.OrderAmount += item.lineItem.Amount;

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.PurchaseOrder,
                        Action = (int)AuditLogPurchaseOrderAction.Update,
                        Description = $"Updated purchase order line item {request.Id}",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response("Unable to update line item");
            }
        }

    }
}
