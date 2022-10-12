using System.ComponentModel;

namespace Spine.Common.Enums
{
    public enum GlobalAccountType
    {
        None = 0,
        [Description("Withholding Tax Account Number")]
        WithholdingTax,
        [Description("VAT Account Number")]
        VAT,
        [Description("Inventory Write-off Account Number")]
        InventoryWriteOff,
        [Description("Discount Allowed Account Number")]
        DiscountAllowed,
        [Description("Reserve Account Number (Retained earnings)")]
        RetainedEarnings,
    }

}
