using Newtonsoft.Json;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public class DamageConfig
    {
        [JsonProperty("defenseConstant")] public float DefenseConstant;
        [JsonProperty("magicDefenseConstant")] public float MagicDefenseConstant;
        [JsonProperty("baseMagicCoefficient")] public float BaseMagicCoefficient;
        [JsonProperty("varianceMin")] public float VarianceMin;
        [JsonProperty("varianceMax")] public float VarianceMax;
        [JsonProperty("critMultiplier")] public float CritMultiplier;
    }

    public class RageConfig
    {
        [JsonProperty("maxRage")] public int MaxRage;
    }

    public class EnemyScalingConfig
    {
        [JsonProperty("dualSpawnChance")] public float DualSpawnChance;
        [JsonProperty("dualStatMultiplier")] public float DualStatMultiplier;
        [JsonProperty("scalingPerChapter")] public float ScalingPerChapter;
        [JsonProperty("scalingPerTowerFloor")] public float ScalingPerTowerFloor;
        [JsonProperty("dayProgressMaxBonus")] public float DayProgressMaxBonus;
    }

    public class CombatGoldRewardConfig
    {
        [JsonProperty("base")] public float Base;
        [JsonProperty("perChapter")] public float PerChapter;
        [JsonProperty("perDay")] public float PerDay;
    }

    public class BattleData
    {
        [JsonProperty("damage")] public DamageConfig Damage;
        [JsonProperty("rage")] public RageConfig Rage;
        [JsonProperty("enemy")] public EnemyScalingConfig Enemy;
        [JsonProperty("combatGoldReward")] public CombatGoldRewardConfig CombatGoldReward;
        [JsonProperty("maxTurns")] public int MaxTurns;
    }

    public static class BattleDataTable
    {
        private static BattleData _data;

        public static BattleData Data
        {
            get
            {
                if (_data == null)
                    _data = JsonDataLoader.Load<BattleData>("battle.data.json");
                return _data;
            }
        }
    }
}
