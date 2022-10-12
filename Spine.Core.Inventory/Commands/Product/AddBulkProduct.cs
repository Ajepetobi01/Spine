using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Spine.Common.Converters;
using Spine.Common.Enums;
using Spine.Core.Inventories.Helper;
using Spine.Core.Inventories.Jobs;
using Spine.Data;
using Spine.Data.Entities.Inventories;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Product
{
    public static class AddBulkProduct
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            public List<ProductModel> Products { get; set; }

        }

        public class ProductModel
        {
            [Required]
            [Description("Category")]
            public string Category { get; set; }

            [Required]
            [Description("Name")]
            public string Name { get; set; }

            [Description("Description")]
            public string Description { get; set; }

            [Required]
            [Description("Serial Number")]
            public string SerialNumber { get; set; }

            [RequiredNonDefault]
            [Description("Quantity")]
            [JsonConverter(typeof(StringToIntConverter))]
            public int Quantity { get; set; }

            [Required]
            [Description("Inventory Date")]
            [JsonConverter(typeof(DateTimeConverterFactory))]
            public DateTime InventoryDate { get; set; }

            [Required]
            [Description("Cost Price")]
            [JsonConverter(typeof(StringToDecimalConverter))]
            public decimal CostPrice { get; set; }

            [Required]
            [Description("Sales Price")]
            [JsonConverter(typeof(StringToDecimalConverter))]
            public decimal SalesPrice { get; set; }

            [Description("Stock Keeping Unit")]
            public string StockKeepingUnit { get; set; }

            [Required]
            [Description("Reorder Level")]
            [JsonConverter(typeof(StringToIntConverter))]
            public int ReorderLevel { get; set; }

        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.Created;
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
                var distinctSerial = request.Products.Select(x => x.SerialNumber).ToHashSet();
                if (distinctSerial.Count != request.Products.Count) return new Response("Serial Number cannot contain duplicates");

                var accountingPeriods = await _dbContext.AccountingPeriods.Where(x =>
                    x.CompanyId == request.CompanyId).ToListAsync();
                    
                var productSerials = await _dbContext.Inventories.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                    .Select(x => x.SerialNo).ToListAsync();

                var categories = await _dbContext.ProductCategories.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                   .Select(x => new { x.Id, x.Name }).ToDictionaryAsync(x => x.Name);

                int skipped = 0;
                var receivedItems = new List<ReceivedGoodsModel>();
                foreach (var item in request.Products)
                {
                    if (productSerials.Contains(item.SerialNumber))
                    {
                        skipped++;
                        continue;
                    }

                    var accountingPeriod = accountingPeriods.FirstOrDefault(x =>
                        item.InventoryDate.Date >= x.StartDate && item.InventoryDate.Date <= x.EndDate);
                    
                    if (accountingPeriod == null) return new Response($"{item.InventoryDate} does not have an accounting period");
                    if (accountingPeriod.IsClosed) return new Response($"Accounting period for {item.InventoryDate} is closed");

                    if (categories.TryGetValue(item.Category, out var cat))
                    {
                        var product = _mapper.Map<Inventory>(item);
                        product.CategoryId = cat.Id;
                        product.CreatedBy = request.UserId;
                        product.CompanyId = request.CompanyId;

                        _dbContext.Inventories.Add(product);

                        _dbContext.ProductStocks.Add(new ProductStock
                        {
                            CompanyId = request.CompanyId,
                            CreatedBy = request.UserId,
                            InventoryId = product.Id,
                            UnitSalesPrice = item.SalesPrice,
                            ReorderQuantity = item.Quantity,
                            RestockDate = item.InventoryDate,
                            UnitCostPrice = item.CostPrice,
                            CreatedOn = DateTime.Today
                        });
                        
                        _dbContext.InventoryPriceHistories.Add(new InventoryPriceHistory
                        {
                            CompanyId = request.CompanyId,
                            CreatedBy = request.UserId,
                            InventoryId = product.Id,
                            UnitCostPrice = item.CostPrice,
                            UnitSalesPrice = item.SalesPrice,
                            RestockDate = item.InventoryDate,
                            CreatedOn = DateTime.Today
                        });

                        receivedItems.Add(new ReceivedGoodsModel
                        {
                            AccountingPeriodId = accountingPeriod.Id,
                            Amount = product.UnitCostPrice * item.Quantity,
                            TaxAmount = 0.00m,
                            TaxId = null,
                            VendorId = null,
                            InventoryId = product.Id,
                            Inventory = product.Name,
                            DateReceived = product.InventoryDate,
                            ReceivedBy = request.UserId
                        });
                        
                        _auditHelper.SaveAction(_dbContext, request.CompanyId,
                          new AuditModel
                          {
                              EntityType = (int)AuditLogEntityType.Inventory,
                              Action = (int)AuditLogInventoryAction.Create,
                              Description = $"Added new product {product.Name} with {product.Id}",
                              UserId = request.UserId
                          });
                    }
                }

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
                
                return new Response("No records saved");
            }
        }

    }
}
