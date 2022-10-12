using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Attributes;
using Spine.Common.Enums;
using Spine.Core.Transactions.Helpers;
using Spine.Data;
using Spine.Data.Entities.Transactions;

namespace Spine.Core.Transactions.Queries.Reports
{
    public static class ProfitAndLoss
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            
            [RequiredIf(nameof(ReportType), PLReportType.Single, ErrorMessage = "Start date is required")]
            public DateTime? StartDate { get; set; }
            
            [RequiredIf(nameof(ReportType), PLReportType.Single, ErrorMessage = "End date is required")]
            public DateTime? EndDate { get; set; }
            
            [RequiredIf(nameof(ReportType), PLReportType.MonthOnMonth, ErrorMessage = "Month is required")]
            public int? Month { get; set; }
            
            [RequiredIfAny(nameof(ReportType), 
                PLReportType.MonthOnMonth, PLReportType.YearOnYear, PLReportType.YearToDateOnLastYear, 
                ErrorMessage = "Year is required")]
            public int? Year { get; set; }
            
            [Required(ErrorMessage = "Report type is required")]
            public PLReportType? ReportType { get; set; }
            
        }
        
        public class Model
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
            
            public int Notes { get; set; }
            
            public decimal FirstDateAmount { get; set; }
            public decimal SecondDateAmount { get; set; }
        }
        
        public class Response
        {
            public List<Model> Data { get; set; }
            public string CompanyName { get; set; }
            public string ReportName { get; set; } = "Profit and Loss";
            public string Description { get; set; }
            public string FirstDate { get; set; }
            public string SecondDate { get; set; }
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

                string description, firstDate, secondDate;
                switch (request.ReportType)
                {
                    case PLReportType.Single:
                        firstDate = $"{request.StartDate:d} - {request.EndDate:d} ";
                        secondDate = "";
                        description =
                            $"Statement of comprehensive income between {firstDate}";
                        break;
                    case PLReportType.MonthOnMonth:
                        firstDate = $"{request.StartDate:Y} ";
                        secondDate = $"{request.StartDate.Value.AddYears(-1):Y}";
                        description =
                            $"Statement of comprehensive income comparison between {firstDate} and {secondDate}";
                        request.StartDate = new DateTime(request.Year.Value, request.Month.Value, 1);
                        request.EndDate = request.StartDate.Value.AddMonths(1).AddDays(-1); 
                        // add one month, and remove 1 day to get last day of the current month
                        break;
                    case PLReportType.YearOnYear:
                        firstDate = $"Jan 01 - Dec 31 {request.Year} ";
                        secondDate = $"Jan 01 - Dec 31 {request.Year - 1}";
                        description =
                            $"Statement of comprehensive income comparison between ({firstDate}) and ({secondDate}) ";
                        request.StartDate = new DateTime(request.Year.Value, 1, 1);
                        request.EndDate = new DateTime(request.Year.Value, 12, 31);
                        break;
                    case PLReportType.YearToDateOnLastYear:
                        firstDate = $"Jan 01 - {today:M} {request.Year} ";
                        secondDate = $"Jan 01 - Dec 31 {request.Year - 1}";
                        description =
                            $"Statement of comprehensive income comparison between ({firstDate}) and ({secondDate}) ";
                        request.StartDate = new DateTime(request.Year.Value, 1, 1);
                        request.EndDate = today;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Report type is not valid");
                }
                
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@CompanyId", request.CompanyId),
                    new SqlParameter("@StartDate", request.StartDate),
                    new SqlParameter("@EndDate", request.EndDate),
                    new SqlParameter("@Mode", request.ReportType)
                };

                var data = await _context.SqlQuery<Model>("spProfitAndLoss @CompanyId, @StartDate, @EndDate, @Mode", parameters);

                return new Response
                {
                    Description = description,
                    CompanyName = await _context.Companies.Where(x => x.Id == request.CompanyId).Select(x => x.Name)
                        .SingleAsync(),
                    Data = data == null ? new List<Model>() : data.ToList(),
                    FirstDate = firstDate,
                    SecondDate = secondDate
                };
            }
        }

    }
}
