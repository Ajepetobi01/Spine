using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    public static class CloseAccountingPeriod
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [JsonIgnore]
            public int? PeriodId { get; set; }

            [Required(ErrorMessage = "Month is required")]
            public Month? Month { get; set; }
            
            [Required(ErrorMessage = "Accounting method is required")]
            public AccountingMethod? AccountingMethod { get; set; }
            
            public bool CloseBook { get; set; }
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
            private readonly IAuditLogHelper _auditHelper;
            private readonly CommandsScheduler _scheduler;

            public Handler(SpineContext context, CommandsScheduler scheduler, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _scheduler = scheduler;
                _auditHelper = auditHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {
                var today = DateTime.Today;

                var periodToClose = await _dbContext.AccountingPeriods
                    .FirstOrDefaultAsync(x => x.CompanyId == request.CompanyId && x.Id == request.PeriodId 
                                                                               && !x.IsClosed);
                
                if (periodToClose == null)
                    return new Response("Accounting period is already closed or does not exist");

                var nextMonth = request.Month != Month.December
                    ? new DateTime(periodToClose.Year, (int) request.Month + 1, 1)
                    : new DateTime(periodToClose.Year + 1, 1, 1);
                
                //var nextMonth = new DateTime(periodToClose.Year, (int) request.Month + 1, 1);
                var endOfClosingMonth = nextMonth.AddDays(-1);
                if (today < endOfClosingMonth)
                    return new Response("You can not close at a future date");
                
                /*
                there will be two types of closure....period closure and year end closure......
                1. period closure will be closing any month, ensuring that records do not flow into that month unless it is reopened.
                2. Year end closure wil mean closing for the year..... entries will be passed here. 
                (a)debit every income account with the balance on the accounts ,cr Retained earnings . 
                (b) credit every expense account with the balance on the accounts, dr retained earnings
                
                */

                HandleCloseAccountingPeriodCommand model = null;
                if (request.CloseBook)
                {
                    // get all previous periods and close them
                    var previousPeriods = await _dbContext.AccountingPeriods.Where(x =>
                        x.CompanyId == periodToClose.CompanyId && x.EndDate < periodToClose.StartDate &&
                        !x.BookClosedDate.HasValue).ToListAsync();

                    var periodIds = new List<int>();
                    previousPeriods.ForEach(x =>
                    {
                        x.IsClosed = true;
                        x.BookClosedDate = endOfClosingMonth;
                        x.ModifiedOn = today;
                        x.BookClosingId = periodToClose.BookClosingId;
                        periodIds.Add(x.Id);
                    });
                    
                    periodToClose.BookClosedDate = endOfClosingMonth;
                    periodIds.Add(periodToClose.Id);
                    
                    model = new HandleCloseAccountingPeriodCommand
                    {
                        CompanyId = request.CompanyId,
                        UserId = request.UserId,
                        ClosingDate = endOfClosingMonth,
                        BookClosingId = periodToClose.BookClosingId,
                        PeriodIds = periodIds
                    };
                }
                
                periodToClose.IsClosed = true;
                periodToClose.ModifiedOn = today;
                periodToClose.AccountingMethod = request.AccountingMethod;

                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.Company,
                        Action = (int)AuditLogCompanyAction.CloseAccountingPeriod,
                        Description = $"Close accounting period {periodToClose.Id} on {periodToClose.EndDate}",
                        UserId = request.UserId
                    });

                if (await _dbContext.SaveChangesAsync() <= 0) return new Response(HttpStatusCode.BadRequest);
                
                if (model != null)
                {
                    _scheduler.SendNow(model, $"Close Accounting Period on {endOfClosingMonth}");
                }
                
                return new Response();

            }
        }
    }
}