using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Core.Invoices.Jobs;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public static class SendInvoice
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public Guid Id { get; set; }

            [Required]
            public Guid? CustomizationId { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.OK;
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
                var invoice = await _dbContext.Invoices.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted);
                if (invoice == null) return new Response("Invoice not found");

                var customization = await _dbContext.InvoiceCustomizations.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.CustomizationId);
                if (customization == null) return new Response("Customization not found");

                _scheduler.SendNow(new SendInvoiceJobCommand
                {
                    CompanyId = request.CompanyId,
                    InvoiceId = request.Id,
                    CustomizationId = customization.Id
                }
                    , $"Send Invoice {invoice.InvoiceNoString}");

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Invoice,
                    Action = (int)AuditLogInvoiceAction.Send,
                    UserId = request.UserId,
                    Description = $"Sent invoice {invoice.InvoiceNoString} to {invoice.CustomerEmail}"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Invoice could not be sent");
            }
        }

    }
}
