using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Battle;
using BattleInstance = CatCatGo.Domain.Battle.Battle;
using CatCatGo.Domain.Data;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Chapter
{
    public class ChapterSessionState
    {
        public int ChapterId;
        public ChapterType ChapterType;
        public int CurrentDay;
        public int TotalDays;
        public ChapterState State;
        public List<SessionSkillWrapper> SessionSkills;
        public Encounter CurrentEncounter;
        public BattleInstance CurrentBattle;
        public Reward TotalReward;
        public int PlayerCurrentHp;
        public int PlayerMaxHp;
    }

    public class Chapter
    {
        public int Id;
        public ChapterType Type;
        public int TotalDays;
        public int CurrentDay;
        public ChapterState State;
        public List<SessionSkillWrapper> SessionSkills;
        public Encounter CurrentEncounter;
        public BattleInstance CurrentBattle;
        public Reward TotalReward;
        public int SessionGold;
        public int SessionCurrentHp;
        public int SessionMaxHp;
        public int BaseSessionMaxHp;
        public int JungbakCount;
        public int DaebakCount;
        public bool OptionalEliteTriggered;
        public int SessionRerollsRemaining;

        private EncounterGenerator _encounterGenerator;
        private SeededRandom _rng;

        public Chapter(int id, ChapterType type, int seed = 0)
        {
            if (seed == 0) seed = Environment.TickCount;
            Id = id;
            Type = type;
            TotalDays = 60;
            CurrentDay = 0;
            State = ChapterState.IN_PROGRESS;
            SessionSkills = new List<SessionSkillWrapper>();
            CurrentEncounter = null;
            CurrentBattle = null;
            TotalReward = Reward.Empty();
            SessionGold = 0;
            SessionCurrentHp = 0;
            SessionMaxHp = 0;
            BaseSessionMaxHp = 0;
            JungbakCount = 0;
            DaebakCount = 0;
            OptionalEliteTriggered = false;
            SessionRerollsRemaining = EncounterDataTable.RerollsPerSession;
            _encounterGenerator = new EncounterGenerator(seed);
            _rng = new SeededRandom(seed + 1);
        }

        public void InitSessionHp(int maxHp)
        {
            BaseSessionMaxHp = maxHp;
            SessionCurrentHp = maxHp;
            SessionMaxHp = maxHp;
        }

        public Encounter AdvanceDay()
        {
            if (State != ChapterState.IN_PROGRESS) return null;

            OptionalEliteTriggered = false;
            CurrentDay += 1;

            if (CurrentDay >= TotalDays)
            {
                CurrentEncounter = null;
                return null;
            }

            if (IsEliteDay() || IsMidBossDay() || RollOptionalElite())
            {
                CurrentEncounter = null;
                return null;
            }

            var threshold = EncounterDataTable.CounterThreshold;

            if (DaebakCount >= threshold.Daebak)
            {
                DaebakCount = 0;
                CurrentEncounter = _encounterGenerator.GenerateDaebakRoulette(SessionSkills);
                return CurrentEncounter;
            }

            if (JungbakCount >= threshold.Jungbak)
            {
                JungbakCount = 0;
                CurrentEncounter = _encounterGenerator.GenerateJungbakRoulette(SessionSkills);
                return CurrentEncounter;
            }

            CurrentEncounter = _encounterGenerator.Generate(
                Type, CurrentDay, SessionSkills, Id);

            return CurrentEncounter;
        }

        public Encounter RerollEncounter()
        {
            if (CurrentEncounter == null || SessionRerollsRemaining <= 0) return null;
            SessionRerollsRemaining--;
            CurrentEncounter = _encounterGenerator.Regenerate(
                CurrentEncounter.Type, SessionSkills, Id);
            return CurrentEncounter;
        }

        public EncounterResult ResolveEncounter(int choiceIndex, int playerCurrentHp, int playerMaxHp)
        {
            if (CurrentEncounter == null) return null;

            float roll = _rng.Next();
            var result = CurrentEncounter.Resolve(
                choiceIndex, playerCurrentHp, playerMaxHp, SessionGold, roll);

            var idsToRemove = result.Chosen.Reward.SkillIdsToRemove;
            foreach (var id in idsToRemove)
            {
                int idx = SessionSkills.FindIndex(s => s.Id == id);
                if (idx >= 0)
                {
                    result.SkillsRemoved.Add(SessionSkills[idx]);
                    SessionSkills.RemoveAt(idx);
                }
            }

            foreach (var skill in result.SkillsGained)
            {
                int existingIdx = SessionSkills.FindIndex(s => s.Id == skill.Id);
                if (existingIdx >= 0)
                {
                    SessionSkills[existingIdx] = skill;
                }
                else
                {
                    SessionSkills.Add(skill);
                }
            }

            if (result.SkillsGained.Count > 0 || result.SkillsRemoved.Count > 0)
            {
                RecalcSessionMaxHp();
            }

            SessionGold += result.GoldChange;
            foreach (var r in result.Reward.Resources)
            {
                if (r.Type == ResourceType.GOLD) SessionGold += r.Amount;
            }
            TotalReward = TotalReward.Merge(result.Reward);

            CurrentEncounter = null;

            SessionCurrentHp = Math.Max(0, Math.Min(
                SessionCurrentHp + result.HpChange,
                SessionMaxHp));

            return result;
        }

        public void UpdateSessionHpAfterBattle(int remainingHp)
        {
            SessionCurrentHp = Math.Max(0, Math.Min(remainingHp, SessionMaxHp));
        }

        public List<ActiveSkill> GetSessionActiveSkills()
        {
            var builtins = ActiveSkillRegistry.GetBuiltinSkills();
            var lowerLowest = ActiveSkillRegistry.GetAll()
                .Where(s => (s.Hierarchy == SkillHierarchy.LOWER || s.Hierarchy == SkillHierarchy.LOWEST) && s.Tier == 1)
                .ToList();

            var userActives = SessionSkills
                .Where(s => s.IsActiveSkill)
                .Select(s => s.AsActiveSkill)
                .ToList();

            var result = new List<ActiveSkill>();
            result.AddRange(builtins);
            result.AddRange(userActives);
            result.AddRange(lowerLowest);
            return result;
        }

        public List<PassiveSkill> GetSessionPassiveSkills()
        {
            return SessionSkills
                .Where(s => !s.IsActiveSkill)
                .Select(s => s.AsPassiveSkill)
                .ToList();
        }

        public List<PassiveSkill> GetBattlePassiveSkills()
        {
            return GetSessionPassiveSkills()
                .Where(s => !(s.Effect.Type == PassiveType.STAT_MODIFIER && s.Effect.Stat == StatType.HP))
                .ToList();
        }

        public void RecalcSessionMaxHp()
        {
            int maxHp = BaseSessionMaxHp;
            foreach (var skill in GetSessionPassiveSkills())
            {
                if (skill.Effect.Type == PassiveType.STAT_MODIFIER && skill.Effect.Stat == StatType.HP)
                {
                    maxHp = skill.Effect.IsPercentage
                        ? (int)(maxHp * (1 + skill.Effect.Value))
                        : maxHp + (int)skill.Effect.Value;
                }
            }
            int oldMax = SessionMaxHp;
            SessionMaxHp = maxHp;
            if (oldMax > 0 && maxHp != oldMax)
            {
                SessionCurrentHp = (int)((long)SessionCurrentHp * maxHp / oldMax);
            }
        }

        public BattleInstance CreateCombatBattle(BattleUnit playerUnit)
        {
            if (CurrentEncounter == null || CurrentEncounter.Type != EncounterType.COMBAT)
            {
                return null;
            }

            float dayProgress = GetProgress();
            var pool = EnemyTable.GetEnemyPoolForChapter(Id);
            int idx1 = _rng.NextInt(0, pool.Length - 1);
            string id1 = pool[idx1];
            var template1 = EnemyTemplate.FromId(id1);
            if (template1 == null) return null;

            bool isDual = _rng.Chance(BattleDataTable.Data.Enemy.DualSpawnChance);
            if (isDual)
            {
                var remaining = pool.Where(id => id != id1).ToArray();
                int idx2 = _rng.NextInt(0, remaining.Length - 1);
                var template2 = EnemyTemplate.FromId(remaining[idx2]);
                if (template2 != null)
                {
                    var e1 = template1.CreateInstance(Id, BattleDataTable.Data.Enemy.DualStatMultiplier, dayProgress);
                    var e2 = template2.CreateInstance(Id, BattleDataTable.Data.Enemy.DualStatMultiplier, dayProgress);
                    CurrentBattle = new BattleInstance(playerUnit, new[] { e1, e2 }, _rng.NextInt(0, 999999));
                    return CurrentBattle;
                }
            }

            var enemy = template1.CreateInstance(Id, 1.0f, dayProgress);
            CurrentBattle = new BattleInstance(playerUnit, enemy, _rng.NextInt(0, 999999));
            return CurrentBattle;
        }

        public BattleInstance CreateEliteBattle(BattleUnit playerUnit)
        {
            var assignment = EnemyTable.GetBossAssignmentForChapter(Id);
            var template = EnemyTemplate.FromId(assignment.Elite);
            if (template == null) return null;

            var elite = template.CreateInstance(Id, 1.0f, GetProgress());
            CurrentBattle = new BattleInstance(playerUnit, elite, _rng.NextInt(0, 999999));
            return CurrentBattle;
        }

        public BattleInstance CreateMidBossBattle(BattleUnit playerUnit)
        {
            var assignment = EnemyTable.GetBossAssignmentForChapter(Id);
            var template = EnemyTemplate.FromId(assignment.MidBoss);
            if (template == null) return null;

            var boss = template.CreateInstance(Id, 1.0f, GetProgress());
            CurrentBattle = new BattleInstance(playerUnit, boss, _rng.NextInt(0, 999999));
            return CurrentBattle;
        }

        public BattleInstance CreateBossBattle(BattleUnit playerUnit)
        {
            var assignment = EnemyTable.GetBossAssignmentForChapter(Id);
            var template = EnemyTemplate.FromId(assignment.Boss);
            if (template == null) return null;

            var boss = template.CreateInstance(Id, 1.0f, GetProgress());
            CurrentBattle = new BattleInstance(playerUnit, boss, _rng.NextInt(0, 999999));
            return CurrentBattle;
        }

        public int OnBattleEnd(BattleState result)
        {
            CurrentBattle = null;

            if (result == BattleState.DEFEAT)
            {
                State = ChapterState.FAILED;
                return 0;
            }

            if (CurrentEncounter != null)
            {
                CurrentEncounter = null;
            }

            var r = BattleDataTable.Data.CombatGoldReward;
            int gold = (int)(r.Base + r.PerChapter * Id + r.PerDay * CurrentDay);
            SessionGold += gold;
            return gold;
        }

        public void OnBossDefeated()
        {
            State = ChapterState.CLEARED;
        }

        public bool IsEliteDay()
        {
            var days = EncounterDataTable.ForcedBattleDays;
            return Type == ChapterType.SIXTY_DAY && CurrentDay == days.Elite;
        }

        public bool IsMidBossDay()
        {
            var days = EncounterDataTable.ForcedBattleDays;
            return Type == ChapterType.SIXTY_DAY && CurrentDay == days.MidBoss;
        }

        public bool IsOptionalEliteDay()
        {
            return OptionalEliteTriggered;
        }

        private bool RollOptionalElite()
        {
            if (Type != ChapterType.SIXTY_DAY) return false;
            var entry = EncounterDataTable.OptionalEliteDays.FirstOrDefault(e => e.Day == CurrentDay);
            if (entry == null) return false;
            OptionalEliteTriggered = _rng.Next() < entry.Chance;
            return OptionalEliteTriggered;
        }

        public bool IsBossDay()
        {
            return CurrentDay >= TotalDays;
        }

        public bool IsCompleted()
        {
            return State == ChapterState.CLEARED;
        }

        public bool IsFailed()
        {
            return State == ChapterState.FAILED;
        }

        public float GetProgress()
        {
            return TotalDays > 0 ? (float)CurrentDay / TotalDays : 0;
        }

        public List<string> GetSessionSkillIds()
        {
            return SessionSkills.Select(s => s.Id).ToList();
        }
    }
}
