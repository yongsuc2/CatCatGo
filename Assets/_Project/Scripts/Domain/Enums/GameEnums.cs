namespace CatCatGo.Domain.Enums
{
    public enum StatType
    {
        HP,
        ATK,
        DEF,
        CRIT,
        RAGE_POWER,
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

    public enum SkillCategory
    {
        ATTACK,
        MASTERY,
        SURVIVAL,
        DEBUFF,
        BUFF,
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
        THIRTY_DAY,
        FIVE_DAY,
    }

    public enum DungeonType
    {
        DRAGON_NEST,
        CELESTIAL_TREE,
        SKY_ISLAND,
    }

    public enum ArenaTier
    {
        BRONZE,
        SILVER,
        GOLD,
        PLATINUM,
        DIAMOND,
        MASTER,
    }

    public enum ChestType
    {
        EQUIPMENT,
        PET,
        GEM,
    }

    public enum EffectType
    {
        DAMAGE,
        HEAL,
        BUFF,
        DEBUFF,
        DOT,
        HOT,
        REVIVE,
        LIFESTEAL,
        COUNTER,
        MULTI_HIT,
        RAGE_POWER,
        RAGE_BOOST,
        SHIELD,
        AOE_DAMAGE,
        MAGIC_BOOST,
    }

    public enum TriggerCondition
    {
        TURN_START,
        ON_ATTACK,
        ON_HIT,
        PASSIVE,
        ON_DEATH,
        ON_RAGE,
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
        ARENA_TICKET,
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
        MAX_HP_DAMAGE,
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
        BUFF_APPLIED,
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
        DAILY_DUNGEON_DRAGON,
        DAILY_DUNGEON_CELESTIAL,
        DAILY_DUNGEON_SKY,
        TOWER_CHALLENGE,
        CATACOMB_RUN,
        ARENA_FIGHT,
        CHAPTER_PROGRESS,
        TRAVEL,
        GOBLIN_MINE,
    }

    public enum TriggerCondition1Kind
    {
        EVERY_N_TURNS,
        ON_SKILL_ACTIVATION,
    }
}
