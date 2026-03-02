using System;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Content;

namespace CatCatGo.Domain.Economy
{
    public class DailyResetSystem
    {
        private string _lastResetDate;

        public DailyResetSystem()
        {
            _lastResetDate = GetTodayString();
        }

        public bool NeedsReset()
        {
            return GetTodayString() != _lastResetDate;
        }

        public void PerformReset(Resources resources, DailyDungeonManager dungeonManager)
        {
            resources.DailyReset();
            dungeonManager.DailyResetAll();
            _lastResetDate = GetTodayString();
        }

        private string GetTodayString()
        {
            var now = DateTime.Now;
            return $"{now.Year}-{now.Month}-{now.Day}";
        }

        public string GetLastResetDate()
        {
            return _lastResetDate;
        }

        public void SetLastResetDate(string date)
        {
            _lastResetDate = date;
        }
    }
}
