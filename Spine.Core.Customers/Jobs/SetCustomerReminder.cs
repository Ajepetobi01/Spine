using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Data;
using Spine.Services;
using Spine.Services.EmailTemplates.Models;

namespace Spine.Core.Customers.Jobs
{
    public class SetCustomerReminderCommand : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid ReminderId { get; set; }
        
    }

    public class SetCustomerReminderHandler : IRequestHandler<SetCustomerReminderCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SetCustomerReminderHandler> _logger;
        private readonly IEmailSender _emailSender;

        public SetCustomerReminderHandler(SpineContext context, IEmailSender emailSender,
                                                    INotificationService notificationService, ILogger<SetCustomerReminderHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
            _emailSender = emailSender;
            _notificationService = notificationService;
        }

        public async Task<Unit> Handle(SetCustomerReminderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var details = await (from rem in _dbContext.CustomerReminders.Where(x => x.CompanyId == request.CompanyId && x.Id == request.ReminderId && !x.IsDeleted)
                                     join cust in _dbContext.Customers on rem.CustomerId equals cust.Id
                                     join user in _dbContext.Users on rem.CreatedBy equals user.Id
                                     select new
                                     {
                                         rem.Description,
                                         rem.ReminderDate,
                                         CustomerName = cust.Name,
                                         CustomerEmail = cust.Email,
                                         UserId = user.Id,
                                         UserName = user.FullName,
                                         UserEmail = user.Email
                                     }).SingleOrDefaultAsync();

                if (details == null)
                {
                    _logger.LogInformation($"Customer reminder with Id {request.ReminderId} for company Id {request.CompanyId} not found");
                    return Unit.Value;
                }

                var emailModel = new CustomerReminder
                {
                    Description = details.Description,
                    CustomerEmail = details.CustomerEmail,
                    CustomerName = details.CustomerName,
                    ReminderDate = details.ReminderDate,
                    Name = details.UserName,
                };

                var emailSent = await _emailSender.SendTemplateEmail(details.UserEmail, $"{emailModel.AppName} - Customer Reminder {details.CustomerName} ", EmailTemplateEnum.CustomerReminder, emailModel);
                if (emailSent) _logger.LogInformation($"sent customer reminder for customer  {details.CustomerEmail} to {details.UserEmail}");
                else _logger.LogWarning("email sending failed");

                await _notificationService.Send(NotificationCategory.CustomerReminder, request.CompanyId, new List<Guid> { details.UserId }, request.ReminderId, details.Description, details.UserId);

                //push notifications for mobile
                var deviceTokens = await _dbContext.DeviceTokens.Where(x => x.UserId == details.UserId)
                    .Select(x => x.Token).ToListAsync();
                
                if (!deviceTokens.IsNullOrEmpty())
                {
                    await _notificationService.PushMultiNotification(deviceTokens, $"Customer Reminder - {details.CustomerEmail}",
                        details.Description);
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while sending reminder  {request.ReminderId} {ex.Message}");
            }

            return Unit.Value;
        }
    }
}