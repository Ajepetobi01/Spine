using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Common.Helper;
using Spine.Common.Helpers;
using Spine.Data;
using Spine.Data.Entities;
using Spine.Data.Entities.Inventories;
using Spine.Data.Entities.Invoices;
using Spine.Data.Entities.Transactions;

namespace Spine.Core.Accounts.Jobs
{
    public class SetupAccountCommand : IRequest
    {
        public Guid CompanyId { get; set; }
    }

    public class SetupAccountHandler : IRequestHandler<SetupAccountCommand>
    {
        private readonly SpineContext _dbContext;
        private readonly ILogger<SetupAccountHandler> _logger;
        private static List<AccountType> _accountTypes = null;

        public SetupAccountHandler(SpineContext context, ILogger<SetupAccountHandler> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public async Task<Unit> Handle(SetupAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var today = DateTime.Today;
                
                var costOfSalesLedgerId = SequentialGuid.Create();
                var inventoryLedgerId = SequentialGuid.Create();
                var salesLedgerId = SequentialGuid.Create();
                var cashLedgerId = SequentialGuid.Create();
                
                //add category for services
                if (!await _dbContext.ProductCategories.AnyAsync(
                    x => x.CompanyId == request.CompanyId && x.IsServiceCategory))
                {
                    _dbContext.ProductCategories.Add(new InventoryCategory
                    {
                        Id = SequentialGuid.Create(),
                        CompanyId = request.CompanyId,
                        IsServiceCategory = true,
                        Status = Status.Active,
                        SalesAccountId = salesLedgerId,
                        InventoryAccountId = inventoryLedgerId,
                        CostOfSalesAccountId = costOfSalesLedgerId,
                        Name = "Services",
                        CreatedBy = Guid.Empty,
                        CreatedOn = today
                    });
                }

                if (!await _dbContext.LedgerAccounts.AnyAsync(
                    x => x.CompanyId == request.CompanyId))
                {
                    _accountTypes = await _dbContext.AccountTypes.ToListAsync();

                    // add ledger accounts
                    var ledgerAccounts = new List<LedgerAccount>
                    {
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Cash, AccountName = "Cash and cash equivalents",
                            SerialNo = 1
                        },

                        new()
                        {
                            AccountTypeId = AccountTypeConstants.AccountsReceivable,
                            AccountName = "Accounts Receivable (A/R)", SerialNo = 1
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.AccountsReceivable,
                            AccountName = "Receivable on sales", SerialNo = 2
                        },

                        new()
                        {
                            AccountTypeId = AccountTypeConstants.OtherExpense, AccountName = "Allowance for bad debts",
                            SerialNo = 1
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.OtherExpense, AccountName = "Depreciation",
                            SerialNo = 2
                        },

                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Inventories, AccountName = "Inventory", SerialNo = 1
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Prepayments, AccountName = "Prepaid Expenses",
                            SerialNo = 1
                        },
                        //     new() { AccountTypeId = AccountTypeConstants.Cu, AccountName = "Undeposited Funds", SerialNo = 1},

                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Property,
                            AccountName = "Accumulated depreciation on property, plant and equipment", SerialNo = 1
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Property, AccountName = "Furniture and Fixtures",
                            SerialNo = 2
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Property, AccountName = "Machinery and equipment",
                            SerialNo = 3
                        },
                        new() {AccountTypeId = AccountTypeConstants.Property, AccountName = "Land", SerialNo = 4},
                        new() {AccountTypeId = AccountTypeConstants.Property, AccountName = "Vehicles", SerialNo = 5},

                        new()
                        {
                            AccountTypeId = AccountTypeConstants.InvestmentPropertyAtFairValue,
                            AccountName = "Assets held for sale", SerialNo = 1
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.DeferredTaxAsset, AccountName = "Deferred tax",
                            SerialNo = 1
                        },

                        new()
                        {
                            AccountTypeId = AccountTypeConstants.IntangibleAsset, AccountName = "Goodwill", SerialNo = 1
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.IntangibleAsset, AccountName = "Intangible Assets",
                            SerialNo = 2
                        },

                        new()
                        {
                            AccountTypeId = AccountTypeConstants.AccountsPayable,
                            AccountName = "Accounts Payable (A/P)", SerialNo = 1
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.AccountsPayable, AccountName = "Accrued liabilities",
                            SerialNo = 2
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.AccountsPayable, AccountName = "Payable on purchase",
                            SerialNo = 3
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.AccountsPayable, AccountName = "Dividends payable",
                            SerialNo = 4
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.AccountsPayable, AccountName = "Payroll Clearing",
                            SerialNo = 5
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.AccountsPayable,
                            AccountName = "Accrued holiday payable", SerialNo = 6
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.AccountsPayable,
                            AccountName = "Accrued non-current liabilities", SerialNo = 7
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.AccountsPayable,
                            AccountName = "Liabilities related to assets held for sale", SerialNo = 8
                        },

                        new()
                        {
                            AccountTypeId = AccountTypeConstants.TaxPayable, AccountName = "Income tax payable",
                            SerialNo = 3
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.TaxPayable,
                            AccountName = "Sales and service tax payable", SerialNo = 4
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.TaxPayable, AccountName = "Tax Suspense", SerialNo = 5
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.TaxPayable, AccountName = "Withholding tax Payable",
                            SerialNo = 2, GlobalAccountType = GlobalAccountType.WithholdingTax
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.TaxPayable, AccountName = "VAT Control Account",
                            SerialNo = 1, GlobalAccountType = GlobalAccountType.VAT
                        },

                        new()
                        {
                            AccountTypeId = AccountTypeConstants.DeferredTaxLiability,
                            AccountName = "Current Tax Liability", SerialNo = 1
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.LongTermInvestment,
                            AccountName = "Long-term investments", SerialNo = 1
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.LongTermDebt, AccountName = "Long-term debt",
                            SerialNo = 1
                        },

                        new()
                        {
                            AccountTypeId = AccountTypeConstants.OwnerEquity, AccountName = "Dividend disbursed",
                            SerialNo = 1
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.OwnerEquity,
                            AccountName = "Equity in earnings of subsidiaries", SerialNo = 2
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.OwnerEquity, AccountName = "Opening Balance Equity",
                            SerialNo = 3
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.OwnerEquity,
                            AccountName = "Other comprehensive income", SerialNo = 4
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.OwnerEquity, AccountName = "Share capital",
                            SerialNo = 5
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.OwnerEquity, AccountName = "Retained Earnings",
                            SerialNo = 6, GlobalAccountType = GlobalAccountType.RetainedEarnings
                        },

                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Income, AccountName = "Sales of Product Income",
                            SerialNo = 1
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Income, AccountName = "Revenue - General", SerialNo = 2
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Income, AccountName = "Sales - retail", SerialNo = 3
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Income, AccountName = "Sales - wholesale", SerialNo = 4
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Income, AccountName = "Unapplied Cash Payment Income",
                            SerialNo = 5
                        },

