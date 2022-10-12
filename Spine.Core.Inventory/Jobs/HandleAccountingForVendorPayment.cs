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

namespace Spine.Core.Inventories.Jobs
{
    public class HandleAccountingForVendorPayment : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        
        public List<VendorPaymentModel> Payments{ get; set; }
    }
    

    public class VendorPaymentModel
    {
        public Guid BankLedgerAccountId { get; set; }
        public Guid? VendorId { get; set; }
        public Guid GoodsReceivedId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string GoodsReceivedNumber { get; set; }
        public string Description { get; set; }
        public string ReferenceNo { get; set; }

        public int AccountingPeriodId { get; set; }
    }
    
    public class HandleAccountingForVendorPaymentHandler : IRequestHandler<HandleAccountingForVendorPayment>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<HandleAccountingForVendorPaymentHandler> _logger;

        public HandleAccountingForVendorPaymentHandler(SpineContext context, ILogger<HandleAccountingForVendorPaymentHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public async Task<Unit> Handle(HandleAccountingForVendorPayment request, CancellationToken cancellationToken)
        {
            Guid? orderId = null;
            try
            {
                //Cr cash/bank
                //Dr Payables

                var payablesAccount = await _dbContext.LedgerAccounts.Where(x => x.CompanyId == request.CompanyId
                    && x.AccountTypeId == AccountTypeConstants.AccountsPayable && x.SerialNo == 3
                    && !x.IsDeleted).Select(x=>x.Id).SingleAsync();

                var baseCurrency = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted)
                    .Select(x=>x.BaseCurrencyId).SingleAsync();

                var groupId = SequentialGuid.Create();
                foreach (var item in request.Payments)
                {
                    orderId = item.GoodsReceivedId;
                    // debit Accounts Payable (gross)
                    var narration = item.Description;
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.CompanyId,
                        LocationId = null,
                        OrderId = item.GoodsReceivedId,
                        CreditAmount = 0,
                        DebitAmount = item.Amount,
                        LedgerAccountId = payablesAccount,
                        Type = TransactionType.PaySupplier,
                        ValueDate = item.PaymentDate,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.UserId,
                        TransactionGroupId = groupId,
                        AccountingPeriodId =  item.AccountingPeriodId,
                        Narration = narration,
                        ReferenceNo = item.ReferenceNo,
                        BaseCurrencyId = baseCurrency,
                        ExchangeRate = 1
                    });
                    
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.CompanyId,
                        LocationId = null,
                        OrderId = item.GoodsReceivedId,
                        CreditAmount = item.Amount,
                        DebitAmount = 0,
                        LedgerAccountId = item.BankLedgerAccountId,
                        Type = TransactionType.PaySupplier,
                        ValueDate = item.PaymentDate,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.UserId,
                        TransactionGroupId = groupId,
                        AccountingPeriodId =  item.AccountingPeriodId,
                        Narration = narration,
                        ReferenceNo = item.ReferenceNo,
                        BaseCurrencyId = baseCurrency,
                        ExchangeRate = 1
                    });
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while creating general entries record for goods received {orderId} {ex.Message}");
                throw ex;
            }

            return Unit.Value;
        }
    }
}