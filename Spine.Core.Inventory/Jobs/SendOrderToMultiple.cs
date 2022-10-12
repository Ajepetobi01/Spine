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
using Spine.Core.Inventories.Helper;
using Spine.Data;
using Spine.PdfGenerator;
using Spine.Services;

namespace Spine.Core.Inventories.Jobs
{
    public class SendOrderToMultipleJobCommand : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid OrderId { get; set; }
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

    public class SendOrderToMultipleJobHandler : IRequestHandler<SendOrderToMultipleJobCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<SendOrderToMultipleJobHandler> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IInventoryHelper _inventoryHelper;
        private readonly IPdfGenerator _pdfGenerator;

        public SendOrderToMultipleJobHandler(SpineContext context, IInventoryHelper invHelper, IPdfGenerator pdfGenerator,
                                                                        IEmailSender emailSender, ILogger<SendOrderToMultipleJobHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
            _emailSender = emailSender;
            _inventoryHelper = invHelper;
            _pdfGenerator = pdfGenerator;
        }

        public async Task<Unit> Handle(SendOrderToMultipleJobCommand request, CancellationToken cancellationToken)
        {
            try
            {

                    var details = await _dbContext.PurchaseOrders.Where(x => x.CompanyId == request.CompanyId && x.Id == request.OrderId && !x.IsDeleted).SingleOrDefaultAsync();
                if (request.Attachments.IsNullOrEmpty())
                {

                    var (pdfByte, orderDate) = await _inventoryHelper.GeneratePurchaseOrderPdf(_pdfGenerator, _dbContext, request.CompanyId, request.OrderId);

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
                            fileName = $"PurchaseOrder-{orderDate}.pdf",
                            fileStream = stream
                        });
                    }
                }

                var emailSent = false;
                foreach (var toEmail in request.To)
                {
                    emailSent = await _emailSender.SendTextEmail(toEmail, request.Subject, request.Body, true, request.CC, request.BCC, request.Attachments);
                }

                if (emailSent) 
                    _logger.LogInformation($"sent purchase order  {request.OrderId} to {string.Join(", ", request.To)}");
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