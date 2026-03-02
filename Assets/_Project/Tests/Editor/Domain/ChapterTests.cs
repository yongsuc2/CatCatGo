using System.Linq;
using NUnit.Framework;
using CatCatGo.Domain.Chapter;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.ValueObjects;

namespace CatCatGo.Tests.Domain
{
    [TestFixture]
    public class ChapterTests
    {
        [Test]
        public void CreatesWithCorrectTotalDays()
        {
            var ch60 = new Chapter(1, ChapterType.SIXTY_DAY, 42);
            Assert.AreEqual(60, ch60.TotalDays);
        }

        [Test]
        public void AdvancesDayAndGeneratesEncounter()
        {
            var chapter = new Chapter(1, ChapterType.SIXTY_DAY, 42);
            var encounter = chapter.AdvanceDay();

            Assert.AreEqual(1, chapter.CurrentDay);
            Assert.AreEqual(ChapterState.IN_PROGRESS, chapter.State);
        }

        [Test]
        public void ReachesBossDayAfterAllDays()
        {
            var chapter = new Chapter(1, ChapterType.SIXTY_DAY, 42);

            for (int i = 0; i < 60; i++)
            {
                chapter.AdvanceDay();
                if (chapter.CurrentEncounter != null && chapter.CurrentEncounter.Type != EncounterType.COMBAT)
                {
                    chapter.ResolveEncounter(0, 100, 100);
                }
            }

            Assert.IsTrue(chapter.IsBossDay());
        }

        [Test]
        public void ResolvesNonCombatEncounterAndGainsSkills()
        {
            bool found = false;

            for (int attempt = 0; attempt < 50; attempt++)
            {
                var testChapter = new Chapter(1, ChapterType.SIXTY_DAY, attempt);
                var encounter = testChapter.AdvanceDay();
                if (encounter != null && encounter.Type == EncounterType.CHANCE)
                {
                    var result = testChapter.ResolveEncounter(0, 100, 100);
                    if (result != null && result.SkillsGained.Count > 0)
                    {
                        Assert.Greater(testChapter.SessionSkills.Count, 0);
                        found = true;
                        break;
                    }
                }
            }

            Assert.IsTrue(found);
        }

        [Test]
        public void TracksProgressCorrectly()
        {
            var chapter = new Chapter(1, ChapterType.SIXTY_DAY, 42);
            Assert.AreEqual(0f, chapter.GetProgress());

            chapter.AdvanceDay();
            Assert.AreEqual(1f / 60f, chapter.GetProgress(), 0.001f);
        }

        [Test]
        public void SetsFailedStateOnBattleDefeat()
        {
            var chapter = new Chapter(1, ChapterType.SIXTY_DAY, 42);
            chapter.AdvanceDay();
            chapter.OnBattleEnd(BattleState.DEFEAT);
            Assert.AreEqual(ChapterState.FAILED, chapter.State);
        }

        [Test]
        public void SetsClearedStateOnBossDefeated()
        {
            var chapter = new Chapter(1, ChapterType.SIXTY_DAY, 42);
            chapter.OnBossDefeated();
            Assert.AreEqual(ChapterState.CLEARED, chapter.State);
        }

        [Test]
        public void CreatesCombatBattleWhenEncounterIsCombat()
        {
            bool battleCreated = false;

            for (int seed = 0; seed < 100; seed++)
            {
                var chapter = new Chapter(1, ChapterType.SIXTY_DAY, seed);
                var encounter = chapter.AdvanceDay();

                if (encounter != null && encounter.Type == EncounterType.COMBAT)
                {
                    var player = new BattleUnit(
                        "Player",
                        Stats.Create(hp: 200, maxHp: 200, atk: 30, def: 10),
                        null, null, true);
                    var battle = chapter.CreateCombatBattle(player);
                    if (battle != null)
                    {
                        Assert.AreSame(player, battle.Player);
                        Assert.IsNotNull(battle.Enemy);
                        battleCreated = true;
                        break;
                    }
                }
            }

            Assert.IsTrue(battleCreated);
        }
    }

    [TestFixture]
    public class ChapterSessionHpTests
    {
        private static PassiveSkill MakeHpPassive(float value)
        {
            return new PassiveSkill("hp_fortify", "\uccb4\ub825 \uac15\ud654", "X", 1, new SkillTag[0], new HeritageRoute[0],
                new PassiveEffect
                {
                    Type = PassiveType.STAT_MODIFIER,
                    Stat = StatType.HP,
                    Value = value,
                    IsPercentage = true,
                });
        }

        [Test]
        public void RecalcSessionMaxHpAppliesHpPassivesFromBase()
        {
            var chapter = new Chapter(1, ChapterType.SIXTY_DAY, 42);
            chapter.InitSessionHp(100);
            chapter.SessionSkills.Add(new SessionSkillWrapper(MakeHpPassive(0.1f)));
            chapter.RecalcSessionMaxHp();

            Assert.AreEqual(110, chapter.SessionMaxHp);
        }

        [Test]
        public void RecalcSessionMaxHpPreservesHpRatio()
        {
            var chapter = new Chapter(1, ChapterType.SIXTY_DAY, 42);
            chapter.InitSessionHp(100);
            chapter.SessionCurrentHp = 50;
            chapter.SessionSkills.Add(new SessionSkillWrapper(MakeHpPassive(0.1f)));
            chapter.RecalcSessionMaxHp();

            Assert.AreEqual(110, chapter.SessionMaxHp);
            Assert.AreEqual(55, chapter.SessionCurrentHp);
        }

        [Test]
        public void RecalcSessionMaxHpRecalculatesFromBaseOnTierUpgrade()
        {
            var chapter = new Chapter(1, ChapterType.SIXTY_DAY, 42);
            chapter.InitSessionHp(100);

            chapter.SessionSkills.Add(new SessionSkillWrapper(MakeHpPassive(0.05f)));
            chapter.RecalcSessionMaxHp();
            Assert.AreEqual(105, chapter.SessionMaxHp);

            chapter.SessionSkills[0] = new SessionSkillWrapper(MakeHpPassive(0.1f));
            chapter.RecalcSessionMaxHp();
            Assert.AreEqual(110, chapter.SessionMaxHp);
        }

        [Test]
        public void GetBattlePassiveSkillsExcludesHpStatModifiers()
        {
            var chapter = new Chapter(1, ChapterType.SIXTY_DAY, 42);
            var hpPassive = MakeHpPassive(0.1f);
            var otherPassive = new PassiveSkill("lifesteal", "\ud761\ud608", "X", 1, new SkillTag[0], new HeritageRoute[0],
                new PassiveEffect { Type = PassiveType.LIFESTEAL, Rate = 0.1f });
            chapter.SessionSkills.Add(new SessionSkillWrapper(hpPassive));
            chapter.SessionSkills.Add(new SessionSkillWrapper(otherPassive));

            var battlePassives = chapter.GetBattlePassiveSkills();
            Assert.AreEqual(1, battlePassives.Count);
            Assert.AreEqual("lifesteal", battlePassives[0].Id);
        }
    }
}
