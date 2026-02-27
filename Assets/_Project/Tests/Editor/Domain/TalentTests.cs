using NUnit.Framework;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Tests.Domain
{
    [TestFixture]
    public class TalentTests
    {
        [Test]
        public void StartsAtDiscipleGradeWithLevel0()
        {
            var talent = new Talent();
            Assert.AreEqual(0, talent.AtkLevel);
            Assert.AreEqual(0, talent.HpLevel);
            Assert.AreEqual(0, talent.DefLevel);
            Assert.AreEqual(TalentGrade.DISCIPLE, talent.Grade);
        }

        [Test]
        public void UpgradesAtkWithEnoughGold()
        {
            var talent = new Talent();
            int cost = talent.GetUpgradeCost(StatType.ATK);
            var result = talent.Upgrade(StatType.ATK, cost);

            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(1, talent.AtkLevel);
            Assert.AreEqual(1, result.Data.NewLevel);
        }

        [Test]
        public void FailsUpgradeWithInsufficientGold()
        {
            var talent = new Talent();
            var result = talent.Upgrade(StatType.ATK, 0);

            Assert.IsTrue(result.IsFail());
            Assert.AreEqual(0, talent.AtkLevel);
        }

        [Test]
        public void AdvancesGradeWhenTotalLevelReachesThreshold()
        {
            var talent = new Talent(20, 20, 20);
            Assert.AreEqual(TalentGrade.ADVENTURER, talent.Grade);
        }

        [Test]
        public void ComputesStatsFromLevels()
        {
            var talent = new Talent(5, 3, 2);
            var stats = talent.GetStats();

            Assert.AreEqual(15, stats.Atk);
            Assert.AreEqual(45, stats.MaxHp);
            Assert.AreEqual(4, stats.Def);
        }

        [Test]
        public void ReportsNextGradeThreshold()
        {
            var talent = new Talent();
            var threshold = talent.GetNextGradeThreshold();
            Assert.AreEqual(60, threshold);
        }

        [Test]
        public void DetectsGradeChangeOnUpgrade()
        {
            var talent = new Talent(20, 20, 19);
            Assert.AreEqual(TalentGrade.DISCIPLE, talent.Grade);

            int cost = talent.GetUpgradeCost(StatType.DEF);
            var result = talent.Upgrade(StatType.DEF, cost + 10000);
            Assert.IsTrue(result.Data.GradeChanged);
            Assert.AreEqual(TalentGrade.ADVENTURER, talent.Grade);
        }

        [Test]
        public void CapsStatAtLevelsPerStatWithinSubGrade()
        {
            var talent = new Talent();
            for (int i = 0; i < 10; i++)
            {
                int cost = talent.GetUpgradeCost(StatType.ATK);
                talent.Upgrade(StatType.ATK, cost + 999999);
            }
            Assert.IsFalse(talent.CanUpgradeStat(StatType.ATK));
            Assert.AreEqual(10, talent.GetStatLevelInTier(StatType.ATK));

            var result = talent.Upgrade(StatType.ATK, 999999);
            Assert.IsTrue(result.IsFail());
        }

        [Test]
        public void AdvancesSubGradeWhenAllStatsReachCap()
        {
            var talent = new Talent(10, 10, 9);
            Assert.AreEqual(0, talent.SubGradeIndex);

            int cost = talent.GetUpgradeCost(StatType.DEF);
            var result = talent.Upgrade(StatType.DEF, cost + 999999);
            Assert.IsTrue(result.IsOk());
            Assert.IsTrue(result.Data.SubGradeAdvanced);
            Assert.AreEqual(1, talent.SubGradeIndex);
            Assert.AreEqual(0, talent.GetStatLevelInTier(StatType.ATK));
            Assert.AreEqual(0, talent.GetStatLevelInTier(StatType.HP));
            Assert.AreEqual(0, talent.GetStatLevelInTier(StatType.DEF));
            Assert.AreEqual(30, talent.GetTotalLevel());
        }
    }
}
