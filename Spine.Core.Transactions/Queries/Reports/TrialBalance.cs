using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Core.Transactions.Helpers;
using Spine.Data;

namespace Spine.Core.Transactions.Queries.Reports
{
    public static class TrialBalance
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            
            public Guid? LedgerAccountId { get; set; }
            
        }
        
        public class Model
        {
            public Guid LedgerAccountId { get; set; }
            public string AccountCode { get; set; }
            public string AccountName { get; set; }
            
            public decimal StartDebit { get; set; }
            public decimal StartCredit { get; set; }
            
            public decimal PeriodDebit { get; set; }
            public decimal PeriodCredit { get; set; }
            public decimal Balance { get; set; }
            
            public decimal ClosingDebit { get; set; }
            public decimal ClosingCredit { get; set; }
        }

        public class Response
        {
            public List<Model> Data { get; set; }
            public string CompanyName { get; set; }
            public string ReportName { get; set; } = "Trial Balance";
            public string Description { get; set; }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _context;
            public Handler(SpineContext context)
            {
                _context = context;
            }

            public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
            {
                var today = DateTime.Today;
                request.StartDate ??= new DateTime(today.Year, 1, 1);
                request.EndDate ??= today;

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@CompanyId", request.CompanyId),
                    new SqlParameter("@StartDate", request.StartDate),
                    new SqlParameter("@EndDate", request.EndDate),
                    new SqlParameter("@LedgerAccountId", request.LedgerAccountId.HasValue ? request.LedgerAccountId : DBNull.Value)
                };

                var data = await _context.SqlQuery<Model>("spTrialBalance @CompanyId, @StartDate, @EndDate, @LedgerAccountId", parameters);

                var reportName = "Trial Balance";
                if (request.LedgerAccountId.HasValue)
                {
                    var account = await _context.LedgerAccounts.Where(x =>
                            x.CompanyId == request.CompanyId && x.Id == request.LedgerAccountId)
                        .Select(x => new {x.AccountName, x.GLAccountNo}).SingleOrDefaultAsync();
                    
                    reportName = $"Trial Balance ({account.AccountName} - {account.GLAccountNo})";
                }

                var items = new List<Model>();
                if (data != null)
                {
                    items = data.ToList();
                    items.Add(new Model
                    {
                        AccountName = "TOTAL",
                        StartCredit = items.Sum(x => x.StartCredit),
                        StartDebit = items.Sum(x => x.StartDebit),
                        PeriodCredit = items.Sum(x => x.PeriodCredit),
                        PeriodDebit = items.Sum(x => x.PeriodDebit),
                        Balance = items.Sum(x => x.Balance),
                        ClosingCredit = items.Sum(x => x.ClosingCredit),
                        ClosingDebit = items.Sum(x => x.ClosingDebit),
                    });
                }
                
                return new Response
                {
                    Description = $"Date Range: {request.StartDate:dd/MM/yyyy} - {request.EndDate:dd/MM/yyyy}",
                    CompanyName = await _context.Companies.Where(x => x.Id == request.CompanyId).Select(x => x.Name)
                        .SingleAsync(),
                    Data = items,
                    ReportName = reportName
                };
            }
        }

    }
}
