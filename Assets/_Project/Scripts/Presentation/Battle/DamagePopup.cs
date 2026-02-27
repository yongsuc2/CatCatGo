using System.Collections;
using UnityEngine;
using TMPro;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Battle
{
    public class DamagePopup : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private RectTransform _rectTransform;
        private Coroutine _animCoroutine;

        public System.Action<DamagePopup> OnAnimationComplete;

        private void EnsureComponents()
        {
            if (_rectTransform != null) return;

            _rectTransform = gameObject.GetComponent<RectTransform>();
            if (_rectTransform == null)
                _rectTransform = gameObject.AddComponent<RectTransform>();

            _text = gameObject.GetComponent<TextMeshProUGUI>();
            if (_text == null)
            {
                _text = gameObject.AddComponent<TextMeshProUGUI>();
                _text.alignment = TextAlignmentOptions.Center;
                _text.enableWordWrapping = false;
                _text.overflowMode = TextOverflowModes.Overflow;
                _text.raycastTarget = false;
            }
        }

        public void Show(int value, bool isHeal, bool isCrit, bool isRage, string skillIcon, float speed)
        {
            EnsureComponents();

            if (_animCoroutine != null)
                StopCoroutine(_animCoroutine);

            gameObject.SetActive(true);

            string prefix = "";
            if (isCrit) prefix = "CRIT! ";
            if (isHeal) prefix = "+";
            string icon = string.IsNullOrEmpty(skillIcon) ? "" : skillIcon + " ";

            _text.text = $"{icon}{prefix}{NumberFormatter.FormatInt(value)}";

            if (isHeal)
            {
                _text.color = ColorPalette.Heal;
                _text.fontSize = 24f;
            }
            else if (isCrit)
            {
                _text.color = ColorPalette.Crit;
                _text.fontSize = 28f;
            }
            else if (isRage)
            {
                _text.color = ColorPalette.Rage;
                _text.fontSize = 26f;
            }
            else
            {
                _text.color = Color.white;
                _text.fontSize = 22f;
            }

            float duration = 0.6f / Mathf.Max(speed, 0.1f);
            _animCoroutine = StartCoroutine(AnimatePopup(duration));
        }

        private IEnumerator AnimatePopup(float duration)
        {
            Vector3 startPos = _rectTransform.anchoredPosition;
            Vector3 endPos = startPos + new Vector3(0f, 60f, 0f);
            Color startColor = _text.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                _rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
                _text.color = Color.Lerp(startColor, endColor, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _text.color = endColor;
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
