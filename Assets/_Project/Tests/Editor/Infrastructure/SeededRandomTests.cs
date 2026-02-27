using System.Collections.Generic;
using NUnit.Framework;
using CatCatGo.Infrastructure;

namespace CatCatGo.Tests.Infrastructure
{
    [TestFixture]
    public class SeededRandomTests
    {
        [Test]
        public void ProducesDeterministicResultsWithSameSeed()
        {
            var a = new SeededRandom(42);
            var b = new SeededRandom(42);

            var resultsA = new List<float>();
            var resultsB = new List<float>();
            for (int i = 0; i < 10; i++)
            {
                resultsA.Add(a.Next());
                resultsB.Add(b.Next());
            }

            Assert.AreEqual(resultsA, resultsB);
        }

        [Test]
        public void ProducesDifferentResultsWithDifferentSeeds()
        {
            var a = new SeededRandom(42);
            var b = new SeededRandom(99);

            Assert.AreNotEqual(a.Next(), b.Next());
        }

        [Test]
        public void GeneratesIntegersWithinRange()
        {
            var rng = new SeededRandom(12345);
            for (int i = 0; i < 100; i++)
            {
                int val = rng.NextInt(1, 10);
                Assert.GreaterOrEqual(val, 1);
                Assert.LessOrEqual(val, 10);
            }
        }

        [Test]
        public void GeneratesFloatsWithinRange()
        {
            var rng = new SeededRandom(12345);
            for (int i = 0; i < 100; i++)
            {
                float val = rng.NextFloat(0.5f, 1.5f);
                Assert.GreaterOrEqual(val, 0.5f);
                Assert.Less(val, 1.5f);
            }
        }

        [Test]
        public void PicksFromArray()
        {
            var rng = new SeededRandom(42);
            var items = new List<string> { "a", "b", "c" };
            var picked = rng.Pick(items);
            Assert.IsTrue(items.Contains(picked));
        }

        [Test]
        public void WeightedPickRespectsWeights()
        {
            var rng = new SeededRandom(42);
            var entries = new List<(string item, float weight)>
            {
                ("common", 90f),
                ("rare", 10f),
            };

            int commonCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                if (rng.WeightedPick(entries) == "common") commonCount++;
            }

            Assert.Greater(commonCount, 800);
            Assert.Less(commonCount, 950);
        }

        [Test]
        public void ChanceReturnsBooleanBasedOnProbability()
        {
            var rng = new SeededRandom(42);
            int trueCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                if (rng.Chance(0.5f)) trueCount++;
            }
            Assert.Greater(trueCount, 400);
            Assert.Less(trueCount, 600);
        }

        [Test]
        public void ShufflePreservesAllElements()
        {
            var rng = new SeededRandom(42);
            var original = new List<int> { 1, 2, 3, 4, 5 };
            var shuffled = rng.Shuffle(original);

            var sortedOriginal = new List<int>(original);
            sortedOriginal.Sort();
            var sortedShuffled = new List<int>(shuffled);
            sortedShuffled.Sort();

            Assert.AreEqual(sortedOriginal, sortedShuffled);
            Assert.AreNotSame(shuffled, original);
        }
    }
}
