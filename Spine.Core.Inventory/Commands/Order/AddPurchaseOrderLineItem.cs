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
    public static class AddPurchaseOrderLineItem
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public Guid PurchaseOrderId { get; set; }

            [RequiredNonDefault]
            public Guid? InventoryId { get; set; }
            [Required]
            public string Item { get; set; }
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
                var order = await _dbContext.PurchaseOrders.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.PurchaseOrderId).SingleOrDefaultAsync();

                if (order == null) return new Response("Order not found");

                if (order.Status != PurchaseOrderStatus.Draft) return new Response("This line item cannot be updated");

                var amount = request.Rate.Value * request.Quantity.Value;
                var taxAmount = (request.TaxRate * amount) / 100;
                order.OrderAmount += (amount + taxAmount);
                _dbContext.LineItems.Add(new Data.Entities.LineItem
                {
                    CompanyId = request.CompanyId,
                    Amount = amount + taxAmount,
                    Description = request.Description,
                    ItemId = request.InventoryId,
                    ParentItemId = request.PurchaseOrderId,
                    Item = request.Item,
                    Quantity = request.Quantity.Value,
                    Rate = request.Rate.Value,
                    TaxAmount = taxAmount,
                    TaxId = request.TaxId,
                    TaxLabel = request.TaxLabel,
                    TaxRate = request.TaxRate,
                    CreatedOn = DateTime.Now
                });

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.PurchaseOrder,
                        Action = (int)AuditLogPurchaseOrderAction.Update,
                        Description = $"Add new line item to purchase order {request.PurchaseOrderId}",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response("Unable to add new item to purchase order");
            }
        }

    }
}
