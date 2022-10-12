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
using Spine.Data.Entities.Inventories;
using Spine.Services;

namespace Spine.Core.Inventories.Commands
{
    public static class AdjustInventory
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore] public Guid CompanyId { get; set; }
            [JsonIgnore] public Guid UserId { get; set; }
            [JsonIgnore] public AdjustmentType? AdjustmentType { get; set; }

            [Required] public DateTime? AdjustmentDate { get; set; }
            [RequiredNotEmpty] public string Description { get; set; }
            public string Reason { get; set; }

            [RequiredNotEmpty] public List<AdjustmentModel> Model { get; set; }

        }

        public class AdjustmentModel
        {
            [Required]
            public Guid? InventoryId { get; set; }
        //    [RequiredIf(nameof(AdjustmentType), AdjustmentType.Quantity, ErrorMessage = "Quantity is required")]
            public int? NewQuantity { get; set; }
       //     [RequiredIf(nameof(AdjustmentType), AdjustmentType.Cost, ErrorMessage = "Cost Price is required")]
            public decimal? NewCostPrice { get; set; }
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

            public Handler(SpineContext context, CommandsScheduler scheduler, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _scheduler = scheduler;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var inventoryIds = request.Model.Select(x => x.InventoryId).ToHashSet();
                if (inventoryIds.Count != request.Model.Count)
                    return new Response("Products must be unique");
                
                var accountingPeriod = await _dbContext.AccountingPeriods.FirstOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId
                    && request.AdjustmentDate.Value.Date >= x.StartDate && request.AdjustmentDate.Value.Date <= x.EndDate);

                if (accountingPeriod == null) return new Response("Adjustment date does not have an accounting period");
                if(accountingPeriod.IsClosed) return new Response("Accounting period for this adjustment date is closed");

                var inventories = await _dbContext.Inventories.Where(x => x.CompanyId == request.CompanyId
                                                                          && inventoryIds.Contains(x.Id) && !x.IsDeleted).ToDictionaryAsync(x=>x.Id);

                var desc = "";
                var invAdjustment = new List<InventoryAdjustmentModel>();
                foreach (var item in request.Model)
                {
                    var inv = inventories[item.InventoryId.Value];
                    if (inv == null)
                        return new Response("Inventory does not exist");

                    if (inv.InventoryType != InventoryType.Product)
                        return new Response("Adjustment can only be done for a product");
                    
                    if (inv.Status != InventoryStatus.Active)
                        return new Response("Inventory is not in Active status");

                    if (request.AdjustmentType == AdjustmentType.Quantity)
                    {
                        if (item.NewQuantity is null or < 0)
                            return new Response("Quantity is required");
                        
                        var qtyToAdd = item.NewQuantity.Value - inv.QuantityInStock ; //could be -ve if it's a reduction
                        
                        if (qtyToAdd == 0)
                            continue;
                        
                        if (qtyToAdd < 0 && inv.QuantityInStock < qtyToAdd) return new Response("Final stock cannot be less than 0");
                        
                        var newStock = new ProductStock
                        {
                            CompanyId = request.CompanyId,
                            CreatedBy = request.UserId,
                            InventoryId = inv.Id,
                            UnitSalesPrice = inv.UnitSalesPrice,
                            ReorderQuantity = qtyToAdd,
                            RestockDate = request.AdjustmentDate.Value,
                            UnitCostPrice = inv.UnitCostPrice,
                            CreatedOn = DateTime.Today,
                            Description = request.Description,
                            Reason = request.Reason
                        };
                        _dbContext.ProductStocks.Add(newStock);

                        inv.LastRestockDate = newStock.RestockDate;
                        inv.QuantityInStock += newStock.ReorderQuantity;
                        
                        desc = $"Adjusted quantity of inventory {inv.Name} with {qtyToAdd}";
                        
                        invAdjustment.Add(new InventoryAdjustmentModel
                        {
                            Type = request.AdjustmentType.Value,
                            IsAddition = qtyToAdd > 0,
                            Name = inv.Name,
                            Amount = Math.Abs(qtyToAdd) * inv.UnitCostPrice,
                            Id = inv.Id,
                            Date = request.AdjustmentDate.Value,
                            Quantity = qtyToAdd
                        });
                    }
                    
                    else if (request.AdjustmentType == AdjustmentType.Cost)
                    {
                        if (item.NewCostPrice is null or < 0)
                            return new Response("Cost price is required");
                        
                        var qtyToAdd =  item.NewCostPrice.Value - inv.UnitCostPrice; //could be -ve if it's a reduction
                        
                        if (qtyToAdd == 0)
                            continue;
                        
                        _dbContext.InventoryPriceHistories.Add(new InventoryPriceHistory
                        {
                            CompanyId = request.CompanyId,
                            CreatedBy = request.UserId,
                            InventoryId = inv.Id,
                            UnitCostPrice = item.NewCostPrice.Value,
                            RestockDate = request.AdjustmentDate.Value,
                            CreatedOn = DateTime.Today,
                            UnitSalesPrice = 0.00m,
                            Description = request.Description,
                            Reason = request.Reason
                        });
                        
                        desc = $"Adjusted cost price of inventory {inv.Name} from {inv.UnitCostPrice} to {item.NewCostPrice}";
                        inv.UnitCostPrice = item.NewCostPrice.Value;
                        
                        invAdjustment.Add(new InventoryAdjustmentModel
                        {
                            Type = request.AdjustmentType.Value,
                            IsAddition = qtyToAdd > 0,
                            Name = inv.Name,
                            Amount = Math.Abs(qtyToAdd) * inv.QuantityInStock,
                            Id = inv.Id,
                            Date = request.AdjustmentDate.Value,
                        });
                    }

                    _auditHelper.SaveAction(_dbContext, request.CompanyId,
                        new AuditModel
                        {
                            EntityType = (int) AuditLogEntityType.Inventory,
                            Action = (int) AuditLogInventoryAction.AdjustInventory,
                            Description = desc,
                            UserId = request.UserId
                        });
                }

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    _scheduler.SendNow(new HandleAccountingForInventoryAdjustment
                    {
                        CompanyId = request.CompanyId,
                        UserId = request.UserId,
                        AccountingPeriodId = accountingPeriod.Id,
                        Model = invAdjustment
                    });
                    return new Response();
                }

                return new Response("adjustment failed");
            }
        }
    }
}
