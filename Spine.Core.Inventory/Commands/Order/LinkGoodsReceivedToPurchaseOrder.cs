using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Order
{
    public static class LinkGoodsReceivedToPurchaseOrder
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            
            [Required]
            public Guid? OrderId { get; set; }

            [JsonIgnore]
            public Guid GoodsReceivedId { get; set; }
          
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
                var order = await _dbContext.PurchaseOrders.SingleOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.OrderId);

                if (order == null) return new Response("Purchase order not found");
                if (order.Status == PurchaseOrderStatus.Closed) return new Response("Purchase order is closed");

                var goodReceived = await _dbContext.ReceivedGoods.SingleOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId && x.Id == request.GoodsReceivedId);

                if (goodReceived == null) return new Response("Goods Received not found");

                goodReceived.PurchaseOrderId = order.Id;

                var grItems = await _dbContext.ReceivedGoodsLineItems.Where(x =>
                    x.CompanyId == request.CompanyId && x.GoodReceivedId == request.GoodsReceivedId)
                    .OrderBy(x=>x.CreatedOn).ToListAsync();

                foreach (var item in grItems)
                {
                    var amount = item.Quantity * item.Rate;
                    var taxAmount = (item.TaxRate * amount) / 100;
                    var totalAmount = amount + taxAmount;
                    var orderId = SequentialGuid.Create();
                    
                    _dbContext.LineItems.Add(new Data.Entities.LineItem
                    {
                        Id = orderId,
                        CompanyId = request.CompanyId,
                        Amount = totalAmount,
                        Description = item.Description,
                        ItemId = item.InventoryId,
                        ParentItemId = order.Id,
                        Item = item.Item,
                        Quantity = item.Quantity,
                        Rate = item.Rate,
                        TaxId = item.TaxId,
                        TaxLabel = item.TaxLabel,
                        TaxRate = item.TaxRate,
                        TaxAmount = taxAmount,
                        CreatedOn = DateTime.Now
                    });

                    item.OrderLineItemId = orderId;
                    order.OrderAmount += item.Amount;
                }
                
                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.PurchaseOrder,
                        Action = (int)AuditLogPurchaseOrderAction.LinkGRToPO,
                        Description = $"Linked GR {goodReceived.GoodReceivedNo} to PO {order.OrderNo}",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0 
                    ? new Response() 
                    : new Response("Unable to link GR to Purchase Order");
            }
        }
    }
}