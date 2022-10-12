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

namespace Spine.Core.Inventories.Jobs
{
    public class StockLevelNotificationCommand : IRequest
    {
    }

    public class StockLevelNotificationHandler : IRequestHandler<StockLevelNotificationCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<StockLevelNotificationHandler> _logger;
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;

        public StockLevelNotificationHandler(SpineContext context, IEmailSender emailSender, INotificationService notificationService, ILogger<StockLevelNotificationHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
            _notificationService = notificationService;
            _emailSender = emailSender;
        }

        public async Task<Unit> Handle(StockLevelNotificationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var lowStocks = (await _dbContext.Inventories.Where(x => x.InventoryType == InventoryType.Product
                                                && x.Status == InventoryStatus.Active
                                                && x.QuantityInStock < x.ReorderLevel
                                                && !x.IsDeleted).ToListAsync()).ToLookup(x => x.CompanyId);

                var companyIds = lowStocks.Select(x => x.Key).ToList();
                var businessOwners = (await _dbContext.Users.Where(x => companyIds.Contains(x.CompanyId) && !x.IsDeleted && x.IsBusinessOwner)
                    .Select(x => new { x.Id, x.CompanyId, x.FullName, x.Email })
                    .ToListAsync())
                    .ToLookup(x => x.CompanyId);

                var businessNames = (await _dbContext.Companies.Where(x => companyIds.Contains(x.Id) && !x.IsDeleted && x.IsVerified)
                .Select(x => new { x.Id, x.Name }).ToListAsync()).ToLookup(x => x.Id);


                foreach (var id in companyIds)
                {
                    var companyLowStocks = lowStocks[id].ToList();
                    if (companyLowStocks.Count > 0)
                    {
                        var businessOwner = businessOwners[id].FirstOrDefault();
                        var businessName = businessNames[id].FirstOrDefault()?.Name;
                        var emailModel = new LowStock
                        {
                            Description = $"You have {companyLowStocks.Count} product(s) low on stock",
                            Name = businessOwner.FullName,
                            Model = companyLowStocks.Select(x => new LowStockModel
                            {
                                Description = x.Description,
                                Item = x.Name,
                                LastRestockDate = x.LastRestockDate.ToLongDateString(),
                                Quantity = x.QuantityInStock
                            }).ToList()
                        };

                        var emailSent = await _emailSender.SendTemplateEmail(businessOwner.Email, $"{emailModel.AppName} - Low Stock Notification for {businessName}", EmailTemplateEnum.LowStock, emailModel);
                        if (emailSent) _logger.LogInformation($"sent low stock notification for {companyLowStocks.Count} products to {businessOwner.Email}");
                        else _logger.LogWarning("email sending failed");

                        await _notificationService.Send(NotificationCategory.LowStock, businessOwner.CompanyId, new List<Guid> { businessOwner.Id }, Guid.Empty, emailModel.Description, businessOwner.Id);

                        //push notifications for mobile
                        var deviceTokens = await _dbContext.DeviceTokens.Where(x => x.UserId == businessOwner.Id)
                            .Select(x => x.Token).ToListAsync();
                
                        if (!deviceTokens.IsNullOrEmpty())
                        {
                            await _notificationService.PushMultiNotification(deviceTokens, $"Low Stock Notification",
                                emailModel.Description);
                        }
                       
                    }
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