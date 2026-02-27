using UnityEngine;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Presentation.Utils
{
    public static class ColorPalette
    {
        public static readonly Color GradeCommon = HexToColor("888888");
        public static readonly Color GradeUncommon = HexToColor("4caf50");
        public static readonly Color GradeRare = HexToColor("2196f3");
        public static readonly Color GradeEpic = HexToColor("9c27b0");
        public static readonly Color GradeLegendary = HexToColor("ff9800");
        public static readonly Color GradeMythic = HexToColor("e94560");

        public static readonly Color TalentDisciple = HexToColor("888888");
        public static readonly Color TalentAdventurer = HexToColor("4caf50");
        public static readonly Color TalentElite = HexToColor("2196f3");
        public static readonly Color TalentMaster = HexToColor("9c27b0");
        public static readonly Color TalentWarrior = HexToColor("ff9800");
        public static readonly Color TalentHero = HexToColor("e94560");

        public static readonly Color Background = HexToColor("141e30");
        public static readonly Color Card = HexToColor("243b55");
        public static readonly Color CardLight = HexToColor("2e4a6b");
        public static readonly Color Text = HexToColor("f0f0f0");
        public static readonly Color TextDim = HexToColor("a0a0b0");
        public static readonly Color Gold = HexToColor("ffd700");
        public static readonly Color Gems = HexToColor("64b5f6");
        public static readonly Color Hp = HexToColor("e74c3c");
        public static readonly Color Rage = HexToColor("ff6b35");
        public static readonly Color Heal = HexToColor("4caf50");
        public static readonly Color Crit = HexToColor("ffd700");
        public static readonly Color Stamina = HexToColor("4caf50");
        public static readonly Color ButtonPrimary = HexToColor("4a8fe7");
        public static readonly Color ButtonSecondary = HexToColor("3a5070");
        public static readonly Color NavBarBackground = HexToColor("0f1828");
        public static readonly Color NavBarActive = HexToColor("4a8fe7");
        public static readonly Color NavBarInactive = HexToColor("8899aa");
        public static readonly Color ResourceBarBackground = HexToColor("0c1220");
        public static readonly Color PopupOverlay = new Color(0f, 0f, 0f, 0.7f);
        public static readonly Color ProgressBarBackground = HexToColor("1e2e4a");
        public static readonly Color ProgressBarFill = HexToColor("4a8fe7");

        public static Color GetEquipmentGradeColor(EquipmentGrade grade)
        {
            switch (grade)
            {
                case EquipmentGrade.COMMON: return GradeCommon;
                case EquipmentGrade.UNCOMMON: return GradeUncommon;
                case EquipmentGrade.RARE: return GradeRare;
                case EquipmentGrade.EPIC: return GradeEpic;
                case EquipmentGrade.LEGENDARY: return GradeLegendary;
                case EquipmentGrade.MYTHIC: return GradeMythic;
                default: return GradeCommon;
            }
        }

        public static Color GetTalentGradeColor(TalentGrade grade)
        {
            switch (grade)
            {
                case TalentGrade.DISCIPLE: return TalentDisciple;
                case TalentGrade.ADVENTURER: return TalentAdventurer;
                case TalentGrade.ELITE: return TalentElite;
                case TalentGrade.MASTER: return TalentMaster;
                case TalentGrade.WARRIOR: return TalentWarrior;
                case TalentGrade.HERO: return TalentHero;
                default: return TalentDisciple;
            }
        }

        private static Color HexToColor(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            return color;
        }
    }
}
