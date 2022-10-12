using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Transactions.Queries
{
    public static class GetTransactionMobileDashboard
    {
        public class Query : IRequest<Model>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

        }

        public class Model
        {
            public decimal TotalTransaction { get; set; }
            public decimal Spent { get; set; }
            public decimal Owed { get; set; }
            public decimal Owing { get; set; }
            public decimal Received { get; set; }
            public decimal Overpayment { get; set; }

        }

        public class Response : Model
        {
        }

        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly SpineContext _dbContext;

            public Handler(SpineContext dbContext)
            {
                _dbContext = dbContext;
            }

            public async Task<Model> Handle(Query request, CancellationToken token)
            {
                var allTransactions = await _dbContext.Transactions.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted).ToListAsync();
                var amountOwed = await _dbContext.Invoices.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted).SumAsync(x => x.InvoiceBalance);

                var response = new Model
                {
                    TotalTransaction = allTransactions.Count,
                    Spent = allTransactions.Sum(x => x.Debit),
                    Received = allTransactions.Sum(x => x.Credit),
                    Owed = amountOwed,
                    Owing = 0,
                    Overpayment = 0
                };

                return response;
            }
        }
    }
}