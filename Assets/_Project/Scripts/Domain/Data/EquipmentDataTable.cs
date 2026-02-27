using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Infrastructure;

namespace CatCatGo.Domain.Data
{
    public static class EquipmentDataTable
    {
        private static Dictionary<string, string> _gradeLabels;
        private static Dictionary<string, string> _slotLabels;
        private static Dictionary<string, string> _weaponSubTypeLabels;
        private static Dictionary<string, int> _sellPrices;

        private static void EnsureLoaded()
        {
            if (_gradeLabels != null) return;

            var data = JsonDataLoader.LoadJObject("equipment-labels.data.json");
            if (data == null) return;

            _gradeLabels = new Dictionary<string, string>();
            foreach (var kv in (JObject)data["gradeLabels"])
                _gradeLabels[kv.Key] = kv.Value.ToString();

            _slotLabels = new Dictionary<string, string>();
            foreach (var kv in (JObject)data["slotLabels"])
                _slotLabels[kv.Key] = kv.Value.ToString();

            _weaponSubTypeLabels = new Dictionary<string, string>();
            foreach (var kv in (JObject)data["weaponSubTypeLabels"])
                _weaponSubTypeLabels[kv.Key] = kv.Value.ToString();

            _sellPrices = new Dictionary<string, int>();
            foreach (var kv in (JObject)data["sellPrices"])
                _sellPrices[kv.Key] = kv.Value.Value<int>();
        }

        public static string GetGradeLabel(EquipmentGrade grade)
        {
            EnsureLoaded();
            _gradeLabels.TryGetValue(grade.ToString(), out var label);
            return label ?? grade.ToString();
        }

        public static string GetSlotLabel(SlotType slot)
        {
            EnsureLoaded();
            _slotLabels.TryGetValue(slot.ToString(), out var label);
            return label ?? slot.ToString();
        }

        public static string GetWeaponSubTypeLabel(WeaponSubType subType)
        {
            EnsureLoaded();
            _weaponSubTypeLabels.TryGetValue(subType.ToString(), out var label);
            return label ?? subType.ToString();
        }

        public static int GetSellPrice(EquipmentGrade grade)
        {
            EnsureLoaded();
            _sellPrices.TryGetValue(grade.ToString(), out var price);
            return price;
        }
    }
}
