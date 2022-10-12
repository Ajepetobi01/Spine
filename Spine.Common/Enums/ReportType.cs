using System.ComponentModel;
using Spine.Common.Attributes;

namespace Spine.Common.Enums
{
    public enum PLReportType
    {
        Single = 1,
        [Description("Month on Month")]
        MonthOnMonth,
        [Description("Year on Year")]
        YearOnYear,
        [Description("Year to Date vs Last Year End")]
        YearToDateOnLastYear,
    }
}
