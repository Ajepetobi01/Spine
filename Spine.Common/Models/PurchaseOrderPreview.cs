using System;
using System.Collections.Generic;

namespace Spine.Common.Models
{
    public class PurchaseOrderPreview
    {
        public string VendorName { get; set; }
        public string Notes { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDate { get; set; }

        public decimal Amount { get; set; }

        public string CompanyLogo { get; set; }
        public string CurrencySymbol { get; set; }

        public CompanyModel Business { get; set; }
        public List<OrderLineItem> LineItems { get; set; }
    }

    public class OrderLineItem
    {
        public string Item { get; set; }
        public string Description { get; set; }

        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }
}
