using UnityEngine;
using CatCatGo.Domain.Enums;

namespace CatCatGo.Presentation.Core
{
    [System.Serializable]
    public struct CharacterSpriteEntry
    {
        public string id;
        public Sprite idle;
        public Sprite hit;
        public Color placeholderColor;
        public string placeholderLabel;
    }

    [System.Serializable]
    public struct EquipmentSpriteEntry
    {
        public SlotType slot;
        public EquipmentGrade grade;
        public Sprite icon;
    }

    [System.Serializable]
    public struct PetSpriteEntry
    {
        public string petId;
        public PetTier tier;
        public Sprite icon;
    }

    [System.Serializable]
    public struct StatusEffectSpriteEntry
    {
        public StatusEffectType type;
        public Sprite icon;
        public Color color;
    }

    [System.Serializable]
    public struct IconSpriteEntry
    {
        public string id;
        public Sprite sprite;
    }

    [CreateAssetMenu(fileName = "SpriteDatabase", menuName = "CatCatGo/SpriteDatabase")]
    public class SpriteDatabase : ScriptableObject
    {
        public CharacterSpriteEntry[] characters;
        public EquipmentSpriteEntry[] equipmentIcons;
        public PetSpriteEntry[] petIcons;
        public StatusEffectSpriteEntry[] statusEffects;
        public IconSpriteEntry[] icons;

        public Sprite buttonSprite;
        public Sprite panelSprite;
        public Sprite frameSprite;
        public Sprite circleSprite;
        public Sprite arrowSprite;
        public Sprite starSprite;
        public Sprite lockSprite;
        public Sprite checkSprite;
    }
}
