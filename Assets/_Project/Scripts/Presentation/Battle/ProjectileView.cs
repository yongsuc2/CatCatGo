using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using CatCatGo.Presentation.Core;

namespace CatCatGo.Presentation.Battle
{
    public class ProjectileView : MonoBehaviour
    {
        private Image _image;
        private RectTransform _rectTransform;

        private void EnsureComponents()
        {
            if (_rectTransform != null) return;

            _rectTransform = gameObject.GetComponent<RectTransform>();
            if (_rectTransform == null)
                _rectTransform = (gameObject.GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>());
            _rectTransform.sizeDelta = new Vector2(16f, 16f);

            _image = gameObject.GetComponent<Image>();
            if (_image == null)
            {
                _image = gameObject.AddComponent<Image>();
                _image.sprite = PlaceholderGenerator.CreateCircle(8, Color.white);
                _image.raycastTarget = false;
            }
        }

        public void Launch(Vector3 from, Vector3 to, float duration, Color color, Action onComplete)
        {
            EnsureComponents();
            _image.color = color;
            gameObject.SetActive(true);
            StartCoroutine(AnimateFlight(from, to, duration, onComplete));
        }

        private IEnumerator AnimateFlight(Vector3 from, Vector3 to, float duration, Action onComplete)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                _rectTransform.anchoredPosition = Vector3.Lerp(from, to, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _rectTransform.anchoredPosition = to;
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }

        public void ForceStop()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
    }
}
