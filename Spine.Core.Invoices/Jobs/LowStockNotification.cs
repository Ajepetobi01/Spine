using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Data;
using Spine.Services;
using Spine.Services.EmailTemplates.Models;
using System.Collections.Generic;
using Spine.Common.Extensions;

namespace Spine.Core.Invoices.Jobs
{
    public class LowStockNotificationCommand : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid InventoryId { get; set; }
    }

    public class LowStockNotificationHandler : IRequestHandler<LowStockNotificationCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<LowStockNotificationHandler> _logger;
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;

        public LowStockNotificationHandler(SpineContext context, IEmailSender emailSender, INotificationService notificationService, ILogger<LowStockNotificationHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
            _notificationService = notificationService;
            _emailSender = emailSender;
        }

        public async Task<Unit> Handle(LowStockNotificationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var lowStock = await _dbContext.Inventories.Where(x => x.CompanyId == request.CompanyId
                                                && x.InventoryType == InventoryType.Product
                                                && x.Status == InventoryStatus.Active
                                                && x.Id == request.InventoryId
                                                && !x.IsDeleted).SingleOrDefaultAsync();

                var businessOwner = await _dbContext.Users.SingleAsync(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.IsBusinessOwner);

                var emailModel = new LowStock
                {
                    Description = $"Product {lowStock.Name} with serial Number {lowStock.SerialNo} is low on stock",
                    Name = businessOwner.FullName,
                    Model = new List<LowStockModel>
                    {
                        new LowStockModel
                            {
                                Description = lowStock.Description,
                                Item = lowStock.Name,
                                LastRestockDate = lowStock.LastRestockDate.ToLongDateString(),
                                Quantity = lowStock.QuantityInStock
                            }
                    }
                };

                var emailSent = await _emailSender.SendTemplateEmail(businessOwner.Email, $"{emailModel.AppName} - Low Stock Notification ", EmailTemplateEnum.LowStock, emailModel);
                if (emailSent) _logger.LogInformation($"sent low stock notification for {lowStock.Name} ");
                else _logger.LogWarning("email sending failed");
                await _notificationService.Send(NotificationCategory.LowStock, request.CompanyId, new List<Guid> { businessOwner.Id }, request.InventoryId, emailModel.Description, businessOwner.Id);

                //push notifications for mobile
                var deviceTokens = await _dbContext.DeviceTokens.Where(x => x.UserId == businessOwner.Id)
                    .Select(x => x.Token).ToListAsync();
                
                if (!deviceTokens.IsNullOrEmpty())
                {
                    await _notificationService.PushMultiNotification(deviceTokens, $"Low Stock Notification for {lowStock.Name}",
                        emailModel.Description);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while sending low stock notification {ex.Message}");
            }

            return Unit.Value;
        }
    }
}