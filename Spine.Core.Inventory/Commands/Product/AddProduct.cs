using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Core.Inventories.Helper;
using Spine.Core.Inventories.Jobs;
using Spine.Data;
using Spine.Data.Entities.Inventories;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Product
{
    public static class AddProduct
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            //    public Guid? ParentInventoryId { get; set; }

            [Required]
            public string Name { get; set; }
            [Required]
            public Guid? CategoryId { get; set; }
            [RequiredNonDefault]
            public int? QuantityInStock { get; set; }
            [Required]
            public DateTime? InventoryDate { get; set; }

            [Required]
            public string SerialNo { get; set; }

            public string SKU { get; set; }

            [Required]
            public int ReorderLevel { get; set; }

            public string Description { get; set; }

            public int? MeasurementUnitId { get; set; }

            [Required]
            public decimal UnitCostPrice { get; set; }
            [Required]
            public decimal UnitSalesPrice { get; set; }
            public bool NotifyOutOfStock { get; set; }

            // only sent from mobile atm
            public List<AllocateProduct.Allocation> Allocations { get; set; }
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
            private readonly IMapper _mapper;
            private readonly CommandsScheduler _scheduler;

            public Handler(SpineContext context, IMapper mapper, CommandsScheduler scheduler, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _mapper = mapper;
                _scheduler = scheduler;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var accountingPeriod = await _dbContext.AccountingPeriods.FirstOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId
                    && request.InventoryDate.Value.Date >= x.StartDate && request.InventoryDate.Value.Date <= x.EndDate);

                if (accountingPeriod == null) return new Response("Inventory date does not have an accounting period");
                if(accountingPeriod.IsClosed) return new Response("Accounting period for this inventory date is closed");

                
                if (await _dbContext.Inventories.AnyAsync(x => x.CompanyId == request.CompanyId && !x.IsDeleted &&
                    (x.Name.ToLower() == request.Name.ToLower()
                    || x.SerialNo.ToLower() == request.SerialNo.ToLower())))
                {
                    return new Response("Name and Serial Number must be unique");
                }

                if (!request.Allocations.IsNullOrEmpty()
                    && request.Allocations.Sum(x => x.Quantity) > request.QuantityInStock)
                {
                    return new Response("Total allocations cannot be more than product quantity");
                }
                
                var product = _mapper.Map<Inventory>(request);
                _dbContext.Inventories.Add(product);

                _dbContext.ProductStocks.Add(new ProductStock
                {
                    CompanyId = request.CompanyId,
                    CreatedBy = request.UserId,
                    InventoryId = product.Id,
                    UnitSalesPrice = request.UnitSalesPrice,
                    ReorderQuantity = request.QuantityInStock.Value,
                    RestockDate = request.InventoryDate.Value, //DateTime.Today,
                    UnitCostPrice = request.UnitCostPrice
                });
                
                _dbContext.InventoryPriceHistories.Add(new InventoryPriceHistory
                {
                    CompanyId = request.CompanyId,
                    CreatedBy = request.UserId,
                    InventoryId = product.Id,
                    UnitCostPrice = request.UnitCostPrice,
                    UnitSalesPrice = request.UnitSalesPrice,
                    RestockDate = request.InventoryDate.Value,
                    CreatedOn = DateTime.Today
                });

                if (!request.Allocations.IsNullOrEmpty())
                {
                    var allocations = new List<ProductLocation>();
                    foreach (var item in request.Allocations)
                    {
                        allocations.Add(new ProductLocation
                        {
                            CompanyId = request.CompanyId,
                            CreatedBy = request.UserId,
                            InventoryId = product.Id,
                            LocationId = item.LocationId,
                            QuantityInStock = item.Quantity,
                            DateAdded = DateTime.Today
                        });
                    }
                    _dbContext.ProductLocations.AddRange(allocations);
                }

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.Inventory,
                        Action = (int)AuditLogInventoryAction.Create,
                        Description = $"Added new product inventory {request.Name} - {request.SerialNo}",
                        UserId = request.UserId
                    });

                var receivedItems = new List<ReceivedGoodsModel>
                {
                    new ReceivedGoodsModel
                    {
                        AccountingPeriodId = accountingPeriod.Id,
                        Amount = product.UnitCostPrice * request.QuantityInStock.Value,
                        TaxAmount = 0.00m,
                        TaxId = null,
                        VendorId = null,
                        InventoryId = product.Id,
                        Inventory = product.Name,
                        DateReceived = request.InventoryDate.Value,
                        ReceivedBy = request.UserId
                    }
                };

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    _scheduler.SendNow(new HandleAccountingForConfirmGoodsReceived
                    {
                        CompanyId = request.CompanyId,
                        ReceivedGoods = receivedItems,
                        UserId = request.UserId,
                    });

                    return new Response(HttpStatusCode.Created);
                }
                
                return new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
