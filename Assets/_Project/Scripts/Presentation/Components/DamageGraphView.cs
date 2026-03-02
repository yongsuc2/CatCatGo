using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Components
{
    public class DamageGraphView : MonoBehaviour
    {
        private RectTransform _root;
        private RectTransform _barsContainer;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _totalText;
        private readonly List<GameObject> _barObjects = new List<GameObject>();
        private Color _barColor = ColorPalette.Hp;

        public void Initialize(string title = null, Color? barColor = null)
        {
            if (_root != null) return;

            _barColor = barColor ?? ColorPalette.Hp;

            _root = gameObject.GetComponent<RectTransform>();
            if (_root == null)
                _root = gameObject.GetComponent<RectTransform>();

                if (_root == null) _root = gameObject.AddComponent<RectTransform>();

            var bg = gameObject.AddComponent<Image>();
            bg.color = ColorPalette.Card;

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.spacing = 4f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var rootFitter = gameObject.AddComponent<ContentSizeFitter>();
            rootFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            if (!string.IsNullOrEmpty(title))
            {
                var headerGo = new GameObject("Header");
                headerGo.transform.SetParent(transform, false);
                headerGo.AddComponent<RectTransform>();
                var headerLe = headerGo.AddComponent<LayoutElement>();
                headerLe.preferredHeight = 30f;
                var headerLayout = headerGo.AddComponent<HorizontalLayoutGroup>();
                headerLayout.childForceExpandWidth = false;
                headerLayout.childForceExpandHeight = true;
                headerLayout.childAlignment = TextAnchor.MiddleLeft;

                var titleGo = new GameObject("Title");
                titleGo.transform.SetParent(headerGo.transform, false);
                titleGo.AddComponent<RectTransform>();
                var titleLe = titleGo.AddComponent<LayoutElement>();
                titleLe.flexibleWidth = 1f;
                _titleText = titleGo.AddComponent<TextMeshProUGUI>();
                _titleText.text = title;
                _titleText.fontSize = 22f;
                _titleText.color = ColorPalette.Text;
                _titleText.alignment = TextAlignmentOptions.MidlineLeft;
                _titleText.textWrappingMode = TextWrappingModes.NoWrap;
                _titleText.raycastTarget = false;

                var totalGo = new GameObject("Total");
                totalGo.transform.SetParent(headerGo.transform, false);
                totalGo.AddComponent<RectTransform>();
                var totalLe = totalGo.AddComponent<LayoutElement>();
                totalLe.preferredWidth = 120f;
                _totalText = totalGo.AddComponent<TextMeshProUGUI>();
                _totalText.text = "";
                _totalText.fontSize = 20f;
                _totalText.color = ColorPalette.TextDim;
                _totalText.alignment = TextAlignmentOptions.MidlineRight;
                _totalText.textWrappingMode = TextWrappingModes.NoWrap;
                _totalText.raycastTarget = false;
            }

            var containerGo = new GameObject("Bars");
            containerGo.transform.SetParent(transform, false);
            _barsContainer = containerGo.GetComponent<RectTransform>();

            if (_barsContainer == null) _barsContainer = containerGo.AddComponent<RectTransform>();
            var containerLayout = containerGo.AddComponent<VerticalLayoutGroup>();
            containerLayout.spacing = 3f;
            containerLayout.childForceExpandWidth = true;
            containerLayout.childForceExpandHeight = false;
            var containerFitter = containerGo.AddComponent<ContentSizeFitter>();
            containerFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public void SetData(Dictionary<string, int> damageMap)
        {
            if (_root == null)
                Initialize();

            ClearBars();

            if (damageMap == null || damageMap.Count == 0)
            {
                if (_totalText != null) _totalText.text = "";
                return;
            }

            var sorted = damageMap.OrderByDescending(kvp => kvp.Value).ToList();
            int maxValue = sorted[0].Value;
            int totalValue = sorted.Sum(kvp => kvp.Value);
            if (maxValue <= 0) return;

            if (_totalText != null)
                _totalText.text = $"\ucd1d {NumberFormatter.FormatInt(totalValue)}";

            foreach (var kvp in sorted)
            {
                float sharePct = totalValue > 0 ? (float)kvp.Value / totalValue * 100f : 0f;
                var barGo = CreateBarEntry(kvp.Key, kvp.Value, maxValue, sharePct);
                barGo.transform.SetParent(_barsContainer, false);
                _barObjects.Add(barGo);
            }
        }

        private GameObject CreateBarEntry(string label, int value, int maxValue, float sharePct)
        {
            var rowGo = new GameObject($"Bar_{label}");
            var rowRt = rowGo.GetComponent<RectTransform>();

            if (rowRt == null) rowRt = rowGo.AddComponent<RectTransform>();
            var rowLe = rowGo.AddComponent<LayoutElement>();
            rowLe.preferredHeight = 32f;

            var rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 4f;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(rowGo.transform, false);
            labelGo.AddComponent<RectTransform>();
            var labelLe = labelGo.AddComponent<LayoutElement>();
            labelLe.preferredWidth = 160f;
            labelLe.minWidth = 100f;
            labelLe.preferredHeight = 32f;
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.fontSize = 24f;
            labelTmp.fontStyle = FontStyles.Bold;
            labelTmp.color = ColorPalette.Text;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            labelTmp.textWrappingMode = TextWrappingModes.NoWrap;
            labelTmp.overflowMode = TextOverflowModes.Ellipsis;
            labelTmp.raycastTarget = false;

            var barWrapper = new GameObject("BarWrapper");
            barWrapper.transform.SetParent(rowGo.transform, false);
            barWrapper.AddComponent<RectTransform>();
            var wrapperLe = barWrapper.AddComponent<LayoutElement>();
            wrapperLe.flexibleWidth = 1f;
            wrapperLe.preferredHeight = 16f;

            var barBg = new GameObject("BarBg");
            barBg.transform.SetParent(barWrapper.transform, false);
            var barBgRt = barBg.GetComponent<RectTransform>();

            if (barBgRt == null) barBgRt = barBg.AddComponent<RectTransform>();
            barBgRt.anchorMin = Vector2.zero;
            barBgRt.anchorMax = Vector2.one;
            barBgRt.offsetMin = Vector2.zero;
            barBgRt.offsetMax = Vector2.zero;
            var barBgImg = barBg.AddComponent<Image>();
            barBgImg.color = ColorPalette.ProgressBarBackground;

            float ratio = maxValue > 0 ? Mathf.Clamp01((float)value / maxValue) : 0f;

            var barFill = new GameObject("BarFill");
            barFill.transform.SetParent(barWrapper.transform, false);
            var barFillRt = barFill.GetComponent<RectTransform>();

            if (barFillRt == null) barFillRt = barFill.AddComponent<RectTransform>();
            barFillRt.anchorMin = Vector2.zero;
            barFillRt.anchorMax = new Vector2(ratio, 1f);
            barFillRt.offsetMin = Vector2.zero;
            barFillRt.offsetMax = Vector2.zero;
            var barFillImg = barFill.AddComponent<Image>();
            barFillImg.color = _barColor;

            var valueGo = new GameObject("Value");
            valueGo.transform.SetParent(rowGo.transform, false);
            valueGo.AddComponent<RectTransform>();
            var valueLe = valueGo.AddComponent<LayoutElement>();
            valueLe.preferredWidth = 110f;
            valueLe.preferredHeight = 32f;
            var valueTmp = valueGo.AddComponent<TextMeshProUGUI>();
            valueTmp.text = $"{NumberFormatter.FormatInt(value)} <color=#{ColorUtility.ToHtmlStringRGB(ColorPalette.TextDim)}>{sharePct:F1}%</color>";
            valueTmp.fontSize = 22f;
            valueTmp.color = ColorPalette.Text;
            valueTmp.alignment = TextAlignmentOptions.MidlineRight;
            valueTmp.textWrappingMode = TextWrappingModes.NoWrap;
            valueTmp.richText = true;
            valueTmp.raycastTarget = false;

            return rowGo;
        }

        private void ClearBars()
        {
            foreach (var obj in _barObjects)
            {
                if (obj != null)
                    Destroy(obj);
            }
            _barObjects.Clear();
        }
    }
}
