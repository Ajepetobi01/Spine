using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Common.Helpers;
using Spine.Core.ManageSubcription.Interface;
using Spine.Core.ManageSubcription.ViewModel;
using Spine.Data;
using Spine.Services;
using Spine.Services.EmailTemplates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.Services
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SpineContext context;
        private readonly IEmailSender emailSender;
        private readonly IConfiguration configuration;
        private readonly ILogger<NotificationRepository> _logger;
        private readonly INotificationService _notificationService;

        public NotificationRepository(SpineContext context, IEmailSender emailSender, IConfiguration configuration, ILogger<NotificationRepository> logger)
        {
            this.context = context;
            this.emailSender = emailSender;
            this.configuration = configuration;
            _logger = logger;
        }

        public async Task<string> ScheduleNotification(NotificationParam model)
        {
            try
            {
                var details = await (from rem in context.SubscriberNotifications.Where(x => x.CompanyId == model.CompanyId && x.ID == model.ReminderId && !x.IsDeleted)
                                     join user in context.Users on rem.CreatedBy equals user.Id
                                     select new
                                     {
                                         rem.Description,
                                         rem.ReminderDate,
                                         UserId = user.Id,
                                         UserName = user.FullName,
                                         CompanyId = model.CompanyId,
                                         UserEmail = user.Email,
                                     }).SingleOrDefaultAsync();

                if (details == null)
                {
                    _logger.LogInformation($"Notification reminder with Id {model.ReminderId} for company Id {model.CompanyId} not found");
                    return "";
                }

                var emailModel = new Notification
                {
                    Description = details.Description,
                    UserEmail = details.UserEmail,
                    UserName = details.UserName,
                    ReminderDate = details.ReminderDate,
                    Name = details.UserName,
                };
                //details.UserEmail
                var emailSent = await emailSender.SendTemplateEmail(details.UserEmail, $"{emailModel.AppName} - Notifiction Reminder {details.UserName} ", EmailTemplateEnum.Notification, emailModel);
                if (emailSent) _logger.LogInformation($"sent notification reminder for user  {details.UserEmail} to {details.UserEmail}");
                else _logger.LogWarning("email sending failed");

                //await _notificationService.Send(NotificationCategory.None, model.CompanyId, new List<Guid> { details.CompanyId }, model.ReminderId, details.Description, details.CompanyId);

                //push notifications for mobile
                var deviceTokens = await context.DeviceTokens.Where(x => x.UserId == details.CompanyId)
                    .Select(x => x.Token).ToListAsync();

                if (!deviceTokens.IsNullOrEmpty())
                {
                    await _notificationService.PushMultiNotification(deviceTokens, $"Notification Reminder - {details.UserEmail}",
                        details.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while sending reminder  {model.ReminderId} {ex.Message}");
            }
            return "";
        }

        public async Task<string> AlmostExpirySubscription()
        {
            var companySubscriptions = context.CompanySubscriptions;

            var OneYearOrSixMonthSubscriptions = (from su in companySubscriptions
                                                  join pl in context.Plans on su.ID_Plan equals pl.PlanId
                                                  where (pl.PlanDuration == 12 || pl.PlanDuration == 6) && su.ExpiredDate > DateTime.Now && su.ExpiredDate.Value.AddMonths(-2).Date <= DateTime.Now.Date
                                                  select new SubscriptionNotifications
                                                  {
                                                      IdSubscription = su.ID_Subscription,
                                                      IdCompany = su.ID_Company
                                                  });

            var OneMonthSubscriptions = (from su in companySubscriptions
                                         join pl in context.Plans on su.ID_Plan equals pl.PlanId
                                         where (pl.PlanDuration == 1) && su.ExpiredDate > DateTime.Now && su.ExpiredDate.Value.AddDays(21).Date <= DateTime.Now.Date
                                         select new SubscriptionNotifications
                                         {
                                             IdSubscription = su.ID_Subscription,
                                             IdCompany = su.ID_Company
                                         });

            List<SubscriptionNotifications> compositeSubscription = new List<SubscriptionNotifications>();

            if (OneYearOrSixMonthSubscriptions.Any() && !OneMonthSubscriptions.Any())
            {
                compositeSubscription = OneYearOrSixMonthSubscriptions.ToList();
            }

            if (!OneYearOrSixMonthSubscriptions.Any() && OneMonthSubscriptions.Any())
            {
                compositeSubscription = OneMonthSubscriptions.ToList();
            }

            if (OneYearOrSixMonthSubscriptions.Any() && OneMonthSubscriptions.Any())
            {
                compositeSubscription = OneMonthSubscriptions.Concat(OneYearOrSixMonthSubscriptions).ToList();
            }

            var subscriptionIds = compositeSubscription.Select(x => x.IdSubscription).ToList();

            foreach (var id in subscriptionIds)
            {
                var subscription = companySubscriptions.Find(id);
                var plan = context.Plans.Find(subscription.ID_Plan);
                var user = context.Users.Where(x => x.CompanyId == subscription.ID_Company).FirstOrDefault();
                var emailModel = new SubscriptionReminder
                {
                    UserName = user.FullName,
                    UserEmail = user.Email,
                    Plan = plan.PlanName,
                    Duration = $"{plan.PlanDuration} Month(s)",
                    SubscriptionDate = DateTime.Now.ToString("dd/MM/yyyy"),
                    Message = "Your subscription will expire on " + subscription.ExpiredDate.Value.ToString("dd/MM/yyyy")
                };
                var emailSent = await emailSender.SendTemplateEmail(user.Email, $"{emailModel.AppName} - Subscription ", EmailTemplateEnum.SubscriptionReminder, emailModel);

            }
            return "";
        }

        public string DisabledExpirySubscription()
        {
            var subscriptions = (context.CompanySubscriptions
                .Where(x => x.ExpiredDate.Value < DateTime.Now)).ToLookup(x => x.ID_Subscription);

            var subscriptionIds = subscriptions.Select(x => x.Key).ToList();

            foreach (var id in subscriptionIds)
            {
                var subscription = subscriptions[id].FirstOrDefault();

                subscription.IsActive = false;

                context.SaveChanges();
            }
            return "";
        }
    }
}
