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
    public class HandleAccountingForReturnGoodsReceived : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public int AccountingPeriodId { get; set; }
        
        public List<InventoryAdjustmentModel> Model { get; set; }
    }
    
    
    public class HandleAccountingForReturnGoodsReceivedHandler : IRequestHandler<HandleAccountingForReturnGoodsReceived>
    {
        private readonly SpineContext _dbContext;
        private readonly ISerialNumberHelper _serialHelper;
        private readonly ILogger<HandleAccountingForReturnGoodsReceivedHandler> _logger;

        public HandleAccountingForReturnGoodsReceivedHandler(SpineContext context, ISerialNumberHelper serialHelper, ILogger<HandleAccountingForReturnGoodsReceivedHandler> logger)
        {
            _dbContext = context;
            _serialHelper = serialHelper;
            _logger = logger;
        }

        public async Task<Unit> Handle(HandleAccountingForReturnGoodsReceived request, CancellationToken cancellationToken)
        {
            try
            {
                // If Vat is applicable: Dr Payable (gross), Cr inventory(net), Cr Vat 
                //If vat not applicable: Dr payable, Cr Inventory
               
                var ledgerAccounts = await _dbContext.LedgerAccounts.Where(x => x.CompanyId == request.CompanyId
                        && ((x.AccountTypeId == AccountTypeConstants.Inventories && x.SerialNo == 1)
                            || (x.AccountTypeId == AccountTypeConstants.AccountsPayable && x.SerialNo == 3))
                        && !x.IsDeleted)
                    .ToDictionaryAsync(x => x.AccountTypeId, y => y.Id);

                var baseCurrency = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted)
                    .Select(x => x.BaseCurrencyId).SingleAsync();

                var groupId = SequentialGuid.Create();
                var lastUsed =
                    await _serialHelper.GetLastUsedDailyTransactionNo(_dbContext, request.CompanyId, DateTime.Today, 1);
                var refNo = Constants.GenerateSerialNo(Constants.SerialNoType.GeneralLedger, lastUsed + 1);
                
                var taxIds = request.Model.Where(x=>x.TaxId.HasValue).Select(x => x.TaxId.Value).ToHashSet();
                var taxLedgerAccounts = await _dbContext.TaxTypes.Where(x => x.CompanyId == request.CompanyId &&
                                                                             !x.IsDeleted
                                                                             && taxIds.Contains(x.Id))
                    .Select(x => new {x.LedgerAccountId, x.Id}).ToDictionaryAsync(x=>x.Id, y=>y.LedgerAccountId);

                
                foreach (var item in request.Model)
                {
                    var narration =
                        $"Returned {item.Quantity} quantity for Inventory {item.Name} on {item.Date:dd/MM/yyyy}";

                    var amountLessTax = item.Amount - item.TaxAmount;
                    if (item.TaxId.HasValue)
                    {
                        if(!taxLedgerAccounts.TryGetValue(item.TaxId.Value, out var taxAccount) && taxAccount != Guid.Empty)
                        {
                            var message =
                                $"Could not find ledger account id {taxAccount}. Accounting for for return goods received {item.Id} failed";
                            _logger.LogError(message);
                            throw new Exception(message);
                        }
                        // credit VAT
                        _dbContext.GeneralLedgers.Add(new GeneralLedger
                        {
                            CompanyId = request.CompanyId,
                            LocationId = null,
                            OrderId = item.Id,
                            CreditAmount = item.TaxAmount,
                            DebitAmount = 0,
                            LedgerAccountId = taxAccount,
                            Type = TransactionType.ReduceInventory,
                            ValueDate = item.Date,
                            TransactionDate = DateTime.Today,
                            CreatedBy = request.UserId,
                            TransactionGroupId = groupId,
                            AccountingPeriodId =  request.AccountingPeriodId,
                            Narration = narration,
                            ReferenceNo = refNo,
                            BaseCurrencyId = baseCurrency,
                            ExchangeRate = 1
                        });
                    }
                    
                    if(!ledgerAccounts.TryGetValue(AccountTypeConstants.AccountsPayable, out var payablesAccount) && payablesAccount != Guid.Empty)
                    {
                        var message =
                            $"Could not find ledger account id {payablesAccount}. Accounting for for return goods received {item.Id} failed";
                        _logger.LogError(message);
                        throw new Exception(message);
                    }
                    
                    // debit Accounts Payable (gross)
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.CompanyId,
                        LocationId = null,
                        OrderId = item.Id,
                        CreditAmount = 0,
                        DebitAmount = item.Amount,
                        LedgerAccountId = payablesAccount,
                        Type = TransactionType.ReduceInventory,
                        ValueDate = item.Date,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.UserId,
                        TransactionGroupId = groupId,
                        AccountingPeriodId =  request.AccountingPeriodId,
                        Narration = narration,
                        ReferenceNo = refNo,
                        BaseCurrencyId = baseCurrency,
                        ExchangeRate = 1
                    });
                    
                    if(!ledgerAccounts.TryGetValue(AccountTypeConstants.Inventories, out var inventoryAccount) && inventoryAccount != Guid.Empty)
                    {
                        var message =
                            $"Could not find ledger account id {inventoryAccount}. Accounting for for return goods received {item.Id} failed";
                        _logger.LogError(message);
                        throw new Exception(message);
                    }
                    // credit inventory (net)
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.CompanyId,
                        LocationId = null,
                        OrderId = item.Id,
                        DebitAmount = 0,
                        CreditAmount = amountLessTax,
                        LedgerAccountId = inventoryAccount,
                        Type = TransactionType.ReduceInventory,
                        ValueDate = item.Date,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.UserId,
                        TransactionGroupId = groupId,
                        AccountingPeriodId =  request.AccountingPeriodId,
                        Narration = narration,
                        ReferenceNo = refNo,
                        BaseCurrencyId = baseCurrency,
                        ExchangeRate = 1
                    });
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Error occured while creating general entries record for inventory return {ex.Message}");
                throw ex;
            }

            return Unit.Value;
        }
    }
}