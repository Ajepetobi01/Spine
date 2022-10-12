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
using Spine.Services;

namespace Spine.Core.Inventories.Jobs
{
    public class HandleAccountingForJournalPosting : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        
        public List<JournalModel> Journals{ get; set; }
    }
    

    public class JournalModel
    {
        public Guid LedgerAccountId { get; set; }
        public DateTime JournalDate { get; set; }
        public string JournalNo { get; set; }
        public Guid JournalId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal ExchangeRate { get; set; }

        public int AccountingPeriodId { get; set; }
        public int CurrencyId { get; set; }
    }
    
    public class HandleAccountingForJournalPostingHandler : IRequestHandler<HandleAccountingForJournalPosting>
    {
        private readonly SpineContext _dbContext;
        private readonly ISerialNumberHelper _serialHelper;
        private readonly ILogger<HandleAccountingForJournalPostingHandler> _logger;

        public HandleAccountingForJournalPostingHandler(SpineContext context, ISerialNumberHelper serialHelper, ILogger<HandleAccountingForJournalPostingHandler> logger)
        {
            _dbContext = context;
            _serialHelper = serialHelper;
            _logger = logger;
        }

        public async Task<Unit> Handle(HandleAccountingForJournalPosting request, CancellationToken cancellationToken)
        {
            Guid? orderId = null;
            try
            {
                var baseCurrency = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted)
                    .Select(x=>x.BaseCurrencyId).SingleAsync();

                var groupId = SequentialGuid.Create();
                var lastUsed =
                    await _serialHelper.GetLastUsedDailyTransactionNo(_dbContext, request.CompanyId, DateTime.Today, 1);
                var refNo = Constants.GenerateSerialNo(Constants.SerialNoType.GeneralLedger, lastUsed + 1);
                foreach (var item in request.Journals)
                {
                    var narration =
                        $"Post journal {item.JournalNo} on {item.JournalDate:dd/MM/yyyy}";
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.CompanyId,
                        LocationId = null,
                        OrderId = item.JournalId,
                        CreditAmount = item.Credit,
                        DebitAmount = item.Debit,
                        LedgerAccountId = item.LedgerAccountId,
                        Type = TransactionType.JournalPosting,
                        ValueDate = item.JournalDate,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.UserId,
                        TransactionGroupId = groupId,
                        AccountingPeriodId = item.AccountingPeriodId,
                        Narration = narration,
                        ReferenceNo = refNo,
                        BaseCurrencyId = baseCurrency,
                        ForexCurrencyId = item.CurrencyId,
                        ForexCreditAmount = item.CurrencyId == baseCurrency ? 0 : item.Credit / item.ExchangeRate,
                        ForexDebitAmount = item.CurrencyId == baseCurrency ? 0 : item.Debit / item.ExchangeRate,
                        ExchangeRate = item.ExchangeRate
                    });
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while posting journal entries {ex.Message}");
                throw ex;
            }

            return Unit.Value;
        }
    }
}