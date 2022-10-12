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
using Spine.Core.Inventories.Helper;
using Spine.Core.Inventories.Jobs;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Order
{
    public static class ReturnGoodsReceived
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public Guid GoodsReceivedId { get; set; }
            
            [RequiredNotEmpty]
            public List<ReturnModel> Data { get; set; }
        }

        public class ReturnModel
        {
            [Required]
            public Guid? LineItemId { get; set; }
            [RequiredNonDefault]
            public int? Quantity { get; set; }
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
            private readonly CommandsScheduler _scheduler;

            public Handler(SpineContext context, IAuditLogHelper auditHelper, CommandsScheduler scheduler)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _scheduler = scheduler;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var returnDate = DateTime.Today;
                var accountingPeriod = await _dbContext.AccountingPeriods.FirstOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId
                    && returnDate.Date >= x.StartDate && returnDate.Date <= x.EndDate);

                if (accountingPeriod == null) return new Response("Business does not have an accounting period for today");
                if(accountingPeriod.IsClosed) return new Response("Accounting period is closed");

                var detail = await (from good in _dbContext.ReceivedGoods.Where(x =>
                            x.CompanyId == request.CompanyId && x.Id == request.GoodsReceivedId)
                        join stock in _dbContext.ProductStocks on good.Id equals stock.GoodsReceivedId
                        join vendor in _dbContext.Vendors on good.VendorId equals vendor.Id
                        select new
                        {
                            good, stock, vendor
                        })
                    .SingleOrDefaultAsync();
                
                if (detail == null) return new Response("Item not found");
                
                var itemIds = request.Data.Select(x => x.LineItemId).ToList();

                var items = await _dbContext.ReceivedGoodsLineItems.Where(x =>
                        x.CompanyId == request.CompanyId && x.GoodReceivedId == request.GoodsReceivedId &&
                        itemIds.Contains(x.Id) && x.Quantity > 0)
                    .ToListAsync();

                var inventoryIds = items.Select(x => x.InventoryId).ToHashSet();
                
                var inventories = await _dbContext.Inventories
                    .Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && inventoryIds.Contains(x.Id))
                    .ToDictionaryAsync(x=>x.Id);
                
                var totalAmount = 0.0m;
                var returnModel = new List<InventoryAdjustmentModel>();
                foreach (var item in items)
                {
                    if (item.Amount != item.Balance)
                        return new Response($"Item {item.Item} has been paid for. It cannot be returned");
                    
                    var dt = request.Data.FirstOrDefault(x => x.LineItemId == item.Id);
                    item.ReturnedQuantity = dt?.Quantity ?? 0;
                    item.Quantity = item.Quantity < item.ReturnedQuantity ? 0 : item.Quantity - item.ReturnedQuantity;

                    var amount = item.ReturnedQuantity * item.Rate;
                    var taxAmount = (item.TaxRate * amount) / 100;
                    totalAmount += (amount + taxAmount);
                    var inv = inventories[item.InventoryId];
                    if (inv != null)
                    {
                        inv.QuantityInStock = inv.QuantityInStock < item.ReturnedQuantity
                            ? 0
                            : inv.QuantityInStock - item.ReturnedQuantity;
                        
                        returnModel.Add(new InventoryAdjustmentModel
                        {
                            Name = inv.Name,
                            Id = inv.Id,
                            Date = returnDate,
                            Amount = amount + taxAmount,
                            TaxAmount = taxAmount,
                            TaxId = item.TaxId,
                            Quantity = item.ReturnedQuantity
                        });
                    }
                    
                    detail.stock.ReturnedQuantity += item.ReturnedQuantity;
                }

                if (detail.vendor != null)
                    detail.vendor.AmountOwed = detail.vendor.AmountOwed < totalAmount
                        ? 0
                        : detail.vendor.AmountOwed - totalAmount;
                
                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.PurchaseOrder,
                        Action = (int)AuditLogPurchaseOrderAction.ReturnGoodsReceived,
                        Description = $"Return goods received for {request.Data.Count} item(s) in GR No {detail.good.GoodReceivedNo}",
                        UserId = request.UserId
                    });

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    _scheduler.SendNow(new HandleAccountingForReturnGoodsReceived()
                    {
                        CompanyId = request.CompanyId,
                        UserId = request.UserId,
                        AccountingPeriodId = accountingPeriod.Id,
                        Model = returnModel
                    });

                    return new Response();
                }
                
                return new Response(HttpStatusCode.BadRequest);
            }
        }

    }
}
