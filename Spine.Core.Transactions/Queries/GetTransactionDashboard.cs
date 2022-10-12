using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Common.Helpers;
using Spine.Data;

namespace Spine.Core.Transactions.Queries
{
    public static class GetTransactionDashboard
    {

        public class Query : IRequest<Model>
        {
            [JsonIgnore]
            public Guid CompanyId { get; set; }

            public List<Guid> AccountIds { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
        }

        public class Model
        {
            public string AccountNumber { get; set; }
            public decimal BankExpenses { get; set; }
            public decimal BankIncome { get; set; }
            public decimal BankBalance { get; set; }
            public decimal CashBalance { get; set; }

            public List<ChartData> ChartData { get; set; }
            public List<TableData> TableData { get; set; }

        }

        public class QueryModel
        {
            public Guid AccountId { get; set; }
            public bool IsActive { get; set; }
            public string AccountNumber { get; set; }
            public decimal CurrentBalance { get; set; }
            public decimal Credit { get; set; }
            public decimal Debit { get; set; }
            public string AccountName { get; set; }
            public DateTime TransactionDate { get; set; }
            public Guid? CategoryId { get; set; }
            public bool IsCash { get; set; }
            public bool IsAutoCreated { get; set; }

        }

        public class ChartData
        {
            public string Key { get; set; }
            public decimal BankBalance { get; set; }
            public decimal CashBalance { get; set; }
        }

        public class TableData
        {
            public Guid AccountId { get; set; }
            public bool IsActive { get; set; }
            
            public bool IsCash { get; set; }
            public string AccountName { get; set; }
            public decimal BankBalance { get; set; }
            public decimal Income { get; set; }
            public decimal Expenses { get; set; }
            public int UncategorizedTransaction { get; set; }
            public int TotalTransaction { get; set; }
            public bool IsAutoCreated { get; set; }
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
                bool excludeCashInSummary = false;
                if (request.AccountIds.IsNullOrEmpty())
                {
                    request.AccountIds = await _dbContext.BankAccounts.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted).Select(x => x.Id).ToListAsync();
                }
                else
                {
                    var cashAccountId = await _dbContext.BankAccounts.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted && x.IsCash)
                        .Select(x => x.Id).SingleOrDefaultAsync();

                    if (!request.AccountIds.Contains(cashAccountId))
                    {
                        excludeCashInSummary = true;
                        request.AccountIds.Add(cashAccountId);
                    }
                }

