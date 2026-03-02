using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Battle
{
    public class DamagePopup : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private Image _iconImage;
        private RectTransform _rectTransform;
        private Coroutine _animCoroutine;

        public System.Action<DamagePopup> OnAnimationComplete;

        private void EnsureComponents()
        {
            if (_rectTransform != null) return;

            _rectTransform = gameObject.GetComponent<RectTransform>();
            if (_rectTransform == null) _rectTransform = gameObject.AddComponent<RectTransform>();

            var layout = gameObject.GetComponent<HorizontalLayoutGroup>();
            if (layout == null)
            {
                layout = gameObject.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 5f;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.childAlignment = TextAnchor.MiddleCenter;
            }

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(transform, false);
            _iconImage = iconGo.AddComponent<Image>();
            _iconImage.preserveAspect = true;
            _iconImage.raycastTarget = false;
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 80f;
            iconLe.preferredHeight = 80f;
            iconGo.SetActive(false);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(transform, false);
            _text = textGo.AddComponent<TextMeshProUGUI>();
            _text.alignment = TextAlignmentOptions.MidlineLeft;
            _text.textWrappingMode = TextWrappingModes.NoWrap;
            _text.overflowMode = TextOverflowModes.Overflow;
            _text.raycastTarget = false;
            var textLe = textGo.AddComponent<LayoutElement>();
            textLe.flexibleWidth = 1f;
        }

        public void Show(int value, bool isHeal, bool isCrit, bool isRage, string skillId, float speed)
        {
            EnsureComponents();

            if (_animCoroutine != null)
                StopCoroutine(_animCoroutine);

            gameObject.SetActive(true);

            Sprite skillSprite = null;
            if (!string.IsNullOrEmpty(skillId) && SpriteManager.Instance != null)
                skillSprite = SpriteManager.Instance.GetSkillIcon(skillId);

            if (skillSprite != null)
            {
                _iconImage.sprite = skillSprite;
                _iconImage.color = Color.white;
                _iconImage.gameObject.SetActive(true);
            }
            else
            {
                _iconImage.gameObject.SetActive(false);
            }

            string prefix = "";
            if (isCrit) prefix = "CRIT! ";
            if (isHeal) prefix = "+";

            _text.text = $"{prefix}{NumberFormatter.FormatInt(value)}";

            if (isHeal)
            {
                _text.color = ColorPalette.Heal;
                _text.fontSize = 60f;
            }
            else if (isCrit)
            {
                _text.color = ColorPalette.Crit;
                _text.fontSize = 70f;
            }
            else if (isRage)
            {
                _text.color = ColorPalette.Rage;
                _text.fontSize = 65f;
            }
            else
            {
                _text.color = Color.white;
                _text.fontSize = 55f;
            }

            float duration = 0.6f / Mathf.Max(speed, 0.1f);
            _animCoroutine = StartCoroutine(AnimatePopup(duration));
        }

        private IEnumerator AnimatePopup(float duration)
        {
            Vector3 startPos = _rectTransform.anchoredPosition;
            Vector3 endPos = startPos + new Vector3(0f, 150f, 0f);
            Color startColor = _text.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
            Color iconStartColor = Color.white;
            Color iconEndColor = new Color(1f, 1f, 1f, 0f);
            bool hasIcon = _iconImage.gameObject.activeSelf;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                _rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
                _text.color = Color.Lerp(startColor, endColor, t);
                if (hasIcon)
                    _iconImage.color = Color.Lerp(iconStartColor, iconEndColor, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _text.color = endColor;
            if (hasIcon) _iconImage.color = iconEndColor;
            _rectTransform.anchoredPosition = endPos;
            gameObject.SetActive(false);
            _animCoroutine = null;
            OnAnimationComplete?.Invoke(this);
        }

        public void ForceStop()
        {
            if (_animCoroutine != null)
            {
                StopCoroutine(_animCoroutine);
                _animCoroutine = null;
            }
            gameObject.SetActive(false);
        }
    }
}
