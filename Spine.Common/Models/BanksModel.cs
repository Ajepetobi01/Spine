namespace Spine.Common.Models
{
    public class BanksModel
    {
        public string BankName { get; set; }
        public string BankCode { get; set; }

        public string Currency { get; set; } // used with the paystack endpoint
    }

    public class CurrencyModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Symbol { get; set; }
    }

}
