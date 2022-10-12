using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Helpers;
using Spine.Data;

namespace Spine.Core.Transactions.Queries.Reports
{
    public static class GetSummary
    {
        public class Query : IRequest<Model>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            [JsonIgnore]
            public Guid? BankAccountId { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
        }

        public class Model
        {
            public decimal TotalExpenses { get; set; }
            public decimal TotalRevenue { get; set; }

            public decimal Expenses { get; set; }
            public decimal Revenue { get; set; }
            public decimal NetProfit { get; set; }

            public List<MonthlyData> MonthlyData { get; set; }
            public List<ExpenseModel> TopExpenses { get; set; }

        }

        public class MonthlyData
        {
            public string Month { get; set; }
            public decimal Expenses { get; set; }
            public decimal Revenue { get; set; }
            public decimal NetProfit { get; set; }
        }

        public class ExpenseModel
        {
            public string Category { get; set; }
            public int TransactionsCount { get; set; }
            public decimal CategoryExpense { get; set; }
            public double Percentage { get; set; }
        }

        public class Response : Model
        {
        }

        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly SpineContext _dbContext;
            private readonly IMapper _mapper;

            public Handler(SpineContext dbContext, IMapper mapper)
            {
                _dbContext = dbContext;
                _mapper = mapper;
            }

            public async Task<Model> Handle(Query request, CancellationToken token)
            {
                var allTransactions = await (from trans in _dbContext.Transactions.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted
                                                                                                                                                                    && x.BankAccountId == request.BankAccountId)
                                             join cat in _dbContext.TransactionCategories on trans.CategoryId equals cat.Id into transCat
                                             from cat in transCat.DefaultIfEmpty()
                                             select new
                                             {
                                                 Category = cat.Name ?? "Uncategorized",
                                                 trans.CategoryId,
                                                 trans.TransactionDate,
                                                 trans.Credit,
                                                 trans.Debit
                                             }).ToListAsync();

                if (request.EndDate == null) request.EndDate = Constants.GetCurrentDateTime().Date;
                if (request.StartDate == null) request.StartDate = request.EndDate.Value.AddYears(-1);

                var datedTransactions = allTransactions.Where(x => x.TransactionDate >= request.StartDate && x.TransactionDate <= request.EndDate)
                    .OrderBy(x => x.TransactionDate)
                    .GroupBy(x => x.TransactionDate.Month)
                    .Select(y => new MonthlyData
                    {
                        Month = y.Key.ToString(),
                        Expenses = y.Sum(z => z.Debit),
                        Revenue = y.Sum(z => z.Credit)
                    }).ToList();

                datedTransactions.ForEach(x => x.NetProfit = x.Revenue - x.Expenses);

                var response = new Model
                {
                    TotalExpenses = allTransactions.Sum(x => x.Debit),
                    TotalRevenue = allTransactions.Sum(x => x.Credit),
                    Expenses = datedTransactions.Sum(x => x.Expenses),
                    Revenue = datedTransactions.Sum(x => x.Revenue),
                    NetProfit = datedTransactions.Sum(x => x.NetProfit),
                    MonthlyData = datedTransactions
                };

                //get top 10 expeness by category
                var topExpenses = allTransactions.Where(x => x.TransactionDate >= request.StartDate && x.TransactionDate <= request.EndDate)
                    .GroupBy(x => x.Category)
                    .Select(x => new ExpenseModel
                    {
                        Category = x.Key,
                        CategoryExpense = x.Sum(x => x.Debit),
                        TransactionsCount = x.Count()
                    })
                    .OrderByDescending(c => c.CategoryExpense)
                    .Take(10).ToList();

                var totalCategoryExpense = topExpenses.Sum(x => x.CategoryExpense);
                topExpenses.ForEach(x => x.Percentage = (double)(x.CategoryExpense / totalCategoryExpense) * 100);
                //foreach (var item in topExpenses)
                //{
                //    item.Percentage = (double)(item.CategoryExpense / totalCategoryExpense) * 100;
                //}

                response.TopExpenses = topExpenses;
                return response;

            }
        }

    }
}
