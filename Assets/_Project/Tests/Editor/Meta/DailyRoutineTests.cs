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
            Assert.AreEqual(9, routine.Count);
        }

        [Test]
        public void FirstActionIsDragonNestDungeon()
        {
            var routine = _scheduler.GetFullRoutine();
            Assert.AreEqual(RoutineAction.DAILY_DUNGEON_DRAGON, routine[0].Action);
        }

        [Test]
        public void ShowsAvailableActionsBasedOnContext()
        {
            var statuses = _scheduler.GetAvailableActions(new RoutineContext
            {
                DungeonDragonRemaining = 3,
                DungeonCelestialRemaining = 0,
                DungeonSkyRemaining = 3,
                ChallengeTokens = 5,
                ArenaTickets = 3,
                Stamina = 50,
                Pickaxes = 10,
            });

            var dragon = statuses.FirstOrDefault(s => s.Action == RoutineAction.DAILY_DUNGEON_DRAGON);
            Assert.IsTrue(dragon.Available);

            var celestial = statuses.FirstOrDefault(s => s.Action == RoutineAction.DAILY_DUNGEON_CELESTIAL);
            Assert.IsFalse(celestial.Available);
        }

        [Test]
        public void ReturnsNextAvailableAction()
        {
            var next = _scheduler.GetNextAction(new RoutineContext
            {
                DungeonDragonRemaining = 0,
                DungeonCelestialRemaining = 0,
                DungeonSkyRemaining = 0,
                ChallengeTokens = 5,
                ArenaTickets = 0,
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
                DungeonDragonRemaining = 0,
                DungeonCelestialRemaining = 0,
                DungeonSkyRemaining = 0,
                ChallengeTokens = 0,
                ArenaTickets = 0,
                Stamina = 0,
                Pickaxes = 0,
            });

            Assert.AreEqual(RoutineAction.CATACOMB_RUN, next);
        }
    }
}
