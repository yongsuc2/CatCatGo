using System.Linq;
using NUnit.Framework;
using CatCatGo.Infrastructure;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.Data;

namespace CatCatGo.Tests
{
    [TestFixture]
    public class CrossValidationTests
    {
        [Test]
        public void SeededRandom_Seed42_ProducesSameSequenceAsTypeScript()
        {
            var rng = new SeededRandom(42);
            var expected = new[] { 60, 45, 86, 67, 17, 53, 27, 63, 87, 47 };
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], rng.NextInt(0, 100), $"Mismatch at index {i}");
            }
        }

        [Test]
        public void Stats_Add_MatchesTypeScriptResult()
        {
            var a = Stats.Create(atk: 100, def: 50, maxHp: 1000, crit: 0.1f);
            var b = Stats.Create(atk: 50, def: 25, maxHp: 500, crit: 0.05f);
            var result = a.Add(b);

            Assert.AreEqual(150, result.Atk);
            Assert.AreEqual(75, result.Def);
            Assert.AreEqual(1500, result.MaxHp);
            Assert.That(result.Crit, Is.EqualTo(0.15f).Within(1e-6f));
            Assert.AreEqual(0, result.Hp);
        }

        [Test]
        public void Talent_UpgradeATK5Times_MatchesTypeScriptResult()
        {
            var talent = new Talent();
            var expectedTotals = new[] { 1, 2, 3, 4, 5 };

            for (int i = 0; i < 5; i++)
            {
                talent.Upgrade(StatType.ATK, 999999);
                Assert.AreEqual(expectedTotals[i], talent.GetTotalLevel(), $"TotalLevel mismatch after upgrade {i + 1}");
            }

            Assert.AreEqual(5, talent.AtkLevel);
            Assert.AreEqual(0, talent.HpLevel);
            Assert.AreEqual(0, talent.DefLevel);
            Assert.AreEqual(TalentGrade.DISCIPLE, talent.Grade);

            var stats = talent.GetStats();
            Assert.AreEqual(15, stats.Atk);
            Assert.AreEqual(0, stats.Def);
            Assert.AreEqual(0, stats.MaxHp);
            Assert.AreEqual(0f, stats.Crit);
        }

        [Test]
        public void Battle_Seed42_BasicUnits_StrongPlayerWins()
        {
            var builtins = ActiveSkillRegistry.GetBuiltinSkills().ToArray();
            var playerStats = Stats.Create(atk: 100, def: 50, maxHp: 1000, crit: 0.1f).WithHp(1000);
            var enemyStats = Stats.Create(atk: 80, def: 30, maxHp: 500, crit: 0.05f).WithHp(500);

            var player = new BattleUnit("Player", playerStats, builtins, null, true);
            var enemy = new BattleUnit("Enemy", enemyStats, builtins, null, false);

            var battle = new Battle(player, enemy, 42);
            battle.RunToCompletion();

            Assert.AreEqual(BattleState.VICTORY, battle.State);
            Assert.IsTrue(player.IsAlive());
            Assert.IsFalse(enemy.IsAlive());
            Assert.Greater(player.CurrentHp, 0);
            Assert.Less(battle.TurnCount, 20);
        }
    }
}
