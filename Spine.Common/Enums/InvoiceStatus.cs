using System.ComponentModel;

namespace Spine.Common.Enums
{
    public enum InvoiceStatus
    {
        Cancelled,
        Generated,
        Sent,
        [Description("Part Payment")]
        PartPayment,
        [Description("Payment Completed")]
        CompletePayment
    }

    public enum PaymentStatusFilter
    {
        [Description("Not Due")]
        NotDue = 1,
        [Description("Payment Due")]
        Due,
        [Description("Payment Completed")]
        Paid
    }

}
