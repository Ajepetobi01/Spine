using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Data.Entities.Transactions;

namespace Spine.Core.Transactions.Jobs
{
    public class HandleAccountingForTransactions : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        
        public List<TransactionModel> Transactions{ get; set; }
    }
    

    public class TransactionModel
    {
        public Guid LedgerAccountId { get; set; }
        
        public Guid TransactionId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string ReferenceNo { get; set; }
        public string Description { get; set; }

        public int AccountingPeriodId { get; set; }
    }
    
    public class HandleAccountingForTransactionsHandler : IRequestHandler<HandleAccountingForTransactions>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<HandleAccountingForTransactionsHandler> _logger;

        public HandleAccountingForTransactionsHandler(SpineContext context, ILogger<HandleAccountingForTransactionsHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public async Task<Unit> Handle(HandleAccountingForTransactions request, CancellationToken cancellationToken)
        {
            try
            {
                var baseCurrency = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted)
                    .Select(x=>x.BaseCurrencyId).SingleAsync();

                // what of the other legs of this transaction?? Match to Ledger Accounts
                var groupId = SequentialGuid.Create();
                foreach (var item in request.Transactions)
                {
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.CompanyId,
                        LocationId = null,
                        OrderId = item.TransactionId,
                        CreditAmount = item.CreditAmount,
                        DebitAmount = 0,
                        LedgerAccountId = item.LedgerAccountId,
                        Type = TransactionType.AddTransaction,
                        ValueDate = item.PaymentDate,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.UserId,
                        TransactionGroupId = groupId,
                        AccountingPeriodId =  item.AccountingPeriodId,
                        Narration = item.Description,
                        ReferenceNo = item.ReferenceNo,
                        BaseCurrencyId = baseCurrency,
                        ExchangeRate = 1
                    });
                    
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.CompanyId,
                        LocationId = null,
                        OrderId = item.TransactionId,
                        CreditAmount = 0,
                        DebitAmount = item.DebitAmount,
                        LedgerAccountId = item.LedgerAccountId,
                        Type = TransactionType.AddTransaction,
                        ValueDate = item.PaymentDate,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.UserId,
                        TransactionGroupId = groupId,
                        AccountingPeriodId =  item.AccountingPeriodId,
                        Narration = item.Description,
                        ReferenceNo = item.ReferenceNo,
                        BaseCurrencyId = baseCurrency,
                        ExchangeRate = 1
                    });
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while creating general entries record for transaction {ex.Message}");
                throw ex;
            }

            return Unit.Value;
        }
    }
}