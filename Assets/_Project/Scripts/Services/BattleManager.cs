using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.Data;

namespace CatCatGo.Services
{
    public struct BattleResult
    {
        public BattleState State;
        public int Turns;
        public int PlayerHpRemaining;
        public Reward Reward;
    }

    public class BattleManager
    {
        public PassiveSkill GetPetAbilitySkill(Player player)
        {
            var pet = player.ActivePet;
            if (pet == null) return null;

            var template = PetTable.GetTemplate(pet.Id.Replace("attendance_pet_", ""));
            var templateByName = template ?? PetTable.GetAllTemplates().FirstOrDefault(t => t.Name == pet.Name);
            if (templateByName == null) return null;

            var ab = templateByName.Ability;
            float val = PetTable.GetAbilityValue(ab, pet.Grade);
            string desc = PetTable.GetAbilityDescription(templateByName.Id, pet.Grade);
            return AbilityToPassiveSkill(ab, val, $"pet_ability_{pet.Id}", desc);
        }

        private PassiveSkill AbilityToPassiveSkill(PetAbilityDef ab, float value, string id, string desc)
        {
            switch (ab.PassiveType)
            {
                case PassiveType.STAT_MODIFIER:
                    return new PassiveSkill(id, desc, "", 1, new SkillTag[0], new HeritageRoute[0],
                        new PassiveEffect { Type = PassiveType.STAT_MODIFIER, Stat = ab.Stat, Value = value, IsPercentage = ab.IsPercentage });
                case PassiveType.COUNTER:
                    return new PassiveSkill(id, desc, "", 1, new SkillTag[0], new HeritageRoute[0],
                        new PassiveEffect { Type = PassiveType.COUNTER, TriggerChance = value });
                case PassiveType.LIFESTEAL:
                    return new PassiveSkill(id, desc, "", 1, new SkillTag[0], new HeritageRoute[0],
                        new PassiveEffect { Type = PassiveType.LIFESTEAL, Rate = value });
                case PassiveType.SHIELD_ON_START:
                    return new PassiveSkill(id, desc, "", 1, new SkillTag[0], new HeritageRoute[0],
                        new PassiveEffect { Type = PassiveType.SHIELD_ON_START, HpPercent = value });
                case PassiveType.REVIVE:
                    return new PassiveSkill(id, desc, "", 1, new SkillTag[0], new HeritageRoute[0],
                        new PassiveEffect { Type = PassiveType.REVIVE, HpPercent = value, MaxUses = 1 });
                case PassiveType.REGEN:
                    return new PassiveSkill(id, desc, "", 1, new SkillTag[0], new HeritageRoute[0],
                        new PassiveEffect { Type = PassiveType.REGEN, HealPerTurn = value });
                case PassiveType.MULTI_HIT:
                    return new PassiveSkill(id, desc, "", 1, new SkillTag[0], new HeritageRoute[0],
                        new PassiveEffect { Type = PassiveType.MULTI_HIT, Chance = value });
                default:
                    return null;
            }
        }

        public BattleUnit CreatePlayerUnit(Player player, ActiveSkill[] activeSkills, PassiveSkill[] passiveSkills)
        {
            var stats = player.ComputeStats();
            var petAbility = GetPetAbilitySkill(player);
            var allPassives = new List<PassiveSkill>(passiveSkills);
            if (petAbility != null) allPassives.Add(petAbility);
            return new BattleUnit("Capybara", stats, activeSkills, allPassives.ToArray(), true);
        }

        public Battle CreateBattle(BattleUnit playerUnit, BattleUnit enemyUnit, int seed = 0)
        {
            return new Battle(playerUnit, enemyUnit, seed);
        }

        public BattleResult SimulateBattle(Battle battle, int maxTurns = 100)
        {
            battle.RunToCompletion(maxTurns);

            var reward = battle.State == BattleState.VICTORY
                ? GenerateCombatReward(battle.TurnCount)
                : Reward.Empty();

            return new BattleResult
            {
                State = battle.State,
                Turns = battle.TurnCount,
                PlayerHpRemaining = battle.Player.CurrentHp,
                Reward = reward,
            };
        }

        private Reward GenerateCombatReward(int turns)
        {
            int goldAmount = 50 + turns * 5;
            return Reward.FromResources(
                new ResourceReward(ResourceType.GOLD, goldAmount));
        }

        public int CalculateDamagePreview(int atk, int def)
        {
            return Math.Max(1, atk - (int)Math.Floor(def * 0.5));
        }

        public int EstimateSurvivalTurns(Stats playerStats, Stats enemyStats)
        {
            int enemyDamagePerTurn = Math.Max(1, enemyStats.Atk - (int)Math.Floor(playerStats.Def * 0.5));
            return (int)Math.Ceiling((double)playerStats.Hp / enemyDamagePerTurn);
        }

        public int EstimateKillTurns(Stats playerStats, Stats enemyStats)
        {
            int playerDamagePerTurn = Math.Max(1, playerStats.Atk - (int)Math.Floor(enemyStats.Def * 0.5));
            return (int)Math.Ceiling((double)enemyStats.Hp / playerDamagePerTurn);
        }
    }
}