                var allTransactions = await (from account in _dbContext.BankAccounts.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted)
                                             join trans in _dbContext.Transactions.Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted
                                             && (x.BankAccountId.HasValue && request.AccountIds.Contains(x.BankAccountId.Value))) on account.Id equals trans.BankAccountId
                                             //join cat in _dbContext.TransactionCategories on trans.CategoryId equals cat.Id into transCat
                                             //from cat in transCat.DefaultIfEmpty()
                                             select new QueryModel
                                             {
                                                 IsCash = account.IsCash,
                                                 IsActive = account.IsActive,
                                                 AccountId = account.Id,
                                                 IsAutoCreated = account.IntegrationProvider != BankAccountIntegrationProvider.None,
                                                 AccountNumber = account.AccountNumber,
                                                 AccountName = account.BankName + " - " + account.AccountName,
                                                 CurrentBalance = trans.Credit - trans.Debit, //account.CurrentBalance,
                                                 CategoryId = trans.CategoryId,
                                                 TransactionDate = trans.TransactionDate,
                                                 Credit = trans.Credit,
                                                 Debit = trans.Debit
                                             }).ToListAsync();

                List<ChartData> chartData;
                var keys = new List<string>();

                request.EndDate ??= Constants.GetCurrentDateTime().Date;
                request.StartDate ??= request.EndDate.Value.AddMonths(-1);

                var datedTransactions = allTransactions.Where(x => x.TransactionDate >= request.StartDate && x.TransactionDate <= request.EndDate)
                                                       .OrderBy(x => x.TransactionDate).ToList();

                // yearly groups by months
                var isYearly = (request.EndDate.Value - request.StartDate.Value).TotalDays > 31;
                if (isYearly)
                {
                    do
                    {
                        keys.Add(request.StartDate.Value.ToString("MMM yy"));
                        request.StartDate = request.StartDate.Value.AddMonths(1);

                    } while (request.StartDate <= request.EndDate);

                    var availableData = datedTransactions.GroupBy(x => x.TransactionDate.ToString("MMM yy"))
                                                       .Select(y => new
                                                       {
                                                           Month = y.Key,
                                                           BankBalance = y.FirstOrDefault(x => !x.IsCash)?.CurrentBalance,
                                                           CashBalance = y.FirstOrDefault(x => x.IsCash)?.CurrentBalance,
                                                       }).ToList();

                    chartData = (from key in keys
                                 join data in availableData on key equals data.Month into monthlyData
                                 from chData in monthlyData.DefaultIfEmpty()
                                 select new ChartData
                                 {
                                     Key = key.ToUpper(),
                                     BankBalance = chData?.BankBalance ?? 0,
                                     CashBalance = chData?.CashBalance ?? 0
                                 }).ToList();

                }
                else //group by day MONTH - 1 APR
                {
                    do
                    {
                        keys.Add(request.StartDate.Value.ToString( "dd MMM"));
                        request.StartDate = request.StartDate.Value.AddDays(1);

                    } while (request.StartDate <= request.EndDate);

                    var availableData = datedTransactions.GroupBy(x => x.TransactionDate.ToString("dd MMM"))
                                                     .Select(y => new
                                                     {
                                                         Day = y.Key,
                                                         BankBalance = y.FirstOrDefault(x => !x.IsCash)?.CurrentBalance,
                                                         CashBalance = y.FirstOrDefault(x => x.IsCash)?.CurrentBalance,
                                                     }).ToList();

                    chartData = (from key in keys
                                 join data in availableData on key equals data.Day into dailyData
                                 from chData in dailyData.DefaultIfEmpty()
                                 select new ChartData
                                 {
                                     Key = key.ToUpper(),
                                     BankBalance = chData?.BankBalance ?? 0,
                                     CashBalance = chData?.CashBalance ?? 0
                                 }).ToList();
                }

                var tableData = datedTransactions.Where(x => request.AccountIds.Contains(x.AccountId))
                    .GroupBy(x => new { x.AccountId, x.AccountName, x.IsActive, x.IsAutoCreated, x.IsCash }).Select(y => new TableData
                    {
                        AccountId = y.Key.AccountId,
                        AccountName = y.Key.AccountName,
                        IsActive = y.Key.IsActive,
                        IsCash = y.Key.IsCash,
                        IsAutoCreated = y.Key.IsAutoCreated,
                        Expenses = y.Sum(x => x.Debit),
                        Income = y.Sum(x => x.Credit),
                        BankBalance = y.Sum(d => d.Credit - d.Debit),
                        TotalTransaction = y.Count(),
                        UncategorizedTransaction = y.Count(z => !z.CategoryId.HasValue)
                    }).ToList();

                if (excludeCashInSummary)
                    tableData = tableData.Where(x => !x.IsCash).ToList();

                var allBankTransactions = allTransactions.Where(x => !x.IsCash).ToList();

                // overriding allTransactions to exclude cash transaction if the request.AccountIds doesn't include a cash account
                allTransactions = excludeCashInSummary ? allBankTransactions : allTransactions;

                var response = new Model
                {
                    BankExpenses = allTransactions.Sum(x => x.Debit),
                    BankIncome = allTransactions.Sum(x => x.Credit),
                    CashBalance = allTransactions.Where(x => x.IsCash).Sum(x => x.CurrentBalance),
                    BankBalance = allBankTransactions.Sum(x => x.Credit - x.Debit),
                    AccountNumber = allBankTransactions.Count > 1 ? "" : allBankTransactions.Select(x => x.AccountNumber).FirstOrDefault(),
                    ChartData = chartData,
                    TableData = tableData
                };

                return response;
            }
        }
    }
}