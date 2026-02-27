using System;
using NUnit.Framework;
using CatCatGo.Infrastructure;

namespace CatCatGo.Tests.Infrastructure
{
    public struct BattleStartEvent
    {
        public string Player;
    }

    public struct BattleEndEvent
    {
        public string Result;
    }

    public struct LevelUpEvent
    {
        public int Level;
    }

    [TestFixture]
    public class EventBusTests
    {
        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
        }

        [Test]
        public void DeliversEventsToSubscribers()
        {
            int callCount = 0;
            string receivedPlayer = null;

            Action<BattleStartEvent> handler = e =>
            {
                callCount++;
                receivedPlayer = e.Player;
            };

            EventBus.Subscribe(handler);
            EventBus.Publish(new BattleStartEvent { Player = "p1" });

            Assert.AreEqual(1, callCount);
            Assert.AreEqual("p1", receivedPlayer);
        }

        [Test]
        public void SupportsMultipleSubscribers()
        {
            int h1Count = 0;
            int h2Count = 0;

            Action<BattleEndEvent> h1 = _ => h1Count++;
            Action<BattleEndEvent> h2 = _ => h2Count++;

            EventBus.Subscribe(h1);
            EventBus.Subscribe(h2);
            EventBus.Publish(new BattleEndEvent { Result = "win" });

            Assert.AreEqual(1, h1Count);
            Assert.AreEqual(1, h2Count);
        }

        [Test]
        public void UnsubscribesCorrectly()
        {
            int callCount = 0;
            Action<LevelUpEvent> handler = _ => callCount++;

            EventBus.Subscribe(handler);
            EventBus.Unsubscribe(handler);
            EventBus.Publish(new LevelUpEvent { Level = 5 });

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void DoesNotCallHandlersForDifferentEventTypes()
        {
            int callCount = 0;
            Action<BattleStartEvent> handler = _ => callCount++;

            EventBus.Subscribe(handler);
            EventBus.Publish(new BattleEndEvent { Result = "lose" });

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void ClearsAllHandlers()
        {
            int callCount = 0;
            Action<BattleStartEvent> handler = _ => callCount++;

            EventBus.Subscribe(handler);
            EventBus.Clear();
            EventBus.Publish(new BattleStartEvent());

            Assert.AreEqual(0, callCount);
        }
    }
}
