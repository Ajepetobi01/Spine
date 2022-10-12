using System;

namespace Spine.Common.Extensions
{
    public static class MoneyExtensions
    {
        /// <summary>
        /// format money
        /// </summary>
        public static string FormatMoney(this decimal number)
        {
            return number.ToString("n2");
        }

        /// <summary>
        /// format money
        /// </summary>
        public static string FormatMoney(this decimal number, string currencySymbol) //CurrencyModel currency
        {
            return currencySymbol + number.ToString("n2");
            //  return Regex.Replace(number.ToString(), @"^|(\d{3}(?=(\d{3})*(\.|$)))", m => m.Value == "" ? currencySymbol : "," + m.Value);
        }

        public static decimal RoundToWhole(this decimal value)
        {
            return Math.Round(value, 0, MidpointRounding.AwayFromZero);

            //double d1 = 1.1;
            //double d2 = 1.5;
            //double d3 = 1.9;

            //int i1 = (int)(d1 + 0.5);
            //int i2 = (int)(d2 + 0.5);
            //int i3 = (int)(d3 + 0.5);

            //var roundedA = Math.Round(1.1, 0); // Output: 1
            //var roundedB = Math.Round(1.5, 0, MidpointRounding.AwayFromZero); // Output: 2
            //var roundedC = Math.Round(1.9, 0); // Output: 2
            //var roundedD = Math.Round(2.5, 0); // Output: 2
            //var roundedE = Math.Round(2.5, 0, MidpointRounding.AwayFromZero); // Output: 3
            //var roundedF = Math.Round(3.49, 0, MidpointRounding.AwayFromZero); // Output: 3
        }
    }
}
