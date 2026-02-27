using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Data;

namespace CatCatGo.Domain.Meta
{
    public class EventMission
    {
        public string Id;
        public string Description;
        public int Target;
        public int Current;
        public Reward Reward;
        public bool Claimed;
    }

    public class GameEvent
    {
        public string Id;
        public string Name;
        public EventType Type;
        public long StartTime;
        public long EndTime;
        public List<EventMission> Missions;

        public GameEvent(
            string id,
            string name,
            EventType type,
            long startTime,
            long endTime,
            List<EventMission> missions = null)
        {
            Id = id;
            Name = name;
            Type = type;
            StartTime = startTime;
            EndTime = endTime;
            Missions = missions ?? new List<EventMission>();
        }

        public bool IsActive(long now = 0)
        {
            if (now == 0) now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return now >= StartTime && now <= EndTime;
        }

        public bool UpdateMissionProgress(string missionId, int amount)
        {
            var mission = Missions.FirstOrDefault(m => m.Id == missionId);
            if (mission == null) return false;
            mission.Current = Math.Min(mission.Current + amount, mission.Target);
            return true;
        }

        public bool IsMissionCompleted(string missionId)
        {
            var mission = Missions.FirstOrDefault(m => m.Id == missionId);
            if (mission == null) return false;
            return mission.Current >= mission.Target;
        }

        public Reward ClaimMissionReward(string missionId)
        {
            var mission = Missions.FirstOrDefault(m => m.Id == missionId);
            if (mission == null || mission.Current < mission.Target || mission.Claimed) return null;
            mission.Claimed = true;
            return mission.Reward;
        }

        public float GetProgress()
        {
            if (Missions.Count == 0) return 0;
            int completed = Missions.Count(m => m.Current >= m.Target);
            return (float)completed / Missions.Count;
        }

        public int GetCompletedMissionCount()
        {
            return Missions.Count(m => m.Current >= m.Target);
        }
    }

    public class EventManager
    {
        public List<GameEvent> Events;

        public EventManager()
        {
            Events = new List<GameEvent>();
        }

        public void AddEvent(GameEvent evt)
        {
            Events.Add(evt);
        }

        public void RemoveEvent(string id)
        {
            Events = Events.Where(e => e.Id != id).ToList();
        }

        public List<GameEvent> GetActiveEvents(long now = 0)
        {
            if (now == 0) now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return Events.Where(e => e.IsActive(now)).ToList();
        }

        public GameEvent GetEvent(string id)
        {
            return Events.FirstOrDefault(e => e.Id == id);
        }

        public void CleanupExpired(long now = 0)
        {
            if (now == 0) now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Events = Events.Where(e => now <= e.EndTime).ToList();
        }

        public GameEvent CreateDailyQuests()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var endOfDay = DateTime.Today.AddDays(1).AddMilliseconds(-1);
            long endOfDayMs = new DateTimeOffset(endOfDay).ToUnixTimeMilliseconds();

            var missions = QuestDataTable.Daily.Select(q => MakeMission(
                q.Id, q.Description, q.Target, q.RewardType, q.RewardAmount)).ToList();

            var evt = new GameEvent(
                $"daily_{now}", "\uc77c\uc77c \ud034\uc2a4\ud2b8", EventType.MISSION,
                now, endOfDayMs, missions);

            AddEvent(evt);
            return evt;
        }

        public GameEvent CreateWeeklyQuests()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var today = DateTime.Today;
            int daysUntilSunday = 7 - (int)today.DayOfWeek;
            if (daysUntilSunday == 0) daysUntilSunday = 7;
            var endOfWeek = today.AddDays(daysUntilSunday + 1).AddMilliseconds(-1);
            long endOfWeekMs = new DateTimeOffset(endOfWeek).ToUnixTimeMilliseconds();

            var missions = QuestDataTable.Weekly.Select(q => MakeMission(
                q.Id, q.Description, q.Target, q.RewardType, q.RewardAmount)).ToList();

            var evt = new GameEvent(
                $"weekly_{now}", "\uc8fc\uac04 \ud034\uc2a4\ud2b8", EventType.MISSION,
                now, endOfWeekMs, missions);

            AddEvent(evt);
            return evt;
        }

        public bool HasActiveWeeklyQuest()
        {
            return Events.Any(e => e.Id.StartsWith("weekly_") && e.IsActive());
        }

        private static EventMission MakeMission(string id, string description, int target, ResourceType type, int amount)
        {
            return new EventMission
            {
                Id = id,
                Description = description,
                Target = target,
                Current = 0,
                Claimed = false,
                Reward = Reward.FromResources(new ResourceReward(type, amount)),
            };
        }
    }
}
