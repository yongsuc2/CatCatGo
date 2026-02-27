using System;
using System.Linq;
using CatCatGo.Domain.Data;

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
            CycleStartDate = GetTodayString();
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
            return LastCheckDate != GetTodayString() && GetCurrentDay() < AttendanceDataTable.GetTotalDays();
        }

        public int CheckIn()
        {
            if (!CanCheckIn()) return -1;
            int dayIndex = GetCurrentDay();
            CheckedDays[dayIndex] = true;
            LastCheckDate = GetTodayString();
            return dayIndex + 1;
        }

        public bool IsComplete()
        {
            return CheckedDays.All(d => d);
        }

        public void ResetCycle()
        {
            CheckedDays = new bool[AttendanceDataTable.GetTotalDays()];
            CycleStartDate = GetTodayString();
            LastCheckDate = "";
        }

        private string GetTodayString()
        {
            var now = DateTime.Now;
            return $"{now.Year}-{now.Month}-{now.Day}";
        }
    }
}
