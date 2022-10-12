using System.ComponentModel;

namespace Spine.Common.Enums
{
    public enum BulkImportType
    {
        Customer = 1,
        [Description("Bank Transaction")]
        BankTransaction,
        [Description("Inventory (Product)")]
        Product,
        [Description("Inventory (Services)")]
        Services,
        [Description("Purchase Order")]
        PurchaseOrder,
        Subscriber,
        [Description("Vendor (Individual)")]
        IndividualVendor,
        [Description("Vendor (Business)")]
        BusinessVendor,
        [Description("Journals")]
        Journal
    }
}
