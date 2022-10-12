using System.ComponentModel;

namespace Spine.Common.Enums
{
    public enum InterswitchAmountType
    {
        [Description("Any amount can be paid")]
        None = 0,
        [Description("If the returned amount is 1000, customer can pay any amount from 1000")]
        Minimum,
        [Description("If the returned amount is 1000, customer can pay any amount greater than 1000")]
        GreaterThanMinimum,
        [Description("If the amount returned is 1000, customer can pay any amount below or equal to 1000")]
        Maximum,
        [Description("If the amount returned is 1000, customer can pay any amount below to 1000")]
        LessThanMaximum,
        [Description("The exact amount returned must be paid")]
        Exact
    }
}
