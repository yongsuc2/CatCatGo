using System;
using System.Linq;
using NUnit.Framework;
using CatCatGo.Domain.Meta;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using System.Collections.Generic;

namespace CatCatGo.Tests.Meta
{
    [TestFixture]
    public class GameEventTests
    {
        [Test]
        public void IsActiveWithinTimeRange()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var evt = new GameEvent("e1", "Test", EventType.MISSION, now - 1000, now + 10000);
            Assert.IsTrue(evt.IsActive(now));
        }

        [Test]
        public void IsNotActiveOutsideTimeRange()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var evt = new GameEvent("e1", "Test", EventType.MISSION, now + 1000, now + 10000);
            Assert.IsFalse(evt.IsActive(now));
        }

        [Test]
        public void TracksMissionProgress()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var evt = new GameEvent("e1", "Test", EventType.MISSION, now, now + 10000, new List<EventMission>
            {
                new EventMission { Id = "m1", Description = "Do something", Target = 5, Current = 0, Reward = Reward.FromResources(new ResourceReward(ResourceType.GOLD, 100)), Claimed = false },
            });

            evt.UpdateMissionProgress("m1", 3);
            Assert.IsFalse(evt.IsMissionCompleted("m1"));

            evt.UpdateMissionProgress("m1", 3);
            Assert.IsTrue(evt.IsMissionCompleted("m1"));
        }

        [Test]
        public void ClaimsMissionRewardOnlyWhenCompleted()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var evt = new GameEvent("e1", "Test", EventType.MISSION, now, now + 10000, new List<EventMission>
            {
                new EventMission { Id = "m1", Description = "Do something", Target = 3, Current = 0, Reward = Reward.FromResources(new ResourceReward(ResourceType.GOLD, 100)), Claimed = false },
            });

            Assert.IsNull(evt.ClaimMissionReward("m1"));

            evt.UpdateMissionProgress("m1", 5);
            var reward = evt.ClaimMissionReward("m1");
            Assert.IsNotNull(reward);
            Assert.Greater(reward.Resources.Count, 0);
        }

        [Test]
        public void ReportsProgressCorrectly()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var evt = new GameEvent("e1", "Test", EventType.MISSION, now, now + 10000, new List<EventMission>
            {
                new EventMission { Id = "m1", Description = "A", Target = 1, Current = 1, Reward = Reward.Empty(), Claimed = false },
                new EventMission { Id = "m2", Description = "B", Target = 1, Current = 0, Reward = Reward.Empty(), Claimed = false },
            });

            Assert.AreEqual(0.5f, evt.GetProgress(), 0.001f);
        }

        [Test]
        public void DoesNotExceedTargetWhenOvershoot()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var evt = new GameEvent("e1", "Test", EventType.MISSION, now, now + 10000, new List<EventMission>
            {
                new EventMission { Id = "m1", Description = "Test", Target = 3, Current = 0, Reward = Reward.Empty(), Claimed = false },
            });

            evt.UpdateMissionProgress("m1", 100);
            Assert.AreEqual(3, evt.Missions[0].Current);
        }

        [Test]
        public void CannotClaimRewardTwice()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var evt = new GameEvent("e1", "Test", EventType.MISSION, now, now + 10000, new List<EventMission>
            {
                new EventMission { Id = "m1", Description = "Test", Target = 1, Current = 0, Reward = Reward.FromResources(new ResourceReward(ResourceType.GOLD, 50)), Claimed = false },
            });

            evt.UpdateMissionProgress("m1", 1);
            Assert.IsNotNull(evt.ClaimMissionReward("m1"));
            Assert.IsNull(evt.ClaimMissionReward("m1"));
        }

        [Test]
        public void ReturnsFalseWhenUpdatingNonExistentMission()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var evt = new GameEvent("e1", "Test", EventType.MISSION, now, now + 10000, new List<EventMission>());
            Assert.IsFalse(evt.UpdateMissionProgress("nonexistent", 1));
        }
    }

    [TestFixture]
    public class EventManagerTests
    {
        [Test]
        public void ManagesActiveEvents()
        {
            var manager = new EventManager();
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            manager.AddEvent(new GameEvent("active", "Active", EventType.MISSION, now - 1000, now + 10000));
            manager.AddEvent(new GameEvent("expired", "Expired", EventType.MISSION, now - 10000, now - 1000));

            Assert.AreEqual(1, manager.GetActiveEvents(now).Count);
        }

        [Test]
        public void CreatesDailyQuests()
        {
            var manager = new EventManager();
            var evt = manager.CreateDailyQuests();

            Assert.AreEqual(5, evt.Missions.Count);
            Assert.IsTrue(evt.IsActive());
        }

        [Test]
        public void CreatesWeeklyQuests()
        {
            var manager = new EventManager();
            var evt = manager.CreateWeeklyQuests();

            Assert.AreEqual(4, evt.Missions.Count);
            Assert.IsTrue(evt.IsActive());
        }

        [Test]
        public void CleansUpExpiredEvents()
        {
            var manager = new EventManager();
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            manager.AddEvent(new GameEvent("old", "Old", EventType.MISSION, now - 10000, now - 1000));
            manager.AddEvent(new GameEvent("active", "Active", EventType.MISSION, now, now + 10000));

            manager.CleanupExpired(now);
            Assert.AreEqual(1, manager.Events.Count);
        }
    }

    [TestFixture]
    public class QuestMissionIdConsistencyTests
    {
        private static readonly string[] DailyMissionIds =
        {
            "daily_chapter",
            "daily_dungeon",
            "daily_tower",
        };

        private static readonly string[] WeeklyMissionIds =
        {
            "weekly_chapter",
            "weekly_gacha",
            "weekly_sell",
            "weekly_tower",
        };

        [Test]
        public void DailyQuestsContainAllRequiredMissionIds()
        {
            var manager = new EventManager();
            var evt = manager.CreateDailyQuests();
            var missionIds = evt.Missions.Select(m => m.Id).ToList();

            foreach (var id in DailyMissionIds)
            {
                Assert.IsTrue(missionIds.Contains(id), $"Missing daily mission: {id}");
            }
        }

        [Test]
        public void WeeklyQuestsContainAllRequiredMissionIds()
        {
            var manager = new EventManager();
            var evt = manager.CreateWeeklyQuests();
            var missionIds = evt.Missions.Select(m => m.Id).ToList();

            foreach (var id in WeeklyMissionIds)
            {
                Assert.IsTrue(missionIds.Contains(id), $"Missing weekly mission: {id}");
            }
        }

        [Test]
        public void DailyQuestMissionsHavePositiveTargetAndReward()
        {
            var manager = new EventManager();
            var evt = manager.CreateDailyQuests();

            foreach (var mission in evt.Missions)
            {
                Assert.Greater(mission.Target, 0);
                Assert.Greater(mission.Reward.Resources.Count, 0);
                Assert.AreEqual(0, mission.Current);
                Assert.IsFalse(mission.Claimed);
            }
        }

        [Test]
        public void WeeklyQuestMissionsHavePositiveTargetAndReward()
        {
            var manager = new EventManager();
            var evt = manager.CreateWeeklyQuests();

            foreach (var mission in evt.Missions)
            {
                Assert.Greater(mission.Target, 0);
                Assert.Greater(mission.Reward.Resources.Count, 0);
                Assert.AreEqual(0, mission.Current);
                Assert.IsFalse(mission.Claimed);
            }
        }
    }

    [TestFixture]
    public class QuestProgressIntegrationTests
    {
        [Test]
        public void DailyChapterQuestCompleteAfter3Clears()
        {
            var manager = new EventManager();
            var evt = manager.CreateDailyQuests();

            foreach (var active in manager.GetActiveEvents())
            {
                active.UpdateMissionProgress("daily_chapter", 1);
            }
            Assert.IsFalse(evt.IsMissionCompleted("daily_chapter"));

            foreach (var active in manager.GetActiveEvents())
            {
                active.UpdateMissionProgress("daily_chapter", 1);
            }
            foreach (var active in manager.GetActiveEvents())
            {
                active.UpdateMissionProgress("daily_chapter", 1);
            }

            Assert.IsTrue(evt.IsMissionCompleted("daily_chapter"));
        }

        [Test]
        public void WeeklyChapterQuestCompleteAfter15Clears()
        {
            var manager = new EventManager();
            var evt = manager.CreateWeeklyQuests();

            for (int i = 0; i < 15; i++)
            {
                foreach (var active in manager.GetActiveEvents())
                {
                    active.UpdateMissionProgress("weekly_chapter", 1);
                }
            }

            Assert.IsTrue(evt.IsMissionCompleted("weekly_chapter"));
        }

        [Test]
        public void DailyAndWeeklyQuestsProgressSimultaneously()
        {
            var manager = new EventManager();
            var daily = manager.CreateDailyQuests();
            var weekly = manager.CreateWeeklyQuests();

            foreach (var active in manager.GetActiveEvents())
            {
                active.UpdateMissionProgress("daily_chapter", 1);
                active.UpdateMissionProgress("weekly_chapter", 1);
            }

            Assert.AreEqual(1, daily.Missions.First(m => m.Id == "daily_chapter").Current);
            Assert.AreEqual(1, weekly.Missions.First(m => m.Id == "weekly_chapter").Current);
        }

        [Test]
        public void ChapterClearProgressesBothDailyAndWeekly()
        {
            var manager = new EventManager();
            var daily = manager.CreateDailyQuests();
            var weekly = manager.CreateWeeklyQuests();

            foreach (var active in manager.GetActiveEvents())
            {
                active.UpdateMissionProgress("daily_chapter", 1);
                active.UpdateMissionProgress("weekly_chapter", 1);
            }

            var dailyMission = daily.Missions.First(m => m.Id == "daily_chapter");
            var weeklyMission = weekly.Missions.First(m => m.Id == "weekly_chapter");

            Assert.AreEqual(1, dailyMission.Current);
            Assert.AreEqual(1, weeklyMission.Current);
        }

        [Test]
        public void AllMissionsCompleteAndAllRewardsClaimable()
        {
            var manager = new EventManager();
            var evt = manager.CreateDailyQuests();

            foreach (var mission in evt.Missions)
            {
                evt.UpdateMissionProgress(mission.Id, mission.Target);
            }

            foreach (var mission in evt.Missions)
            {
                Assert.IsTrue(evt.IsMissionCompleted(mission.Id));
                var reward = evt.ClaimMissionReward(mission.Id);
                Assert.IsNotNull(reward);
            }

            Assert.AreEqual(1f, evt.GetProgress());
            Assert.AreEqual(evt.Missions.Count, evt.GetCompletedMissionCount());
        }
    }
}
