using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Data;

namespace Spine.Core.Transactions.Queries
{
    public static class GetTransactionSummary
    {
        public class Query : IRequest<Model>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid AccountId { get; set; }
        }

        public class Model
        {
            public string BankName { get; set; }
            public string AccountNumber { get; set; }
            public string AccountName { get; set; }
            public int Months { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Currency { get; set; }

            public decimal TotalInflow { get; set; }
            public decimal TotalOutflow { get; set; }
            public decimal AvgMonthlyInflow { get; set; }
            public decimal AvgMonthlyOutflow { get; set; }
            public decimal MedianDebit { get; set; }
            public decimal MedianCredit { get; set; }
            public decimal MaximumDebit { get; set; }
            public decimal MaximumCredit { get; set; }

            [JsonIgnore]
            public List<Values> Values { get; set; }

        }

        public class Values
        {
            public decimal Credit { get; set; }
            public decimal Debit { get; set; }
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
                var query = await (from bank in _dbContext.BankAccounts.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.Id == request.AccountId)
                                   join trans in _dbContext.Transactions.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                                   on bank.Id equals trans.BankAccountId
                                   select new
                                   {
                                       bank.BankName,
                                       bank.AccountName,
                                       bank.AccountNumber,
                                       bank.CreatedOn,
                                       bank.DateDeactivated,
                                       bank.Currency,
                                       trans.Debit,
                                       trans.Credit,
                                       trans.Amount
                                   }).ToListAsync();

                var accountTrans = (query.GroupBy(x => new
                {
                    x.BankName,
                    x.AccountName,
                    x.AccountNumber,
                    x.CreatedOn,
                    x.DateDeactivated,
                    x.Currency
                })
                               .Select(x => new Model
                               {
                                   BankName = x.Key.BankName,
                                   AccountNumber = x.Key.AccountNumber,
                                   AccountName = x.Key.AccountName,
                                   StartDate = x.Key.CreatedOn,
                                   EndDate = x.Key.DateDeactivated == null ? DateTime.Today : x.Key.DateDeactivated.Value,
                                   Currency = x.Key.Currency,
                                   TotalInflow = x.Sum(y => y.Credit),
                                   TotalOutflow = x.Sum(y => y.Debit),
                                   MaximumCredit = x.Max(y => y.Credit),
                                   MaximumDebit = x.Max(y => y.Debit),
                                   Values = x.Select(y => new Values { Debit = y.Debit, Credit = y.Credit }).ToList()
                               })).FirstOrDefault();

                if (accountTrans != null)
                {
                    var monthDiff = (accountTrans.EndDate - accountTrans.StartDate).Days / (365.2425 / 12);
                    accountTrans.Months = (int)Math.Round(monthDiff, MidpointRounding.AwayFromZero);

                    if (accountTrans.Months == 0) accountTrans.Months = 1; // if monthdiff = 0, just divide by 1 to get average

                    accountTrans.AvgMonthlyInflow = accountTrans.TotalInflow / accountTrans.Months;
                    accountTrans.AvgMonthlyOutflow = accountTrans.TotalOutflow / accountTrans.Months;

                    accountTrans.MedianDebit = GetMedian(accountTrans.Values.Select(x => x.Debit));
                    accountTrans.MedianCredit = GetMedian(accountTrans.Values.Select(x => x.Credit));
                }

                return accountTrans;
            }

            private static decimal GetMedian(IEnumerable<decimal> values)
            {
                var list = values.OrderBy(x => x).ToList();
                var medianValue = (list[list.Count / 2] + list[(list.Count - 1) / 2]) / 2; // will handle both odd and even list
                return medianValue;
            }
        }

    }
}
