using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Content;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Economy
{
    public class DailyResetSystem
    {
        private string _lastResetDate;

        public DailyResetSystem()
        {
            _lastResetDate = DateHelper.GetTodayString();
        }

        public bool NeedsReset()
        {
            return DateHelper.GetTodayString() != _lastResetDate;
        }

        public void PerformReset(Resources resources, DailyDungeonManager dungeonManager)
        {
            resources.DailyReset();
            dungeonManager.DailyResetAll();
            _lastResetDate = DateHelper.GetTodayString();
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
