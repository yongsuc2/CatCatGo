using System;
using System.Collections.Generic;
using System.Linq;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.ValueObjects;

namespace CatCatGo.Domain.Entities
{
    public class Resources
    {
        private readonly Dictionary<ResourceType, float> _amounts = new Dictionary<ResourceType, float>();
        private readonly int _staminaMax;
        private readonly int _staminaRegenPerMinute;

        private const int STAMINA_MAX = 100;
        private const int STAMINA_REGEN_PER_MINUTE = 1;

        public Resources()
        {
            _staminaMax = STAMINA_MAX;
            _staminaRegenPerMinute = STAMINA_REGEN_PER_MINUTE;
            InitDefaults();
        }

        private void InitDefaults()
        {
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                _amounts[type] = 0;
            }
            _amounts[ResourceType.STAMINA] = _staminaMax;
        }

        public float Get(ResourceType type)
        {
            return _amounts.TryGetValue(type, out var v) ? v : 0;
        }

        public float Gold => Get(ResourceType.GOLD);
        public float Gems => Get(ResourceType.GEMS);
        public float Stamina => Get(ResourceType.STAMINA);
        public float ChallengeTokens => Get(ResourceType.CHALLENGE_TOKEN);
        public float ArenaTickets => Get(ResourceType.ARENA_TICKET);
        public float Pickaxes => Get(ResourceType.PICKAXE);
        public float EquipmentStones => Get(ResourceType.EQUIPMENT_STONE);
        public float PowerStones => Get(ResourceType.POWER_STONE);

        public void Add(ResourceType type, float amount)
        {
            float current = Get(type);
            float newAmount = current + amount;

            if (type == ResourceType.STAMINA)
            {
                newAmount = Math.Min(newAmount, _staminaMax);
            }

            _amounts[type] = newAmount;
        }

        public Result Spend(ResourceType type, float amount)
        {
            if (!CanAfford(type, amount))
            {
                return Result.Fail($"Not enough {type}");
            }
            _amounts[type] = Get(type) - amount;
            return Result.Ok();
        }

        public bool CanAfford(ResourceType type, float amount)
        {
            return Get(type) >= amount;
        }

        public bool CanAffordMultiple(IEnumerable<(ResourceType type, float amount)> entries)
        {
            return entries.All(e => CanAfford(e.type, e.amount));
        }

        public Result SpendMultiple(IEnumerable<(ResourceType type, float amount)> entries)
        {
            var list = entries.ToList();
            if (!CanAffordMultiple(list))
            {
                return Result.Fail("Not enough resources");
            }
            foreach (var entry in list)
            {
                _amounts[entry.type] = Get(entry.type) - entry.amount;
            }
            return Result.Ok();
        }

        public void Tick(float deltaMs)
        {
            float currentStamina = Get(ResourceType.STAMINA);
            if (currentStamina >= _staminaMax) return;

            float regenAmount = (deltaMs / 60000f) * _staminaRegenPerMinute;
            float newStamina = Math.Min(currentStamina + regenAmount, _staminaMax);
            _amounts[ResourceType.STAMINA] = newStamina;
        }

        public int GetStaminaMax()
        {
            return _staminaMax;
        }

        public void DailyReset()
        {
            _amounts[ResourceType.CHALLENGE_TOKEN] = 5;
            _amounts[ResourceType.ARENA_TICKET] = 5;
            _amounts[ResourceType.PICKAXE] = 10;
        }

        public void SetAmount(ResourceType type, float amount)
        {
            _amounts[type] = amount;
        }

        public Dictionary<string, float> ToJSON()
        {
            var obj = new Dictionary<string, float>();
            foreach (var kv in _amounts)
            {
                obj[kv.Key.ToString()] = kv.Value;
            }
            return obj;
        }

        public static Resources FromJSON(Dictionary<string, float> data)
        {
            var res = new Resources();
            foreach (var kv in data)
            {
                if (Enum.TryParse<ResourceType>(kv.Key, out var type))
                {
                    res.SetAmount(type, kv.Value);
                }
            }
            return res;
        }
    }
}
