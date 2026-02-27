using NUnit.Framework;
using CatCatGo.Domain.Content;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Infrastructure;

namespace CatCatGo.Tests.Content
{
    [TestFixture]
    public class ArenaTests
    {
        [Test]
        public void StartsAtBronzeTier()
        {
            var arena = new Arena();
            Assert.AreEqual(ArenaTier.BRONZE, arena.Tier);
        }

        [Test]
        public void Matches4Opponents()
        {
            var arena = new Arena();
            var rng = new SeededRandom(42);
            var opponents = arena.MatchOpponents(rng);

            Assert.AreEqual(4, opponents.Length);
            foreach (var o in opponents)
            {
                Assert.Greater(o.MaxHp, 0);
                Assert.Greater(o.BaseAtk, 0);
            }
        }

        [Test]
        public void ConsumesEntryOnFight()
        {
            var arena = new Arena();
            var player = new BattleUnit("Player", Stats.Create(hp: 500, maxHp: 500, atk: 100, def: 20), null, null, true);
            var rng = new SeededRandom(42);

            arena.Fight(player, 1, rng);
            Assert.AreEqual(1, arena.TodayEntries);
        }

        [Test]
        public void RespectsDailyEntryLimit()
        {
            var arena = new Arena();
            var player = new BattleUnit("Player", Stats.Create(hp: 500, maxHp: 500, atk: 100, def: 20), null, null, true);
            var rng = new SeededRandom(42);

            for (int i = 0; i < 5; i++)
            {
                arena.Fight(player, 1, rng);
            }

            Assert.IsFalse(arena.IsAvailable());
            var result = arena.Fight(player, 1, rng);
            Assert.IsTrue(result.IsFail());
        }

        [Test]
        public void ResetsEntriesOnDailyReset()
        {
            var arena = new Arena();
            var player = new BattleUnit("Player", Stats.Create(hp: 500, maxHp: 500, atk: 100, def: 20), null, null, true);
            var rng = new SeededRandom(42);

            arena.Fight(player, 1, rng);
            arena.DailyReset();
            Assert.AreEqual(5, arena.GetRemainingEntries());
        }

        [Test]
        public void ReturnsBattleResults()
        {
            var arena = new Arena();
            var player = new BattleUnit("Player", Stats.Create(hp: 500, maxHp: 500, atk: 100, def: 20), null, null, true);
            var rng = new SeededRandom(42);

            var result = arena.Fight(player, 1, rng);
            Assert.IsTrue(result.IsOk());
            Assert.AreEqual(4, result.Data.Results.Count);
        }
    }
}
