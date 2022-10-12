using System.ComponentModel;

namespace Spine.Common.Enums
{
    public enum PaymentMode
    {
        Account = 1,
        Cash,
        Card,
        [Description("Mobile Transfer")]
        MobileTransfer
    }
}
