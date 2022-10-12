using System;
using System.Text.Json;
using Hangfire;
using MediatR;
using Spine.Common.Helpers;

namespace Spine.Core.Customers
{
    public class CommandsScheduler
    {
        private readonly CommandsExecutor commandsExecutor;

        public CommandsScheduler(CommandsExecutor commandsExecutor)
        {
            this.commandsExecutor = commandsExecutor;
        }

        public string SendNow(IRequest request, string description = null)
        {
            var mediatorSerializedObject = this.SerializeObject(request, description);

            return BackgroundJob.Enqueue(() => this.commandsExecutor.ExecuteCommand(mediatorSerializedObject));
        }

        public string SendNow(IRequest request, string parentJobId, JobContinuationOptions continuationOption, string description = null)
        {
            var mediatorSerializedObject = this.SerializeObject(request, description);
            return BackgroundJob.ContinueJobWith(parentJobId, () => this.commandsExecutor.ExecuteCommand(mediatorSerializedObject), continuationOption);
        }

        public void Schedule(IRequest request, DateTimeOffset scheduleAt, string description = null)
        {
            var mediatorSerializedObject = this.SerializeObject(request, description);

            BackgroundJob.Schedule(() => this.commandsExecutor.ExecuteCommand(mediatorSerializedObject), scheduleAt);
        }
        public void Schedule(IRequest request, TimeSpan delay, string description = null)
        {
            var mediatorSerializedObject = this.SerializeObject(request, description);
            var newTime = Constants.GetCurrentDateTime(TimeZoneInfo.Utc) + delay; // use Utc time
            BackgroundJob.Schedule(() => this.commandsExecutor.ExecuteCommand(mediatorSerializedObject), newTime);
        }

        public void ScheduleRecurring(IRequest request, string name, string cronExpression, string description = null)
        {
            var mediatorSerializedObject = this.SerializeObject(request, description);

            RecurringJob.AddOrUpdate(name, () => this.commandsExecutor.ExecuteCommand(mediatorSerializedObject), cronExpression, TimeZoneInfo.Local);
        }


        private MediatorSerializedObject SerializeObject(object mediatorObject, string description)
        {
            string fullTypeName = mediatorObject.GetType().FullName;
            string data = JsonSerializer.Serialize(mediatorObject);
            //    , new JsonSerializerOptions
            //{
            //    f
            //    Formatting = Formatting.None,
            //    ContractResolver = new PrivateJsonDefaultContractResolver()
            //});

            return new MediatorSerializedObject(fullTypeName, data, description);
        }
    }
}
