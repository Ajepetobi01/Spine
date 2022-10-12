using System.ComponentModel;
using Spine.Common.Attributes;

namespace Spine.Common.Enums
{
    public enum InvoiceFrequency
    {
        [ExcludeEnumValue]
        None = 0,
        [Description("Every Day")]
        Daily,
        [Description("Every Week")]
        Weekly,
        [Description("Every Month")]
        Monthly,
        [Description("Every Year")]
        Yearly
    }
}
