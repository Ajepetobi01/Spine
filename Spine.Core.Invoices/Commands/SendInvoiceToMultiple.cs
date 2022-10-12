using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Core.Invoices.Jobs;
using Spine.Data;
using Spine.Data.Entities.Invoices;
using Spine.Services;
using Spine.Services.Attributes;

namespace Spine.Core.Invoices.Commands
{
    public static class SendInvoiceToMultiple
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
            public string Subject { get; set; }
            [Required]
            public string Body { get; set; }

            [Required]
            public string To { get; set; }
            public string CC { get; set; }
            public string BCC { get; set; }

            //[FileExtensions(Extensions = "jpg,png,jpeg,pdf,doc,docx,xls,xlsx")]
            //[FileSize(1024)]
            //public List<IFormFile> Attachments { get; set; }
            //public IFormFile Attach { get; set; }

            [MaxFileSize(2 * 1024 * 1024)] //2MB
            [AllowedExtensions(new string[] { ".pdf", ".doc", ".docx", ".png", ".jpeg", ".jpg", })]
            public IFormFileCollection Attachments { get; set; }
            [Required]
            public Guid? CustomizationId { get; set; }

            [JsonIgnore]
            public List<AttachmentModel> ConvertedAttachments { get; set; }
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

                InvoiceCustomization customization = null;
                if (invoice.InvoiceStatus >= InvoiceStatus.Sent) //if invoice has been sent, use the customization that was it was sent with instead of default
                {
                    var sentInvoice = await _dbContext.SentInvoices.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.InvoiceId == invoice.Id);
                    if (sentInvoice != null)
                        customization = await _dbContext.InvoiceCustomizations.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == sentInvoice.CustomizationId);
                }
                else
                    customization = await _dbContext.InvoiceCustomizations.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.CustomizationId);

                if (customization == null) return new Response("Customization not found");

                _scheduler.SendNow(new SendInvoiceToMultipleJobCommand
                {
                    CompanyId = request.CompanyId,
                    InvoiceId = request.Id,
                    CustomizationId = customization.Id,
                    Subject = request.Subject,
                    Body = request.Body,
                    To = request.To.Split(',').ToList(),
                    CC = request.CC?.Split(',').ToList(),
                    BCC = request.BCC?.Split(',').ToList(),
                    Attachments = request.ConvertedAttachments
                }
                    , $"Send Invoice {invoice.InvoiceNoString} to multiple contact");

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Invoice,
                    Action = (int)AuditLogInvoiceAction.Send,
                    UserId = request.UserId,
                    Description = $"Sent invoice {invoice.InvoiceNoString} to {string.Join(", ", request.To)}"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response() : new Response("Invoice could not be sent");
            }
        }

    }
}
