using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Spine.Core.Transactions.Helpers;
using Spine.Data;

namespace Spine.Core.Transactions.Queries.Reports
{
    public static class ProfitAndLossDetailed
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            
            [Required]
            public int? CategoryId { get; set; }
        }

        public class Model
        {
            public string AccountName { get; set; }
            public string CategoryName { get; set; }
            public decimal Amount { get; set; }
        }
        
        public class Response
        {
            public OutputModel Data { get; set; }
            public string CompanyName { get; set; }
            public string ReportName { get; set; } = "Profit and Loss - Detailed";
            public string Description { get; set; }
        }

        public class OutputModel
        {
            public string Category { get; set; }
            public dynamic Accounts { get; set; }
        }

        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Response> Handle(Query request, CancellationToken token)
            {
                var endDate = DateTime.Today;
                var startDate = new DateTime(endDate.Year, 1, 1);
                
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@CompanyId", request.CompanyId),
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate),
                    new SqlParameter("@CategoryId", request.CategoryId)
                };

                var data = await _dbContext.SqlQuery<Model>("spProfitAndLossDetailed @CompanyId, @StartDate, @EndDate, @Mode", parameters);

                var groupedData = data.GroupBy(x => x.CategoryName).Select(x => new OutputModel
                {
                    Category = x.Key,
                    Accounts = x.Select(d => new {d.AccountName, d.Amount}).ToList()
                }).FirstOrDefault();
                
                return new Response
                {
                    Description = $"{data.FirstOrDefault()?.CategoryName} between {startDate:D} - {endDate:D}",
                    CompanyName = await _dbContext.Companies.Where(x => x.Id == request.CompanyId).Select(x => x.Name)
                        .SingleAsync(),
                    Data = groupedData
                };
            }
        }
    }
}
