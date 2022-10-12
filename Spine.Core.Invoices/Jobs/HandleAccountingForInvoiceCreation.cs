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
using Spine.Data.Entities;
using Spine.Data.Entities.Invoices;
using Spine.Data.Entities.Transactions;
using Spine.Services;

namespace Spine.Core.Invoices.Jobs
{
    public class HandleAccountingForInvoiceCreation : IRequest
    {
        public int AccountingPeriodId { get; set; }
        
        public Invoice NewInvoice { get; set; }
        public List<LineItem> LineItems { get; set; }
    }
    
    public class HandleAccountingForInvoiceCreationHandler : IRequestHandler<HandleAccountingForInvoiceCreation>
    {
        private readonly SpineContext _dbContext;
        private readonly ISerialNumberHelper _serialHelper;
        private readonly ILogger<HandleAccountingForInvoiceCreationHandler> _logger;

        public HandleAccountingForInvoiceCreationHandler(SpineContext context, ISerialNumberHelper serialHelper, ILogger<HandleAccountingForInvoiceCreationHandler> logger)
        {
            _dbContext = context;
            _serialHelper = serialHelper;
            _logger = logger;
        }

        public async Task<Unit> Handle(HandleAccountingForInvoiceCreation request, CancellationToken cancellationToken)
        {
            try
            {
              var taxIds = request.LineItems.Where(x=>x.TaxId.HasValue).Select(x => x.TaxId.Value).ToHashSet();

             if (request.NewInvoice.TaxId.HasValue && !taxIds.Contains(request.NewInvoice.TaxId.Value))
             {
                 taxIds.Add(request.NewInvoice.TaxId.Value);
             }

             var taxLedgerAccounts = await _dbContext.TaxTypes.Where(x => x.CompanyId == request.NewInvoice.CompanyId &&
                                                                           !x.IsDeleted
                                                                           && taxIds.Contains(x.Id))
                 .Select(x => new {x.LedgerAccountId, x.Id}).ToDictionaryAsync(x=>x.Id, y=>y.LedgerAccountId);

             var accountReceivableId = await _dbContext.LedgerAccounts.Where(x => x.CompanyId == request.NewInvoice.CompanyId
                                                                             && (x.AccountTypeId == AccountTypeConstants.AccountsReceivable && x.SerialNo == 1)
                                                                             && !x.IsDeleted)
                 .Select(x=>x.Id).FirstAsync();
             
             var discountAccountId = await _dbContext.LedgerAccounts.Where(x => x.CompanyId == request.NewInvoice.CompanyId
                     && (x.AccountTypeId == AccountTypeConstants.CostOfSales && x.SerialNo == 5) // discount allowed cost of sales
                     && !x.IsDeleted)
                 .Select(x=>x.Id).FirstAsync();

             var inventoryIds = request.LineItems.Select(x => x.ItemId).ToHashSet();
             var ledgerAccountsDic = await (from inv in _dbContext.Inventories.Where(x =>
                         x.CompanyId == request.NewInvoice.CompanyId && inventoryIds.Contains(x.Id))
                     join cat in _dbContext.ProductCategories on inv.CategoryId equals cat.Id
                     select new {inv.Id, cat.InventoryAccountId, cat.SalesAccountId, cat.CostOfSalesAccountId})
                 .ToDictionaryAsync(x => x.Id);

             var groupId = SequentialGuid.Create();
                var lastUsed =
                    await _serialHelper.GetLastUsedDailyTransactionNo(_dbContext, request.NewInvoice.CompanyId, DateTime.Today, 1);
                var refNo = Constants.GenerateSerialNo(Constants.SerialNoType.GeneralLedger, lastUsed + 1);
                var narration = $"Generated Invoice No {request.NewInvoice.InvoiceNoString}";

                //add records for inventory --> debit cost of sales, credit inventory
                // this should use the cost price, not sales price
                var inventoryCostPrice = await _dbContext.Inventories
                    .Where(x => x.CompanyId == request.NewInvoice.CompanyId && inventoryIds.Contains(x.Id))
                    .ToDictionaryAsync(d=>d.Id, x => x.UnitCostPrice);

                var isSameCurrency = request.NewInvoice.BaseCurrencyId == request.NewInvoice.CurrencyId;

                foreach (var item in request.LineItems)
                {
                    inventoryCostPrice.TryGetValue(item.ItemId.Value, out var costPrice);
                    var itemPrice = costPrice * item.Quantity;

                    if (!ledgerAccountsDic.TryGetValue(item.ItemId.Value, out var itemAccounts)
                        && itemAccounts == null
                        || (!itemAccounts.InventoryAccountId.HasValue
                            || !itemAccounts.SalesAccountId.HasValue
                            || !itemAccounts.CostOfSalesAccountId.HasValue))
                    {
                        var message =
                            $"Could not find all ledger accounts for item {item.Item}. Accounting for invoice creation failed";
                        _logger.LogError(message);
                        throw new Exception(message);
                    }

                    // inventory record --->  debit cost of sales, credit inventory
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.NewInvoice.CompanyId,
                        LocationId = null,
                        OrderId = request.NewInvoice.Id,
                        CustomerId = request.NewInvoice.CustomerId,
                        DebitAmount = itemPrice,
                        CreditAmount = 0,
                        LedgerAccountId = itemAccounts.CostOfSalesAccountId.Value,
                        Type = TransactionType.GenerateInvoice,
                        ValueDate = request.NewInvoice.InvoiceDate,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.NewInvoice.CreatedBy,
                        TransactionGroupId = groupId,
                        AccountingPeriodId = request.AccountingPeriodId,
                        Narration = narration,
                        ReferenceNo = refNo,
                        ForexCurrencyId = request.NewInvoice.CurrencyId,
                        BaseCurrencyId = request.NewInvoice.BaseCurrencyId,
                        ForexDebitAmount = isSameCurrency ? 0 : itemPrice / request.NewInvoice.RateToBaseCurrency,
                        ForexCreditAmount = 0,
                        ExchangeRate = request.NewInvoice.RateToBaseCurrency,
                    });
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.NewInvoice.CompanyId,
                        LocationId = null,
                        OrderId = request.NewInvoice.Id,
                        CustomerId = request.NewInvoice.CustomerId,
                        DebitAmount = 0,
                        CreditAmount = itemPrice,
                        LedgerAccountId = itemAccounts.InventoryAccountId.Value,
                        Type = TransactionType.GenerateInvoice,
                        ValueDate = request.NewInvoice.InvoiceDate,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.NewInvoice.CreatedBy,
                        TransactionGroupId = groupId,
                        AccountingPeriodId = request.AccountingPeriodId,
                        Narration = narration,
                        ReferenceNo = refNo,
                        ForexCurrencyId = request.NewInvoice.CurrencyId,
                        BaseCurrencyId = request.NewInvoice.BaseCurrencyId,
                        ForexDebitAmount = 0,
                        ForexCreditAmount = isSameCurrency ? 0 : itemPrice / request.NewInvoice.RateToBaseCurrency,
                        ExchangeRate = request.NewInvoice.RateToBaseCurrency,
                    });
                    
