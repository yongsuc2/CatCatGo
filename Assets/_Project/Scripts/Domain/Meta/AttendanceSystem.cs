using System.Linq;
using CatCatGo.Domain.Data;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Meta
{
    public class AttendanceSystem
    {
        public bool[] CheckedDays;
        public string CycleStartDate;
        public string LastCheckDate;

        public AttendanceSystem()
        {
            CheckedDays = new bool[AttendanceDataTable.GetTotalDays()];
            CycleStartDate = DateHelper.GetTodayString();
            LastCheckDate = "";
        }

        public int GetCurrentDay()
        {
            for (int i = 0; i < CheckedDays.Length; i++)
            {
                if (!CheckedDays[i]) return i;
            }
            return CheckedDays.Length;
        }

        public bool CanCheckIn()
        {
            return LastCheckDate != DateHelper.GetTodayString() && GetCurrentDay() < AttendanceDataTable.GetTotalDays();
        }

        public int CheckIn()
        {
            if (!CanCheckIn()) return -1;
            int dayIndex = GetCurrentDay();
            CheckedDays[dayIndex] = true;
            LastCheckDate = DateHelper.GetTodayString();
            return dayIndex + 1;
        }

        public bool IsComplete()
        {
            return CheckedDays.All(d => d);
        }

        public void ResetCycle()
        {
            CheckedDays = new bool[AttendanceDataTable.GetTotalDays()];
            CycleStartDate = DateHelper.GetTodayString();
            LastCheckDate = "";
        }

    }
}
