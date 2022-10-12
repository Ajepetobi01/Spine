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
using Spine.Core.Inventories.Helper;
using Spine.Data;
using Spine.PdfGenerator;
using Spine.Services;

namespace Spine.Core.Inventories.Jobs
{
    public class SendOrderJobCommand : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid OrderId { get; set; }
    }

    public class SendOrderJobHandler : IRequestHandler<SendOrderJobCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<SendOrderJobHandler> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IInventoryHelper _inventoryHelper;
        private readonly IPdfGenerator _pdfGenerator;

        public SendOrderJobHandler(SpineContext context, IInventoryHelper invHelper, IPdfGenerator pdfGenerator,
                                                                        IEmailSender emailSender, ILogger<SendOrderJobHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
            _emailSender = emailSender;
            _inventoryHelper = invHelper;
            _pdfGenerator = pdfGenerator;
        }

        public async Task<Unit> Handle(SendOrderJobCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var details = await (from ord in _dbContext.PurchaseOrders.Where(x => x.CompanyId == request.CompanyId && x.Id == request.OrderId && !x.IsDeleted)
                                     join comp in _dbContext.Companies on ord.CompanyId equals comp.Id
                                     where !comp.IsDeleted
                                     select new { ord, comp }).SingleOrDefaultAsync();

                if (details == null) return Unit.Value;

                var order = details.ord;
                var business = details.comp;

                var (pdfByte, orderDate) = await _inventoryHelper.GeneratePurchaseOrderPdf(_pdfGenerator, _dbContext, request.CompanyId, request.OrderId);

                var emailModel = new Services.EmailTemplates.Models.SendPurchaseOrder
                {
                    Recipient = order.VendorName,
                    OrderDate = order.OrderDate,
                    ExpecedDtae = order.ExpectedDate,
                    Name = order.VendorName,
                    CompanyName = business.Name,
                    CompanyEmail = business.Email
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
                        fileName = $"PurchaseOrder-{orderDate}.pdf",
                        fileStream = stream
                    });
                }

                var emailSent = await _emailSender.SendTemplateEmail(order.VendorEmail, $"{emailModel.AppName} - Purchase Order ", EmailTemplateEnum.SendPurchaseOrder, emailModel, attachments: attachments);
                if (emailSent)
                    _logger.LogInformation($"sent purchase order with id {request.OrderId} to {order.VendorEmail}");
                else 
                    _logger.LogWarning("email sending failed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while sending invoice with invoice {request.OrderId} {ex.Message}");
            }

            return Unit.Value;
        }
    }
}