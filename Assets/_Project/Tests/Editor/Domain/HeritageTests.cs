using NUnit.Framework;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Tests.Domain
{
    [TestFixture]
    public class HeritageTests
    {
        [Test]
        public void StartsAtLevel0()
        {
            var heritage = new Heritage();
            Assert.AreEqual(0, heritage.Level);
            Assert.AreEqual(HeritageRoute.SKULL, heritage.Route);
        }

        [Test]
        public void IsUnlockedOnlyForHeroGrade()
        {
            Assert.IsFalse(Heritage.IsUnlocked(TalentGrade.DISCIPLE));
            Assert.IsFalse(Heritage.IsUnlocked(TalentGrade.ADVENTURER));
            Assert.IsFalse(Heritage.IsUnlocked(TalentGrade.ELITE));
            Assert.IsFalse(Heritage.IsUnlocked(TalentGrade.MASTER));
            Assert.IsFalse(Heritage.IsUnlocked(TalentGrade.WARRIOR));
            Assert.IsTrue(Heritage.IsUnlocked(TalentGrade.HERO));
        }

        [Test]
        public void UpgradeIncreasesLevel()
        {
            var heritage = new Heritage();
            int cost = heritage.GetUpgradeCost();
            var result = heritage.Upgrade(cost);

            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(1, heritage.Level);
            Assert.AreEqual(1, result.Data.NewLevel);
        }

        [Test]
        public void UpgradeFailsWithInsufficientBooks()
        {
            var heritage = new Heritage();
            var result = heritage.Upgrade(0);

            Assert.IsTrue(result.IsFail());
            Assert.AreEqual(0, heritage.Level);
        }

        [Test]
        public void ChangeRouteResetsLevel()
        {
            var heritage = new Heritage(HeritageRoute.SKULL, 5);
            Assert.AreEqual(5, heritage.Level);

            heritage.ChangeRoute(HeritageRoute.KNIGHT);
            Assert.AreEqual(HeritageRoute.KNIGHT, heritage.Route);
            Assert.AreEqual(0, heritage.Level);
        }

        [Test]
        public void GetPassiveBonusIsZeroAtLevel0()
        {
            var heritage = new Heritage(HeritageRoute.SKULL, 0);
            var bonus = heritage.GetPassiveBonus();

            Assert.AreEqual(0, bonus.Atk);
            Assert.AreEqual(0, bonus.MaxHp);
            Assert.AreEqual(0, bonus.Def);
        }

        [Test]
        public void GetPassiveBonusScalesWithLevel()
        {
            var heritage1 = new Heritage(HeritageRoute.SKULL, 1);
            var heritage5 = new Heritage(HeritageRoute.SKULL, 5);

            var bonus1 = heritage1.GetPassiveBonus();
            var bonus5 = heritage5.GetPassiveBonus();

            Assert.Greater(bonus5.Atk, bonus1.Atk);
        }

        [Test]
        public void GetRequiredBookTypeMatchesRoute()
        {
            var skull = new Heritage(HeritageRoute.SKULL);
            var knight = new Heritage(HeritageRoute.KNIGHT);

            Assert.AreEqual(ResourceType.SKULL_BOOK, skull.GetRequiredBookType());
            Assert.AreEqual(ResourceType.KNIGHT_BOOK, knight.GetRequiredBookType());
        }
    }
}
