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
using Spine.Core.Transactions.Helpers;
using Spine.Data;

namespace Spine.Core.Transactions.Queries.Reports
{
    public static class TrialBalanceDetailed
    {
        public class Query : IRequest<Response>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }
            [JsonIgnore]
            public Guid UserId { get; set; }
            
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            
            [Required]
            public Guid? LedgerAccountId { get; set; }
            
        }
        
        public class Model
        {
            public DateTime? ValueDate { get; set; }
            public string Narration { get; set; }
            
            public decimal DebitAmount { get; set; }
            public decimal CreditAmount { get; set; }
            
            public decimal Balance { get; set; }
        }

        public class Response
        {
            public List<Model> Data { get; set; }
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

                var data = _context.GeneralLedgers.Where(x =>
                        x.CompanyId == request.CompanyId && x.LedgerAccountId == request.LedgerAccountId)
                    .Select(y => new
                    {
                        y.CreditAmount, y.DebitAmount, y.ValueDate, y.TransactionDate, y.Narration
                    });
                
                /*
                 * (note the group by 0)
                    It does not translate into a sql GROUP BY, but a SELECT of multiple SUMs from a subquery. 
                    In terms of execution plan it may even be the same.
                 */
                var opening = await (from prev in data.Where(x => x.ValueDate < request.StartDate)
                    group prev by 0
                    into openingRecord
                    select new
                    {
                        Credit = openingRecord.Sum(x => x.CreditAmount),
                        Debit = openingRecord.Sum(x => x.DebitAmount)
                    }).FirstOrDefaultAsync();

                var items = await data.Where(x => x.ValueDate >= request.StartDate
                                                  && x.ValueDate.Date <= request.EndDate)
                    .OrderBy(x => x.TransactionDate)
                    .Select(x => new Model
                    {
                        ValueDate = x.ValueDate,
                        DebitAmount = x.DebitAmount,
                        CreditAmount = x.CreditAmount,
                        Narration = x.Narration
                    }).ToListAsync();

                var openingBal = opening.Debit - opening.Credit;
                
                items.Insert(0, new Model
                {
                    Balance = openingBal,
                    DebitAmount = opening.Debit,
                    CreditAmount = opening.Credit,
                    Narration = "Opening Balance"
                });
                
                for (int i = 1; i < items.Count; i++)
                {
                    openingBal += items[i].DebitAmount - items[i].CreditAmount;
                    items[i].Balance = openingBal;
                }
                
                return new Response
                {
                    Data = items
                };
            }
        }
    }
}
