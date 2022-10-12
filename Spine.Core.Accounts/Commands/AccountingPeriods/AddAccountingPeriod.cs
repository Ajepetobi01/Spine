using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RazorLight.Extensions;
using Spine.Common.ActionResults;
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Data.Entities.Transactions;
using Spine.Services;

namespace Spine.Core.Accounts.Commands.AccountingPeriods
{
    public static class AddAccountingPeriod
    {
        public class Command : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }

            [Required(ErrorMessage = "Start Date is required")]
            public DateTime? StartDate { get; set; }
            
            [Required(ErrorMessage = "End Date is required")]
            public DateTime? EndDate { get; set; }
            
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

            public Handler(SpineContext context, IAuditLogHelper auditHelper)
            {
                _dbContext = context;
                _auditHelper = auditHelper;
            }

            public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
            {

                if (request.EndDate < request.StartDate)
                    return new Response("Start date cannot be greater than End date");
                
                var today = DateTime.Today;

                var companyPeriods = await _dbContext.AccountingPeriods
                    .Where(x => x.CompanyId == request.CompanyId).ToListAsync();

                //(StartDate1 <= EndDate2) and (StartDate2 <= EndDate1)
                if (companyPeriods.Any(x => request.StartDate <= x.EndDate && x.StartDate <= request.EndDate))
                    return new Response("Start date and End date cannot fall in between another period");

                var serialRecord = await _dbContext.CompanySerials.FirstAsync(x => x.CompanyId == request.CompanyId);

                serialRecord.LastUsedPeriodNo += 1;
                
                _dbContext.AccountingPeriods.Add(new AccountingPeriod
                {
                    CompanyId = request.CompanyId,
                    BookClosingId = Guid.NewGuid(),
                    PeriodCode = Constants.GenerateSerialNo(Constants.SerialNoType.Period, serialRecord.LastUsedPeriodNo),
                    Year = request.StartDate.Value.Year,
                    StartDate = request.StartDate.Value,
                    EndDate = request.EndDate.Value,
                    CreatedOn = today
                });
                
                _auditHelper.SaveAction(_dbContext, request.CompanyId,
                    new AuditModel
                    {
                        EntityType = (int)AuditLogEntityType.Company,
                        Action = (int)AuditLogCompanyAction.CreateAccountingPeriod,
                        Description = $"Add new accounting period {request.StartDate} to {request.EndDate}",
                        UserId = request.UserId
                    });

                return await _dbContext.SaveChangesAsync() > 0
                    ? new Response()
                    : new Response(HttpStatusCode.BadRequest);
            }
        }
    }
}