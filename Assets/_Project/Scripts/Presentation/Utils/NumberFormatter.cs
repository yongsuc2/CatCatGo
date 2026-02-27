using System.Collections.Generic;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Presentation.Utils
{
    public static class NumberFormatter
    {
        private static readonly Dictionary<ResourceType, string> ResourceLabels =
            new Dictionary<ResourceType, string>
            {
                { ResourceType.GOLD, "\uace8\ub4dc" },
                { ResourceType.GEMS, "\ubcf4\uc11d" },
                { ResourceType.STAMINA, "\uc2a4\ud0dc\ubbf8\ub098" },
                { ResourceType.CHALLENGE_TOKEN, "\ub3c4\uc804 \ud1a0\ud070" },
                { ResourceType.ARENA_TICKET, "\uc544\ub808\ub098 \ud2f0\ucf13" },
                { ResourceType.PICKAXE, "\uace1\uad2d\uc774" },
                { ResourceType.EQUIPMENT_STONE, "\uc7a5\ube44\uc11d" },
                { ResourceType.POWER_STONE, "\uac15\ud654\uc11d" },
                { ResourceType.SKULL_BOOK, "\ud574\uace8 \uc11c\uc801" },
                { ResourceType.KNIGHT_BOOK, "\uae30\uc0ac \uc11c\uc801" },
                { ResourceType.RANGER_BOOK, "\ub808\uc778\uc800 \uc11c\uc801" },
                { ResourceType.GHOST_BOOK, "\uc720\ub839 \uc11c\uc801" },
                { ResourceType.PET_EGG, "\ud3ab \uc54c" },
                { ResourceType.PET_FOOD, "\ud3ab \uba39\uc774" },
            };

        public static string FormatResourceType(ResourceType type)
        {
            return ResourceLabels.TryGetValue(type, out var label) ? label : type.ToString();
        }

        private static readonly string[] Suffixes = { "", "K", "M", "B", "T" };

        public static string Format(float value)
        {
            if (value < 0) return "-" + Format(-value);
            if (value < 1000f) return ((int)value).ToString();

            int tier = 0;
            float reduced = value;
            while (reduced >= 1000f && tier < Suffixes.Length - 1)
            {
                reduced /= 1000f;
                tier++;
            }

            if (reduced >= 100f) return $"{(int)reduced}{Suffixes[tier]}";
            if (reduced >= 10f) return $"{reduced:F1}{Suffixes[tier]}";
            return $"{reduced:F2}{Suffixes[tier]}";
        }

        public static string FormatInt(int value)
        {
            return Format(value);
        }
    }
}
