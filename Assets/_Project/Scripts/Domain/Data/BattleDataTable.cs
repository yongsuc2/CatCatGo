using Newtonsoft.Json;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public class DamageConfig
    {
        [JsonProperty("defenseConstant")] public float DefenseConstant;
        [JsonProperty("magicDefenseConstant")] public float MagicDefenseConstant;
        [JsonProperty("baseMagicCoefficient")] public float BaseMagicCoefficient;
        [JsonProperty("critMultiplier")] public float CritMultiplier;
        [JsonProperty("counterCoefficient")] public float CounterCoefficient;
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
    }

    public class CombatGoldRewardConfig
    {
        [JsonProperty("base")] public float Base;
        [JsonProperty("perChapter")] public float PerChapter;
        [JsonProperty("perDay")] public float PerDay;
    }

    public class PlayerBaseStatsConfig
    {
        [JsonProperty("hp")] public int Hp;
        [JsonProperty("atk")] public int Atk;
        [JsonProperty("def")] public int Def;
    }

    public class DailyResetConfig
    {
        [JsonProperty("challengeToken")] public int ChallengeToken;
        [JsonProperty("pickaxe")] public int Pickaxe;
    }

    public class StaminaConfig
    {
        [JsonProperty("max")] public int Max;
        [JsonProperty("regenPerMinute")] public int RegenPerMinute;
    }

    public class NewGameResourcesConfig
    {
        [JsonProperty("gold")] public int Gold;
        [JsonProperty("gems")] public int Gems;
        [JsonProperty("stamina")] public int Stamina;
    }

    public class BattleData
    {
        [JsonProperty("damage")] public DamageConfig Damage;
        [JsonProperty("rage")] public RageConfig Rage;
        [JsonProperty("enemy")] public EnemyScalingConfig Enemy;
        [JsonProperty("combatGoldReward")] public CombatGoldRewardConfig CombatGoldReward;
        [JsonProperty("maxTurns")] public int MaxTurns;
        [JsonProperty("playerBaseStats")] public PlayerBaseStatsConfig PlayerBaseStats;
        [JsonProperty("dailyReset")] public DailyResetConfig DailyReset;
        [JsonProperty("stamina")] public StaminaConfig Stamina;
        [JsonProperty("chapterStaminaCost")] public int ChapterStaminaCost;
        [JsonProperty("newGameResources")] public NewGameResourcesConfig NewGameResources;
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
