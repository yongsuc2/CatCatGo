using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Components
{
    public class NavBarView : MonoBehaviour
    {
        private struct NavEntry
        {
            public string Label;
            public ScreenType Screen;
        }

        private readonly List<NavEntry> _primaryEntries = new List<NavEntry>
        {
            new NavEntry { Label = "\uD648", Screen = ScreenType.Main },
            new NavEntry { Label = "\uBAA8\uD5D8", Screen = ScreenType.Chapter },
            new NavEntry { Label = "\uC7AC\uB2A5", Screen = ScreenType.Talent },
            new NavEntry { Label = "\uC7A5\uBE44", Screen = ScreenType.Equipment },
            new NavEntry { Label = "\uCEE8\uD150\uCE20", Screen = ScreenType.Content },
        };

        private readonly List<NavEntry> _secondaryEntries = new List<NavEntry>
        {
            new NavEntry { Label = "\uD3AB", Screen = ScreenType.Pet },
            new NavEntry { Label = "\uC0C1\uC810", Screen = ScreenType.Shop },
            new NavEntry { Label = "\uD018\uC2A4\uD2B8", Screen = ScreenType.Quest },
            new NavEntry { Label = "7\uC77C \uCD9C\uC11D\uCCB4\uD06C", Screen = ScreenType.Event },
        };

        private ScreenType _activeScreen = ScreenType.Main;
        private Dictionary<ScreenType, Image> _tabBackgrounds = new Dictionary<ScreenType, Image>();
        private Dictionary<ScreenType, TextMeshProUGUI> _tabTexts = new Dictionary<ScreenType, TextMeshProUGUI>();
        private GameObject _secondaryPanel;
        private bool _secondaryVisible;

        public void Initialize()
        {
            var bg = gameObject.AddComponent<Image>();
            bg.color = ColorPalette.NavBarBackground;

            var mainLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            mainLayout.spacing = 0;
            mainLayout.childForceExpandWidth = true;
            mainLayout.childForceExpandHeight = false;
            mainLayout.childAlignment = TextAnchor.LowerCenter;
            mainLayout.reverseArrangement = true;

            var primaryRow = CreateNavRow("PrimaryRow");
            foreach (var entry in _primaryEntries)
            {
                CreateNavButton(primaryRow.transform, entry);
            }
            CreateMoreButton(primaryRow.transform);

            _secondaryPanel = CreateNavRow("SecondaryRow");
            foreach (var entry in _secondaryEntries)
            {
                CreateNavButton(_secondaryPanel.transform, entry);
            }

            var primaryLe = primaryRow.AddComponent<LayoutElement>();
            primaryLe.preferredHeight = 60;

            var secondaryLe = _secondaryPanel.AddComponent<LayoutElement>();
            secondaryLe.preferredHeight = 60;

            _secondaryPanel.SetActive(false);
            _secondaryVisible = false;
        }

        public void SetActiveScreen(ScreenType screen)
        {
            _activeScreen = screen;
            UpdateHighlights();
        }

        public void Refresh()
        {
            UpdateHighlights();
        }

        private GameObject CreateNavRow(string name)
        {
            var rowGo = new GameObject(name);
            rowGo.transform.SetParent(transform, false);
            var rowRt = rowGo.GetComponent<RectTransform>();

            if (rowRt == null) rowRt = rowGo.AddComponent<RectTransform>();

            var layout = rowGo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 2;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(4, 4, 2, 2);

            return rowGo;
        }

        private void CreateNavButton(Transform parent, NavEntry entry)
        {
            var btnGo = new GameObject("Nav_" + entry.Label);
            btnGo.transform.SetParent(parent, false);

            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = ColorPalette.ButtonSecondary;
            _tabBackgrounds[entry.Screen] = btnImage;

            var button = btnGo.AddComponent<Button>();
            button.targetGraphic = btnImage;
            var screen = entry.Screen;
            button.onClick.AddListener(() => OnNavButtonClicked(screen));

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(btnGo.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = entry.Label;
            text.fontSize = 30;
            text.color = ColorPalette.NavBarInactive;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            _tabTexts[entry.Screen] = text;
        }

        private void CreateMoreButton(Transform parent)
        {
            var btnGo = new GameObject("Nav_More");
            btnGo.transform.SetParent(parent, false);

            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = ColorPalette.ButtonSecondary;

            var button = btnGo.AddComponent<Button>();
            button.targetGraphic = btnImage;
            button.onClick.AddListener(ToggleSecondaryPanel);

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(btnGo.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = "\u2022\u2022\u2022";
            text.fontSize = 30;
            text.color = ColorPalette.NavBarInactive;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
        }

        private void ToggleSecondaryPanel()
        {
            _secondaryVisible = !_secondaryVisible;
            _secondaryPanel.SetActive(_secondaryVisible);
        }

        private void OnNavButtonClicked(ScreenType screen)
        {
            UIManager.Instance.ShowScreen(screen);
        }

        private void UpdateHighlights()
        {
            foreach (var kvp in _tabBackgrounds)
            {
                bool active = kvp.Key == _activeScreen;
                kvp.Value.color = active ? new Color(ColorPalette.NavBarActive.r, ColorPalette.NavBarActive.g, ColorPalette.NavBarActive.b, 0.5f) : ColorPalette.ButtonSecondary;
            }

            foreach (var kvp in _tabTexts)
            {
                bool active = kvp.Key == _activeScreen;
                kvp.Value.color = active ? ColorPalette.NavBarActive : ColorPalette.NavBarInactive;
            }
        }
    }
}
