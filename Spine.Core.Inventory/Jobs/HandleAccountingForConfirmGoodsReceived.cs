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
    public class HandleAccountingForConfirmGoodsReceived : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        
        public List<ReceivedGoodsModel> ReceivedGoods{ get; set; }
    }
    

    public class ReceivedGoodsModel
    {
        public Guid? VendorId { get; set; }
        public Guid InventoryId { get; set; }
        public string Inventory { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateReceived { get; set; }
        public Guid ReceivedBy { get; set; }

        public int AccountingPeriodId { get; set; }
        
        public Guid? TaxId { get; set; }
        public decimal TaxAmount { get; set; }
    }
    
    public class HandleAccountingForConfirmGoodsHandler : IRequestHandler<HandleAccountingForConfirmGoodsReceived>
    {
        private readonly SpineContext _dbContext;
        private readonly ISerialNumberHelper _serialHelper;
        private readonly ILogger<HandleAccountingForConfirmGoodsHandler> _logger;

        public HandleAccountingForConfirmGoodsHandler(SpineContext context, ISerialNumberHelper serialHelper, ILogger<HandleAccountingForConfirmGoodsHandler> logger)
        {
            _dbContext = context;
            _serialHelper = serialHelper;
            _logger = logger;
        }
        
        public async Task<Unit> Handle(HandleAccountingForConfirmGoodsReceived request, CancellationToken cancellationToken)
        {
            Guid? orderId = null;
            try
            {
                // If vat is applicable : Dr inventory(net), cr payables(gross), dr vat control
                // If vat is not applicable : Dr inventory, cr payables

                var taxIds = request.ReceivedGoods.Where(x=>x.TaxId.HasValue).Select(x => x.TaxId.Value).ToHashSet();
                var taxLedgerAccounts = await _dbContext.TaxTypes.Where(x => x.CompanyId == request.CompanyId &&
                                                                             !x.IsDeleted
                                                                             && taxIds.Contains(x.Id))
                    .Select(x => new {x.LedgerAccountId, x.Id}).ToDictionaryAsync(x=>x.Id, y=>y.LedgerAccountId);
                
                var ledgerAccounts = await _dbContext.LedgerAccounts.Where(x => x.CompanyId == request.CompanyId
                        && ((x.AccountTypeId == AccountTypeConstants.Inventories && x.SerialNo == 1)
                            || (x.AccountTypeId == AccountTypeConstants.AccountsPayable && x.SerialNo == 3))
                        && !x.IsDeleted)
                    .ToDictionaryAsync(x => x.AccountTypeId, y => y.Id);

                var baseCurrency = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted)
                    .Select(x=>x.BaseCurrencyId).SingleAsync();

                var vendorIds = request.ReceivedGoods.Where(x => x.VendorId.HasValue)
                    .Select(x => x.VendorId).Distinct().ToList();
                var vendorDetails = await _dbContext.Vendors
                    .Where(x => x.CompanyId == request.CompanyId && !x.IsDeleted
                                                                 && vendorIds.Contains(x.Id))
                    .Select(x => new {x.Email, x.Id}).ToDictionaryAsync(x => (Guid?)x.Id);

                var lastUsed =
                    await _serialHelper.GetLastUsedDailyTransactionNo(_dbContext, request.CompanyId, DateTime.Today, 1);
                var refNo = Constants.GenerateSerialNo(Constants.SerialNoType.GeneralLedger, lastUsed + 1);
                
                var groupId = SequentialGuid.Create();
                foreach (var item in request.ReceivedGoods)
                {
                    var narration = item.VendorId.HasValue
                        ? $"Confirm Receipt of new goods ({item.Inventory}) from Vendor {vendorDetails[item.VendorId].Email} on {item.DateReceived:dd/MM/yyyy}"
                        : $"Confirm Receipt of new goods ({item.Inventory}) on {item.DateReceived:dd/MM/yyyy}";
                    
                    orderId = item.InventoryId;
                    
                    var amountLessTax = item.Amount - item.TaxAmount;
                    if (item.TaxId.HasValue)
                    {
                        if(!taxLedgerAccounts.TryGetValue(item.TaxId.Value, out var taxAccount) && taxAccount != Guid.Empty)
                        {
                            _logger.LogError($"Could not find ledger account id {taxAccount}. Accounting for Confirm goods received {item.Inventory} failed");
                            return Unit.Value;
                        }
                        
                        // debit VAT
                        _dbContext.GeneralLedgers.Add(new GeneralLedger
                        {
                            CompanyId = request.CompanyId,
                            LocationId = null,
                            OrderId = orderId,
                            VendorId = item.VendorId,
                            DebitAmount = item.TaxAmount,
                            CreditAmount = 0,
                            LedgerAccountId = taxAccount,
                            Type = TransactionType.ConfirmGoodsReceived,
                            ValueDate = item.DateReceived,
                            TransactionDate = DateTime.Today,
                            CreatedBy = request.UserId,
                            TransactionGroupId = groupId,
                            AccountingPeriodId =  item.AccountingPeriodId,
                            Narration = narration,
                            ReferenceNo = refNo,
                            BaseCurrencyId = baseCurrency,
                            ExchangeRate = 1
                        });
                    }
                    
                    if(!ledgerAccounts.TryGetValue(AccountTypeConstants.AccountsPayable, out var payablesAccount) && payablesAccount != Guid.Empty)
                    {
                        var message =
                            $"Could not find ledger account id {payablesAccount}. Accounting for Confirm goods received {item.Inventory} failed";
                        _logger.LogError(message);
                        throw new Exception(message);
                    }
                    
                    // credit Accounts Payable (gross)
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.CompanyId,
                        LocationId = null,
                        OrderId = orderId,
                        VendorId = item.VendorId,
                        CreditAmount = item.Amount,
                        DebitAmount = 0,
                        LedgerAccountId = payablesAccount,
                        Type = TransactionType.ConfirmGoodsReceived,
                        ValueDate = item.DateReceived,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.UserId,
                        TransactionGroupId = groupId,
                        AccountingPeriodId =  item.AccountingPeriodId,
                        Narration = narration,
                        ReferenceNo = refNo,
                        BaseCurrencyId = baseCurrency,
                        ExchangeRate = 1
                    });
                    
                    if(!ledgerAccounts.TryGetValue(AccountTypeConstants.Inventories, out var inventoriesAccount) && inventoriesAccount != Guid.Empty)
                    {
                        var message =
                            $"Could not find ledger account id {inventoriesAccount}. Accounting for Confirm goods received {item.Inventory} failed";
                        _logger.LogError(message);
                        throw new Exception(message);
                    }
                    
                    // debit inventory (net)
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.CompanyId,
                        LocationId = null,
                        OrderId = orderId,
                        VendorId = item.VendorId,
                        DebitAmount = amountLessTax,
                        CreditAmount = 0,
                        LedgerAccountId = inventoriesAccount,
                        Type = TransactionType.ConfirmGoodsReceived,
                        ValueDate = item.DateReceived,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.UserId,
                        TransactionGroupId = groupId,
                        AccountingPeriodId =  item.AccountingPeriodId,
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
                _logger.LogError($"Error occured while creating general entries record for goods received {orderId} {ex.Message}");
                throw ex;
            }

            return Unit.Value;
        }
    }
}