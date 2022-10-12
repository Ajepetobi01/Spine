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

namespace Spine.Core.Transactions.Jobs
{
    public class SetTransactionReminderCommand : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid ReminderId { get; set; }
    }

    public class SetTransactionReminderHandler : IRequestHandler<SetTransactionReminderCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SetTransactionReminderHandler> _logger;
        private readonly IEmailSender _emailSender;

        public SetTransactionReminderHandler(SpineContext context, IEmailSender emailSender,
                                                    INotificationService notificationService, ILogger<SetTransactionReminderHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
            _emailSender = emailSender;
            _notificationService = notificationService;
        }

        public async Task<Unit> Handle(SetTransactionReminderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var details = await (from rem in _dbContext.TransactionReminders.Where(x => x.CompanyId == request.CompanyId && x.Id == request.ReminderId && !x.IsDeleted)
                                     join trans in _dbContext.Transactions on rem.TransactionId equals trans.Id
                                     join user in _dbContext.Users on rem.CreatedBy equals user.Id
                                     select new
                                     {
                                         rem.Description,
                                         rem.ReminderDate,
                                         TransRef = trans.ReferenceNo,
                                         Amount = trans.Amount,
                                         TransDate = trans.TransactionDate,
                                         UserId = user.Id,
                                         UserName = user.FullName,
                                         UserEmail = user.Email
                                     }).SingleOrDefaultAsync();

                if (details == null)
                {
                    _logger.LogInformation($"Transaction reminder with Id {request.ReminderId} for company Id {request.CompanyId} not found");
                    return Unit.Value;
                }

                var emailModel = new TransactionReminder
                {
                    Description = details.Description,
                    TransRef = details.TransRef,
                    Amount = details.Amount,
                    TransDate = details.TransDate,
                    ReminderDate = details.ReminderDate,
                    Name = details.UserName,
                };

                var emailSent = await _emailSender.SendTemplateEmail(details.UserEmail, $"{emailModel.AppName} - Transaction Reminder {details.TransRef} ", EmailTemplateEnum.TransactionReminder, emailModel);
                if (emailSent) _logger.LogInformation($"sent transaction reminder for transaction  {details.TransRef} to {details.UserEmail}");
                else _logger.LogWarning("email sending failed");

                await _notificationService.Send(NotificationCategory.TransactionReminder, request.CompanyId, new List<Guid> { details.UserId }, request.ReminderId, details.Description, details.UserId);

                //push notifications for mobile
                var deviceTokens = await _dbContext.DeviceTokens.Where(x => x.UserId == details.UserId)
                    .Select(x => x.Token).ToListAsync();
                
                if (!deviceTokens.IsNullOrEmpty())
                {
                    await _notificationService.PushMultiNotification(deviceTokens, $"Transaction Reminder - {details.TransRef}",
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