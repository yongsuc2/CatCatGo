using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Chapter;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Popups
{
    public class OwnedSkillInfo
    {
        public string SkillId;
        public string Name;
    }

    public class SkillSelectPopupData
    {
        public string Title;
        public List<SkillSelectOption> Options;
        public Action<int> OnSelected;
        public List<OwnedSkillInfo> OwnedSkills;
    }

    public class SkillSelectOption
    {
        public string SkillId;
        public string Name;
        public string Description;
    }

    public class SkillSelectPopup : BasePopup
    {
        public override void Show(object data = null)
        {
            base.Show(data);
            BuildUI();
        }

        private void BuildUI()
        {
            var popupData = PopupData as SkillSelectPopupData;
            if (popupData == null) return;

            var dimmer = gameObject.AddComponent<Image>();
            dimmer.color = new Color(0f, 0f, 0f, 0.6f);

            var cardGo = new GameObject("Card");
            cardGo.transform.SetParent(transform, false);
            var cardRt = cardGo.GetComponent<RectTransform>();
            if (cardRt == null) cardRt = cardGo.AddComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.05f, 0.15f);
            cardRt.anchorMax = new Vector2(0.95f, 0.85f);
            cardRt.offsetMin = Vector2.zero;
            cardRt.offsetMax = Vector2.zero;

            var cardBg = cardGo.AddComponent<Image>();
            cardBg.color = ColorPalette.Card;

            var cardLayout = cardGo.AddComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(20, 20, 20, 20);
            cardLayout.spacing = 16f;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(cardGo.transform, false);
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 50f;
            titleLe.flexibleHeight = 0;
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = popupData.Title;
            titleTmp.fontSize = 36f;
            titleTmp.color = ColorPalette.Gold;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            titleTmp.raycastTarget = false;

            for (int i = 0; i < popupData.Options.Count; i++)
            {
                int idx = i;
                var opt = popupData.Options[i];
                BuildSkillCard(cardGo.transform, opt, () =>
                {
                    UIManager.Instance.ClosePopup();
                    popupData.OnSelected?.Invoke(idx);
                });
            }

            if (popupData.OwnedSkills != null && popupData.OwnedSkills.Count > 0)
                BuildOwnedSkillsSection(cardGo.transform, popupData.OwnedSkills);
        }

        private void BuildOwnedSkillsSection(Transform parent, List<OwnedSkillInfo> ownedSkills)
        {
            var dividerGo = new GameObject("Divider");
            dividerGo.transform.SetParent(parent, false);
            var dividerLe = dividerGo.AddComponent<LayoutElement>();
            dividerLe.preferredHeight = 2f;
            dividerLe.flexibleHeight = 0;
            var dividerImg = dividerGo.AddComponent<Image>();
            dividerImg.color = ColorPalette.TextDim;

            var labelGo = new GameObject("OwnedLabel");
            labelGo.transform.SetParent(parent, false);
            var labelLe = labelGo.AddComponent<LayoutElement>();
            labelLe.preferredHeight = 50f;
            labelLe.flexibleHeight = 0;
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = $"\ubcf4\uc720 \uc2a4\ud0ac ({ownedSkills.Count})";
            labelTmp.fontSize = 28f;
            labelTmp.color = ColorPalette.TextDim;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            labelTmp.raycastTarget = false;

            var gridGo = new GameObject("OwnedGrid");
            gridGo.transform.SetParent(parent, false);
            var gridLe = gridGo.AddComponent<LayoutElement>();
            gridLe.preferredHeight = 100f;
            gridLe.flexibleHeight = 0;
            var gridHlg = gridGo.AddComponent<HorizontalLayoutGroup>();
            gridHlg.spacing = 8f;
            gridHlg.childForceExpandWidth = false;
            gridHlg.childForceExpandHeight = false;
            gridHlg.childAlignment = TextAnchor.MiddleLeft;

            foreach (var skill in ownedSkills)
            {
                var iconGo = new GameObject("Icon_" + skill.SkillId);
                iconGo.transform.SetParent(gridGo.transform, false);
                var iconLe = iconGo.AddComponent<LayoutElement>();
                iconLe.preferredWidth = 90f;
                iconLe.preferredHeight = 90f;
                var iconImg = iconGo.AddComponent<Image>();
                if (SpriteManager.Instance != null)
                    iconImg.sprite = SpriteManager.Instance.GetSkillIcon(skill.SkillId);
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
            }
        }

        private void BuildSkillCard(Transform parent, SkillSelectOption opt, Action onClick)
        {
            var go = new GameObject("SkillCard");
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 160f;
            le.flexibleHeight = 0;

            var bg = go.AddComponent<Image>();
            bg.color = ColorPalette.CardLight;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => onClick());

            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(12, 12, 12, 12);
            hlg.spacing = 16f;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            if (!string.IsNullOrEmpty(opt.SkillId))
            {
                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(go.transform, false);
                var iconLe = iconGo.AddComponent<LayoutElement>();
                iconLe.preferredWidth = 130f;
                iconLe.preferredHeight = 130f;
                iconLe.flexibleWidth = 0;
                var iconImg = iconGo.AddComponent<Image>();
                if (SpriteManager.Instance != null)
                    iconImg.sprite = SpriteManager.Instance.GetSkillIcon(opt.SkillId);
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
            }

            var textAreaGo = new GameObject("TextArea");
            textAreaGo.transform.SetParent(go.transform, false);
            var textAreaLe = textAreaGo.AddComponent<LayoutElement>();
            textAreaLe.flexibleWidth = 1f;
            textAreaLe.flexibleHeight = 1f;

            var textLayout = textAreaGo.AddComponent<VerticalLayoutGroup>();
            textLayout.spacing = 6f;
            textLayout.childForceExpandWidth = true;
            textLayout.childForceExpandHeight = false;
            textLayout.childAlignment = TextAnchor.MiddleLeft;

            var nameGo = new GameObject("Name");
            nameGo.transform.SetParent(textAreaGo.transform, false);
            var nameLe = nameGo.AddComponent<LayoutElement>();
            nameLe.preferredHeight = 40f;
            nameLe.flexibleHeight = 0;
            var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
            nameTmp.text = opt.Name;
            nameTmp.fontSize = 32f;
            nameTmp.color = ColorPalette.Text;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.alignment = TextAlignmentOptions.MidlineLeft;
            nameTmp.textWrappingMode = TextWrappingModes.NoWrap;
            nameTmp.overflowMode = TextOverflowModes.Ellipsis;
            nameTmp.raycastTarget = false;

            var descGo = new GameObject("Desc");
            descGo.transform.SetParent(textAreaGo.transform, false);
            var descLe = descGo.AddComponent<LayoutElement>();
            descLe.flexibleHeight = 1f;
            var descTmp = descGo.AddComponent<TextMeshProUGUI>();
            descTmp.text = opt.Description;
            descTmp.fontSize = 26f;
            descTmp.color = ColorPalette.TextDim;
            descTmp.alignment = TextAlignmentOptions.TopLeft;
            descTmp.textWrappingMode = TextWrappingModes.Normal;
            descTmp.overflowMode = TextOverflowModes.Ellipsis;
            descTmp.raycastTarget = false;
        }
    }
}
