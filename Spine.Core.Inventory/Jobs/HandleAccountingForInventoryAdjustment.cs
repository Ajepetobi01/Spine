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
    public class HandleAccountingForInventoryAdjustment : IRequest
    {
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public int AccountingPeriodId { get; set; }
        
        public List<InventoryAdjustmentModel> Model { get; set; }
    }

    public class InventoryAdjustmentModel
    {
        public AdjustmentType Type  { get; set; }
        public bool IsAddition { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        
        public int Quantity { get; set; }
        
        public DateTime Date { get; set; }
        
        // used only during return goods received
        public Guid? TaxId { get; set; }
        public decimal TaxAmount { get; set; } 
    }
    
    public class HandleAccountingForInventoryAdjustmentHandler : IRequestHandler<HandleAccountingForInventoryAdjustment>
    {
        private readonly SpineContext _dbContext;
        private readonly ISerialNumberHelper _serialHelper;
        private readonly ILogger<HandleAccountingForInventoryAdjustmentHandler> _logger;

        public HandleAccountingForInventoryAdjustmentHandler(SpineContext context, ISerialNumberHelper serialHelper, ILogger<HandleAccountingForInventoryAdjustmentHandler> logger)
        {
            _dbContext = context;
            _serialHelper = serialHelper;
            _logger = logger;
        }

        //this is for only quantity adjustment
        public async Task<Unit> Handle(HandleAccountingForInventoryAdjustment request, CancellationToken cancellationToken)
        {
            try
            {
                var ledgerAccounts = await _dbContext.LedgerAccounts.Where(x => x.CompanyId == request.CompanyId
                        && ((x.AccountTypeId == AccountTypeConstants.Inventories && x.SerialNo == 1)
                            || (x.AccountTypeId == AccountTypeConstants.CostOfSales
                                && x.GlobalAccountType == GlobalAccountType.InventoryWriteOff))
                        && !x.IsDeleted)
                    .ToDictionaryAsync(x => x.AccountTypeId, y => y.Id);

                var baseCurrency = await _dbContext.Companies.Where(x => x.Id == request.CompanyId && !x.IsDeleted)
                    .Select(x => x.BaseCurrencyId).SingleAsync();

                Guid creditAccount, debitAccount;
                TransactionType type = TransactionType.None;
                string narration = "";
                var groupId = SequentialGuid.Create();
                var lastUsed =
                    await _serialHelper.GetLastUsedDailyTransactionNo(_dbContext, request.CompanyId, DateTime.Today, 1);
                var refNo = Constants.GenerateSerialNo(Constants.SerialNoType.GeneralLedger, lastUsed + 1);
                
                foreach (var item in request.Model)
                {
                    //this costofsales will be the inventory-writeoff
                    if(!ledgerAccounts.TryGetValue(AccountTypeConstants.CostOfSales, out var inventoryWriteOffAccount) && inventoryWriteOffAccount != Guid.Empty)
                    {
                        var message =
                            $"Could not find ledger account id {inventoryWriteOffAccount}. Accounting for inventory adjustment for inventory {item.Id} failed";
                        _logger.LogError(message);
                        throw new Exception(message);
                    }

                    if(!ledgerAccounts.TryGetValue(AccountTypeConstants.Inventories, out var inventoriesAccount) && inventoriesAccount != Guid.Empty)
                    {
                        var message =
                            $"Could not find ledger account id {inventoriesAccount}. Accounting for inventory adjustment for inventory {item.Id} failed";
                        _logger.LogError(message);
                        throw new Exception(message);
                    }

                    if (item.IsAddition)
                    {
                        // debit inventory and credit inventory shrinkage for addition
                        creditAccount = inventoryWriteOffAccount; 
                        debitAccount = inventoriesAccount;
                        switch (item.Type)
                        {
                            case AdjustmentType.Quantity:
                                narration = $"Add {item.Quantity} unit(s) for Inventory {item.Name} on {item.Date:dd/MM/yyyy}";
                                type = TransactionType.AddInventory;
                                break;
                            case AdjustmentType.Cost:
                                narration = $"Adjust cost for Inventory {item.Name} on {item.Date:dd/MM/yyyy} - add {item.Amount}";
                                type = TransactionType.AddInventoryCost;
                                break;
                        }
                    }
                    else
                    {
                        // credit inventory and debit inventory shrinkage for reduction
                        debitAccount = inventoryWriteOffAccount;
                        creditAccount = inventoriesAccount;
                        switch (item.Type)
                        {
                            case AdjustmentType.Quantity:
                                narration = $"Reduced {item.Quantity} unit(s) for Inventory {item.Name} on {item.Date:dd/MM/yyyy}";
                                type = TransactionType.ReduceInventory;
                                break;
                            case AdjustmentType.Cost:
                                narration = $"Adjust cost for Inventory {item.Name} on {item.Date:dd/MM/yyyy} - reduced {item.Amount}";
                                type = TransactionType.ReduceInventoryCost;
                                break;
                        }
                    }
                
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.CompanyId,
                        LocationId = null,
                        OrderId = item.Id,
                        CreditAmount = item.Amount,
                        DebitAmount = 0,
                        LedgerAccountId = creditAccount,
                        Type = type,
                        ValueDate = item.Date,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.UserId,
                        TransactionGroupId = groupId,
                        AccountingPeriodId = request.AccountingPeriodId,
                        Narration = narration,
                        ReferenceNo = refNo,
                        BaseCurrencyId = baseCurrency,
                        ExchangeRate = 1,
                    });
                    
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.CompanyId,
                        LocationId = null,
                        OrderId = item.Id,
                        DebitAmount = item.Amount,
                        CreditAmount = 0,
                        LedgerAccountId = debitAccount,
                        Type = type,
                        ValueDate = item.Date,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.UserId,
                        TransactionGroupId = groupId,
                        AccountingPeriodId = request.AccountingPeriodId,
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
                _logger.LogError($"Error occured while creating general entries record for inventory adjustment {ex.Message}");
                throw ex;
            }

            return Unit.Value;
        }
    }
}