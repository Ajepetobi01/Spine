using System;
namespace Spine.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime CombineDateAndTime(this DateTime date, DateTime? time)
        {
            return time == null ? date.Date : new DateTime(date.Year, date.Month, date.Day, time.Value.Hour, time.Value.Minute, time.Value.Second);
        }

        public static int DiffDays(this DateTime start, DateTime end)
        {
            return (start - end).Duration().Days;
        }

        public static DateTime ToEndOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
        }

    }
}
