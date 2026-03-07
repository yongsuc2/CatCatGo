using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Components
{
    public class ProgressBarView : MonoBehaviour
    {
        private Image _backgroundImage;
        private Image _fillImage;
        private TextMeshProUGUI _labelText;
        private RectTransform _fillRect;

        private float _currentValue;
        private float _maxValue = 1f;

        public void Initialize(float width = 0, float height = 0)
        {
            var rt = GetOrAddRectTransform(gameObject);

            var backgroundGo = new GameObject("Background");
            backgroundGo.transform.SetParent(transform, false);
            _backgroundImage = backgroundGo.AddComponent<Image>();
            _backgroundImage.color = ColorPalette.ProgressBarBackground;
            var bgRt = backgroundGo.GetComponent<RectTransform>();
            StretchFull(bgRt);

            var fillAreaGo = new GameObject("FillArea");
            fillAreaGo.transform.SetParent(transform, false);
            var fillAreaRt = GetOrAddRectTransform(fillAreaGo);
            StretchFull(fillAreaRt);
            fillAreaRt.offsetMin = new Vector2(2, 2);
            fillAreaRt.offsetMax = new Vector2(-2, -2);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            _fillImage = fillGo.AddComponent<Image>();
            _fillImage.color = ColorPalette.ProgressBarFill;
            _fillRect = fillGo.GetComponent<RectTransform>();
            _fillRect.anchorMin = Vector2.zero;
            _fillRect.anchorMax = new Vector2(0, 1);
            _fillRect.offsetMin = Vector2.zero;
            _fillRect.offsetMax = Vector2.zero;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(transform, false);
            _labelText = labelGo.AddComponent<TextMeshProUGUI>();
            _labelText.fontSize = Mathf.Max(height * 0.6f, 12f);
            _labelText.color = ColorPalette.Text;
            _labelText.alignment = TextAlignmentOptions.Center;
            _labelText.raycastTarget = false;
            var labelRt = labelGo.GetComponent<RectTransform>();
            StretchFull(labelRt);
        }

        public void SetProgress(float current, float max, string label = null)
        {
            _currentValue = current;
            _maxValue = Mathf.Max(max, 0.001f);

            float ratio = Mathf.Clamp01(current / _maxValue);
            _fillRect.anchorMax = new Vector2(ratio, 1);

            if (label != null)
                _labelText.text = label;
            else
                _labelText.text = $"{NumberFormatter.Format(current)} / {NumberFormatter.Format(max)}";
        }

        public void SetColor(Color fillColor)
        {
            _fillImage.color = fillColor;
        }

        private RectTransform GetOrAddRectTransform(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) rt = go.AddComponent<RectTransform>();
            return rt;
        }

        private void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
