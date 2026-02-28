using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Economy;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Popups
{
    public class GachaRewardPopupData
    {
        public List<PullResult> Results;
        public Action OnPullAgain;
    }

    public class GachaRewardPopup : BasePopup
    {
        private Transform _gridContainer;

        public override void Show(object data = null)
        {
            base.Show(data);
            BuildUI();
        }

        private void BuildUI()
        {
            var popupData = PopupData as GachaRewardPopupData;
            if (popupData == null) return;

            var bg = gameObject.AddComponent<Image>();
            bg.color = ColorPalette.Card;

            var mainLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            mainLayout.spacing = 12;
            mainLayout.padding = new RectOffset(20, 20, 20, 20);
            mainLayout.childForceExpandWidth = true;
            mainLayout.childForceExpandHeight = false;
            mainLayout.childAlignment = TextAnchor.UpperCenter;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(transform, false);
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 50f;
            titleLe.flexibleHeight = 0;
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            int count = popupData.Results.Count;
            titleTmp.text = count > 1 ? $"\ubd51\uae30 \uacb0\uacfc ({count}\ud68c)" : "\ubd51\uae30 \uacb0\uacfc";
            titleTmp.fontSize = 36f;
            titleTmp.color = ColorPalette.Gold;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.raycastTarget = false;

            var gridScrollGo = new GameObject("GridScroll");
            gridScrollGo.transform.SetParent(transform, false);
            var gridScrollLe = gridScrollGo.AddComponent<LayoutElement>();
            gridScrollLe.flexibleHeight = 1f;

            var gridScrollRt = gridScrollGo.GetComponent<RectTransform>();
            if (gridScrollRt == null) gridScrollRt = gridScrollGo.AddComponent<RectTransform>();

            var gridScrollImg = gridScrollGo.AddComponent<Image>();
            gridScrollImg.color = ColorPalette.Background;

            var scrollRect = gridScrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(gridScrollGo.transform, false);
            var viewportRt = viewportGo.GetComponent<RectTransform>();
            if (viewportRt == null) viewportRt = viewportGo.AddComponent<RectTransform>();
            UIManager.StretchFull(viewportRt);
            viewportGo.AddComponent<RectMask2D>();

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.offsetMin = new Vector2(0, 0);
            contentRt.offsetMax = new Vector2(0, 0);
            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var gridLayout = contentGo.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(140f, 160f);
            gridLayout.spacing = new Vector2(12f, 12f);
            gridLayout.padding = new RectOffset(12, 12, 12, 12);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 5;
            gridLayout.childAlignment = TextAnchor.UpperCenter;

            scrollRect.viewport = viewportRt;
            scrollRect.content = contentRt;

            _gridContainer = contentGo.transform;
            BuildResultGrid(popupData.Results);

            var btnRow = new GameObject("ButtonRow");
            btnRow.transform.SetParent(transform, false);
            var btnRowLe = btnRow.AddComponent<LayoutElement>();
            btnRowLe.minHeight = UISize.NormalButtonMinHeight;
            btnRowLe.preferredHeight = UISize.NormalButtonHeight;
            btnRowLe.flexibleHeight = 0;
            var btnRowLayout = btnRow.AddComponent<HorizontalLayoutGroup>();
            btnRowLayout.spacing = 16f;
            btnRowLayout.childForceExpandWidth = true;
            btnRowLayout.childForceExpandHeight = true;

            CreateButton(btnRow.transform, "\ub2eb\uae30", ColorPalette.ButtonSecondary, Hide);
            CreateButton(btnRow.transform, "\ud55c\ubc88 \ub354", ColorPalette.ButtonPrimary, () =>
            {
                var pd = PopupData as GachaRewardPopupData;
                Hide();
                pd?.OnPullAgain?.Invoke();
            });
        }

        private void BuildResultGrid(List<PullResult> results)
        {
            foreach (var result in results)
            {
                var cellGo = new GameObject("Cell");
                cellGo.transform.SetParent(_gridContainer, false);

                var cellBg = cellGo.AddComponent<Image>();

                var cellLayout = cellGo.AddComponent<VerticalLayoutGroup>();
                cellLayout.spacing = 4f;
                cellLayout.padding = new RectOffset(4, 4, 6, 6);
                cellLayout.childForceExpandWidth = true;
                cellLayout.childForceExpandHeight = false;
                cellLayout.childAlignment = TextAnchor.UpperCenter;

                if (result.Equipment != null)
                {
                    var eq = result.Equipment;
                    Color gradeColor = ColorPalette.GetEquipmentGradeColor(eq.Grade);
                    cellBg.color = new Color(gradeColor.r * 0.3f, gradeColor.g * 0.3f, gradeColor.b * 0.3f, 1f);

                    var iconGo = new GameObject("Icon");
                    iconGo.transform.SetParent(cellGo.transform, false);
                    var iconLe = iconGo.AddComponent<LayoutElement>();
                    iconLe.preferredWidth = 80f;
                    iconLe.preferredHeight = 80f;
                    var iconImg = iconGo.AddComponent<Image>();
                    iconImg.sprite = SpriteManager.Instance.GetEquipmentIcon(eq.Slot, eq.Grade);
                    iconImg.preserveAspect = true;
                    iconImg.raycastTarget = false;

                    if (eq.IsS)
                    {
                        var sGo = new GameObject("SMark");
                        sGo.transform.SetParent(iconGo.transform, false);
                        var sRt = sGo.AddComponent<RectTransform>();
                        sRt.anchorMin = new Vector2(1, 1);
                        sRt.anchorMax = new Vector2(1, 1);
                        sRt.pivot = new Vector2(1, 1);
                        sRt.sizeDelta = new Vector2(24, 24);
                        sRt.anchoredPosition = new Vector2(4, 4);
                        var sTmp = sGo.AddComponent<TextMeshProUGUI>();
                        sTmp.text = "S";
                        sTmp.fontSize = 20f;
                        sTmp.color = ColorPalette.Gold;
                        sTmp.fontStyle = FontStyles.Bold;
                        sTmp.alignment = TextAlignmentOptions.Center;
                        sTmp.raycastTarget = false;
                    }

                    var nameGo = new GameObject("Name");
                    nameGo.transform.SetParent(cellGo.transform, false);
                    var nameLe = nameGo.AddComponent<LayoutElement>();
                    nameLe.preferredHeight = 28f;
                    var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
                    nameTmp.text = eq.Name;
                    nameTmp.fontSize = 20f;
                    nameTmp.color = gradeColor;
                    nameTmp.fontStyle = FontStyles.Bold;
                    nameTmp.alignment = TextAlignmentOptions.Center;
                    nameTmp.enableWordWrapping = false;
                    nameTmp.overflowMode = TextOverflowModes.Truncate;
                    nameTmp.raycastTarget = false;

                    var gradeGo = new GameObject("Grade");
                    gradeGo.transform.SetParent(cellGo.transform, false);
                    var gradeLe = gradeGo.AddComponent<LayoutElement>();
                    gradeLe.preferredHeight = 22f;
                    var gradeTmp = gradeGo.AddComponent<TextMeshProUGUI>();
                    string pityMark = result.IsPity ? " PITY" : "";
                    gradeTmp.text = $"{eq.Grade}{pityMark}";
                    gradeTmp.fontSize = 18f;
                    gradeTmp.color = ColorPalette.TextDim;
                    gradeTmp.alignment = TextAlignmentOptions.Center;
                    gradeTmp.raycastTarget = false;
                }
                else
                {
                    cellBg.color = ColorPalette.CardLight;

                    var iconGo = new GameObject("Icon");
                    iconGo.transform.SetParent(cellGo.transform, false);
                    var iconLe = iconGo.AddComponent<LayoutElement>();
                    iconLe.preferredWidth = 80f;
                    iconLe.preferredHeight = 80f;
                    var iconImg = iconGo.AddComponent<Image>();
                    iconImg.color = ColorPalette.Gems;
                    iconImg.raycastTarget = false;

                    foreach (var r in result.Resources)
                    {
                        var rGo = new GameObject("Resource");
                        rGo.transform.SetParent(cellGo.transform, false);
                        var rLe = rGo.AddComponent<LayoutElement>();
                        rLe.preferredHeight = 24f;
                        var rTmp = rGo.AddComponent<TextMeshProUGUI>();
                        rTmp.text = $"{NumberFormatter.FormatResourceType(r.Type)}";
                        rTmp.fontSize = 20f;
                        rTmp.color = ColorPalette.Gold;
                        rTmp.alignment = TextAlignmentOptions.Center;
                        rTmp.raycastTarget = false;

                        var amtGo = new GameObject("Amount");
                        amtGo.transform.SetParent(cellGo.transform, false);
                        var amtLe = amtGo.AddComponent<LayoutElement>();
                        amtLe.preferredHeight = 22f;
                        var amtTmp = amtGo.AddComponent<TextMeshProUGUI>();
                        amtTmp.text = $"+{r.Amount}";
                        amtTmp.fontSize = 18f;
                        amtTmp.color = ColorPalette.TextDim;
                        amtTmp.alignment = TextAlignmentOptions.Center;
                        amtTmp.raycastTarget = false;
                    }
                }
            }
        }

        private void CreateButton(Transform parent, string label, Color color, Action onClick)
        {
            var btnGo = new GameObject(label);
            btnGo.transform.SetParent(parent, false);
            var btnBg = btnGo.AddComponent<Image>();
            btnBg.color = color;
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnBg;
            btn.onClick.AddListener(() => onClick());

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 24f;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }
    }
}
