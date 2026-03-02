using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Domain.Data;

namespace CatCatGo.Domain.Entities
{
    public class Talent
    {
        private int _subGradeIndex;
        private int _atkLevel;
        private int _hpLevel;
        private int _defLevel;
        public TalentGrade Grade { get; private set; }

        public Talent(int atkLevel = 0, int hpLevel = 0, int defLevel = 0)
        {
            int perStat = TalentTable.GetLevelsPerStat();
            _subGradeIndex = Math.Min(
                Math.Min(atkLevel / perStat, hpLevel / perStat),
                defLevel / perStat
            );
            _atkLevel = Math.Min(atkLevel - _subGradeIndex * perStat, perStat);
            _hpLevel = Math.Min(hpLevel - _subGradeIndex * perStat, perStat);
            _defLevel = Math.Min(defLevel - _subGradeIndex * perStat, perStat);
            Grade = ComputeGrade();
        }

        public int AtkLevel => _subGradeIndex * TalentTable.GetLevelsPerStat() + _atkLevel;
        public int HpLevel => _subGradeIndex * TalentTable.GetLevelsPerStat() + _hpLevel;
        public int DefLevel => _subGradeIndex * TalentTable.GetLevelsPerStat() + _defLevel;
        public int SubGradeIndex => _subGradeIndex;

        public int GetStatLevelInTier(StatType statType)
        {
            switch (statType)
            {
                case StatType.ATK: return _atkLevel;
                case StatType.HP: return _hpLevel;
                case StatType.DEF: return _defLevel;
                default: return 0;
            }
        }

        private void SetStatInTier(StatType statType, int level)
        {
            switch (statType)
            {
                case StatType.ATK: _atkLevel = level; break;
                case StatType.HP: _hpLevel = level; break;
                case StatType.DEF: _defLevel = level; break;
            }
        }

        public int GetTotalLevel()
        {
            return AtkLevel + HpLevel + DefLevel;
        }

        private TalentGrade ComputeGrade()
        {
            return TalentTable.GetGradeForTotalLevel(GetTotalLevel());
        }

        public int GetUpgradeCost(StatType statType)
        {
            return TalentTable.GetUpgradeCost(GetTotalLevel());
        }

        public bool CanUpgradeStat(StatType statType)
        {
            if (statType == StatType.CRIT) return false;
            if (GetTotalLevel() >= TalentTable.GetMaxLevel()) return false;
            return GetStatLevelInTier(statType) < TalentTable.GetLevelsPerStat();
        }

        public Result<TalentUpgradeResult> Upgrade(StatType statType, int availableGold)
        {
            if (statType == StatType.CRIT)
                return Result.Fail<TalentUpgradeResult>("CRIT cannot be upgraded via talent");

            if (!CanUpgradeStat(statType))
                return Result.Fail<TalentUpgradeResult>("Stat at max for current sub-grade");

            int cost = GetUpgradeCost(statType);
            if (availableGold < cost)
                return Result.Fail<TalentUpgradeResult>("Not enough gold");

            var oldGrade = Grade;
            int perStat = TalentTable.GetLevelsPerStat();
            SetStatInTier(statType, GetStatLevelInTier(statType) + 1);

            bool subGradeAdvanced = false;
            if (_atkLevel >= perStat && _hpLevel >= perStat && _defLevel >= perStat)
            {
                _subGradeIndex++;
                _atkLevel = 0;
                _hpLevel = 0;
                _defLevel = 0;
                subGradeAdvanced = true;
            }

            Grade = ComputeGrade();

            return Result.Ok(new TalentUpgradeResult
            {
                Cost = cost,
                NewLevel = GetStatLevelInTier(statType),
                GradeChanged = oldGrade != Grade,
                SubGradeAdvanced = subGradeAdvanced,
            });
        }

        public Stats GetStats()
        {
            int hp = HpLevel * TalentTable.GetStatPerLevel(StatType.HP);
            int atk = AtkLevel * TalentTable.GetStatPerLevel(StatType.ATK);
            int def = DefLevel * TalentTable.GetStatPerLevel(StatType.DEF);
            return Stats.Create(maxHp: hp, hp: hp, atk: atk, def: def);
        }

        public int? GetNextGradeThreshold()
        {
            return TalentTable.GetNextGradeThreshold(Grade);
        }

        public bool IsMaxGrade()
        {
            return Grade == TalentGrade.HERO;
        }

        public string GetMilestoneKey(int level)
        {
            return $"LV_{level}";
        }

        public List<TalentMilestone> GetClaimableMilestones(HashSet<string> claimedMilestones)
        {
            int totalLevel = GetTotalLevel();
            return TalentTable.GetAllMilestones()
                .Where(m => totalLevel >= m.Level && !claimedMilestones.Contains(GetMilestoneKey(m.Level)))
                .ToList();
        }
    }

    public class TalentUpgradeResult
    {
        public int Cost;
        public int NewLevel;
        public bool GradeChanged;
        public bool SubGradeAdvanced;
    }
}
