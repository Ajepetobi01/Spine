using System;
using System.Collections.Generic;
using Spine.Data.Entities;
using Spine.Data.Entities.BillsPayments;
using Spine.Data.Entities.Inventories;
using Spine.Data.Entities.Invoices;
using Spine.Data.Entities.Transactions;

namespace Spine.Data.Helpers
{
    public static class StaticData
    {
        public static List<ApplicationRole> SystemDefinedRoles()
        {
            var date = new DateTime(2021, 7, 14, 11, 20, 30);

            return new List<ApplicationRole>
            {
                new ApplicationRole
                {
                    Id = new Guid("59e59b8d-2210-4198-b161-027723393399"),
                    CompanyId = null,
                    IsOwnerRole = true,
                    IsSystemDefined = true,
                    Name = "Account Admin",
                    NormalizedName = "ACCOUNT ADMIN",
                    ConcurrencyStamp = "9af31f72-0b95-4b31-b151-7924b93a7f37",
                    CreatedBy = Guid.Empty,
                    CreatedOn = date,
                    ModifiedOn = date
                }
            };
        }

        public static List<Currency> Currencies()
        {
            return new List<Currency>
            {
                new Currency {Id = 1, Name = "NIGERIAN NAIRA", Code = "NGN", Symbol = "₦"},
                new Currency {Id = 2, Name = "US DOLLARS", Code = "USD", Symbol = "$"},
                new Currency {Id = 3, Name = "BRITISH POUNDS", Code = "GBP", Symbol = "£"},
                new Currency {Id = 4, Name = "EURO", Code = "EUR", Symbol = "€"},
            };
        }

        public static List<InvoiceType> InvoiceTypes()
        {
            return new List<InvoiceType>
            {
                new InvoiceType {Id = 1, Type = "Standard Invoice"},
                new InvoiceType {Id = 2, Type = "Retainer Invoice"},
                new InvoiceType {Id = 3, Type = "Credit Invoice"},
                new InvoiceType {Id = 4, Type = "Debit Invoice"},
                new InvoiceType {Id = 5, Type = "Mixed Invoice"},
                new InvoiceType {Id = 6, Type = "Commercial Invoice"},
                new InvoiceType {Id = 7, Type = "Timesheet Invoice"},
                new InvoiceType {Id = 8, Type = "Expense Report"},
                new InvoiceType {Id = 9, Type = "Pro Forma Invoice"}
            };
        }

        public static List<BusinessType> BusinessTypes()
        {
            return new List<BusinessType>
            {
                new BusinessType {Id = 1, Type = "Sole Proprietorship"},
                new BusinessType {Id = 2, Type = "Partnership"},
                new BusinessType {Id = 3, Type = "Limited Liability Company (LLC)"},
                new BusinessType {Id = 4, Type = "Corporation"},
                new BusinessType {Id = 5, Type = "Nonprofit Organization"},
            };
        }

        public static List<OperatingSector> OperatingSectors()
        {
            return new List<OperatingSector>
            {
                new OperatingSector {Id = 1, Sector = "Aerospace"},
                new OperatingSector {Id = 2, Sector = "Entertainment"},
                new OperatingSector {Id = 3, Sector = "Agriculture"},
                new OperatingSector {Id = 4, Sector = "Financial Services"},
                new OperatingSector {Id = 5, Sector = "Courier"},
                new OperatingSector {Id = 6, Sector = "Government/Public Sector Services"},
                new OperatingSector {Id = 7, Sector = "Purchase Stock"},
                new OperatingSector {Id = 8, Sector = "Healthcare"},
                new OperatingSector {Id = 9, Sector = "Data Analytics/Data Science"},
                new OperatingSector {Id = 10, Sector = "Insurance"},
                new OperatingSector {Id = 11, Sector = "Education"},
                new OperatingSector {Id = 12, Sector = "IT"},
                new OperatingSector {Id = 13, Sector = "Manufacturing"},
                new OperatingSector {Id = 14, Sector = "Retail"},
                new OperatingSector {Id = 15, Sector = "Media"},
                new OperatingSector {Id = 16, Sector = "Wholesale"},
                new OperatingSector {Id = 17, Sector = "Print/Publishing"},
                new OperatingSector {Id = 18, Sector = "Energy"},
                new OperatingSector {Id = 19, Sector = "Fashion and accessories"},
                new OperatingSector {Id = 20, Sector = "E-commerce"},
                new OperatingSector {Id = 21, Sector = "Recruitment Services"},
                new OperatingSector {Id = 22, Sector = "Import/Export"},
                new OperatingSector {Id = 23, Sector = "Transportation Services"},
                new OperatingSector {Id = 24, Sector = "Logistics Stock"},
                new OperatingSector {Id = 25, Sector = "Construction"}
            };
        }

