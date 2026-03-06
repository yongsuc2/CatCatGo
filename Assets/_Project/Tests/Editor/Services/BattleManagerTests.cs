using NUnit.Framework;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Services;

namespace CatCatGo.Tests.Services
{
    [TestFixture]
    public class BattleManagerTests
    {
        private BattleManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new BattleManager();
        }

        [Test]
        public void GetPetAbilitySkillReturnsNullWithoutActivePet()
        {
            var player = new Player();
            var result = _manager.GetPetAbilitySkill(player);
            Assert.IsNull(result);
        }

        [Test]
        public void CreatePlayerUnitNameIsCapybara()
        {
            var player = new Player();
            var unit = _manager.CreatePlayerUnit(player, new ActiveSkill[0], new PassiveSkill[0]);

            Assert.AreEqual("Capybara", unit.Name);
            Assert.IsTrue(unit.IsPlayer);
        }

        [Test]
        public void CreatePlayerUnitUsesPlayerComputedStats()
        {
            var player = new Player();
            var expectedStats = player.ComputeStats();
            var unit = _manager.CreatePlayerUnit(player, new ActiveSkill[0], new PassiveSkill[0]);

            Assert.AreEqual(expectedStats.MaxHp, unit.MaxHp);
        }

        [Test]
        public void CreateBattleReturnsNonNull()
        {
            var player = new Player();
            var playerUnit = _manager.CreatePlayerUnit(player, new ActiveSkill[0], new PassiveSkill[0]);
            var enemyStats = Stats.Create(maxHp: 100, atk: 10, def: 5);
            var enemyUnit = new BattleUnit("Goblin", enemyStats, null, null, false);

            var battle = _manager.CreateBattle(playerUnit, enemyUnit);
            Assert.IsNotNull(battle);
        }

        [Test]
        public void CreateBattleAcceptsCustomSeed()
        {
            var player = new Player();
            var playerUnit = _manager.CreatePlayerUnit(player, new ActiveSkill[0], new PassiveSkill[0]);
            var enemyStats = Stats.Create(maxHp: 100, atk: 10, def: 5);
            var enemyUnit = new BattleUnit("Goblin", enemyStats, null, null, false);

            var battle = _manager.CreateBattle(playerUnit, enemyUnit, 12345);
            Assert.IsNotNull(battle);
        }

        [Test]
        public void CreatePlayerUnitWithPassiveSkillsPreservesThem()
        {
            var player = new Player();
            var effect = new PassiveEffect { Type = PassiveType.LIFESTEAL, Value = 0.1f };
            var passive = new PassiveSkill("test_passive", "Test", "", 1,
                new SkillTag[0], new HeritageRoute[0], effect);

            var unit = _manager.CreatePlayerUnit(player, new ActiveSkill[0], new[] { passive });
            Assert.IsTrue(unit.PassiveSkills.Length >= 1);
        }
    }
}
