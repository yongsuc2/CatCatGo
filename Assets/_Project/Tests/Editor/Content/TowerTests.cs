using System.Linq;
using NUnit.Framework;
using CatCatGo.Domain.Content;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.ValueObjects;

namespace CatCatGo.Tests.Content
{
    [TestFixture]
    public class TowerTests
    {
        [Test]
        public void StartsAtFloor1Stage1()
        {
            var tower = new Tower();
            Assert.AreEqual(1, tower.CurrentFloor);
            Assert.AreEqual(1, tower.CurrentStage);
        }

        [Test]
        public void CreatesAChallengeBattle()
        {
            var tower = new Tower();
            var player = new BattleUnit("Player", Stats.Create(hp: 500, maxHp: 500, atk: 50, def: 10), null, null, true);
            var result = tower.Challenge(player, 1);

            Assert.IsTrue(result.IsOk());
            Assert.IsNotNull(result.Data.Battle);
        }

        [Test]
        public void FailsChallengeWithoutTokens()
        {
            var tower = new Tower();
            var player = new BattleUnit("Player", Stats.Create(hp: 500, maxHp: 500, atk: 50, def: 10), null, null, true);
            var result = tower.Challenge(player, 0);

            Assert.IsTrue(result.IsFail());
        }

        [Test]
        public void AdvancesStageOnVictory()
        {
            var tower = new Tower();
            var result = tower.OnBattleResult(BattleState.VICTORY);

            Assert.IsTrue(result.Advanced);
            Assert.AreEqual(2, tower.CurrentStage);
        }

        [Test]
        public void DoesNotAdvanceOnDefeatAndDoesNotConsumeToken()
        {
            var tower = new Tower();
            var result = tower.OnBattleResult(BattleState.DEFEAT);

            Assert.IsFalse(result.Advanced);
            Assert.IsFalse(result.TokenConsumed);
            Assert.AreEqual(1, tower.CurrentStage);
        }

        [Test]
        public void AdvancesFloorAfterClearingAllStages()
        {
            var tower = new Tower(1, 10);
            tower.OnBattleResult(BattleState.VICTORY);

            Assert.AreEqual(2, tower.CurrentFloor);
            Assert.AreEqual(1, tower.CurrentStage);
        }

        [Test]
        public void GivesPowerStoneRewardAtStage5And10()
        {
            var tower = new Tower();
            var r5 = tower.GetReward(1, 5);
            var r10 = tower.GetReward(1, 10);
            var r3 = tower.GetReward(1, 3);

            Assert.IsTrue(r5.Resources.Any(r => r.Type == ResourceType.POWER_STONE));
            Assert.IsTrue(r10.Resources.Any(r => r.Type == ResourceType.POWER_STONE));
            Assert.IsFalse(r3.Resources.Any(r => r.Type == ResourceType.POWER_STONE));
        }
    }
}
