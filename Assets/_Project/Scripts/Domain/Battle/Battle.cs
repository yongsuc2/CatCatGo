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

            _engine.ResolveInjections(Player.ActiveSkills);
        }

        public Battle(BattleUnit player, BattleUnit enemy, int seed = 0)
            : this(player, new[] { enemy }, seed)
        {
        }

        public BattleUnit Enemy => Enemies[0];

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
                ProcessPlayerTurn(Player, target);
                if (CheckDeath()) return BuildTurnResult();
            }

            foreach (var enemy in Enemies)
            {
                if (!enemy.IsAlive() || !Player.IsAlive()) continue;
                ProcessEnemyTurn(enemy, Player);
                if (CheckDeath()) return BuildTurnResult();
            }

            ProcessStatusEffects(Player);
            foreach (var enemy in Enemies)
            {
                if (enemy.IsAlive())
                    ProcessStatusEffects(enemy);
            }
            CheckDeath();

            return BuildTurnResult();
        }

        private void ProcessPlayerTurn(BattleUnit player, BattleUnit target)
        {
            var builtins = player.GetBuiltinSkills();
            var ilban = builtins.FirstOrDefault(s => s.Id == "ilban_attack");
            var bunno = builtins.FirstOrDefault(s => s.Id == "bunno_attack");

            ActiveSkill mainSkill = null;
            bool isBunno = false;

            if (bunno != null && _engine.EvaluateTrigger(bunno.Trigger, TurnCount, player))
            {
                mainSkill = bunno;
                isBunno = true;
            }
            else
            {
                mainSkill = ilban;
            }

            if (!target.IsAlive()) return;

            if (mainSkill == null)
            {
                ProcessBasicAttack(player, target);
                return;
            }

            var allSkills = player.GetAllSkillsForEngine();
            var allTargets = Enemies.Cast<ISkillExecutionUnit>().ToList();
            var mainResults = _engine.ExecuteSkillEffects(mainSkill, player, target, allSkills, 0, allTargets);

            if (isBunno)
            {
                foreach (var r in mainResults)
                {
                    if (r.Damage > 0)
                        r.Damage = (int)(r.Damage * player.RagePowerMultiplier);
                }
            }

            LogSkillResults(mainResults, player, target, isBunno);
            ApplyLifesteal(player, mainResults);

            ExecuteUpperSkills(player, target, allSkills, mainSkill.Id);

            if (!isBunno && target.IsAlive() && player.MultiHitChance > 0 && _rng.Chance(player.MultiHitChance))
            {
                var multiResults = _engine.ExecuteSkillEffects(mainSkill, player, target, allSkills, 0, allTargets);
                LogSkillResults(multiResults, player, target, false);
                ApplyLifesteal(player, multiResults);

                ExecuteUpperSkills(player, target, allSkills, mainSkill.Id);
            }

            if (!isBunno && bunno != null && target.IsAlive() && player.Rage >= player.MaxRage)
            {
                var bunnoResults = _engine.ExecuteSkillEffects(bunno, player, target, allSkills, 0, allTargets);
                foreach (var r in bunnoResults)
                {
                    if (r.Damage > 0)
                        r.Damage = (int)(r.Damage * player.RagePowerMultiplier);
                }
                LogSkillResults(bunnoResults, player, target, true);
                ApplyLifesteal(player, bunnoResults);

                ExecuteUpperSkills(player, target, allSkills, bunno.Id);
            }

            if (target.IsAlive() && target.CounterTriggerChance > 0 && _rng.Chance(target.CounterTriggerChance))
            {
                ProcessCounter(target, player);
            }
        }

        private void ExecuteUpperSkills(
            BattleUnit player, BattleUnit target,
            List<ActiveSkill> allSkills, string triggerSkillId)
        {
            bool anyAlive = Enemies.Any(e => e.IsAlive());
            if (!anyAlive) return;
            var allTargets = Enemies.Cast<ISkillExecutionUnit>().ToList();
            foreach (var skill in player.ActiveSkills)
            {
                if (!Enemies.Any(e => e.IsAlive())) break;
                if (skill.Hierarchy == SkillHierarchy.BUILTIN) continue;
                if (skill.Hierarchy != SkillHierarchy.UPPER) continue;
                if (_engine.EvaluateTrigger(skill.Trigger, TurnCount, player, triggerSkillId))
                {
                    var results = _engine.ExecuteSkillEffects(skill, player, target, allSkills, 0, allTargets);
                    LogSkillResults(results, player, target, false);
                    ApplyLifesteal(player, results);
                }
            }
        }

        private void ProcessBasicAttack(BattleUnit attacker, BattleUnit target)
        {
            int baseDamage = CalculateBaseDamage(attacker, target);
            bool isCrit = _rng.Chance(attacker.GetEffectiveCrit());
            int finalDamage = isCrit ? (int)(baseDamage * BattleDataTable.Data.Damage.CritMultiplier) : baseDamage;

            int dealt = target.TakeDamage(finalDamage);

            Log.Add(new BattleLogEntry
            {
                Turn = TurnCount,
                Type = isCrit ? BattleLogType.CRIT : BattleLogType.ATTACK,
                Source = attacker.Name,
                Target = target.Name,
                Value = dealt,
                Message = $"{attacker.Name} {(isCrit ? "CRIT" : "attacks")} {target.Name} for {dealt}",
            });

            if (attacker.LifestealRate > 0 && dealt > 0)
            {
                int healAmount = (int)(dealt * attacker.LifestealRate);
                int healed = attacker.Heal(healAmount);
                if (healed > 0)
                {
                    Log.Add(new BattleLogEntry
                    {
                        Turn = TurnCount,
                        Type = BattleLogType.LIFESTEAL,
                        Source = attacker.Name,
                        Target = attacker.Name,
                        Value = healed,
                        Message = $"{attacker.Name} heals {healed} from lifesteal",
                    });
                }
            }

            if (attacker.MultiHitChance > 0 && target.IsAlive() && _rng.Chance(attacker.MultiHitChance))
            {
                int extraDamage = CalculateBaseDamage(attacker, target);
                int extraDealt = target.TakeDamage(extraDamage);
                Log.Add(new BattleLogEntry
                {
                    Turn = TurnCount,
                    Type = BattleLogType.ATTACK,
                    Source = attacker.Name,
                    Target = target.Name,
                    Value = extraDealt,
                    Message = $"{attacker.Name} multi-hit {target.Name} for {extraDealt}",
                });
            }
        }

        private void ProcessEnemyTurn(BattleUnit enemy, BattleUnit target)
        {
            if (!target.IsAlive()) return;

            var stunEffect = enemy.StatusEffects.FirstOrDefault(e => e.IsStun());
            if (stunEffect != null)
            {
                Log.Add(new BattleLogEntry
                {
                    Turn = TurnCount,
                    Type = BattleLogType.STUN,
                    Source = enemy.Name,
                    Target = enemy.Name,
                    Value = 0,
                    Message = $"{enemy.Name} is stunned",
                });
                return;
            }

            int baseDamage = CalculateBaseDamage(enemy, target);
            bool isCrit = _rng.Chance(enemy.GetEffectiveCrit());
            int finalDamage = isCrit ? (int)(baseDamage * BattleDataTable.Data.Damage.CritMultiplier) : baseDamage;

            int dealt = target.TakeDamage(finalDamage);

            Log.Add(new BattleLogEntry
            {
                Turn = TurnCount,
                Type = isCrit ? BattleLogType.CRIT : BattleLogType.ATTACK,
                Source = enemy.Name,
                Target = target.Name,
                Value = dealt,
                Message = $"{enemy.Name} {(isCrit ? "CRIT" : "attacks")} {target.Name} for {dealt}",
            });

            if (enemy.LifestealRate > 0 && dealt > 0)
            {
                int healAmount = (int)(dealt * enemy.LifestealRate);
                int healed = enemy.Heal(healAmount);
                if (healed > 0)
                {
                    Log.Add(new BattleLogEntry
                    {
                        Turn = TurnCount,
                        Type = BattleLogType.LIFESTEAL,
                        Source = enemy.Name,
                        Target = enemy.Name,
                        Value = healed,
                        Message = $"{enemy.Name} heals {healed} from lifesteal",
                    });
                }
            }

            if (!target.IsAlive()) return;

            if (enemy.MultiHitChance > 0 && _rng.Chance(enemy.MultiHitChance))
            {
                int extraDamage = CalculateBaseDamage(enemy, target);
                int extraDealt = target.TakeDamage(extraDamage);
                Log.Add(new BattleLogEntry
                {
                    Turn = TurnCount,
                    Type = BattleLogType.ATTACK,
                    Source = enemy.Name,
                    Target = target.Name,
                    Value = extraDealt,
                    Message = $"{enemy.Name} multi-hit {target.Name} for {extraDealt}",
                });
                if (!target.IsAlive()) return;
            }

            foreach (var skill in enemy.ActiveSkills)
            {
                if (!target.IsAlive()) break;
                if (_engine.EvaluateTrigger(skill.Trigger, TurnCount, enemy))
                {
                    var results = _engine.ExecuteSkillEffects(skill, enemy, target, enemy.ActiveSkills);
                    LogSkillResults(results, enemy, target, false);
                }
            }

            if (!target.IsAlive()) return;
            if (enemy.IsPlayer) return;

            if (enemy.Rage < enemy.MaxRage)
            {
                enemy.Rage = Math.Min(enemy.Rage + BattleDataTable.Data.Rage.PlayerRagePerAttack, enemy.MaxRage);
                if (enemy.Rage >= enemy.MaxRage)
                {
                    enemy.Rage = 0;
                    int rageDamage = (int)(enemy.GetEffectiveAtk() * BattleDataTable.Data.Rage.AttackMultiplier);
                    int rageDealt = target.TakeDamage(rageDamage);
                    Log.Add(new BattleLogEntry
                    {
                        Turn = TurnCount,
                        Type = BattleLogType.RAGE_ATTACK,
                        Source = enemy.Name,
                        Target = target.Name,
                        Value = rageDealt,
                        SkillName = "분노 공격",
                        SkillIcon = "💢",
                        Message = $"{enemy.Name} RAGE ATTACK {target.Name} for {rageDealt}",
                    });
                }
            }

            if (target.IsAlive() && target.CounterTriggerChance > 0 && _rng.Chance(target.CounterTriggerChance))
            {
                ProcessCounter(target, enemy);
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
            int totalDamage = results.Sum(r => r.Damage);
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
