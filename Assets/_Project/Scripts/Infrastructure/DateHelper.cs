using System;

namespace CatCatGo.Infrastructure
{
    public static class DateHelper
    {
        public static string GetTodayString()
        {
            var now = DateTime.Now;
            return $"{now.Year}-{now.Month}-{now.Day}";
        }
    }
}
