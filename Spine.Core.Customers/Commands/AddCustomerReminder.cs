using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Common.Helpers;
using Spine.Core.Customers.Jobs;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Services;

namespace Spine.Core.Customers.Commands
{
    public static class AddCustomerReminder
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

            [Required]
            public string Description { get; set; }
        }

        public class Response : BasicActionResult
        {
            public Response()
            {
                Status = HttpStatusCode.Created;
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
            private readonly IAuditLogHelper _auditHelper;
            private readonly IMapper _mapper;
            private readonly CommandsScheduler _scheduler;

            public Handler(SpineContext context, IAuditLogHelper auditHelper, IMapper mapper, CommandsScheduler scheduler)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
                _mapper = mapper;
                _scheduler = scheduler;
            }

            public async Task<Response> Handle(Command request, CancellationToken token)
            {
                var scheduleDateInUtc = request.ReminderDate.Value.ToUniversalTime();
                var currentDateTimeUtc = Constants.GetCurrentDateTime(TimeZoneInfo.Utc);
                if (scheduleDateInUtc < currentDateTimeUtc) return new Response("Reminder date and time must be a future date/time");

                var customer = await _dbContext.Customers.SingleOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.Id && !x.IsDeleted);

                if (customer == null)
                {
                    return new Response("Customer not found");
                }
                var reminder = _mapper.Map<CustomerReminder>(request);
                _dbContext.CustomerReminders.Add(reminder);

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
               new AuditModel
               {
                   EntityType = (int)AuditLogEntityType.Customer,
                   Action = (int)AuditLogCustomerAction.SetReminder,
                   Description = $"Set new reminder for customer with email {customer.Email} at  {request.ReminderDate}",
                   UserId = request.UserId
               });

                //schedule reminder
                TimeSpan span = scheduleDateInUtc - currentDateTimeUtc;
                double totalMinutes = span.TotalMinutes;
                _scheduler.Schedule(new SetCustomerReminderCommand
                {
                    CompanyId = request.CompanyId,
                    ReminderId = reminder.Id
                }, TimeSpan.FromMinutes(totalMinutes)
                    , $"Customer Reminder {customer.Email}");

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response(HttpStatusCode.BadRequest);

            }
        }

    }
}
