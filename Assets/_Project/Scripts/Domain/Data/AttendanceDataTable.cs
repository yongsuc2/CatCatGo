using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public class AttendanceRewardDef
    {
        public int Day;
        public string Type;
        public List<AttendanceResourceDef> Resources;
        public PetGrade? PetGrade;
        public string Description;
    }

    public class AttendanceResourceDef
    {
        public ResourceType Type;
        public int Amount;
    }

    public static class AttendanceDataTable
    {
        private static List<AttendanceRewardDef> _rewards;

        private static void EnsureLoaded()
        {
            if (_rewards != null) return;

            var arr = JsonDataLoader.LoadJArray("attendance.data.json");
            if (arr == null) { _rewards = new List<AttendanceRewardDef>(); return; }

            _rewards = new List<AttendanceRewardDef>();
            foreach (var row in arr)
            {
                var def = new AttendanceRewardDef
                {
                    Day = row["day"].Value<int>(),
                    Type = row["type"].ToString(),
                    Description = row["description"].ToString(),
                };

                if (row["resources"] != null)
                {
                    def.Resources = new List<AttendanceResourceDef>();
                    foreach (var r in row["resources"])
                    {
                        def.Resources.Add(new AttendanceResourceDef
                        {
                            Type = (ResourceType)System.Enum.Parse(typeof(ResourceType), r["type"].ToString()),
                            Amount = r["amount"].Value<int>(),
                        });
                    }
                }

                if (row["petGrade"] != null && row["petGrade"].Type != JTokenType.Null)
                    def.PetGrade = (PetGrade)System.Enum.Parse(typeof(PetGrade), row["petGrade"].ToString());

                _rewards.Add(def);
            }
        }

        public static AttendanceRewardDef GetReward(int day)
        {
            EnsureLoaded();
            return _rewards.FirstOrDefault(r => r.Day == day);
        }

        public static List<AttendanceRewardDef> GetAllRewards()
        {
            EnsureLoaded();
            return _rewards;
        }

        public static int GetTotalDays()
        {
            EnsureLoaded();
            return _rewards.Count;
        }
    }
}