                        new()
                        {
                            AccountTypeId = AccountTypeConstants.CostOfSales, AccountName = "Cost of Sales",
                            SerialNo = 1
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.CostOfSales,
                            AccountName = "Supplies and materials - COS", SerialNo = 2
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.CostOfSales,
                            AccountName = "Shipping, Freight and Delivery - COS", SerialNo = 3
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.CostOfSales,
                            AccountName = "Inventory write-off account no", SerialNo = 4,
                            GlobalAccountType = GlobalAccountType.InventoryWriteOff
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.CostOfSales, AccountName = "Discount allowed",
                            SerialNo = 5, GlobalAccountType = GlobalAccountType.DiscountAllowed
                        },

                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Amortisation expense",
                            SerialNo = 1
                        },
                        new() {AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Bad debts", SerialNo = 2},
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Bank charges", SerialNo = 3
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Commissions and fees",
                            SerialNo = 4
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Dues and Subscriptions",
                            SerialNo = 5
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Equipment rental",
                            SerialNo = 6
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Income tax expense",
                            SerialNo = 7
                        },
                        new() {AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Insurance", SerialNo = 8},
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Interest paid", SerialNo = 9
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Legal and professional fees",
                            SerialNo = 10
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses,
                            AccountName = "Loss on discontinued operations, net of tax", SerialNo = 11
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Management compensation",
                            SerialNo = 12
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Meals and entertainment",
                            SerialNo = 13
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses,
                            AccountName = "Office/General Administrative Expenses", SerialNo = 14
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Other selling expenses",
                            SerialNo = 15
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Advertising/Promotional",
                            SerialNo = 16
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Supplies and materials",
                            SerialNo = 17
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Rent or Lease of Buildings",
                            SerialNo = 18
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Repair and maintenance",
                            SerialNo = 19
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses,
                            AccountName = "Shipping and delivery expense", SerialNo = 20
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Supplies and materials",
                            SerialNo = 21
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses,
                            AccountName = "Travel expenses - general and admin expenses", SerialNo = 22
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses,
                            AccountName = "Travel expenses - selling expense", SerialNo = 23
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses,
                            AccountName = "Unapplied Cash Bill Payment Expense", SerialNo = 24
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses,
                            AccountName = "Other Miscellaneous Service Cost", SerialNo = 25
                        },
                        new() {AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Utilities", SerialNo = 26},
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.Expenses, AccountName = "Payroll Expenses",
                            SerialNo = 27
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.IncomeTaxExpense, AccountName = "Income Tax Expense",
                            SerialNo = 1
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.OtherIncome, AccountName = "Dividend income",
                            SerialNo = 1
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.OtherIncome, AccountName = "Interest earned",
                            SerialNo = 2
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.OtherIncome,
                            AccountName = "Loss on disposal of assets", SerialNo = 3
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.OtherIncome, AccountName = "Other operating income",
                            SerialNo = 4
                        },
                        new()
                        {
                            AccountTypeId = AccountTypeConstants.OtherIncome,
                            AccountName = "Unrealised loss on securities, net of tax", SerialNo = 5
                        },

                    };

                    ledgerAccounts.First(x => x.AccountTypeId == AccountTypeConstants.Cash).Id = cashLedgerId;
                    ledgerAccounts.First(x => x.AccountTypeId == AccountTypeConstants.CostOfSales && x.SerialNo == 1).Id = costOfSalesLedgerId;
                    ledgerAccounts.First(x => x.AccountTypeId == AccountTypeConstants.Inventories && x.SerialNo == 1).Id = inventoryLedgerId;
                    ledgerAccounts.First(x => x.AccountTypeId == AccountTypeConstants.Income && x.SerialNo == 1).Id = salesLedgerId;
                    ledgerAccounts.ForEach(x =>
                    {
                        x.CompanyId = request.CompanyId;
                        x.GLAccountNo = GetGLAccountNo(x.AccountTypeId, x.SerialNo);
                    });

                    _dbContext.LedgerAccounts.AddRange(ledgerAccounts);

                    //add VAT TaxType
                    if (!await _dbContext.TaxTypes.AnyAsync(
                        x => x.CompanyId == request.CompanyId && x.IsVAT))
                    {
                        _dbContext.TaxTypes.Add(new TaxType
                        {
                            CompanyId = request.CompanyId,
                            CreatedBy = Guid.Empty,
                            IsVAT = true,
                            LedgerAccountId =
                                ledgerAccounts.SingleOrDefault(x => x.GlobalAccountType == GlobalAccountType.VAT)
                                    ?.Id ?? Guid.Empty,
                            Tax = "Value Added Tax (VAT)",
                            TaxRate = 7.5,
                        });
                    }

                    // add cash account
                    if (!await _dbContext.BankAccounts.AnyAsync(
                        x => x.CompanyId == request.CompanyId && x.IsCash))
                    {
                        _dbContext.BankAccounts.Add(new BankAccount
                        {
                            AccountName = "CASH AT HAND",
                            AccountNumber = "",
                            CompanyId = request.CompanyId,
                            AccountType = "",
                            BankCode = "",
                            BankName = "CASH",
                            CreatedBy = Guid.Empty,
                            IsCash = true,
                            IsActive = true,
                            Currency = Constants.NigerianCurrencyCode,
                            LedgerAccountId = cashLedgerId
                        });
                    }

                    //add accounting period
                    if (!await _dbContext.AccountingPeriods.AnyAsync(
                        x => x.CompanyId == request.CompanyId))
                    {
                        //add company serial table
                        _dbContext.CompanySerials.Add(new CompanySerial
                        {
                            CompanyId = request.CompanyId,
                            LastUsedPeriodNo = 1,
                            CurrentDate = today
                        });
                        
                        _dbContext.AccountingPeriods.Add(new AccountingPeriod
                        {
                            CompanyId = request.CompanyId,
                            BookClosingId = Guid.NewGuid(),
                            PeriodCode = $"PED-00001",
                            Year = today.Year,
                            StartDate = new DateTime(today.Year, 1, 1),
                            EndDate = new DateTime(today.Year, 12, 31),
                            CreatedOn = today,
                        });

                        var equityLedger = ledgerAccounts.Where(x => x.AccountTypeId == AccountTypeConstants.OwnerEquity && x.SerialNo == 3 )
                            .Select(x => x.Id).FirstOrDefault();
                        _dbContext.OpeningBalances.Add(new OpeningBalance
                        {
                            CompanyId = request.CompanyId,
                            SerialNo = 1,
                            TransactionDate = today,
                            ValueDate = today,
                            CreditAmount = 0,
                            DebitAmount = 0,
                            Balance = 0,
                            BookClosingId = Guid.NewGuid(),
                            LedgerAccountId = equityLedger
                           // CreatedBy = 
                        });
                    }
                }

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Error occured while setting up accounts for company id {request.CompanyId} {ex.Message}");
            }

            return Unit.Value;
        }

        private string GetGLAccountNo(int typeId, int serialNo)
        {
            if (_accountTypes.IsNullOrEmpty()) return "";

            var type = _accountTypes.FirstOrDefault(x => x.Id == typeId);
            if (type == null) return "";
            
            return $"GL-{type.Id:D2}{type.AccountClassId:D2}{type.AccountSubClassId:D2}{serialNo:D2}";
        } 
    }
}