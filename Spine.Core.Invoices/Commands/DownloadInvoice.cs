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
using Spine.Core.Invoices.Helpers;
using Spine.Data;
using Spine.Data.Entities.Invoices;
using Spine.PdfGenerator;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public static class DownloadInvoice
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
            public byte[] PdfByte { get; set; }
            public string InvoiceNo { get; set; }
            public Response(byte[] pdfByte, string invNo)
            {
                PdfByte = pdfByte;
                InvoiceNo = invNo;
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
            private readonly IInvoiceHelper _invoiceHelper;
            private readonly IPdfGenerator _pdfGenerator;

            public Handler(SpineContext context, IAuditLogHelper auditHelper, IInvoiceHelper invoiceHelper, IPdfGenerator pdfGenerator)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _invoiceHelper = invoiceHelper;
                _pdfGenerator = pdfGenerator;
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

                var (pdfByte, paymentLink) = await _invoiceHelper.GenerateInvoicePdf(_pdfGenerator, _dbContext, invoice.CompanyId, invoice, customization);

                _auditHelper.SaveAction(_dbContext, request.CompanyId, new AuditModel
                {
                    EntityType = (int)AuditLogEntityType.Invoice,
                    Action = (int)AuditLogInvoiceAction.Download,
                    UserId = request.UserId,
                    Description = $"Downloaded invoice {invoice.InvoiceNoString} for customer {invoice.CustomerEmail}"
                });

                return await _dbContext.SaveChangesAsync() > 0 ? new Response(pdfByte, invoice.InvoiceNoString) : new Response("Invoice could not be downloaded");
            }
        }

    }
}
