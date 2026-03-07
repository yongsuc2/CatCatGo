using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CatCatGo.Presentation.Components
{
    public class ToastView : MonoBehaviour
    {
        private RectTransform _rt;
        private TextMeshProUGUI _text;
        private CanvasGroup _canvasGroup;
        private Coroutine _activeCoroutine;

        private const float DisplayDuration = 2.5f;
        private const float FadeDuration = 0.5f;

        public void Initialize()
        {
            _rt = GetComponent<RectTransform>();
            if (_rt == null) _rt = gameObject.AddComponent<RectTransform>();
            _rt.anchorMin = new Vector2(0.1f, 0.85f);
            _rt.anchorMax = new Vector2(0.9f, 0.92f);
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;

            gameObject.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.92f);
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.blocksRaycasts = false;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(20, 8);
            textRt.offsetMax = new Vector2(-20, -8);
            _text = textGo.AddComponent<TextMeshProUGUI>();
            _text.fontSize = 26f;
            _text.color = Color.white;
            _text.alignment = TextAlignmentOptions.Center;
            _text.textWrappingMode = TextWrappingModes.Normal;
            _text.raycastTarget = false;

            gameObject.SetActive(false);
        }

        public void Show(string message)
        {
            if (_activeCoroutine != null)
                StopCoroutine(_activeCoroutine);
            _activeCoroutine = StartCoroutine(ShowCoroutine(message));
        }

        private IEnumerator ShowCoroutine(string message)
        {
            _text.text = message;
            _canvasGroup.alpha = 1f;
            gameObject.SetActive(true);

            yield return new WaitForSeconds(DisplayDuration);

            float elapsed = 0f;
            while (elapsed < FadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = 1f - (elapsed / FadeDuration);
                yield return null;
            }

            gameObject.SetActive(false);
            _activeCoroutine = null;
        }
    }
}
