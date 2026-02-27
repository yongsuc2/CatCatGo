using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Components
{
    public class TabBarView : MonoBehaviour
    {
        public event Action<int> OnTabSelected;

        private string[] _labels;
        private int _activeIndex;
        private GameObject[] _tabButtons;
        private Image[] _tabBackgrounds;
        private TextMeshProUGUI[] _tabTexts;

        public void Initialize(string[] labels, int defaultIndex = 0)
        {
            _labels = labels;
            _activeIndex = defaultIndex;
            _tabButtons = new GameObject[labels.Length];
            _tabBackgrounds = new Image[labels.Length];
            _tabTexts = new TextMeshProUGUI[labels.Length];

            var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.padding = new RectOffset(4, 4, 2, 2);

            for (int i = 0; i < labels.Length; i++)
            {
                int index = i;
                var tabGo = new GameObject("Tab_" + labels[i]);
                tabGo.transform.SetParent(transform, false);

                var bg = tabGo.AddComponent<Image>();
                bg.color = (i == defaultIndex) ? ColorPalette.ButtonPrimary : ColorPalette.ButtonSecondary;
                _tabBackgrounds[i] = bg;

                var button = tabGo.AddComponent<Button>();
                button.targetGraphic = bg;
                button.onClick.AddListener(() => SelectTab(index));

                var textGo = new GameObject("Text");
                textGo.transform.SetParent(tabGo.transform, false);
                var text = textGo.AddComponent<TextMeshProUGUI>();
                text.text = labels[i];
                text.fontSize = 24;
                text.color = (i == defaultIndex) ? Color.white : ColorPalette.TextDim;
                text.alignment = TextAlignmentOptions.Center;
                text.raycastTarget = false;
                var textRt = textGo.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;
                _tabTexts[i] = text;

                _tabButtons[i] = tabGo;
            }
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= _labels.Length) return;

            _activeIndex = index;

            for (int i = 0; i < _labels.Length; i++)
            {
                bool active = (i == index);
                _tabBackgrounds[i].color = active ? ColorPalette.ButtonPrimary : ColorPalette.ButtonSecondary;
                _tabTexts[i].color = active ? Color.white : ColorPalette.TextDim;
            }

            OnTabSelected?.Invoke(index);
        }

        public int ActiveIndex => _activeIndex;
    }
}
