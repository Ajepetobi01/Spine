using System.ComponentModel;
using Spine.Common.Attributes;

namespace Spine.Common.Enums
{
    public enum InventoryType
    {
        Product = 1,
        Service
    }

    public enum InventoryStatus
    {
        [ExcludeEnumValue]
        None = 0,
        Active,
        OnHold,
        Discontinued
    }

    public enum PurchaseOrderStatus
    {
        Draft = 0,
        Issued,
        [Description("Partially received")]
        PartiallyReceived,
        Closed
    }

}
