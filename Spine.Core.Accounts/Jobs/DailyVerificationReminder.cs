using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Services;
using Spine.Services.EmailTemplates.Models;

namespace Spine.Core.Accounts.Jobs
{
    public class DailyVerificationReminderCommand : IRequest
    {
    }

    public class DailyVerificationReminderHandler : IRequestHandler<DailyVerificationReminderCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<DailyVerificationReminderHandler> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public DailyVerificationReminderHandler(SpineContext context, IConfiguration config, IEmailSender emailSender, ILogger<DailyVerificationReminderHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
            _emailSender = emailSender;
            _configuration = config;
        }

        public async Task<Unit> Handle(DailyVerificationReminderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var unverifiedAccounts = await _dbContext.Companies.Where(x => !x.IsVerified && !x.IsDeleted).ToListAsync();
                var dueAccounts = unverifiedAccounts.Where(d => (Constants.GetCurrentDateTime().Date - d.CreatedOn.Date).TotalDays > Constants.DaysToStartVerificationReminder)
                                                                            .Select(x => x.Id).ToList();

                foreach (var item in unverifiedAccounts)
                {
                    if (dueAccounts.Contains(item.Id))
                    {
                        //can this be outside of the for loop???
                        var code = await _dbContext.AccountConfirmationTokens.OrderByDescending(x => x.CreatedOn).FirstAsync(x => x.Email == item.Email);

                        var webUrl = _configuration["SpineWeb"];
                        var emailModel = new ConfirmAccountReminder
                        {
                            ActionLink = Constants.GetConfirmAccountLink(webUrl, code.Token),
                            Name = item.Name,
                            Date = item.CreatedOn.Date.AddDays(Constants.DaysToDisableAccount).ToLongDateString()
                        };

                        var emailSent = await _emailSender.SendTemplateEmail(item.Email, $"{emailModel.AppName} - Verify Account Reminder", EmailTemplateEnum.ConfirmAccountReminder, emailModel);
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while sending daily account verification reminder {ex.Message}");
            }

            return Unit.Value;
        }
    }
}