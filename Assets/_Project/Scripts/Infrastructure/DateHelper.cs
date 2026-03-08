using System;

namespace CatCatGo.Infrastructure
{
    public static class DateHelper
    {
        public static string GetTodayString()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
    }
}
