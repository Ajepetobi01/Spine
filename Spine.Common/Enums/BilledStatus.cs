using System.ComponentModel;
using Spine.Common.Attributes;

namespace Spine.Common.Enums
{
    public enum BilledStatus
    {
        Open = 0,
        Due,
        Paid,
    }

    public enum PaymentStatus
    {
        Open, 
        [Description("Partially Paid")]
        PartiallyPaid,
        Paid,
    }
}
