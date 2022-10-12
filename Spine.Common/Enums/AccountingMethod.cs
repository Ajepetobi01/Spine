using System.ComponentModel;
using Spine.Common.Attributes;

namespace Spine.Common.Enums
{
    public enum AccountingMethod
    {
        [Description("Accrual basis")]
        Accrual = 1,
        [Description("Cash basis")]
        Cash,
    }
}
