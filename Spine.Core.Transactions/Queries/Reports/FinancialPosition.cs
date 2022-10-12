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
    public static class FinancialPosition
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
        }
        
        public class Model
        {
            public string Class { get; set; }
            public string SubClass { get; set; }
            public string AccountName { get; set; }
            public decimal Amount { get; set; }
        }

        public class Response
        {
            public dynamic Data { get; set; }
            public string CompanyName { get; set; }
            public string ReportName { get; set; } = "Statement of Financial Position";
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
                    new SqlParameter("@EndDate", request.EndDate)
                };

                var data = await _context.SqlQuery<Model>("spFinancialPosition @CompanyId, @StartDate, @EndDate", parameters);
                
                var items = data.ToList();
                var grouped = items.GroupBy(x => x.Class)
                    .Select(x => new
                    {
                        Class = x.Key,
                        Items = x.GroupBy(y => y.SubClass)
                            .Select(z => new
                            {
                                SubClass = z.Key,
                                Items = z.Select(v=> new
                                {
                                    v.Amount,
                                    v.AccountName
                                }).ToList(),
                                Total = z.Sum(xy=>xy.Amount)
                            }).ToList(),
                        Total = x.Sum(xx=>xx.Amount)
                    }).ToList();
                
                return new Response
                {
                    Description = $"Statement of Financial Position between {request.StartDate:dd MMMM yyyy} - {request.EndDate:dd MMMM yyyy}",
                    CompanyName = await _context.Companies.Where(x => x.Id == request.CompanyId).Select(x => x.Name)
                        .SingleAsync(),
                    Data = grouped
                };
            }
        }
    }
}
