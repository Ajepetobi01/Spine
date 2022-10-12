using System.ComponentModel.DataAnnotations;

namespace Spine.Data.Entities.Transactions
{
    public class AccountType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int AccountClassId { get; set; } 
        public int AccountSubClassId { get; set; }
        
    }
    
    //constants, to use to get the Id (instead of name)
    public static class AccountTypeConstants
    {
        public const int Cash = 1;
        public const int AccountsReceivable = 2;
        public const int AccountsPayable = 3;
        public const int OwnerEquity = 4;
        public const int Income = 5;
        public const int CostOfSales = 6;
        public const int Expenses = 7;
        public const int Inventories = 8;
        public const int TradeReceivables = 9;
        public const int Prepayments = 10;
        public const int TradePayables = 11;
        public const int Property = 12;
        public const int LongTermInvestment = 13;
        public const int LongTermDebt = 14;
        public const int OtherIncome = 15;
        public const int OtherExpense = 16;
        public const int TaxPayable = 17;
        public const int IntangibleAsset = 18;
        public const int InvestmentPropertyAtCost = 19;
        public const int InvestmentPropertyAtFairValue = 20;
        public const int BiologicalAsset = 21;
        public const int Provisions = 22;
        public const int DeferredTaxAsset = 23;
        public const int DeferredTaxLiability = 24;
        public const int DistributionCost = 25;
        public const int AdminExpenses = 26;
        public const int FinanceCost = 27;
        public const int IncomeTaxExpense = 28;
        

    }
}
