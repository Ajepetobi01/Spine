using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Core.Invoices.Helpers;
using Spine.Data;
using Spine.PdfGenerator;
using Spine.Services;

namespace Spine.Core.Invoices.Commands
{
    public class SendInvoiceJobCommand : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid CustomizationId { get; set; }
    }

    public class SendInvoiceJobHandler : IRequestHandler<SendInvoiceJobCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<SendInvoiceJobHandler> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IInvoiceHelper _invoiceHelper;
        private readonly IPdfGenerator _pdfGenerator;

        public SendInvoiceJobHandler(SpineContext context, IInvoiceHelper invoiceHelper, IPdfGenerator pdfGenerator,
                                                                        IEmailSender emailSender, ILogger<SendInvoiceJobHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
            _emailSender = emailSender;
            _invoiceHelper = invoiceHelper;
            _pdfGenerator = pdfGenerator;
        }

        public async Task<Unit> Handle(SendInvoiceJobCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var details = await (from inv in _dbContext.Invoices.Where(x => x.CompanyId == request.CompanyId && x.Id == request.InvoiceId && !x.IsDeleted)
                                     join user in _dbContext.Users on inv.CreatedBy equals user.Id
                                     where user.CompanyId == inv.CompanyId
                                     join cust in _dbContext.InvoiceCustomizations on request.CustomizationId equals cust.Id
                                     //     join theme in _dbContext.InvoiceColorThemes on cust.ColorThemeId equals theme.Id
                                     join pref in _dbContext.InvoicePreferences on inv.CompanyId equals pref.CompanyId
                                     where cust.CompanyId == request.CompanyId
                                     select new
                                     {
                                         inv,
                                         cust,
                                         pref,
                                         user,
                                         //  theme.Theme
                                     }).SingleOrDefaultAsync();

                var invoice = details.inv;
                var customization = details.cust;
                var creator = details.user;
                var preference = details.pref;
                if (invoice == null)
                {
                    _logger.LogInformation($"Invoice with Invoice Id {request.InvoiceId} for company Id {request.CompanyId} not found");
                    return Unit.Value;
                }

                var (pdfByte, paymentLink) = await _invoiceHelper.GenerateInvoicePdf(_pdfGenerator, _dbContext, invoice.CompanyId, invoice, customization);

                if (invoice.InvoiceStatus < InvoiceStatus.Sent)
                    invoice.InvoiceStatus = InvoiceStatus.Sent;

                await _dbContext.SaveChangesAsync();

                var emailModel = new Services.EmailTemplates.Models.SendInvoice
                {
                    CompanyLogo = "", //customization.CompanyLogo,
                    Subject = invoice.Subject,
                    Recipient = invoice.CustomerName,
                    InvoiceNo = invoice.InvoiceNoString,
                    DueDate = invoice.DueDate,
                    InvoiceDate = invoice.InvoiceDate,
                    Name = invoice.CustomerName,
                    PaymentLink = paymentLink,
                    EnableDueDate = preference.EnableDueDate,
                    EnablePaymentLink = preference.PaymentLinkEnabled,
                    InvoiceCreator = creator.FullName,
                    InvoiceCreatorEmail = creator.Email,
                };
                var attachments = new List<AttachmentModel>();
                if (pdfByte != null)
                {
                    //  var base64 = Convert.ToBase64String(pdfByte);
                    var stream = new MemoryStream(pdfByte)
                    {
                        Position = 0
                    };
                    stream.Seek(0, SeekOrigin.Begin);
                    attachments.Add(new AttachmentModel
                    {
                        fileName = $"{invoice.InvoiceNoString}.pdf",
                        fileStream = stream
                    });
                }

                var emailSent = await _emailSender.SendTemplateEmail(invoice.CustomerEmail, $"{emailModel.AppName} - Invoice {invoice.InvoiceNoString} ", EmailTemplateEnum.SendInvoice, emailModel, attachments: attachments);
                if (emailSent) _logger.LogInformation($"sent invoice with invoice no {invoice.InvoiceNoString} to {invoice.CustomerEmail}");
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