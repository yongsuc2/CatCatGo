using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Enums;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Battle
{
    public class StatusEffectIconView : MonoBehaviour
    {
        private Image _background;
        private TextMeshProUGUI _label;

        public void Setup(StatusEffectType type, int count, int remainingTurns)
        {
            if (_background == null)
                BuildUI();

            bool isBuff = type == StatusEffectType.ATK_UP
                       || type == StatusEffectType.DEF_UP
                       || type == StatusEffectType.CRIT_UP
                       || type == StatusEffectType.REGEN;

            _background.color = isBuff ? ColorPalette.Heal : ColorPalette.Hp;

            string abbrev = GetAbbreviation(type);
            string turnsText = remainingTurns < 999 ? remainingTurns.ToString() : "";
            string countText = count > 1 ? $"x{count}" : "";
            _label.text = $"{abbrev}{countText}\n{turnsText}";
        }

        private void BuildUI()
        {
            var rt = gameObject.GetComponent<RectTransform>();

            if (rt == null) rt = gameObject.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(90f, 90f);

            _background = gameObject.AddComponent<Image>();
            _background.sprite = PlaceholderGenerator.CreateCircle(10, Color.white);
            _background.type = Image.Type.Simple;

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(transform, false);
            var textRt = textGo.GetComponent<RectTransform>();

            if (textRt == null) textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            _label = textGo.AddComponent<TextMeshProUGUI>();
            _label.fontSize = 55f;
            _label.color = Color.white;
            _label.alignment = TextAlignmentOptions.Center;
            _label.enableWordWrapping = false;
            _label.overflowMode = TextOverflowModes.Overflow;
        }

        private static string GetAbbreviation(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.POISON: return "P";
                case StatusEffectType.BURN: return "B";
                case StatusEffectType.REGEN: return "R";
                case StatusEffectType.ATK_UP: return "A+";
                case StatusEffectType.ATK_DOWN: return "A-";
                case StatusEffectType.DEF_UP: return "D+";
                case StatusEffectType.DEF_DOWN: return "D-";
                case StatusEffectType.CRIT_UP: return "C+";
                case StatusEffectType.STUN: return "S";
                default: return "?";
            }
        }
    }
}
