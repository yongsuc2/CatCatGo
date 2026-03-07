using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Data;

namespace CatCatGo.Presentation.Utils
{
    public static class NumberFormatter
    {
        public static string FormatResourceType(ResourceType type)
        {
            return ResourceDataTable.GetLabel(type);
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
