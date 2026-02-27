using NUnit.Framework;
using CatCatGo.Domain.ValueObjects;

namespace CatCatGo.Tests.Domain
{
    [TestFixture]
    public class StatsTests
    {
        [Test]
        public void CreatesWithDefaultZeroValues()
        {
            var stats = Stats.Zero;
            Assert.AreEqual(0, stats.Hp);
            Assert.AreEqual(0, stats.MaxHp);
            Assert.AreEqual(0, stats.Atk);
            Assert.AreEqual(0, stats.Def);
            Assert.AreEqual(0f, stats.Crit);
        }

        [Test]
        public void CreatesWithPartialValues()
        {
            var stats = Stats.Create(atk: 50, maxHp: 200);
            Assert.AreEqual(50, stats.Atk);
            Assert.AreEqual(200, stats.MaxHp);
            Assert.AreEqual(0, stats.Hp);
            Assert.AreEqual(0, stats.Def);
        }

        [Test]
        public void AddsTwoStatsCorrectly()
        {
            var a = Stats.Create(hp: 10, maxHp: 100, atk: 20, def: 5, crit: 0.1f);
            var b = Stats.Create(hp: 5, maxHp: 50, atk: 10, def: 3, crit: 0.05f);
            var result = a.Add(b);

            Assert.AreEqual(15, result.Hp);
            Assert.AreEqual(150, result.MaxHp);
            Assert.AreEqual(30, result.Atk);
            Assert.AreEqual(8, result.Def);
            Assert.AreEqual(0.15f, result.Crit, 0.001f);
        }

        [Test]
        public void MultipliesStatsByFactor()
        {
            var stats = Stats.Create(hp: 100, maxHp: 100, atk: 20, def: 10, crit: 0.1f);
            var result = stats.Multiply(2f);

            Assert.AreEqual(200, result.Hp);
            Assert.AreEqual(200, result.MaxHp);
            Assert.AreEqual(40, result.Atk);
            Assert.AreEqual(20, result.Def);
            Assert.AreEqual(0.2f, result.Crit, 0.001f);
        }

        [Test]
        public void FloorsValuesWhenMultiplying()
        {
            var stats = Stats.Create(hp: 10, maxHp: 10, atk: 7);
            var result = stats.Multiply(1.5f);

            Assert.AreEqual(15, result.Hp);
            Assert.AreEqual(10, result.Atk);
        }

        [Test]
        public void ClonesWithoutSharingReference()
        {
            var original = Stats.Create(atk: 50);
            var cloned = new Stats(original.Hp, original.MaxHp, original.Atk, original.Def, original.Crit);
            Assert.AreEqual(50, cloned.Atk);
        }

        [Test]
        public void CreatesStatsWithModifiedHpViaWithHp()
        {
            var stats = Stats.Create(hp: 50, maxHp: 100, atk: 20);
            var result = stats.WithHp(80);
            Assert.AreEqual(80, result.Hp);
            Assert.AreEqual(100, result.MaxHp);
            Assert.AreEqual(20, result.Atk);
        }
    }
}