        public static List<MeasurementUnit> MeasurementUnits()
        {
            return new List<MeasurementUnit>
            {
                new MeasurementUnit {Id = 1, Name = "Metres", Unit = "m"},
                new MeasurementUnit {Id = 2, Name = "Gram", Unit = "g"},
                new MeasurementUnit {Id = 3, Name = "Kilogram", Unit = "Kg"},
                new MeasurementUnit {Id = 4, Name = "Litres", Unit = "l"},
                new MeasurementUnit {Id = 5, Name = "Millilitres", Unit = "ml"},
            };
        }

        public static List<InvoiceColorTheme> InvoiceColorThemes()
        {
            var date = new DateTime(2021, 09, 09);
            return new List<InvoiceColorTheme>
            {
                 new InvoiceColorTheme {Id = new Guid("ef7d0d51-3635-4125-a21b-d5826d985101"), CreatedOn = date, Theme ="#2E5BFF,#E5E9F2,#3B4863", Name ="Shelob", TextColor ="#001737" },
                 new InvoiceColorTheme {Id = new Guid("bf2c9819-5a80-4737-9475-75dc902369b8"), CreatedOn = date, Theme = "#7987A1,#10B759,#2E5BFF", Name ="Denethor", TextColor ="#001737" } ,
                 new InvoiceColorTheme {Id = new Guid("a67d4caf-5428-454b-8f5f-6211a5efcacf"), CreatedOn = date, Theme = "#00B8D4,#E5E9F2,#3B4863", Name = "Quickbeam", TextColor = "#001737" } ,
                 new InvoiceColorTheme {Id = new Guid("c7bcd9a9-2bd0-4e83-a9b6-a286ec4fd027"), CreatedOn = date, Theme = "#3B4863,#FFFFFF,#10B759", Name ="Shadowfax", TextColor ="#001737" },
                 new InvoiceColorTheme {Id = new Guid("75bce879-8b3f-4774-870a-2d39460231c6"), CreatedOn = date, Theme = "#902145,#E5E9F2,#00B8D4", Name = "Grima", TextColor = "#001737" }
            };
        }

        public static List<BillCategory> BillCategories()
        {
            return new List<BillCategory>
             {
                new BillCategory { CategoryId= "1", CategoryName = "Utility Bills", Description = "Pay your utility bills here" },
                new BillCategory { CategoryId = "2", CategoryName = "Cable TV Bills", Description = "Pay for your cable TV subscriptions here" },
                new BillCategory { CategoryId = "4", CategoryName = "Mobile Recharge", Description = "Recharge your phone" },
                new BillCategory { CategoryId = "9", CategoryName = "Subscriptions", Description = "Pay for your other subscriptions (like ISP) here" },
                new BillCategory { CategoryId= "12", CategoryName = "Tax Payments", Description = "Tax Payments" },
                new BillCategory { CategoryId= "13", CategoryName = "Insurance Payments" , Description = "Insurance Payments" }
            };
        }
        
        public static List<AccountClass> AccountClasses()
        {
            //B is Balance Sheet, P is Profit and Loss account
            return new List<AccountClass>
            {
                new AccountClass {Id = 1, Class = "Assets", Type = 'B', AccountTreatment = 'A'},
                new AccountClass {Id = 2, Class = "Liability", Type = 'B', AccountTreatment = 'L'},
                new AccountClass {Id = 3, Class = "Equity", Type = 'B', AccountTreatment = 'E'},
                new AccountClass {Id = 4, Class = "Income", Type = 'P', AccountTreatment = 'I'},
                new AccountClass {Id = 5, Class = "Expense", Type = 'P', AccountTreatment = 'E'},
            };
        }
        
