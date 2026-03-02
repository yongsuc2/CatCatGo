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
        private Image _icon;
        private TextMeshProUGUI _label;

        public void Setup(StatusEffectType type, int count, int remainingTurns)
        {
            if (_icon == null)
                BuildUI();

            var sprite = Resources.Load<Sprite>($"StatusEffects/{GetResourceName(type)}");
            if (sprite != null)
            {
                _icon.sprite = sprite;
                _icon.color = Color.white;
            }
            else
            {
                bool isBuff = type == StatusEffectType.ATK_UP
                           || type == StatusEffectType.DEF_UP
                           || type == StatusEffectType.CRIT_UP
                           || type == StatusEffectType.REGEN;
                _icon.color = isBuff ? ColorPalette.Heal : ColorPalette.Hp;
            }

            string turnsText = remainingTurns < 999 ? remainingTurns.ToString() : "";
            string countText = count > 1 ? $"x{count}" : "";
            _label.text = countText + (countText.Length > 0 && turnsText.Length > 0 ? "\n" : "") + turnsText;
        }

        private const float ICON_SIZE = 36f;
        private const float LABEL_HEIGHT = 18f;

        private void BuildUI()
        {
            var rt = gameObject.GetComponent<RectTransform>();
            if (rt == null) rt = gameObject.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(ICON_SIZE, ICON_SIZE + LABEL_HEIGHT);

            var le = gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = ICON_SIZE;
            le.preferredHeight = ICON_SIZE + LABEL_HEIGHT;
            le.flexibleHeight = 0;
            le.flexibleWidth = 0;

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0f, 1f);
            iconRt.anchorMax = new Vector2(1f, 1f);
            iconRt.pivot = new Vector2(0.5f, 1f);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(0f, ICON_SIZE);
            _icon = iconGo.AddComponent<Image>();
            _icon.preserveAspect = true;

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0f, 0f);
            textRt.anchorMax = new Vector2(1f, 0f);
            textRt.pivot = new Vector2(0.5f, 0f);
            textRt.anchoredPosition = Vector2.zero;
            textRt.sizeDelta = new Vector2(0f, LABEL_HEIGHT);

            _label = textGo.AddComponent<TextMeshProUGUI>();
            _label.fontSize = 14f;
            _label.color = Color.white;
            _label.alignment = TextAlignmentOptions.Center;
            _label.textWrappingMode = TextWrappingModes.NoWrap;
            _label.overflowMode = TextOverflowModes.Overflow;
        }

        private static string GetResourceName(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.POISON: return "status_poison";
                case StatusEffectType.BURN: return "status_burn";
                case StatusEffectType.REGEN: return "status_regen";
                case StatusEffectType.ATK_UP: return "status_atk_up";
                case StatusEffectType.ATK_DOWN: return "status_atk_down";
                case StatusEffectType.DEF_UP: return "status_def_up";
                case StatusEffectType.DEF_DOWN: return "status_def_down";
                case StatusEffectType.CRIT_UP: return "status_crit_up";
                case StatusEffectType.STUN: return "status_stun";
                default: return "";
            }
        }
    }
}
