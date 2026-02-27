using CatCatGo.Domain.Enums;

namespace CatCatGo.Domain.Battle
{
    public class StatusEffect
    {
        public readonly StatusEffectType Type;
        public int RemainingTurns;
        public readonly float Value;
        public readonly string SourceSkillId;

        public StatusEffect(StatusEffectType type, int remainingTurns, float value, string sourceSkillId = null)
        {
            Type = type;
            RemainingTurns = remainingTurns;
            Value = value;
            SourceSkillId = sourceSkillId;
        }

        public void Tick()
        {
            if (RemainingTurns > 0)
                RemainingTurns -= 1;
        }

        public bool IsExpired()
        {
            return RemainingTurns <= 0;
        }

        public bool IsStatBuff()
        {
            return Type == StatusEffectType.ATK_UP ||
                   Type == StatusEffectType.DEF_UP ||
                   Type == StatusEffectType.CRIT_UP;
        }

        public bool IsStatDebuff()
        {
            return Type == StatusEffectType.ATK_DOWN ||
                   Type == StatusEffectType.DEF_DOWN;
        }

        public bool IsStun()
        {
            return Type == StatusEffectType.STUN;
        }

        public bool IsDot()
        {
            return Type == StatusEffectType.POISON ||
                   Type == StatusEffectType.BURN;
        }

        public bool IsHot()
        {
            return Type == StatusEffectType.REGEN;
        }

        public float GetDamagePerTurn()
        {
            if (IsDot()) return Value;
            return 0;
        }

        public float GetHealPerTurn()
        {
            if (IsHot()) return Value;
            return 0;
        }
    }
}
