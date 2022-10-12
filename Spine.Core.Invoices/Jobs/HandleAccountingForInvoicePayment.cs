using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Common.Helper;
using Spine.Data;
using Spine.Data.Entities.Transactions;

namespace Spine.Core.Invoices.Jobs
{
    public class HandleAccountingForInvoicePayment : IRequest
    {
        public Guid BankLedgerAccountId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public int AccountingPeriodId { get; set; }
        
        public InvoicePaymentModel Model { get; set; }
    }

    public class InvoicePaymentModel
    {
        public Guid InvoiceId { get; set; }
        public Guid? CustomerId { get; set; }
        public string Narration { get; set; }
        public string RefNo { get; set; }
        public decimal Amount { get; set; }
        
        public DateTime Date { get; set; }
    }
    
    public class HandleAccountingForInvoicePaymentHandler : IRequestHandler<HandleAccountingForInvoicePayment>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<HandleAccountingForInvoicePaymentHandler> _logger;

        public HandleAccountingForInvoicePaymentHandler(SpineContext context, ILogger<HandleAccountingForInvoicePaymentHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public async Task<Unit> Handle(HandleAccountingForInvoicePayment request, CancellationToken cancellationToken)
        {
            try
            {
                //Dr cash/bank
                //Cr Receivables

                var receivablesAccount = await _dbContext.LedgerAccounts.Where(x => x.CompanyId == request.CompanyId
                        && x.AccountTypeId == AccountTypeConstants.AccountsReceivable && x.SerialNo == 2
                        && !x.IsDeleted).Select(x=>x.Id).SingleAsync();

                var baseCurrency = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted)
                    .Select(x => x.BaseCurrencyId).SingleAsync();

                _dbContext.GeneralLedgers.Add(new GeneralLedger
                {
                    CompanyId = request.CompanyId,
                    LocationId = null,
                    OrderId = request.Model.InvoiceId,
                    CustomerId = request.Model.CustomerId,
                    DebitAmount = 0,
                    CreditAmount = request.Model.Amount,
                    LedgerAccountId = receivablesAccount,
                    Type = TransactionType.ReceiveInvoicePayment,
                    ValueDate = request.Model.Date,
                    TransactionDate = DateTime.Today,
                    CreatedBy = request.UserId,
                    TransactionGroupId = SequentialGuid.Create(),
                    AccountingPeriodId = request.AccountingPeriodId,
                    Narration = request.Model.Narration,
                    ReferenceNo = request.Model.RefNo,
                    BaseCurrencyId = baseCurrency,
                    ExchangeRate = 1,
                });
                
                _dbContext.GeneralLedgers.Add(new GeneralLedger
                {
                    CompanyId = request.CompanyId,
                    LocationId = null,
                    OrderId = request.Model.InvoiceId,
                    CustomerId = request.Model.CustomerId,
                    DebitAmount = request.Model.Amount,
                    CreditAmount = 0,
                    LedgerAccountId = request.BankLedgerAccountId,
                    Type = TransactionType.ReceiveInvoicePayment,
                    ValueDate = request.Model.Date,
                    TransactionDate = DateTime.Today,
                    CreatedBy = request.UserId,
                    TransactionGroupId = SequentialGuid.Create(),
                    AccountingPeriodId = request.AccountingPeriodId,
                    Narration = request.Model.Narration,
                    ReferenceNo = request.Model.RefNo,
                    BaseCurrencyId = baseCurrency,
                    ExchangeRate = 1,
                });
                
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while creating general entries record for invoice payment {request.Model.InvoiceId} {ex.Message}");
                throw ex;
            }

            return Unit.Value;
        }
    }
}