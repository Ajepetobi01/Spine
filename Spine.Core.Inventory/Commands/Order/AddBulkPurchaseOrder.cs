using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentEmail.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Attributes;
using Spine.Common.Converters;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Core.Inventories.Helper;
using Spine.Data;
using Spine.Data.Entities.Inventories;
using Spine.Services;

namespace Spine.Core.Inventories.Commands.Order
{
    public static class AddBulkPurchaseOrder
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [RequiredNotEmpty]
            public List<PurchaseOrderModel> OrderItems { get; set; }
        }

        public class PurchaseOrderModel
        {
            [Required]
            public string Vendor { get; set; }
            
            [Required]
            public string Product { get; set; }

            [RequiredNonDefault] 
            [JsonConverter(typeof(StringToIntConverter))]
            public int Quantity { get; set; }
            
            [Required]
            [JsonConverter(typeof(StringToDecimalConverter))]
            public decimal Rate { get; set; }

            [Required] 
            [JsonConverter(typeof(DateTimeConverterFactory))]
            public DateTime OrderDate { get; set; }

            [JsonConverter(typeof(DateTimeConverterFactory))]
            public DateTime? ExpectedDate { get; set; }

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
            private readonly ISerialNumberHelper _seriallHelper;

            public Handler(SpineContext context, IAuditLogHelper auditHelper, ISerialNumberHelper seriallHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _seriallHelper = seriallHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var items = request.OrderItems.GroupBy(x => new { x.OrderDate, x.Vendor }).Select(x => new
                {
                    x.Key.Vendor,
                    x.Key.OrderDate,
                    x.First().ExpectedDate,
                    LineItems = x.Select(y => new
                    {
                        y.Product,
                        y.Quantity,
                        y.Rate
                    }).ToList()
                }).ToList();

                var allInventory = await _dbContext.Inventories.Where(x =>
                        x.CompanyId == request.CompanyId && x.InventoryType == InventoryType.Product && !x.IsDeleted)
                    .Select(x => new {x.Id, x.Name, x.Description}).ToDictionaryAsync(x => x.Name);

                var allVendors = await _dbContext.Vendors.Where(x=>x.CompanyId == request.CompanyId && x.Status == Status.Active && !x.IsDeleted)
                    .Select(x=> new {x.Id, x.Email, Name = x.Name + " - " + x.DisplayName })
                    .ToDictionaryAsync(x=>x.Name);
                
                var lastUsed =
                    await _seriallHelper.GetLastUsedPurchaseOrderNo(_dbContext, request.CompanyId, items.Count);
                
                foreach (var item in items)
                {
                    allVendors.TryGetValue(item.Vendor, out var vendor);
                    
                    // if(vendor == null)
                    //     continue;
                    
                    lastUsed++;
                    var order = new PurchaseOrder
                    {
                        CompanyId = request.CompanyId,
                        Id = SequentialGuid.Create(),
                        Status = PurchaseOrderStatus.Draft,
                        CreatedBy = request.UserId,
                        AdditionalNote = "",
                        OrderAmount = 0,
                        OrderDate = item.OrderDate,
                        ExpectedDate = item.ExpectedDate,
                        VendorEmail = vendor?.Email,
                        VendorName = vendor?.Name,
                        VendorId = vendor?.Id,
                        OrderNo = Constants.GenerateSerialNo(Constants.SerialNoType.PO, lastUsed)
                    };

                    _dbContext.PurchaseOrders.Add(order);
                    foreach (var lineItem in item.LineItems)
                    {
                        if (allInventory.TryGetValue(lineItem.Product, out var inv))
                        {
                            var amount = lineItem.Rate * lineItem.Quantity;
                            order.OrderAmount += amount;
                            _dbContext.LineItems.Add(new Data.Entities.LineItem
                            {
                                CompanyId = request.CompanyId,
                                Amount = amount,
                                Description = inv.Description,
                                ItemId = inv.Id,
                                ParentItemId = order.Id,
                                Item = inv.Name,
                                Quantity = lineItem.Quantity,
                                Rate = lineItem.Rate,
                                CreatedOn = DateTime.Now
                            });
                        }
                    }

                    _auditHelper.SaveAction(_dbContext, request.CompanyId,
                  new AuditModel
                  {
                      EntityType = (int)AuditLogEntityType.PurchaseOrder,
                      Action = (int)AuditLogPurchaseOrderAction.Create,
                      Description = $"Add new purchase order for vendor {item.Vendor}",
                      UserId = request.UserId
                  });
                }

                return await _dbContext.SaveChangesAsync() > 0 
                    ? new Response() 
                    : new Response("No records saved");
            }
        }
    }
}
