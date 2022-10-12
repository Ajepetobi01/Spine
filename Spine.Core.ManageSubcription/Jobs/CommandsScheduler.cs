using Hangfire;
using Newtonsoft.Json;
using Spine.Common.Helpers;
using Spine.Core.ManageSubcription.Interface;
using Spine.Core.ManageSubcription.Services;
using Spine.Core.ManageSubcription.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.Jobs
{
    public class CommandsScheduler
    {
        private readonly INotificationRepository commandsExecutor;

        public CommandsScheduler(INotificationRepository commandsExecutor)
        {
            this.commandsExecutor = commandsExecutor;
        }

        //public string SendNow(NotificationParam model, string description = null)
        //{
        //    var mediatorSerializedObject = this.SerializeObject(request, description);

        //    return BackgroundJob.Enqueue(() => this.commandsExecutor.ExecuteCommand(mediatorSerializedObject));
        //}

        //public string SendNow(NotificationParam model, string parentJobId, JobContinuationOptions continuationOption, string description = null)
        //{
        //    var mediatorSerializedObject = this.SerializeObject(request, description);
        //    return BackgroundJob.ContinueJobWith(parentJobId, () => this.commandsExecutor.ExecuteCommand(mediatorSerializedObject), continuationOption);
        //}

        //public void Schedule(NotificationParam model, DateTimeOffset scheduleAt, string description = null)
        //{
        //    var mediatorSerializedObject = this.SerializeObject(request, description);

        //    BackgroundJob.Schedule(() => this.commandsExecutor.ExecuteCommand(mediatorSerializedObject), scheduleAt);
        //}
        public void Schedule(NotificationParam model, TimeSpan delay, string description = null)
        {
            var newTime = DateTime.Now + delay;  //Constants.GetCurrentDateTime(TimeZoneInfo.Utc) + delay;
            BackgroundJob.Schedule(() => this.commandsExecutor.ScheduleNotification(model), newTime);
        }

        //public void ScheduleRecurring(NotificationParam model, string name, string cronExpression, string description = null)
        //{
        //    var mediatorSerializedObject = this.SerializeObject(request, description);

        //    RecurringJob.AddOrUpdate(name, () => this.commandsExecutor.ExecuteCommand(mediatorSerializedObject), cronExpression, TimeZoneInfo.Local);
        //}

    }
}
