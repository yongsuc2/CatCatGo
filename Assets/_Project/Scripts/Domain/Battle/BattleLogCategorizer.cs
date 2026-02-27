using System.Collections.Generic;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Domain.Battle
{
    public struct BattleDamageResult
    {
        public Dictionary<string, int> DamageMap;
        public Dictionary<string, int> HealMap;
    }

    public static class BattleLogCategorizer
    {
        public static BattleDamageResult Categorize(List<BattleLogEntry> entries, string playerName)
        {
            var damageMap = new Dictionary<string, int>();
            var healMap = new Dictionary<string, int>();

            foreach (var entry in entries)
            {
                bool isDmg = entry.Type == BattleLogType.ATTACK
                    || entry.Type == BattleLogType.CRIT
                    || entry.Type == BattleLogType.SKILL_DAMAGE
                    || entry.Type == BattleLogType.COUNTER
                    || entry.Type == BattleLogType.RAGE_ATTACK;
                bool isDot = entry.Type == BattleLogType.DOT_DAMAGE;
                bool isHeal = entry.Type == BattleLogType.LIFESTEAL
                    || entry.Type == BattleLogType.HOT_HEAL
                    || entry.Type == BattleLogType.REVIVE
                    || entry.Type == BattleLogType.HEAL;

                if (isDmg && entry.Source == playerName && entry.Target != playerName)
                {
                    string key;
                    if (entry.Type == BattleLogType.RAGE_ATTACK)
                        key = "\ubd84\ub178 \uacf5\uaca9";
                    else if (!string.IsNullOrEmpty(entry.SkillName) && entry.SkillName != "\uc77c\ubc18 \uacf5\uaca9")
                        key = entry.SkillName;
                    else if (entry.Type == BattleLogType.COUNTER)
                        key = "\ubc18\uaca9";
                    else
                        key = "\uc77c\ubc18 \uacf5\uaca9";

                    if (damageMap.ContainsKey(key)) damageMap[key] += entry.Value;
                    else damageMap[key] = entry.Value;
                }
                else if (isDot && entry.Target != playerName)
                {
                    string key = "\ub3c5 \ud53c\ud574";
                    if (damageMap.ContainsKey(key)) damageMap[key] += entry.Value;
                    else damageMap[key] = entry.Value;
                }

                if (isHeal && entry.Target == playerName)
                {
                    string key;
                    if (entry.Type == BattleLogType.LIFESTEAL) key = "\ud761\ud608";
                    else if (entry.Type == BattleLogType.HOT_HEAL) key = "\uc7ac\uc0dd";
                    else if (entry.Type == BattleLogType.REVIVE) key = "\ubd80\ud65c";
                    else key = !string.IsNullOrEmpty(entry.SkillName) ? entry.SkillName : "\ud68c\ubcf5";

                    if (healMap.ContainsKey(key)) healMap[key] += entry.Value;
                    else healMap[key] = entry.Value;
                }
            }

            return new BattleDamageResult { DamageMap = damageMap, HealMap = healMap };
        }
    }
}
