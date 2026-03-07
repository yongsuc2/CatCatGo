namespace CatCatGo.Domain.Enums
{
    public enum StatType
    {
        HP,
        ATK,
        DEF,
        CRIT,
        MAGIC_COEFFICIENT,
    }

    public enum TalentGrade
    {
        DISCIPLE,
        ADVENTURER,
        ELITE,
        MASTER,
        WARRIOR,
        HERO,
    }

    public enum HeritageRoute
    {
        SKULL,
        KNIGHT,
        RANGER,
        GHOST,
    }

    public enum SkillGrade
    {
        NORMAL,
        LEGENDARY,
        MYTHIC,
        IMMORTAL,
    }

    public static class SkillGradeHelper
    {
        private static readonly System.Collections.Generic.Dictionary<int, SkillGrade> TierToGrade =
            new System.Collections.Generic.Dictionary<int, SkillGrade>
            {
                { 1, SkillGrade.NORMAL },
                { 2, SkillGrade.LEGENDARY },
                { 3, SkillGrade.MYTHIC },
                { 4, SkillGrade.IMMORTAL },
            };

        public static SkillGrade GetGradeForTier(int tier)
        {
            return TierToGrade.TryGetValue(tier, out var g) ? g : SkillGrade.NORMAL;
        }
    }

    public enum EquipmentGrade
    {
        COMMON,
        UNCOMMON,
        RARE,
        EPIC,
        LEGENDARY,
        MYTHIC,
    }

    public enum SlotType
    {
        WEAPON,
        ARMOR,
        RING,
        NECKLACE,
        SHOES,
        GLOVES,
        HAT,
    }

    public enum WeaponSubType
    {
        SWORD,
        STAFF,
        BOW,
    }

    public enum PetTier
    {
        S,
        A,
        B,
    }

    public enum PetGrade
    {
        COMMON,
        RARE,
        EPIC,
        LEGENDARY,
        IMMORTAL,
    }

    public enum EncounterType
    {
        DEMON,
        CHANCE,
        COMBAT,
        JUNGBAK_ROULETTE,
        DAEBAK_ROULETTE,
    }

    public enum ChapterType
    {
        SIXTY_DAY,
    }

    public enum DungeonType
    {
        GIANT_BEEHIVE,
        ANCIENT_TREE,
        TIGER_CLIFF,
    }

    public enum ChestType
    {
        EQUIPMENT,
        PET,
    }

    public enum StatusEffectType
    {
        POISON,
        BURN,
        REGEN,
        ATK_UP,
        ATK_DOWN,
        DEF_UP,
        DEF_DOWN,
        CRIT_UP,
        STUN,
    }

    public enum BattleState
    {
        IN_PROGRESS,
        VICTORY,
        DEFEAT,
    }

    public enum ResourceType
    {
        GOLD,
        GEMS,
        STAMINA,
        CHALLENGE_TOKEN,
        PICKAXE,
        EQUIPMENT_STONE,
        POWER_STONE,
        SKULL_BOOK,
        KNIGHT_BOOK,
        RANGER_BOOK,
        GHOST_BOOK,
        PET_EGG,
        PET_FOOD,
    }

    public enum SkillHierarchy
    {
        BUILTIN,
        UPPER,
        LOWER,
        LOWEST,
    }

    public enum AttackType
    {
        PHYSICAL,
        MAGIC,
        FIXED,
    }

    public enum DamageBase
    {
        ATK,
        SOURCE_MAX_HP,
        TARGET_MAX_HP,
    }

    public enum SkillEffectType
    {
        ATTACK,
        TRIGGER_SKILL,
        INJECT_EFFECT,
        HEAL_HP,
        ADD_RAGE,
        CONSUME_RAGE,
        DEBUFF,
        STUN,
    }

    public enum SpecialConditionType
    {
        NONE,
        RAGE_FULL,
        HP_BELOW,
        HP_ABOVE,
        HP_BELOW_ONCE,
    }

    public enum SkillTag
    {
        SHURIKEN,
        LIGHTNING,
        RAGE,
        HP_RECOVERY,
        POISON,
        PHYSICAL,
        MAGIC,
        LANCE,
        SWORD_AURA,
        FLAME,
        DEBUFF,
        NORMAL_ATTACK,
    }

    public enum PassiveType
    {
        STAT_MODIFIER,
        COUNTER,
        LIFESTEAL,
        SHIELD_ON_START,
        REVIVE,
        REGEN,
        MULTI_HIT,
        SKILL_MODIFIER,
        LOW_HP_MODIFIER,
    }

    public enum BattleLogType
    {
        ATTACK,
        SKILL_DAMAGE,
        COUNTER,
        HEAL,
        LIFESTEAL,
        DOT_DAMAGE,
        HOT_HEAL,
        DEBUFF_APPLIED,
        REVIVE,
        CRIT,
        DEATH,
        TURN_START,
        RAGE_ATTACK,
        STUN,
    }

    public enum ChapterState
    {
        IN_PROGRESS,
        CLEARED,
        FAILED,
    }

    public enum EventType
    {
        COLLECTION,
        MISSION,
        LIMITED_GACHA,
    }

    public enum RoutineAction
    {
        DAILY_DUNGEON_BEEHIVE,
        DAILY_DUNGEON_ANCIENT,
        DAILY_DUNGEON_TIGER,
        TOWER_CHALLENGE,
        CATACOMB_RUN,
        CHAPTER_PROGRESS,
        GOBLIN_MINE,
    }

    public enum TriggerCondition1Kind
    {
        EVERY_N_TURNS,
        ON_SKILL_ACTIVATION,
    }
}
