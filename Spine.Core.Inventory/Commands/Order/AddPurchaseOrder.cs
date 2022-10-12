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
    public static class AddPurchaseOrder
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public bool Send { get; set; }

            [Required]
            public Guid? VendorId { get; set; }
            
            public string AdditionalNote { get; set; }
            [RequiredNonDefault]
            public DateTime? OrderDate { get; set; }
            public DateTime? ExpectedDate { get; set; }
            [RequiredNotEmpty]
            public List<LineItemModel> LineItems { get; set; }
        }

        public class LineItemModel
        {
            public Guid InventoryId { get; set; }
            public string Item { get; set; }
            public string Description { get; set; }
            public int Quantity { get; set; }
            public decimal Rate { get; set; }
            
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
            private readonly CommandsScheduler _scheduler;
            private readonly ISerialNumberHelper _serialHelper;

            public Handler(SpineContext context, IAuditLogHelper auditHelper, ISerialNumberHelper serialHelper, CommandsScheduler scheduler)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _scheduler = scheduler;
                _serialHelper = serialHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var lastUsed =
                    await _serialHelper.GetLastUsedPurchaseOrderNo(_dbContext, request.CompanyId, 1);

                var vendor = await _dbContext.Vendors
                    .Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.VendorId)
                    .Select(x => new {x.Email, x.Name}).SingleOrDefaultAsync();

                var order = new PurchaseOrder
                {
                    CompanyId = request.CompanyId,
                    Id = SequentialGuid.Create(),
                    Status = request.Send ? PurchaseOrderStatus.Issued :PurchaseOrderStatus.Draft,
                    CreatedBy = request.UserId,
                    AdditionalNote = request.AdditionalNote,
                    OrderAmount = 0,
                    OrderDate = request.OrderDate.Value,
                    ExpectedDate = request.ExpectedDate,
                    VendorId = request.VendorId,
                    VendorEmail = vendor?.Email,
                    VendorName = vendor?.Name,
                    OrderNo = Constants.GenerateSerialNo(Constants.SerialNoType.PO, lastUsed + 1)
                };

                foreach (var item in request.LineItems)
                {
                    var amount = item.Rate * item.Quantity;
                    var taxAmount = (item.TaxRate * amount) / 100;
                    order.OrderAmount += (amount + taxAmount);
                    _dbContext.LineItems.Add(new Data.Entities.LineItem
                    {
                        CompanyId = request.CompanyId,
                        Amount = amount + taxAmount,
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
                }

                _dbContext.PurchaseOrders.Add(order);

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.PurchaseOrder,
                        Action = (int)AuditLogPurchaseOrderAction.Create,
                        Description = $"Add new purchase order for vendor {vendor?.Name}",
                        UserId = request.UserId
                    });

                if (await _dbContext.SaveChangesAsync() > 0)
                {
                    if (request.Send)
                    {
                        _scheduler.SendNow(new SendOrderJobCommand
                        {
                            CompanyId = request.CompanyId,
                            OrderId = order.Id,
                        },
                $"Send Purchase Order {order.Id}");
                    }
                    return new Response();
                }
                return new Response("Unable to add purchase order");

            }
        }

    }
}
