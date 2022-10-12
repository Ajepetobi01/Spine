using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

namespace Spine.Core.Inventories.Commands.Product
{
    public static class RestockProduct
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public Guid InventoryId { get; set; }

            [RequiredNonDefault]
            public int ReorderQuantity { get; set; }

            [Required]
            public DateTime? RestockDate { get; set; }

            [Required]
            public decimal UnitCostPrice { get; set; }
            [Required]
            public decimal UnitSalesPrice { get; set; }
            public bool NotifyOutOfStock { get; set; }

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
                var accountingPeriod = await _dbContext.AccountingPeriods.FirstOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId
                    && request.RestockDate.Value.Date >= x.StartDate && request.RestockDate.Value.Date <= x.EndDate);

                if (accountingPeriod == null) return new Response("Order date does not have an accounting period");
                if(accountingPeriod.IsClosed) return new Response("Accounting period for this order date is closed");

                var product = await _dbContext.Inventories.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId
                        && x.InventoryType == InventoryType.Product && x.Id == request.InventoryId && !x.IsDeleted);

                if (product == null) return new Response("Product not found");

                if (product.Status != InventoryStatus.Active) return new Response("Product is not in active status");
                var newStock = new ProductStock
                {
                    CompanyId = request.CompanyId,
                    CreatedBy = request.UserId,
                    InventoryId = request.InventoryId,
                    UnitSalesPrice = request.UnitSalesPrice,
                    ReorderQuantity = request.ReorderQuantity,
                    RestockDate = request.RestockDate.Value,
                    UnitCostPrice = request.UnitCostPrice,
                    CreatedOn = DateTime.Today
                };
                _dbContext.ProductStocks.Add(newStock);

                product.LastRestockDate = newStock.RestockDate;
                product.QuantityInStock += newStock.ReorderQuantity;

                if (request.NotifyOutOfStock)
                    product.NotifyOutOfStock = request.NotifyOutOfStock;

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.Inventory,
                        Action = (int)AuditLogInventoryAction.Restock,
                        Description = $"Added {request.ReorderQuantity} stock(s) for product inventory {product.Name } - {product.SerialNo}",
                        UserId = request.UserId
                    });

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    _scheduler.SendNow(new HandleAccountingForInventoryAdjustment
                    {
                        CompanyId = request.CompanyId,
                        UserId = request.UserId,
                        AccountingPeriodId = accountingPeriod.Id,
                        Model = new List<InventoryAdjustmentModel>
                        {
                            new InventoryAdjustmentModel
                            {
                                Type = AdjustmentType.Quantity,
                                IsAddition = true,
                                Name = product.Name,
                                Amount = request.ReorderQuantity * request.UnitCostPrice,
                                Id = product.Id,
                                Date = request.RestockDate.Value,
                                Quantity = request.ReorderQuantity,
                            }
                        }
                    });
                    return new Response();
                }
                
                return new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
