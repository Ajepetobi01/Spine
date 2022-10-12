using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Core.Invoices.Helpers;
using Spine.Data;
using Spine.PdfGenerator;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public class SendInvoiceToMultipleJobCommand : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid CustomizationId { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Body { get; set; }
        [RequiredNotEmpty]
        public List<string> To { get; set; }
        public List<string> CC { get; set; }
        public List<string> BCC { get; set; }

        public List<AttachmentModel> Attachments { get; set; }
    }

    public class SendInvoiceToMultipleJobHandler : IRequestHandler<SendInvoiceToMultipleJobCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<SendInvoiceJobHandler> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IInvoiceHelper _invoiceHelper;
        private readonly IPdfGenerator _pdfGenerator;

        public SendInvoiceToMultipleJobHandler(SpineContext context, IInvoiceHelper invoiceHelper, IPdfGenerator pdfGenerator,
                                                                        IEmailSender emailSender, ILogger<SendInvoiceJobHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
            _emailSender = emailSender;
            _invoiceHelper = invoiceHelper;
            _pdfGenerator = pdfGenerator;
        }

        public async Task<Unit> Handle(SendInvoiceToMultipleJobCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var details = await (from inv in _dbContext.Invoices.Where(x => x.CompanyId == request.CompanyId && x.Id == request.InvoiceId && !x.IsDeleted)
                                     join pref in _dbContext.InvoicePreferences on inv.CompanyId equals pref.CompanyId
                                     join cust in _dbContext.InvoiceCustomizations on request.CustomizationId equals cust.Id
                                     where cust.CompanyId == request.CompanyId
                                     select new { inv, cust, pref }).SingleOrDefaultAsync();

                var invoice = details.inv;
                var customization = details.cust;
                var preference = details.pref;
                if (invoice == null)
                {
                    _logger.LogInformation($"Invoice with Invoice Id {request.InvoiceId} for company Id {request.CompanyId} not found");
                    return Unit.Value;
                }
                if (request.Attachments.IsNullOrEmpty())
                {
                    var (pdfByte, paymentLink) = await _invoiceHelper.GenerateInvoicePdf(_pdfGenerator, _dbContext, invoice.CompanyId, invoice, customization);
                    if (pdfByte != null)
                    {
                        request.Attachments = new List<AttachmentModel>();
                        var stream = new MemoryStream(pdfByte)
                        {
                            Position = 0
                        };
                        stream.Seek(0, SeekOrigin.Begin);
                        request.Attachments.Add(new AttachmentModel
                        {
                            fileName = $"{invoice.InvoiceNoString}.pdf",
                            fileStream = stream
                        });
                    }
                }

                if (invoice.InvoiceStatus < InvoiceStatus.Sent)
                    invoice.InvoiceStatus = InvoiceStatus.Sent;

                await _dbContext.SaveChangesAsync();
                var emailSent = false;
                foreach (var toEmail in request.To)
                {
                    emailSent = await _emailSender.SendTextEmail(toEmail, request.Subject, request.Body, true, request.CC, request.BCC, request.Attachments);
                }

                if (emailSent) _logger.LogInformation($"sent invoice with invoice no {invoice.InvoiceNoString} to {string.Join(", ", request.To)}");
                else _logger.LogWarning("email sending failed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while sending invoice with invoice {request.InvoiceId} {ex.Message}");
            }

            return Unit.Value;
        }
    }
}