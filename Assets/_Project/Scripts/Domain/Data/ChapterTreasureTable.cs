using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public class ChapterMilestone
    {
        public string Id;
        public int ChapterId;
        public string Type;
        public int RequiredDay;
        public string Label;
        public Reward MilestoneReward;
    }

    public static class ChapterTreasureTable
    {
        private static List<SurvivalMilestoneRow> _survivalMilestones;
        private static ClearMilestoneRow _clearMilestone;
        private static int _totalDays;

        private class SurvivalMilestoneRow
        {
            public int Day;
            public int Gold;
            public int Gems;
            public int EqStone;
            public int PwStone;
        }

        private class ClearMilestoneRow
        {
            public int Gold;
            public int Gems;
            public int EqStone;
            public int PwStone;
        }

        private static void EnsureLoaded()
        {
            if (_survivalMilestones != null) return;

            var data = JsonDataLoader.LoadJObject("chapter-treasure.data.json");
            if (data == null) return;

            _totalDays = data["totalDays"]?.Value<int>() ?? 60;

            _survivalMilestones = new List<SurvivalMilestoneRow>();
            foreach (var m in data["survivalMilestones"])
            {
                _survivalMilestones.Add(new SurvivalMilestoneRow
                {
                    Day = m["day"].Value<int>(),
                    Gold = m["gold"].Value<int>(),
                    Gems = m["gems"].Value<int>(),
                    EqStone = m["eqStone"].Value<int>(),
                    PwStone = m["pwStone"].Value<int>(),
                });
            }

            var c = data["clearMilestone"];
            _clearMilestone = new ClearMilestoneRow
            {
                Gold = c["gold"].Value<int>(),
                Gems = c["gems"].Value<int>(),
                EqStone = c["eqStone"].Value<int>(),
                PwStone = c["pwStone"].Value<int>(),
            };
        }

        public static ChapterType GetChapterType(int chapterId)
        {
            return ChapterType.SIXTY_DAY;
        }

        public static int GetTotalDays(int chapterId)
        {
            EnsureLoaded();
            return _totalDays;
        }

        public static int GetClearSentinelDay(int chapterId)
        {
            return GetTotalDays(chapterId) + 1;
        }

        public static List<ChapterMilestone> GetMilestonesForChapter(int chapterId)
        {
            EnsureLoaded();
            var milestones = new List<ChapterMilestone>();
            foreach (var m in _survivalMilestones)
                milestones.Add(MakeSurviveMilestone(chapterId, m.Day, m.Gold, m.Gems, m.EqStone, m.PwStone));
            milestones.Add(MakeClearMilestone(chapterId, _clearMilestone.Gold, _clearMilestone.Gems, _clearMilestone.EqStone, _clearMilestone.PwStone));
            return milestones;
        }

        public static ChapterMilestone GetMilestoneById(string milestoneId)
        {
            EnsureLoaded();
            var match = Regex.Match(milestoneId, @"^ch(\d+)_");
            if (!match.Success) return null;
            int chapterId = int.Parse(match.Groups[1].Value);
            var milestones = GetMilestonesForChapter(chapterId);
            return milestones.FirstOrDefault(m => m.Id == milestoneId);
        }

        public static List<int> GetAvailableChapterIds(int clearedChapterMax)
        {
            var ids = new List<int>();
            for (int i = 1; i <= clearedChapterMax + 1; i++)
                ids.Add(i);
            return ids;
        }

        private static Reward BuildReward(int id, int gold, int gems, int eqStone, int pwStone)
        {
            var resources = new List<ResourceReward>();
            if (gold > 0) resources.Add(new ResourceReward(ResourceType.GOLD, gold * id));
            if (gems > 0) resources.Add(new ResourceReward(ResourceType.GEMS, gems));
            if (eqStone > 0) resources.Add(new ResourceReward(ResourceType.EQUIPMENT_STONE, eqStone));
            if (pwStone > 0) resources.Add(new ResourceReward(ResourceType.POWER_STONE, pwStone));
            return Reward.FromResources(resources.ToArray());
        }

        private static ChapterMilestone MakeSurviveMilestone(int chapterId, int day, int gold, int gems, int eqStone, int pwStone)
        {
            return new ChapterMilestone
            {
                Id = $"ch{chapterId}_d{day}",
                ChapterId = chapterId,
                Type = "SURVIVE",
                RequiredDay = day,
                Label = $"{day}\uc77c",
                MilestoneReward = BuildReward(chapterId, gold, gems, eqStone, pwStone),
            };
        }

        private static ChapterMilestone MakeClearMilestone(int chapterId, int gold, int gems, int eqStone, int pwStone)
        {
            return new ChapterMilestone
            {
                Id = $"ch{chapterId}_clear",
                ChapterId = chapterId,
                Type = "CLEAR",
                RequiredDay = 0,
                Label = "\ucc55\ud130 \ud074\ub9ac\uc5b4",
                MilestoneReward = BuildReward(chapterId, gold, gems, eqStone, pwStone),
            };
        }
    }
}