        public static List<AccountSubClass> AccountSubClasses()
        {
            return new List<AccountSubClass>
            {
                new AccountSubClass {Id = 1, AccountClassId = 1, SubClass = "Current Asset"},
                new AccountSubClass {Id = 2, AccountClassId = 1, SubClass = "Non-current Asset"},
                new AccountSubClass {Id = 3, AccountClassId = 2, SubClass = "Current Liability"},
                new AccountSubClass {Id = 4, AccountClassId = 2, SubClass = "Non-current Liability"},
                new AccountSubClass {Id = 5, AccountClassId = 3, SubClass = "Equity"},
                new AccountSubClass {Id = 6, AccountClassId = 4, SubClass = "Income"},
                new AccountSubClass {Id = 7, AccountClassId = 4, SubClass = "Revenue"},
                new AccountSubClass {Id = 8, AccountClassId = 5, SubClass = "Cost of Sales"},
                new AccountSubClass {Id = 9, AccountClassId = 5, SubClass = "Expenses"},
                new AccountSubClass {Id = 10, AccountClassId = 5, SubClass = "Distribution Costs"},
                new AccountSubClass {Id = 11, AccountClassId = 5, SubClass = "Administrative Expenses"},
                new AccountSubClass {Id = 12, AccountClassId = 5, SubClass = "Finance Costs"},
                new AccountSubClass {Id = 13, AccountClassId = 5, SubClass = "Income Tax Expense"}
                
            };
        }

        public static List<AccountType> AccountTypes()
        {
            return new List<AccountType>
            {
                new AccountType{ Id = AccountTypeConstants.Cash, Name = "Cash and cash equivalents", AccountClassId = 1, AccountSubClassId = 1 },
                new AccountType{ Id = AccountTypeConstants.AccountsReceivable, Name = "Accounts receivable (A/R)", AccountClassId = 1, AccountSubClassId = 1 },
                new AccountType{ Id = AccountTypeConstants.AccountsPayable, Name = "Accounts payable (A/P)", AccountClassId = 2, AccountSubClassId = 3},
                new AccountType{ Id = AccountTypeConstants.OwnerEquity, Name = "Owner's equity", AccountClassId = 3, AccountSubClassId = 5},
                new AccountType{ Id = AccountTypeConstants.Income, Name = "Income", AccountClassId = 4, AccountSubClassId = 7},
                new AccountType{ Id = AccountTypeConstants.CostOfSales, Name = "Cost of sales", AccountClassId = 5, AccountSubClassId = 8},
                new AccountType{ Id = AccountTypeConstants.Expenses, Name = "Expenses", AccountClassId = 5, AccountSubClassId = 9},
                new AccountType{ Id = AccountTypeConstants.Inventories, Name = "Inventories", AccountClassId = 1, AccountSubClassId = 1},
                new AccountType{ Id = AccountTypeConstants.TradeReceivables, Name = "Trade and other receivables", AccountClassId = 1, AccountSubClassId = 1},
                new AccountType{ Id = AccountTypeConstants.Prepayments, Name = "Prepayments", AccountClassId = 1, AccountSubClassId = 1},
                new AccountType{ Id = AccountTypeConstants.TradePayables, Name = "Trade and other payables", AccountClassId = 2, AccountSubClassId = 3},
                new AccountType{ Id = AccountTypeConstants.Property, Name = "Property, plant and equipment", AccountClassId = 1, AccountSubClassId = 2},
                new AccountType{ Id = AccountTypeConstants.LongTermInvestment, Name = "Long-term investments", AccountClassId = 1, AccountSubClassId = 2},
                new AccountType{ Id = AccountTypeConstants.LongTermDebt, Name = "Long-term debt", AccountClassId = 2, AccountSubClassId = 4},
                new AccountType{ Id = AccountTypeConstants.OtherIncome, Name = "Other income", AccountClassId = 4, AccountSubClassId = 7},
                new AccountType{ Id = AccountTypeConstants.OtherExpense, Name = "Other expense", AccountClassId = 5, AccountSubClassId = 9},
                new AccountType{ Id = AccountTypeConstants.TaxPayable, Name = "Tax payable", AccountClassId = 2, AccountSubClassId = 3},
                new AccountType{ Id = AccountTypeConstants.IntangibleAsset, Name = "Intangible assets", AccountClassId = 1, AccountSubClassId = 2},
                new AccountType{ Id = AccountTypeConstants.InvestmentPropertyAtCost, Name = "Investment property carried at cost", AccountClassId = 1, AccountSubClassId = 2},
                new AccountType{ Id = AccountTypeConstants.InvestmentPropertyAtFairValue, Name = "Investment property carried at fair value", AccountClassId = 1, AccountSubClassId = 2},
                new AccountType{ Id = AccountTypeConstants.BiologicalAsset, Name = "Biological assets", AccountClassId = 1, AccountSubClassId = 1 },
                new AccountType{ Id = AccountTypeConstants.Provisions, Name = "Provisions", AccountClassId = 2, AccountSubClassId = 3},
                new AccountType{ Id = AccountTypeConstants.DeferredTaxAsset, Name = "Deferred Tax Asset", AccountClassId = 1, AccountSubClassId = 2},
                new AccountType{ Id = AccountTypeConstants.DeferredTaxLiability, Name = "Deferred Tax Liability", AccountClassId = 2, AccountSubClassId = 4},
                new AccountType{ Id = AccountTypeConstants.DistributionCost, Name = "Distribution costs", AccountClassId = 5, AccountSubClassId = 10},
                new AccountType{ Id = AccountTypeConstants.AdminExpenses, Name = "Administrative expenses", AccountClassId = 5, AccountSubClassId = 11},
                new AccountType{ Id = AccountTypeConstants.FinanceCost, Name = "Finance costs", AccountClassId = 5, AccountSubClassId = 12},
                new AccountType{ Id = AccountTypeConstants.IncomeTaxExpense, Name = "Income Tax Expense", AccountClassId = 5, AccountSubClassId = 13}
            };
        }

