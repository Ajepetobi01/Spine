using System;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.ActionResults;
using Spine.Common.Enums;
using Spine.Core.Accounts.Jobs;
using Spine.Data;
using Spine.Services;

namespace Spine.Core.Accounts.Commands.AccountingPeriods
{
    public static class ReopenAccountingPeriod
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            
            [JsonIgnore]
            public int? PeriodId { get; set; }

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
            private readonly IAuditLogHelper _auditHelper;

            public Handler(SpineContext context, CommandsScheduler scheduler, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _scheduler = scheduler;
                _auditHelper = auditHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var today = DateTime.Today;

                var period = await _dbContext.AccountingPeriods
                    .FirstOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.PeriodId);

                if (period == null)
                    return new Response("Accounting period does not exist");

                if (!period.IsClosed)
                    return new Response("Accounting period is not closed");

                if (await _dbContext.AccountingPeriods.AnyAsync(x =>
                    x.CompanyId == request.CompanyId && x.BookClosedDate.HasValue && x.StartDate > period.EndDate))
                    return new Response("You cannot reopen this period because a future date has been closed");
                
                period.IsClosed = false;
                period.BookClosedDate = null;
                period.ModifiedOn = today;
                
                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.Company,
                        Action = (int)AuditLogCompanyAction.ReopenAccountingPeriod,
                        Description = $"Reopen accounting period {period.Id} on {today}",
                        UserId = request.UserId
                    });

                if (await _dbContext.SaveChangesAsync() <= 0) return new Response(HttpStatusCode.BadRequest);
                
                if (period.BookClosedDate.HasValue)
                {
                    _scheduler.SendNow(new HandleReopenAccountingPeriodCommand
                    {
                        BookClosingId = period.BookClosingId,
                        CompanyId = request.CompanyId,
                        UserId = request.UserId
                    }, $"Reopen Accounting Period {period.StartDate}");
                }
                
                return new Response();
            }
        }
    }
}