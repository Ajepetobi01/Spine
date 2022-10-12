namespace Spine.Core.Invoices.Helpers
{
    public static class Extensions
    {
        /// <summary>
        /// turn the long number to invoice no string
        /// </summary>
        public static string ToInvoiceNo(this long invNo, string prefix)
        {
            int length = 6;
            return $"{prefix.Trim()}-" + invNo.ToString($"D{length}");
        }

    }
}