        public static List<Bank> Banks()
        {
            return new List<Bank>
            {
                new Bank {Id = 1, BankCode = "044", BankName = "Access Bank"},
                new Bank {Id = 2, BankCode = "063", BankName = "Access Bank (Diamond)"},
                new Bank {Id = 3, BankCode = "035A", BankName = "ALAT by WEMA"},
                new Bank {Id = 4, BankCode = "401", BankName = "ASO Savings and Loans"},
                new Bank {Id = 5, BankCode = "023", BankName = "Citibank Nigeria"},
                new Bank {Id = 6, BankCode = "050", BankName = "Ecobank Bank Nigeria"},
                new Bank {Id = 7, BankCode = "562", BankName = "Ekondo Microfinance Bank"},
                new Bank {Id = 8, BankCode = "084", BankName = "Enterprise Bank"},
                new Bank {Id = 9, BankCode = "070", BankName = "Fidelity Bank"},
                new Bank {Id = 10, BankCode = "011", BankName = "First Bank of Nigeria"},
                new Bank {Id = 11, BankCode = "214", BankName = "First City Monument Bank"},
                new Bank {Id = 12, BankCode = "058", BankName = "Guaranty Trust Bank"},
                new Bank {Id = 13, BankCode = "030", BankName = "Heritage Bank"},
                new Bank {Id = 14, BankCode = "301", BankName = "Jaiz Bank"},
                new Bank {Id = 15, BankCode = "082", BankName = "Keystone Bank"},
                new Bank {Id = 16, BankCode = "014", BankName = "MainStreet Bank"},
                new Bank {Id = 17, BankCode = "526", BankName = "Parallex Bank"},
                new Bank {Id = 18, BankCode = "076", BankName = "Polaris Bank"},
                new Bank {Id = 19, BankCode = "101", BankName = "Providus Bank"},
                new Bank {Id = 20, BankCode = "221", BankName = "Stanbic IBTC Bank"},
                new Bank {Id = 21, BankCode = "068", BankName = "Standard Chartered Bank"},
                new Bank {Id = 22, BankCode = "232", BankName = "Sterling Bank"},
                new Bank {Id = 23, BankCode = "100", BankName = "Suntrust Bank"},
                new Bank {Id = 24, BankCode = "032", BankName = "Union Bank of Bank"},
                new Bank {Id = 25, BankCode = "033", BankName = "United Bank for Africa"},
                new Bank {Id = 26, BankCode = "215", BankName = "Unity Bank"},
                new Bank {Id = 27, BankCode = "035", BankName = "Wema Bank"},
                new Bank {Id = 28, BankCode = "057", BankName = "Zenith Bank"}

            };
        }
    }
}
