using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Domain.ValueObjects
{
    public struct ResourceReward
    {
        public ResourceType Type;
        public int Amount;

        public ResourceReward(ResourceType type, int amount)
        {
            Type = type;
            Amount = amount;
        }
    }

    public class Reward
    {
        public List<ResourceReward> Resources { get; }
        public List<string> EquipmentIds { get; }
        public List<string> SkillIds { get; }
        public List<string> PetIds { get; }

        public Reward(
            List<ResourceReward> resources = null,
            List<string> equipmentIds = null,
            List<string> skillIds = null,
            List<string> petIds = null)
        {
            Resources = resources ?? new List<ResourceReward>();
            EquipmentIds = equipmentIds ?? new List<string>();
            SkillIds = skillIds ?? new List<string>();
            PetIds = petIds ?? new List<string>();
        }

        public static Reward Empty()
        {
            return new Reward();
        }

        public static Reward FromResources(params ResourceReward[] resources)
        {
            return new Reward(new List<ResourceReward>(resources));
        }

        public Reward Merge(Reward other)
        {
            return new Reward(
                Resources.Concat(other.Resources).ToList(),
                EquipmentIds.Concat(other.EquipmentIds).ToList(),
                SkillIds.Concat(other.SkillIds).ToList(),
                PetIds.Concat(other.PetIds).ToList()
            );
        }

        public bool IsEmpty()
        {
            return Resources.Count == 0 &&
                   EquipmentIds.Count == 0 &&
                   SkillIds.Count == 0 &&
                   PetIds.Count == 0;
        }
    }
}
