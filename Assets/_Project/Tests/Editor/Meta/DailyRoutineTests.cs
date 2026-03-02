using System.Linq;
using NUnit.Framework;
using CatCatGo.Domain.Meta;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Tests.Meta
{
    [TestFixture]
    public class DailyRoutineTests
    {
        private DailyRoutineScheduler _scheduler;

        [SetUp]
        public void SetUp()
        {
            _scheduler = new DailyRoutineScheduler();
        }

        [Test]
        public void ReturnsFullRoutineOf9Steps()
        {
            var routine = _scheduler.GetFullRoutine();
            Assert.AreEqual(7, routine.Count);
        }

        [Test]
        public void FirstActionIsBeehiveDungeon()
        {
            var routine = _scheduler.GetFullRoutine();
            Assert.AreEqual(RoutineAction.DAILY_DUNGEON_BEEHIVE, routine[0].Action);
        }

        [Test]
        public void ShowsAvailableActionsBasedOnContext()
        {
            var statuses = _scheduler.GetAvailableActions(new RoutineContext
            {
                DungeonBeehiveRemaining = 3,
                DungeonAncientTreeRemaining = 0,
                DungeonTigerCliffRemaining = 3,
                ChallengeTokens = 5,

                Stamina = 50,
                Pickaxes = 10,
            });

            var beehive = statuses.FirstOrDefault(s => s.Action == RoutineAction.DAILY_DUNGEON_BEEHIVE);
            Assert.IsTrue(beehive.Available);

            var ancientTree = statuses.FirstOrDefault(s => s.Action == RoutineAction.DAILY_DUNGEON_ANCIENT);
            Assert.IsFalse(ancientTree.Available);
        }

        [Test]
        public void ReturnsNextAvailableAction()
        {
            var next = _scheduler.GetNextAction(new RoutineContext
            {
                DungeonBeehiveRemaining = 0,
                DungeonAncientTreeRemaining = 0,
                DungeonTigerCliffRemaining = 0,
                ChallengeTokens = 5,

                Stamina = 50,
                Pickaxes = 10,
            });

            Assert.AreEqual(RoutineAction.TOWER_CHALLENGE, next);
        }

        [Test]
        public void ReturnsCatacombRunWhenNothingElseAvailable()
        {
            var next = _scheduler.GetNextAction(new RoutineContext
            {
                DungeonBeehiveRemaining = 0,
                DungeonAncientTreeRemaining = 0,
                DungeonTigerCliffRemaining = 0,
                ChallengeTokens = 0,

                Stamina = 0,
                Pickaxes = 0,
            });

            Assert.AreEqual(RoutineAction.CATACOMB_RUN, next);
        }
    }
}
