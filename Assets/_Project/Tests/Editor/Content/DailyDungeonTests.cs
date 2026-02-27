using System.Linq;
using NUnit.Framework;
using CatCatGo.Domain.Content;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.ValueObjects;

namespace CatCatGo.Tests.Content
{
    [TestFixture]
    public class DailyDungeonTests
    {
        private BattleUnit MakePlayerUnit()
        {
            var stats = Stats.Create(hp: 0, maxHp: 500, atk: 50, def: 20, crit: 5);
            return new BattleUnit("Capybara", stats, null, null, true);
        }

        [Test]
        public void CreatesBattleWithStageScaling()
        {
            var dungeon = new DailyDungeon(DungeonType.DRAGON_NEST);
            var result = dungeon.CreateBattle(MakePlayerUnit());
            Assert.IsTrue(result.IsOk());
            Assert.IsNotNull(result.Data.Battle);
            Assert.IsNotNull(result.Data.Battle.Enemy);
        }

        [Test]
        public void IncrementsClearedStageOnVictory()
        {
            var dungeon = new DailyDungeon(DungeonType.DRAGON_NEST);
            Assert.AreEqual(0, dungeon.ClearedStage);

            var reward = dungeon.OnBattleVictory();
            Assert.AreEqual(1, dungeon.ClearedStage);
            Assert.Greater(reward.Resources.Count, 0);
        }

        [Test]
        public void ScalesRewardsPerStage()
        {
            var dungeon = new DailyDungeon(DungeonType.DRAGON_NEST);
            var stage1 = dungeon.GetRewardForStage(1);
            var stage5 = dungeon.GetRewardForStage(5);

            for (int i = 0; i < stage1.Count; i++)
            {
                Assert.Greater(stage5[i].Amount, stage1[i].Amount);
            }
        }

        [Test]
        public void ReturnsAccumulatedSweepReward()
        {
            var dungeon = new DailyDungeon(DungeonType.CELESTIAL_TREE);
            Assert.AreEqual(0, dungeon.GetSweepReward().Resources.Count);

            dungeon.OnBattleVictory();
            dungeon.OnBattleVictory();
            dungeon.OnBattleVictory();

            var sweep = dungeon.GetSweepReward();
            Assert.Greater(sweep.Resources.Count, 0);

            var stage1 = dungeon.GetRewardForStage(1);
            var stage2 = dungeon.GetRewardForStage(2);
            var stage3 = dungeon.GetRewardForStage(3);

            foreach (var r in sweep.Resources)
            {
                var s1 = stage1.FirstOrDefault(s => s.Type == r.Type);
                var s2 = stage2.FirstOrDefault(s => s.Type == r.Type);
                var s3 = stage3.FirstOrDefault(s => s.Type == r.Type);
                int expected = (s1.Type == r.Type ? s1.Amount : 0)
                             + (s2.Type == r.Type ? s2.Amount : 0)
                             + (s3.Type == r.Type ? s3.Amount : 0);
                Assert.AreEqual(expected, r.Amount);
            }
        }

        [Test]
        public void ReturnsRewardPreviewForNextStage()
        {
            var dungeon = new DailyDungeon(DungeonType.SKY_ISLAND);
            var preview = dungeon.GetRewardPreview();
            Assert.Greater(preview.Count, 0);

            var expected = dungeon.GetRewardForStage(1);
            Assert.AreEqual(expected.Count, preview.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i].Type, preview[i].Type);
                Assert.AreEqual(expected[i].Amount, preview[i].Amount);
            }
        }
    }

    [TestFixture]
    public class DailyDungeonManagerTests
    {
        [Test]
        public void ManagesAllThreeDungeons()
        {
            var manager = new DailyDungeonManager();
            Assert.AreEqual(3, manager.GetAvailableDungeons().Count);
        }

        [Test]
        public void UsesSharedDailyLimitAcrossAllDungeons()
        {
            var manager = new DailyDungeonManager();
            Assert.IsTrue(manager.IsAvailable());
            Assert.AreEqual(manager.DailyLimit, manager.GetRemainingCount());

            manager.ConsumeEntry();
            manager.ConsumeEntry();
            manager.ConsumeEntry();

            Assert.IsFalse(manager.IsAvailable());
            Assert.AreEqual(0, manager.GetRemainingCount());
        }

        [Test]
        public void ResetsTodayCountOnDailyReset()
        {
            var manager = new DailyDungeonManager();
            manager.ConsumeEntry();
            manager.ConsumeEntry();
            manager.ConsumeEntry();

            manager.DailyResetAll();
            Assert.IsTrue(manager.IsAvailable());
            Assert.AreEqual(manager.DailyLimit, manager.GetRemainingCount());
        }

        [Test]
        public void PreservesClearedStageAcrossDailyReset()
        {
            var manager = new DailyDungeonManager();
            var dungeon = manager.GetDungeon(DungeonType.DRAGON_NEST);
            dungeon.OnBattleVictory();
            dungeon.OnBattleVictory();

            manager.DailyResetAll();
            Assert.AreEqual(2, dungeon.ClearedStage);
        }

        [Test]
        public void ReportsTotalRemainingCount()
        {
            var manager = new DailyDungeonManager();
            Assert.AreEqual(manager.DailyLimit, manager.GetTotalRemainingCount());

            manager.ConsumeEntry();
            Assert.AreEqual(manager.DailyLimit - 1, manager.GetTotalRemainingCount());
        }
    }
}
