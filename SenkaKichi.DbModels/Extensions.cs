using System;

namespace SenkaKichi.DbModels
{
    public static class DateTimeExtensions
    {
        public static string ToUnixTimestamp(this DateTime d) {
            var duration = d - new DateTime(1970, 1, 1, 0, 0, 0);

            return duration.TotalSeconds.ToString("F0");
        }
    } 
}
