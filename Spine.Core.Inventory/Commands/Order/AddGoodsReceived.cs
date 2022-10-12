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
    //add goods received without PO
    public static class AddGoodsReceived
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore] public Guid UserId { get; set; }

            [RequiredNonDefault]
            public DateTime? ReceivedDate { get; set; }
            
            [Required]
            public Guid? VendorId { get; set; }

            public DateTime? PaymentDueDate { get; set; }
            
            [RequiredNotEmpty]
            public List<AddPurchaseOrder.LineItemModel> LineItems { get; set; }
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

                var inventoryToRecieve = request.LineItems.Select(x => x.InventoryId).ToHashSet();
                var orderInventory = await _dbContext.Inventories.Where(x => x.CompanyId == request.CompanyId &&
                                                                             x.InventoryType == InventoryType.Product
                                                                             && inventoryToRecieve.Contains(x.Id) &&
                                                                             !x.IsDeleted).ToListAsync();
                
                var vendor = await _dbContext.Vendors.FirstOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.VendorId);

                var receivedItems = new List<ReceivedGoodsModel>();
                var lastUsed =
                    await _serialHelper.GetLastUsedGoodsReceivedNo(_dbContext, request.CompanyId, 1);
                
                var receivedGoodId = SequentialGuid.Create();
                var receivedAmount = 0.0m;
                foreach (var item in request.LineItems)
                {
                    var inventory = orderInventory.SingleOrDefault(x => x.Id == item.InventoryId);
                    if (inventory == null) continue;
                    
                    inventory.QuantityInStock += item.Quantity;

                    var amount = item.Quantity * item.Rate;
                    var taxAmount = (item.TaxRate * amount) / 100;
                    var totalAmount = amount + taxAmount;
                    _dbContext.ReceivedGoodsLineItems.Add(new ReceivedGoodsLineItem
                    {
                        Id = SequentialGuid.Create(),
                        CompanyId = request.CompanyId,
                        Amount = totalAmount,
                        Balance = totalAmount,
                        Description = item.Description,
                        InventoryId = item.InventoryId,
                        GoodReceivedId = receivedGoodId,
                        Item = item.Item,
                        Quantity = item.Quantity,
                        Rate = item.Rate,
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
                        TaxId = item.TaxId,
                        VendorId = request.VendorId,
                        InventoryId = item.InventoryId,
                        Inventory = inventory.Name,
                        DateReceived = request.ReceivedDate.Value,
                        ReceivedBy = request.UserId
                    });

                    _dbContext.ProductStocks.Add(new ProductStock
                    {
                        CompanyId = request.CompanyId,
                        CreatedBy = request.UserId,
                        InventoryId = inventory.Id,
                        UnitSalesPrice = item.Rate,
                        ReorderQuantity = item.Quantity,
                        RestockDate = request.ReceivedDate.Value,
                        UnitCostPrice = item.Rate,
                        CreatedOn = DateTime.Today,
                        GoodsReceivedId = receivedGoodId,
                    });

                    if (item.Rate != inventory.UnitCostPrice)
                    {
                        _dbContext.InventoryPriceHistories.Add(new InventoryPriceHistory
                        {
                            CompanyId = request.CompanyId,
                            CreatedBy = request.UserId,
                            InventoryId = inventory.Id,
                            UnitCostPrice = item.Rate,
                            UnitSalesPrice = inventory.UnitSalesPrice,
                            RestockDate = DateTime.Today,
                            CreatedOn = DateTime.Today
                        });
                        inventory.UnitCostPrice = item.Rate;
                    }
                    //    inventory.UnitSalesPrice = item.Rate.Value;
                    inventory.LastRestockDate = request.ReceivedDate.Value;
                }

                _dbContext.ReceivedGoods.Add(new ReceivedGood
                {
                    Id = receivedGoodId,
                    CompanyId = request.CompanyId,
                    Amount = receivedAmount,
                    DateReceived = request.ReceivedDate.Value,
                    VendorId = request.VendorId,
                    PurchaseOrderId = null,
                    ReceivedBy = request.UserId,
                    CreatedOn = DateTime.Today,
                    PaymentDueDate = request.PaymentDueDate,
                    GoodReceivedNo = Constants.GenerateSerialNo(Constants.SerialNoType.GR, lastUsed + 1)
                });

                if (vendor != null)
                    vendor.AmountOwed += receivedAmount;

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.PurchaseOrder,
                        Action = (int)AuditLogPurchaseOrderAction.ReceiveGoodsWithoutPO,
                        Description = $"Receive goods on {request.ReceivedDate.Value.ToShortDateString()}",
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

                return new Response("Unable to add goods received");
            }
        }

    }
}
