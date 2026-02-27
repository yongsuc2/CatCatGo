using System;
using System.Collections.Generic;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.ValueObjects;

namespace CatCatGo.Domain.Chapter
{
    public interface ISessionSkill
    {
        string Id { get; }
        string Name { get; }
        string Icon { get; }
        int Tier { get; }
        string Description { get; }
    }

    public class SessionSkillWrapper : ISessionSkill
    {
        private readonly object _skill;

        public SessionSkillWrapper(ActiveSkill skill)
        {
            _skill = skill;
        }

        public SessionSkillWrapper(PassiveSkill skill)
        {
            _skill = skill;
        }

        public string Id => _skill is ActiveSkill a ? a.Id : ((PassiveSkill)_skill).Id;
        public string Name => _skill is ActiveSkill a ? a.Name : ((PassiveSkill)_skill).Name;
        public string Icon => _skill is ActiveSkill a ? a.Icon : ((PassiveSkill)_skill).Icon;
        public int Tier => _skill is ActiveSkill a ? a.Tier : ((PassiveSkill)_skill).Tier;
        public string Description => _skill is ActiveSkill a ? a.Description : ((PassiveSkill)_skill).Description;

        public bool IsActiveSkill => _skill is ActiveSkill;
        public ActiveSkill AsActiveSkill => _skill as ActiveSkill;
        public PassiveSkill AsPassiveSkill => _skill as PassiveSkill;
        public object Raw => _skill;
    }

    public class EncounterOption
    {
        public string Label;
        public string Description;
        public float HpCostPercent;
        public int GoldCost;
        public float SuccessRate;
        public EncounterReward Reward;
        public string SkillId;
    }

    public class EncounterReward
    {
        public List<SessionSkillWrapper> Skills;
        public float HealPercent;
        public Reward Reward;
        public List<string> SkillIdsToRemove;

        public EncounterReward()
        {
            Skills = new List<SessionSkillWrapper>();
            HealPercent = 0;
            Reward = ValueObjects.Reward.Empty();
            SkillIdsToRemove = new List<string>();
        }
    }

    public class EncounterResult
    {
        public EncounterOption Chosen;
        public bool Success;
        public List<SessionSkillWrapper> SkillsGained;
        public List<SessionSkillWrapper> SkillsRemoved;
        public int HpChange;
        public int GoldChange;
        public Reward Reward;
    }

    public class Encounter
    {
        public readonly EncounterType Type;
        public readonly List<EncounterOption> Options;

        public Encounter(EncounterType type, List<EncounterOption> options)
        {
            Type = type;
            Options = options;
        }

        public EncounterResult Resolve(int choiceIndex, int currentHp, int maxHp, int currentGold, float roll)
        {
            var chosen = Options[Math.Min(choiceIndex, Options.Count - 1)];

            bool success = roll <= chosen.SuccessRate;
            int hpCost = (int)(maxHp * chosen.HpCostPercent);
            int goldCost = chosen.GoldCost;

            if (!success)
            {
                return new EncounterResult
                {
                    Chosen = chosen,
                    Success = false,
                    SkillsGained = new List<SessionSkillWrapper>(),
                    SkillsRemoved = new List<SessionSkillWrapper>(),
                    HpChange = -hpCost,
                    GoldChange = -goldCost,
                    Reward = ValueObjects.Reward.Empty(),
                };
            }

            int healAmount = (int)(maxHp * chosen.Reward.HealPercent);

            return new EncounterResult
            {
                Chosen = chosen,
                Success = true,
                SkillsGained = new List<SessionSkillWrapper>(chosen.Reward.Skills),
                SkillsRemoved = new List<SessionSkillWrapper>(),
                HpChange = healAmount - hpCost,
                GoldChange = -goldCost,
                Reward = chosen.Reward.Reward,
            };
        }
    }
}
