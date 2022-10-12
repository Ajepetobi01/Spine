using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Data.Entities.Transactions;

namespace Spine.Core.Accounts.Jobs
{
    public class HandleCloseAccountingPeriodCommand : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public Guid BookClosingId { get; set; }
        
        public List<int> PeriodIds { get; set; }
        public DateTime ClosingDate { get; set; }
    }

    public class HandleCloseAccountingPeriodHandler : IRequestHandler<HandleCloseAccountingPeriodCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<HandleCloseAccountingPeriodHandler> _logger;

        public HandleCloseAccountingPeriodHandler(SpineContext context, ILogger<HandleCloseAccountingPeriodHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public async Task<Unit> Handle(HandleCloseAccountingPeriodCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var periods = await _dbContext.AccountingPeriods
                    .Where(x => x.CompanyId == request.CompanyId && request.PeriodIds.Contains(x.Id)).ToListAsync();
                
                var startDate = periods.Min(x => x.StartDate);
                var endDate = periods.Max(x => x.EndDate);

                var ledgerEntries = await _dbContext.GeneralLedgers.Where(x =>
                        x.CompanyId == request.CompanyId && x.ValueDate >= startDate && x.ValueDate <= endDate)
                    .ToListAsync();

                var accountIds = ledgerEntries.Select(x => x.LedgerAccountId).ToHashSet();
                
                var ledgerAccounts = await (from account in _dbContext.LedgerAccounts.Where(x =>
                            x.CompanyId == request.CompanyId
                            && (accountIds.Contains(x.Id)
                                || (x.AccountTypeId == AccountTypeConstants.OwnerEquity && x.SerialNo == 3) // opening balance equity account
                            ))
                        join accountType in _dbContext.AccountTypes on account.AccountTypeId equals accountType.Id
                        join accountClass in _dbContext.AccountClasses on accountType.AccountClassId equals accountClass.Id
                        select new
                        {
                            account.Id,
                            account.AccountName,
                            account.AccountTypeId,
                            account.SerialNo,
                           // accountType.Name,
                            accountType.AccountClassId,
                            // accountClass.Class,
                            // accountClass.Type,
                            // accountClass.AccountTreatment
                        })
                    .ToListAsync();

                var equityLedgerAccountId = ledgerAccounts.First(x => x.AccountTypeId == AccountTypeConstants.OwnerEquity && x.SerialNo == 3).Id;
                var incomeAccounts = ledgerAccounts.Where(x => x.AccountClassId == 4).Select(x=>x.Id).ToList();
                var expenseAccounts = ledgerAccounts.Where(x => x.AccountClassId == 5).Select(x=>x.Id).ToList();

                var incomeLedgerEntries = ledgerEntries.Where(x => incomeAccounts.Contains(x.LedgerAccountId)).ToList();
                var expenseLedgerEntries = ledgerEntries.Where(x => expenseAccounts.Contains(x.LedgerAccountId)).ToList();

                var today = DateTime.Today;
                var baseCurrency = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted)
                    .Select(x => x.BaseCurrencyId).SingleAsync();
                
                // add a counter entry into general ledgers, (Dr all income, Cr Equity)
                var incomeAmount = incomeLedgerEntries.Sum(x => x.CreditAmount);
                _dbContext.GeneralLedgers.Add(new GeneralLedger
                {
                    CompanyId = request.CompanyId,
                    CreditAmount = incomeAmount,
                    DebitAmount = 0,
                    ExchangeRate = 1,
                    IsClosingEntry = true,
                    BookClosingId = request.BookClosingId,
                    LedgerAccountId = equityLedgerAccountId,
                    Type = TransactionType.CloseAccounting,
                    ValueDate = today,
                    TransactionDate = today,
                    CreatedBy = request.UserId,
                    TransactionGroupId = Guid.Empty,
                    AccountingPeriodId = 0,
                    Narration = "",
                    ReferenceNo = "",
                    BaseCurrencyId = baseCurrency
                });
                foreach (var entry in incomeLedgerEntries)
                {
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.CompanyId,
                        CreditAmount = 0,
                        DebitAmount = entry.CreditAmount,
                        ExchangeRate = 1,
                        IsClosingEntry = true,
                        BookClosingId = request.BookClosingId,
                        LedgerAccountId = entry.LedgerAccountId,
                        Type = TransactionType.CloseAccounting,
                        ValueDate = today,
                        TransactionDate = today,
                        CreatedBy = request.UserId,
                        TransactionGroupId = Guid.Empty,
                        AccountingPeriodId =  0,
                        Narration = "",
                        ReferenceNo = "",
                        BaseCurrencyId = baseCurrency
                    });
                }
                
                // add a counter entry into general ledgers, (Cr all expense, Dr Equity)
                var expenseAmount = incomeLedgerEntries.Sum(x => x.DebitAmount);
                _dbContext.GeneralLedgers.Add(new GeneralLedger
                {
                    CompanyId = request.CompanyId,
                    CreditAmount = 0,
                    DebitAmount = expenseAmount,
                    ExchangeRate = 1,
                    IsClosingEntry = true,
                    BookClosingId = request.BookClosingId,
                    LedgerAccountId = equityLedgerAccountId,
                    Type = TransactionType.CloseAccounting,
                    ValueDate = today,
                    TransactionDate = today,
                    CreatedBy = request.UserId,
                    TransactionGroupId = Guid.Empty,
                    AccountingPeriodId =  0,
                    Narration = "",
                    ReferenceNo = "",
                    BaseCurrencyId = baseCurrency
                });
                foreach (var entry in expenseLedgerEntries)
                {
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.CompanyId,
                        CreditAmount = entry.DebitAmount,
                        DebitAmount = 0,
                        ExchangeRate = 1,
                        IsClosingEntry = true,
                        BookClosingId = request.BookClosingId,
                        LedgerAccountId = entry.LedgerAccountId,
                        Type = TransactionType.CloseAccounting,
                        ValueDate = today,
                        TransactionDate = today,
                        CreatedBy = request.UserId,
                        TransactionGroupId = Guid.Empty,
                        AccountingPeriodId =  0,
                        Narration = "",
                        ReferenceNo = "",
                        BaseCurrencyId = baseCurrency
                    });
                }

                var lastOpeningBalance = await _dbContext.OpeningBalances.Where(x => x.CompanyId == request.CompanyId)
                    .OrderByDescending(x => x.SerialNo).FirstAsync();

                _dbContext.OpeningBalances.Add(new OpeningBalance
                {
                    CompanyId = request.CompanyId,
                    DebitAmount = expenseAmount,
                    CreditAmount = incomeAmount,
                    Balance = lastOpeningBalance.Balance + incomeAmount - expenseAmount,
                    SerialNo = lastOpeningBalance.SerialNo + 1,
                    BookClosingId = request.BookClosingId,
                    ValueDate = request.ClosingDate,
                    CreatedBy = request.UserId,
                    TransactionDate = today,
                    LedgerAccountId = equityLedgerAccountId
                });
                
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while closing accounting period {ex.Message}");
            }

            return Unit.Value;
        }
    }
}