                    //sales record ---> Cr Sales accounts 
                    var itemSalesAmount = item.Quantity * item.Rate;
                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.NewInvoice.CompanyId,
                        LocationId = null,
                        OrderId = request.NewInvoice.Id,
                        CustomerId = request.NewInvoice.CustomerId,
                        CreditAmount = itemSalesAmount,
                        DebitAmount = 0,
                        LedgerAccountId = itemAccounts.SalesAccountId.Value,
                        Type = TransactionType.GenerateInvoice,
                        ValueDate = request.NewInvoice.InvoiceDate,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.NewInvoice.CreatedBy,
                        TransactionGroupId = groupId,
                        AccountingPeriodId =  request.AccountingPeriodId,
                        Narration = narration,
                        ReferenceNo = refNo,
                        ForexCurrencyId = request.NewInvoice.CurrencyId,
                        BaseCurrencyId = request.NewInvoice.BaseCurrencyId,
                        ForexCreditAmount = isSameCurrency ? 0 : itemSalesAmount / request.NewInvoice.RateToBaseCurrency,
                        ForexDebitAmount = 0,
                        ExchangeRate = request.NewInvoice.RateToBaseCurrency,
                    });
                }
                
                //add sales records ---> Dr receivables(gross)
                _dbContext.GeneralLedgers.Add(new GeneralLedger
                {
                    CompanyId = request.NewInvoice.CompanyId,
                    LocationId = null,
                    OrderId = request.NewInvoice.Id,
                    CustomerId = request.NewInvoice.CustomerId,
                    CreditAmount = 0,
                    DebitAmount = request.NewInvoice.InvoiceTotalAmount,
                    LedgerAccountId = accountReceivableId,
                    Type = TransactionType.GenerateInvoice,
                    ValueDate = request.NewInvoice.InvoiceDate,
                    TransactionDate = DateTime.Today,
                    CreatedBy = request.NewInvoice.CreatedBy,
                    TransactionGroupId = groupId,
                    AccountingPeriodId =  request.AccountingPeriodId,
                    Narration = narration,
                    ReferenceNo = refNo,
                    ForexCurrencyId = request.NewInvoice.CurrencyId,
                    BaseCurrencyId = request.NewInvoice.BaseCurrencyId,
                    ForexCreditAmount = 0,
                    ForexDebitAmount = isSameCurrency ? 0 : request.NewInvoice.InvoiceTotalAmount / request.NewInvoice.RateToBaseCurrency,
                    ExchangeRate = request.NewInvoice.RateToBaseCurrency,
                });

                // add records for tax ----> credit each tax account
                foreach (var taxId in taxIds)
                {
                    var invoiceTax = 0m;
                    if (request.NewInvoice.TaxId == taxId)
                    {
                        invoiceTax = request.NewInvoice.TaxAmount;
                    }

                    var taxAmount = request.LineItems.Where(x => x.TaxId == taxId).Sum(x => x.TaxAmount) + invoiceTax;

                    if (!taxLedgerAccounts.TryGetValue(taxId, out var taxAccount) && taxAccount != Guid.Empty)
                    {
                        var message =
                            $"Could not find ledger account id {taxAccount}. Accounting for create invoice {request.NewInvoice.InvoiceNoString} failed";
                        _logger.LogError(message);
                        throw new Exception(message);
                    }

                    _dbContext.GeneralLedgers.Add(new GeneralLedger
                    {
                        CompanyId = request.NewInvoice.CompanyId,
                        LocationId = null,
                        OrderId = request.NewInvoice.Id,
                        CustomerId = request.NewInvoice.CustomerId,
                        CreditAmount = taxAmount,
                        DebitAmount = 0,
                        LedgerAccountId = taxAccount,
                        Type = TransactionType.GenerateInvoice,
                        ValueDate = request.NewInvoice.InvoiceDate,
                        TransactionDate = DateTime.Today,
                        CreatedBy = request.NewInvoice.CreatedBy,
                        TransactionGroupId = groupId,
                        AccountingPeriodId = request.AccountingPeriodId,
                        Narration = narration,
                        ReferenceNo = refNo,
                        ForexCurrencyId = request.NewInvoice.CurrencyId,
                        BaseCurrencyId = request.NewInvoice.BaseCurrencyId,
                        ForexCreditAmount = isSameCurrency ? 0 : taxAmount / request.NewInvoice.RateToBaseCurrency,
                        ForexDebitAmount = 0,
                        ExchangeRate = request.NewInvoice.RateToBaseCurrency,
                    });
                }

                //add record for discount  ---> Cr Receivables, Dr Cost of Sales (discount)
                var discountAmount = request.LineItems.Sum(x => x.DiscountAmount) + request.NewInvoice.DiscountAmount;
                _dbContext.GeneralLedgers.Add(new GeneralLedger
                {
                    CompanyId = request.NewInvoice.CompanyId,
                    LocationId = null,
                    OrderId = request.NewInvoice.Id,
                    CustomerId = request.NewInvoice.CustomerId,
                    DebitAmount = discountAmount,
                    CreditAmount = 0,
                    LedgerAccountId = discountAccountId,
                    Type = TransactionType.GenerateInvoice,
                    ValueDate = request.NewInvoice.InvoiceDate,
                    TransactionDate = DateTime.Today,
                    CreatedBy = request.NewInvoice.CreatedBy,
                    TransactionGroupId = groupId,
                    AccountingPeriodId =  request.AccountingPeriodId,
                    Narration = narration,
                    ReferenceNo = refNo,
                    ForexCurrencyId = request.NewInvoice.CurrencyId,
                    BaseCurrencyId = request.NewInvoice.BaseCurrencyId,
                    ForexDebitAmount = isSameCurrency ? 0 : discountAmount / request.NewInvoice.RateToBaseCurrency,
                    ForexCreditAmount = 0,
                    ExchangeRate = request.NewInvoice.RateToBaseCurrency,
                });

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while creating general entries record for generating invoice {request.NewInvoice.Id} {ex.Message}");
                throw ex;
            }

            return Unit.Value;
        }
    }
}