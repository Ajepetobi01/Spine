using System.ComponentModel;
using Spine.Common.Attributes;

namespace Spine.Common.Enums
{
    public enum NotificationCategory
    {
        [ExcludeEnumValue]
        None = 0,
        [Description("Customer Reminder")]
        CustomerReminder,
        [Description("Transaction Reminder")]
        TransactionReminder,
        [Description("Low Stock")]
        LowStock
    }

}
