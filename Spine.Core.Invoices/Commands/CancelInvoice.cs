using System;
using System.Net;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public static class CancelInvoice
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public Guid Id { get; set; }

        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.NoContent;
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
                var invoice = await _dbContext.Invoices.SingleOrDefaultAsync(x =>
                    x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted);

                if (invoice == null) return new Response("Invoice not found");

                if (await _dbContext.InvoicePayments.AnyAsync(x =>
                    x.CompanyId == request.CompanyId && x.InvoiceId == invoice.Id))
                    return new Response("Invoice cannot be cancelled as it has received payment");

                invoice.IsDeleted = true;
                invoice.DeletedBy = request.UserId;

                var lineItems = await _dbContext.LineItems.Where(x =>
                    x.CompanyId == request.CompanyId &&
                    x.ParentItemId == invoice.Id).ToListAsync();

                var productIds = lineItems.Where(x=>x.ItemId.HasValue).Select(x => x.ItemId).ToList();
                var products = await _dbContext.Inventories.Where(x => x.CompanyId == request.CompanyId &&
                                                                       x.InventoryType == InventoryType.Product
                                                                       && productIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id);
                
                foreach (var item in lineItems)
                {
                    var inventory = products[item.ItemId.Value];
                    if (inventory != null)
                        inventory.QuantityInStock += item.Quantity;
                }
                
                var invoiceEntries = await _dbContext.GeneralLedgers.Where(x =>
                    x.CompanyId == request.CompanyId &&
                    x.OrderId == invoice.Id).ToListAsync();

                _dbContext.GeneralLedgers.RemoveRange(invoiceEntries);

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int) AuditLogEntityType.Invoice,
                    Action = (int) AuditLogInvoiceAction.Cancel,
                    UserId = request.UserId,
                    Description = $"Cancelled invoice {invoice.InvoiceNoString}"
                });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response("Invoice could not be cancelled");
            }
        }

    }
}
