using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Helpers;
using Spine.Core.Transactions.Jobs;
using Spine.Data;

namespace Spine.Core.Transactions.Commands
{
    public static class ExtendTransactionReminder
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            [JsonIgnore]
            public Guid Id { get; set; }

            [Required]
            public DateTime? ReminderDate { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.OK;
            }

            public Response(HttpStatusCode statusCode)
            {
                Status = statusCode;
            }

            public Response(string message)
            {
                ErrorMessage = message;
                Status = HttpStatusCode.BadRequest;
            }
        }


        public class Handler : IRequestHandler<Command, Response>
        {
            private readonly SpineContext _dbContext;
            private readonly CommandsScheduler _scheduler;

            public Handler(SpineContext context, CommandsScheduler scheduler)
            {
                _dbContext = context;
                _scheduler = scheduler;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var scheduleDateInUtc = request.ReminderDate.Value.ToUniversalTime();
                var currentDateTimeUtc = Constants.GetCurrentDateTime(TimeZoneInfo.Utc);
                if (scheduleDateInUtc < currentDateTimeUtc) return new Response("Reminder date and time must be a future date/time");

                var reminder = await _dbContext.TransactionReminders.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted);

                if (reminder == null)
                {
                    return new Response("Reminder not found");
                }

                //schedule reminder
                TimeSpan span = scheduleDateInUtc - currentDateTimeUtc;
                double totalMinutes = span.TotalMinutes;
                _scheduler.Schedule(new SetTransactionReminderCommand
                {
                    CompanyId = request.CompanyId,
                    ReminderId = reminder.Id
                }, TimeSpan.FromMinutes(totalMinutes)
                    , $"Extend Transaction Reminder {reminder.Id}");

                return new Response();
            }
        }

    }
}
