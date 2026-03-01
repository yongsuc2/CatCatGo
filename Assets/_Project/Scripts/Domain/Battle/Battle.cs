using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Data;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Battle
{
    public class TurnResult
    {
        public int TurnNumber;
        public BattleState State;
        public int PlayerHp;
        public int EnemyHp;
        public List<int> EnemyHps;
        public List<BattleLogEntry> Entries;
    }

    public class Battle
    {
        public BattleUnit Player;
        public List<BattleUnit> Enemies;
        public int TurnCount;
        public BattleState State;
        public BattleLog Log;
        private SeededRandom _rng;
        private SkillExecutionEngine _engine;

        public Battle(BattleUnit player, BattleUnit[] enemies, int seed = 0)
        {
            Player = player;
            Enemies = new List<BattleUnit>(enemies);
            TurnCount = 0;
            State = BattleState.IN_PROGRESS;
            Log = new BattleLog();
            _rng = new SeededRandom(seed == 0 ? Environment.TickCount : seed);
            _engine = new SkillExecutionEngine(_rng);

            foreach (var unit in GetAllUnits())
            {
                _engine.ResolveInjections(unit.ActiveSkills);
            }
        }

        public Battle(BattleUnit player, BattleUnit enemy, int seed = 0)
            : this(player, new[] { enemy }, seed)
        {
        }

        public BattleUnit Enemy => Enemies[0];

        private IEnumerable<BattleUnit> GetAllUnits()
        {
            yield return Player;
            foreach (var enemy in Enemies)
                yield return enemy;
        }

        private BattleUnit GetFirstAliveEnemy()
        {
            return Enemies.FirstOrDefault(e => e.IsAlive());
        }

        public TurnResult ExecuteTurn()
        {
            if (State != BattleState.IN_PROGRESS)
                return BuildTurnResult();

            TurnCount += 1;

            Log.Add(new BattleLogEntry
            {
                Turn = TurnCount,
                Type = BattleLogType.TURN_START,
                Source = "",
                Target = "",
                Value = TurnCount,
                Message = $"Turn {TurnCount}",
            });

            var target = GetFirstAliveEnemy();
            if (target != null && Player.IsAlive())
            {
                ProcessUnitTurn(Player, target, Enemies.Cast<ISkillExecutionUnit>().ToList());
                if (CheckDeath()) return BuildTurnResult();
            }

            foreach (var enemy in Enemies)
            {
                if (!enemy.IsAlive() || !Player.IsAlive()) continue;
                ProcessUnitTurn(enemy, Player, new List<ISkillExecutionUnit> { Player });
                if (CheckDeath()) return BuildTurnResult();
            }

            foreach (var unit in GetAllUnits())
            {
                if (unit.IsAlive())
                    ProcessStatusEffects(unit);
            }
            CheckDeath();

            return BuildTurnResult();
        }

        private void ProcessUnitTurn(BattleUnit unit, BattleUnit target, List<ISkillExecutionUnit> allTargets)
        {
            if (!target.IsAlive()) return;

            var stunEffect = unit.StatusEffects.FirstOrDefault(e => e.IsStun());
            if (stunEffect != null)
            {
                Log.Add(new BattleLogEntry
                {
                    Turn = TurnCount,
                    Type = BattleLogType.STUN,
                    Source = unit.Name,
                    Target = unit.Name,
                    Value = 0,
                    Message = $"{unit.Name} is stunned",
                });
                return;
            }

            var builtins = unit.GetBuiltinSkills();
            var ilban = builtins.FirstOrDefault(s => s.Id == "ilban_attack");
            var bunno = builtins.FirstOrDefault(s => s.Id == "bunno_attack");

            ActiveSkill mainSkill = null;
            bool isBunno = false;

            if (bunno != null && _engine.EvaluateTrigger(bunno.Trigger, TurnCount, unit))
            {
                mainSkill = bunno;
                isBunno = true;
            }
            else
            {
                mainSkill = ilban;
            }

            if (!target.IsAlive()) return;

            if (mainSkill == null) return;

            var allSkills = unit.GetAllSkillsForEngine();
            var mainResults = _engine.ExecuteSkillEffects(mainSkill, unit, target, allSkills, 0, allTargets);

            LogSkillResults(mainResults, unit, target, isBunno);
            ApplyLifesteal(unit, mainResults);

            ExecuteUpperSkills(unit, target, allSkills, mainSkill.Id, allTargets);

            if (!isBunno && target.IsAlive() && unit.MultiHitChance > 0 && _rng.Chance(unit.MultiHitChance))
            {
                var multiResults = _engine.ExecuteSkillEffects(mainSkill, unit, target, allSkills, 0, allTargets);
                LogSkillResults(multiResults, unit, target, false);
                ApplyLifesteal(unit, multiResults);

                ExecuteUpperSkills(unit, target, allSkills, mainSkill.Id, allTargets);
            }

            if (!isBunno && bunno != null && target.IsAlive() && unit.Rage >= unit.MaxRage)
            {
                var bunnoResults = _engine.ExecuteSkillEffects(bunno, unit, target, allSkills, 0, allTargets);
                LogSkillResults(bunnoResults, unit, target, true);
                ApplyLifesteal(unit, bunnoResults);

                ExecuteUpperSkills(unit, target, allSkills, bunno.Id, allTargets);
            }

            if (target.IsAlive() && target.CounterTriggerChance > 0 && _rng.Chance(target.CounterTriggerChance))
            {
                ProcessCounter(target, unit);
            }
        }

        private void ExecuteUpperSkills(
            BattleUnit unit, BattleUnit target,
            List<ActiveSkill> allSkills, string triggerSkillId,
            List<ISkillExecutionUnit> allTargets)
        {
            bool anyAlive = allTargets.Any(e => e.IsAlive());
            if (!anyAlive) return;
            foreach (var skill in unit.ActiveSkills)
            {
                if (!allTargets.Any(e => e.IsAlive())) break;
                if (skill.Hierarchy == SkillHierarchy.BUILTIN) continue;
                if (skill.Hierarchy == SkillHierarchy.LOWEST) continue;
                if (_engine.EvaluateTrigger(skill.Trigger, TurnCount, unit, triggerSkillId))
                {
                    var results = _engine.ExecuteSkillEffects(skill, unit, target, allSkills, 0, allTargets);
                    LogSkillResults(results, unit, target, false);
                    ApplyLifesteal(unit, results);
                }
            }
        }

        private void LogSkillResults(List<SkillDamageResult> results, BattleUnit source, BattleUnit target, bool isRage)
        {
            foreach (var r in results)
            {
                var tName = r.TargetName ?? target.Name;
                if (r.Damage > 0)
                {
                    if (isRage && r.SkillName == "분노 공격")
                    {
                        Log.Add(new BattleLogEntry
                        {
                            Turn = TurnCount,
                            Type = BattleLogType.RAGE_ATTACK,
                            Source = source.Name,
                            Target = tName,
                            Value = r.Damage,
                            SkillName = r.SkillName,
                            SkillId = r.SkillId,
                            SkillIcon = r.SkillIcon,
                            Message = $"{source.Name} RAGE ATTACK {tName} for {r.Damage}",
                        });
                    }
                    else if (r.IsCrit)
                    {
                        Log.Add(new BattleLogEntry
                        {
                            Turn = TurnCount,
                            Type = BattleLogType.CRIT,
                            Source = source.Name,
                            Target = tName,
                            Value = r.Damage,
                            SkillName = r.SkillName,
                            SkillId = r.SkillId,
                            SkillIcon = r.SkillIcon,
                            Message = $"{source.Name}'s {r.SkillName} CRIT {tName} for {r.Damage}",
                        });
                    }
                    else
                    {
                        Log.Add(new BattleLogEntry
                        {
                            Turn = TurnCount,
                            Type = r.SkillName == "일반 공격" ? BattleLogType.ATTACK : BattleLogType.SKILL_DAMAGE,
                            Source = source.Name,
                            Target = tName,
                            Value = r.Damage,
                            SkillName = r.SkillName,
                            SkillId = r.SkillId,
                            SkillIcon = r.SkillIcon,
                            Message = $"{source.Name}'s {r.SkillName} deals {r.Damage} to {tName}",
                        });
                    }
                }
                if (r.HealAmount > 0)
                {
                    Log.Add(new BattleLogEntry
                    {
                        Turn = TurnCount,
                        Type = BattleLogType.HEAL,
                        Source = source.Name,
                        Target = source.Name,
                        Value = r.HealAmount,
                        SkillName = r.SkillName,
                        SkillIcon = r.SkillIcon,
                        Message = $"{source.Name} heals {r.HealAmount}",
                    });
                }
                if (r.DebuffApplied)
                {
                    Log.Add(new BattleLogEntry
                    {
                        Turn = TurnCount,
                        Type = BattleLogType.DEBUFF_APPLIED,
                        Source = source.Name,
                        Target = target.Name,
                        Value = 0,
                        SkillName = r.SkillName,
                        SkillIcon = r.SkillIcon,
                        Message = $"{source.Name}'s {r.SkillName} debuffs {target.Name}",
                    });
                }
            }
        }

        private void ApplyLifesteal(BattleUnit unit, List<SkillDamageResult> results)
        {
            if (unit.LifestealRate <= 0) return;
            int totalDamage = results.Where(r => r.AttackType == AttackType.PHYSICAL).Sum(r => r.Damage);
            if (totalDamage <= 0) return;

            int healAmount = (int)(totalDamage * unit.LifestealRate);
            int healed = unit.Heal(healAmount);
            if (healed > 0)
            {
                Log.Add(new BattleLogEntry
                {
                    Turn = TurnCount,
                    Type = BattleLogType.LIFESTEAL,
                    Source = unit.Name,
                    Target = unit.Name,
                    Value = healed,
                    Message = $"{unit.Name} heals {healed} from lifesteal",
                });
            }
        }

        private void ProcessCounter(BattleUnit defender, BattleUnit attacker)
        {
            int baseDamage = CalculateBaseDamage(defender, attacker);
            bool isCrit = _rng.Chance(defender.GetEffectiveCrit());
            int finalDamage = isCrit ? (int)(baseDamage * BattleDataTable.Data.Damage.CritMultiplier) : baseDamage;
            int dealt = attacker.TakeDamage(finalDamage);
            Log.Add(new BattleLogEntry
            {
                Turn = TurnCount,
                Type = BattleLogType.COUNTER,
                Source = defender.Name,
                Target = attacker.Name,
                Value = dealt,
                Message = $"{defender.Name} counters {attacker.Name} for {dealt}",
            });
        }

        private void ProcessStatusEffects(BattleUnit unit)
        {
            var result = unit.TickStatusEffects();
            if (result.Damage > 0)
            {
                Log.Add(new BattleLogEntry
                {
                    Turn = TurnCount,
                    Type = BattleLogType.DOT_DAMAGE,
                    Source = "Status",
                    Target = unit.Name,
                    Value = result.Damage,
                    Message = $"{unit.Name} takes {result.Damage} from DoT",
                });
            }
            if (result.Heal > 0)
            {
                Log.Add(new BattleLogEntry
                {
                    Turn = TurnCount,
                    Type = BattleLogType.HOT_HEAL,
                    Source = "Status",
                    Target = unit.Name,
                    Value = result.Heal,
                    Message = $"{unit.Name} heals {result.Heal} from regen",
                });
            }
        }

        private int CalculateBaseDamage(BattleUnit attacker, BattleUnit defender)
        {
            int atk = attacker.GetEffectiveAtk() + attacker.GetHpBonusDamage();
            int def = defender.GetEffectiveDef();
            float k = BattleDataTable.Data.Damage.DefenseConstant;
            int raw = Math.Max(1, (int)(atk * (k / (k + def))));
            float variance = _rng.NextFloat(BattleDataTable.Data.Damage.VarianceMin, BattleDataTable.Data.Damage.VarianceMax);
            return Math.Max(1, (int)(raw * variance));
        }

        private bool CheckDeath()
        {
            bool allEnemiesDead = Enemies.All(e => !e.IsAlive());
            if (allEnemiesDead)
            {
                State = BattleState.VICTORY;
                foreach (var enemy in Enemies)
                {
                    if (!enemy.IsAlive())
                    {
                        bool alreadyLogged = Log.Entries.Any(
                            e => e.Type == BattleLogType.DEATH && e.Target == enemy.Name);
                        if (!alreadyLogged)
                        {
                            Log.Add(new BattleLogEntry
                            {
                                Turn = TurnCount,
                                Type = BattleLogType.DEATH,
                                Source = "",
                                Target = enemy.Name,
                                Value = 0,
                                Message = $"{enemy.Name} defeated",
                            });
                        }
                    }
                }
                return true;
            }

            if (!Player.IsAlive())
            {
                if (Player.CanRevive())
                {
                    Player.TryRevive();
                    Log.Add(new BattleLogEntry
                    {
                        Turn = TurnCount,
                        Type = BattleLogType.REVIVE,
                        Source = Player.Name,
                        Target = Player.Name,
                        Value = Player.CurrentHp,
                        Message = $"{Player.Name} revives with {Player.CurrentHp} HP",
                    });
                    return false;
                }

                State = BattleState.DEFEAT;
                Log.Add(new BattleLogEntry
                {
                    Turn = TurnCount,
                    Type = BattleLogType.DEATH,
                    Source = "",
                    Target = Player.Name,
                    Value = 0,
                    Message = $"{Player.Name} defeated",
                });
                return true;
            }

            return false;
        }

        private TurnResult BuildTurnResult()
        {
            return new TurnResult
            {
                TurnNumber = TurnCount,
                State = State,
                PlayerHp = Player.CurrentHp,
                EnemyHp = Enemies[0].CurrentHp,
                EnemyHps = Enemies.Select(e => e.CurrentHp).ToList(),
                Entries = Log.GetEntriesForTurn(TurnCount),
            };
        }

        public TurnResult RunToCompletion(int maxTurns = 0)
        {
            if (maxTurns == 0) maxTurns = BattleDataTable.Data.MaxTurns;

            while (State == BattleState.IN_PROGRESS && TurnCount < maxTurns)
            {
                ExecuteTurn();
            }

            if (State == BattleState.IN_PROGRESS)
            {
                State = BattleState.DEFEAT;
            }

            return BuildTurnResult();
        }

        public bool IsFinished()
        {
            return State != BattleState.IN_PROGRESS;
        }
    }
}
