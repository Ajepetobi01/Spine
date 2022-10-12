using System.ComponentModel;
using Spine.Common.Attributes;

namespace Spine.Common.Enums
{
    public enum DiscountType
    {
        [ExcludeEnumValue]
        None = 0,
        [Description("% Percent")]
        Percentage = 1,
        [Description("Amount")]
        Amount
    }
}
