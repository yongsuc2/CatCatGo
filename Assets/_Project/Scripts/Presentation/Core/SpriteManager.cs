using System.Collections.Generic;
using UnityEngine;
using CatCatGo.Domain.Enums;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Core
{
    public class SpriteManager : MonoBehaviour
    {
        public static SpriteManager Instance { get; private set; }

        private static readonly Color PlaceholderPink = new Color(1f, 0.41f, 0.71f);

        private SpriteDatabase _database;
        private Dictionary<string, CharacterSpriteEntry> _characterLookup;
        private Dictionary<string, EquipmentSpriteEntry> _equipmentLookup;
        private Dictionary<string, PetSpriteEntry> _petLookup;
        private Dictionary<StatusEffectType, StatusEffectSpriteEntry> _statusEffectLookup;
        private Dictionary<string, Sprite> _iconLookup;
        private Dictionary<string, Sprite> _equipIconByKey;
        private Dictionary<string, Sprite> _skillIconByKey;

        private Dictionary<string, Sprite> _battleSpriteCache;
        private Dictionary<string, Sprite[]> _animFrameCache;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _database = Resources.Load<SpriteDatabase>("SpriteDatabase");

            BuildLookups();
        }

        private void BuildLookups()
        {
            _characterLookup = new Dictionary<string, CharacterSpriteEntry>();
            _equipmentLookup = new Dictionary<string, EquipmentSpriteEntry>();
            _petLookup = new Dictionary<string, PetSpriteEntry>();
            _statusEffectLookup = new Dictionary<StatusEffectType, StatusEffectSpriteEntry>();
            _iconLookup = new Dictionary<string, Sprite>();

            var iconSprites = Resources.LoadAll<Sprite>("Icons");
            foreach (var sprite in iconSprites)
            {
                _iconLookup[sprite.name] = sprite;
                string baseName = StripSpriteModeSuffix(sprite.name);
                if (baseName != sprite.name && !_iconLookup.ContainsKey(baseName))
                    _iconLookup[baseName] = sprite;
            }

            _equipIconByKey = new Dictionary<string, Sprite>();
            var equipSprites = Resources.LoadAll<Sprite>("Icons/equip");
            foreach (var sprite in equipSprites)
            {
                string name = StripSpriteModeSuffix(sprite.name);
                if (name.Contains("256")) continue;
                if (!name.StartsWith("equip_")) continue;
                string key = name.Substring(6).ToUpperInvariant();
                if (!_equipIconByKey.ContainsKey(key))
                    _equipIconByKey[key] = sprite;
            }

            _skillIconByKey = new Dictionary<string, Sprite>();
            var skillSprites = Resources.LoadAll<Sprite>("Icons/skill");
            foreach (var sprite in skillSprites)
            {
                string name = StripSpriteModeSuffix(sprite.name);
                if (!name.StartsWith("icon_skill_")) continue;
                string key = name.Substring(11);
                if (!_skillIconByKey.ContainsKey(key))
                    _skillIconByKey[key] = sprite;
            }

            Debug.Log($"[SpriteManager] Icons loaded: {_iconLookup.Count}, equip: {_equipIconByKey.Count}, skill: {_skillIconByKey.Count}");

            if (iconSprites.Length == 0)
            {
                var textures = Resources.LoadAll<Texture2D>("Icons");
                foreach (var tex in textures)
                {
                    var sprite = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f),
                        100f);
                    sprite.name = tex.name;
                    _iconLookup[tex.name] = sprite;
                }
                Debug.Log($"[SpriteManager] Fallback Texture2D loaded: {textures.Length}");
            }

            if (_database == null) return;

            if (_database.characters != null)
            {
                foreach (var entry in _database.characters)
                    _characterLookup[entry.id] = entry;
            }

            if (_database.equipmentIcons != null)
            {
                foreach (var entry in _database.equipmentIcons)
                    _equipmentLookup[$"{entry.slot}_{entry.grade}"] = entry;
            }

            if (_database.petIcons != null)
            {
                foreach (var entry in _database.petIcons)
                    _petLookup[entry.petId] = entry;
            }

            if (_database.statusEffects != null)
            {
                foreach (var entry in _database.statusEffects)
                    _statusEffectLookup[entry.type] = entry;
            }

            if (_database.icons != null)
            {
                foreach (var entry in _database.icons)
                {
                    if (entry.sprite != null)
                        _iconLookup[entry.id] = entry.sprite;
                }
            }
        }

        public Sprite GetPlayerSprite() => GetBattleSprite("player");

        public Sprite GetBattleSprite(string id)
        {
            if (_battleSpriteCache == null)
                _battleSpriteCache = new Dictionary<string, Sprite>();

            if (_battleSpriteCache.TryGetValue(id, out var cached))
                return cached;

            var sprite = LoadBattleSprite(id);
            _battleSpriteCache[id] = sprite;
            return sprite;
        }

        public Sprite[] GetAnimationFrames(string id, string animType)
        {
            if (_animFrameCache == null)
                _animFrameCache = new Dictionary<string, Sprite[]>();

            string key = $"{id}/{animType}";
            if (_animFrameCache.TryGetValue(key, out var cached))
                return cached;

            var frames = LoadAnimationFrames(id, animType);
            _animFrameCache[key] = frames;
            return frames;
        }

        private Sprite[] LoadAnimationFrames(string id, string animType)
        {
            var list = new List<Sprite>();
            for (int i = 1; i <= 100; i++)
            {
                var tex = Resources.Load<Texture2D>($"Chars/{id}/{animType}/frame_{i:D4}");
                if (tex == null) break;
                list.Add(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0f), 100f));
            }
            return list.Count > 0 ? list.ToArray() : null;
        }

        private Sprite LoadBattleSprite(string id)
        {
            var tex = Resources.Load<Texture2D>($"Chars/{id}/sprite");
            if (tex != null)
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0f), 100f);

            var legacyIndividual = Resources.Load<Texture2D>($"Chars/{id}/idle_0");
            if (legacyIndividual != null)
                return Sprite.Create(legacyIndividual, new Rect(0, 0, legacyIndividual.width, legacyIndividual.height), new Vector2(0.5f, 0f), 100f);

            var legacySheet = Resources.Load<Texture2D>($"Chars/{id}");
            if (legacySheet != null)
            {
                int frameW = legacySheet.width / 4;
                int frameH = legacySheet.height / 3;
                return Sprite.Create(legacySheet, new Rect(0, frameH * 2, frameW, frameH), new Vector2(0.5f, 0f), 100f);
            }

            return null;
        }

        public Sprite GetIcon(string iconId)
        {
            if (_iconLookup.TryGetValue(iconId, out var sprite))
                return sprite;

            return PlaceholderGenerator.CreateRect(64, 64, PlaceholderPink, iconId);
        }

        public Sprite GetCharacterSprite(string id)
        {
            if (_characterLookup.TryGetValue(id, out var entry) && entry.idle != null)
                return entry.idle;

            Color color = Color.gray;
            if (entry.placeholderColor != default)
                color = entry.placeholderColor;

            return PlaceholderGenerator.CreateRect(128, 128, color, id);
        }

        public Sprite GetCharacterHitSprite(string id)
        {
            if (_characterLookup.TryGetValue(id, out var entry) && entry.hit != null)
                return entry.hit;

            return GetCharacterSprite(id);
        }

        public Sprite GetEquipmentIcon(SlotType slot, EquipmentGrade grade)
        {
            string key = $"{slot}_{grade}";

            if (_equipIconByKey.TryGetValue(key, out var resourceSprite))
                return resourceSprite;

            if (_equipmentLookup.TryGetValue(key, out var entry) && entry.icon != null)
                return entry.icon;

            Color color = ColorPalette.GetEquipmentGradeColor(grade);
            return PlaceholderGenerator.CreateRect(64, 64, color, slot.ToString().Substring(0, 2));
        }

        public Sprite GetPetIcon(string petId)
        {
            if (_petLookup.TryGetValue(petId, out var entry) && entry.icon != null)
                return entry.icon;

            Color color = new Color(0.4f, 0.7f, 0.9f);
            return PlaceholderGenerator.CreateCircle(32, color);
        }

        public Sprite GetStatusEffectIcon(StatusEffectType type)
        {
            if (_statusEffectLookup.TryGetValue(type, out var entry) && entry.icon != null)
                return entry.icon;

            Color color = GetStatusEffectFallbackColor(type);
            return PlaceholderGenerator.CreateCircle(16, color);
        }

        private Color GetStatusEffectFallbackColor(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.POISON: return new Color(0.5f, 0.0f, 0.8f);
                case StatusEffectType.BURN: return new Color(1.0f, 0.3f, 0.0f);
                case StatusEffectType.REGEN: return ColorPalette.Heal;
                case StatusEffectType.ATK_UP: return new Color(1.0f, 0.5f, 0.5f);
                case StatusEffectType.ATK_DOWN: return new Color(0.5f, 0.2f, 0.2f);
                case StatusEffectType.DEF_UP: return new Color(0.5f, 0.5f, 1.0f);
                case StatusEffectType.DEF_DOWN: return new Color(0.2f, 0.2f, 0.5f);
                case StatusEffectType.CRIT_UP: return ColorPalette.Crit;
                case StatusEffectType.STUN: return new Color(1.0f, 1.0f, 0.0f);
                default: return Color.white;
            }
        }

        private static string StripSpriteModeSuffix(string name)
        {
            int lastUnderscore = name.LastIndexOf('_');
            if (lastUnderscore > 0 && int.TryParse(name.Substring(lastUnderscore + 1), out _))
                return name.Substring(0, lastUnderscore);
            return name;
        }

        public Sprite GetSkillIcon(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return null;

            if (_skillIconByKey.TryGetValue(skillId, out var sprite))
                return sprite;

            string path = $"Icons/skill/icon_skill_{skillId}";

            var loaded = Resources.Load<Sprite>(path);
            if (loaded != null)
            {
                _skillIconByKey[skillId] = loaded;
                return loaded;
            }

            var tex = Resources.Load<Texture2D>(path);
            if (tex != null)
            {
                var created = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
                created.name = skillId;
                _skillIconByKey[skillId] = created;
                return created;
            }

            var placeholder = PlaceholderGenerator.CreateRect(48, 48, ColorPalette.ButtonPrimary, "SK");
            _skillIconByKey[skillId] = placeholder;
            return placeholder;
        }

        public Sprite GetUISprite(string spriteName)
        {
            string resourceKey = spriteName + "Sprite";
            if (_iconLookup.TryGetValue(resourceKey, out var loaded))
                return loaded;

            if (_database == null) return null;

            switch (spriteName)
            {
                case "button": return _database.buttonSprite;
                case "panel": return _database.panelSprite;
                case "frame": return _database.frameSprite;
                case "circle": return _database.circleSprite;
                case "arrow": return _database.arrowSprite;
                case "star": return _database.starSprite;
                case "lock": return _database.lockSprite;
                case "check": return _database.checkSprite;
                default: return null;
            }
        }
    }
}
