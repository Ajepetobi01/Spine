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
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Core.Inventories.Helper;
using Spine.Core.Inventories.Jobs;
using Spine.Data;
using Spine.Data.Entities.Inventories;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Order
{
    public static class ConfirmGoodsReceived
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
            public DateTime? ReceivedDate { get; set; }

            [RequiredNotEmpty]
            public List<LineItemModel> OrderReceipts { get; set; }
        }

        public class LineItemModel
        {
            [Required]
            public Guid? LineItemId { get; set; }
            [Required]
            public Guid? InventoryId { get; set; }
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
            private readonly ISerialNumberHelper _serialHelper;
            private readonly CommandsScheduler _scheduler;

            public Handler(SpineContext context, IAuditLogHelper auditHelper, ISerialNumberHelper serialHelper, CommandsScheduler scheduler)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _scheduler = scheduler;
                _serialHelper = serialHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var accountingPeriod = await _dbContext.AccountingPeriods.FirstOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId
                    && request.ReceivedDate.Value.Date >= x.StartDate && request.ReceivedDate.Value.Date <= x.EndDate);

                if (accountingPeriod == null) return new Response("Received date does not have an accounting period");
                if (accountingPeriod.IsClosed) return new Response("Accounting period for this received date is closed");

                var order = await _dbContext.PurchaseOrders.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.PurchaseOrderId && !x.IsDeleted);
                if (order == null) return new Response("Item not found");

                if (order.Status == PurchaseOrderStatus.Closed) return new Response("All items in this purchase order has been received");

                var lineItemsToReceive = request.OrderReceipts.Select(x => x.LineItemId).ToHashSet();
                var inventoryToRecieve = request.OrderReceipts.Select(x => x.InventoryId).ToHashSet();

                var orderItems = await _dbContext.LineItems.Where(x => x.CompanyId == order.CompanyId && x.ParentItemId == order.Id
                                                                                                            && lineItemsToReceive.Contains(x.Id)).ToListAsync();

                var vendor = await _dbContext.Vendors.FirstOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == order.VendorId);
                
                var orderInventory = await _dbContext.Inventories.Where(x => x.CompanyId == order.CompanyId && x.InventoryType == InventoryType.Product
                                                                                                          && inventoryToRecieve.Contains(x.Id) && !x.IsDeleted).ToListAsync();
                
                var alreadyReceived = await _dbContext.ReceivedGoods.Where(x => x.CompanyId == order.CompanyId && x.PurchaseOrderId == order.Id).ToListAsync();
                var alreadyRecievedGRIds = alreadyReceived.Select(x => x.Id);
                
                var alreadyReceivedItems = await _dbContext.ReceivedGoodsLineItems.Where(x =>
                    x.CompanyId == order.CompanyId && alreadyRecievedGRIds.Contains(x.GoodReceivedId)).ToListAsync();
                
                var fullyReceived = true;
                var receivedItems = new List<ReceivedGoodsModel>(); 
                
                var lastUsed =
                    await _serialHelper.GetLastUsedGoodsReceivedNo(_dbContext, request.CompanyId, 1);

                var receivedAmount = 0.0m;
                var receivedGoodId = SequentialGuid.Create();
                
                foreach (var item in request.OrderReceipts)
                {
                    var orderItem = orderItems.Where(x => x.Id == item.LineItemId).SingleOrDefault();
                    if (orderItem != null)
                    {
                        var totalReceived = alreadyReceivedItems.Where(x => x.OrderLineItemId == item.LineItemId).Sum(x => x.Quantity);
                        if (totalReceived < orderItem.Quantity)
                        {
                            var newTotal = totalReceived + item.Quantity;
                            if (newTotal > orderItem.Quantity) return new Response($"Total received items cannot be more than the {orderItem.Quantity} for {orderItem.Item} in the purchase order");

                            if (newTotal < orderItem.Quantity) // if new quantity of any item in the list is less than order quantity, the whole order shouldn't be fully received
                            {
                                fullyReceived = false;
                            }

                            var inventory = orderInventory.Where(x => x.Id == item.InventoryId).SingleOrDefault();
                            if (inventory != null)
                            {
                                inventory.QuantityInStock += item.Quantity.Value;

                                var amount = item.Quantity.Value * item.Rate.Value;
                                var taxAmount = (orderItem.TaxRate * amount) / 100;
                                var totalAmount = amount + taxAmount;
                                var id = SequentialGuid.Create();

                                _dbContext.ReceivedGoodsLineItems.Add(new ReceivedGoodsLineItem
                                {
                                    Id = SequentialGuid.Create(),
                                    CompanyId = request.CompanyId,
                                    Amount = totalAmount,
                                    Balance = totalAmount,
                                    Description = orderItem.Description,
                                    InventoryId = item.InventoryId.Value,
                                    GoodReceivedId = receivedGoodId,
                                    OrderLineItemId = orderItem.Id,
                                    Item = orderItem.Item,
                                    Quantity = item.Quantity.Value,
                                    Rate = item.Rate.Value,
                                    TaxId = item.TaxId,
                                    TaxLabel = item.TaxLabel,
                                    TaxRate = item.TaxRate,
                                    TaxAmount = taxAmount,
                                    CreatedOn = DateTime.Now
                                });

                                receivedAmount += totalAmount;
                                receivedItems.Add(new ReceivedGoodsModel
                                {
                                    AccountingPeriodId = accountingPeriod.Id,
                                    Amount = totalAmount,
                                    TaxAmount = taxAmount,
                                    TaxId = orderItem.TaxId,
                                    VendorId = order.VendorId,
                                    InventoryId = inventory.Id,
                                    Inventory = inventory.Name,
                                    DateReceived = request.ReceivedDate.Value,
                                    ReceivedBy = request.UserId
                                });

                                _dbContext.ProductStocks.Add(new ProductStock
                                {
                                    CompanyId = request.CompanyId,
                                    CreatedBy = request.UserId,
                                    InventoryId = inventory.Id,
                                    UnitSalesPrice = item.Rate.Value,
                                    ReorderQuantity = item.Quantity.Value,
                                    RestockDate = request.ReceivedDate.Value,
                                    UnitCostPrice = item.Rate.Value,
                                    CreatedOn = DateTime.Today,
                                    GoodsReceivedId = id,
                                });

                                if (item.Rate != inventory.UnitCostPrice)
                                {
                                    _dbContext.InventoryPriceHistories.Add(new InventoryPriceHistory
                                    {
                                        CompanyId = request.CompanyId,
                                        CreatedBy = request.UserId,
                                        InventoryId = inventory.Id,
                                        UnitCostPrice = item.Rate.Value,
                                        UnitSalesPrice = inventory.UnitSalesPrice,
                                        RestockDate = DateTime.Today,
                                        CreatedOn = DateTime.Today
                                    });
                                    inventory.UnitCostPrice = item.Rate.Value;
                                }
                                //    inventory.UnitSalesPrice = item.Rate.Value;
                                inventory.LastRestockDate = request.ReceivedDate.Value;
                            }
                        }
                        else
                            return new Response($"All quantities for item {orderItem.Item} has been received");
                    }
                    else
                        fullyReceived = false;
                }
                
                _dbContext.ReceivedGoods.Add(new ReceivedGood
                {
                    Id = receivedGoodId,
                    CompanyId = request.CompanyId,
                    Amount = receivedAmount,
                    DateReceived = request.ReceivedDate.Value,
                    VendorId = order.VendorId,
                    PurchaseOrderId = request.PurchaseOrderId,
                    ReceivedBy = request.UserId,
                    CreatedOn = DateTime.Today,
                    GoodReceivedNo = Constants.GenerateSerialNo(Constants.SerialNoType.GR, lastUsed + 1)
                });
                
                if (vendor != null)
                    vendor.AmountOwed += receivedAmount;
                
                order.Status = fullyReceived ? PurchaseOrderStatus.Closed : PurchaseOrderStatus.PartiallyReceived;

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.PurchaseOrder,
                        Action = (int)AuditLogPurchaseOrderAction.ConfirmReceipt,
                        Description = $"Confirm receipt for items in  purchase order for vendor {order.VendorName} on {order.OrderDate.ToShortDateString()}",
                        UserId = request.UserId
                    });

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    _scheduler.SendNow(new HandleAccountingForConfirmGoodsReceived
                    {
                        CompanyId = request.CompanyId,
                        ReceivedGoods = receivedItems,
                        UserId = request.UserId,
                    });

                    return new Response();
                }
                
                return new Response(HttpStatusCode.BadRequest);
            }
        }

    }
}
