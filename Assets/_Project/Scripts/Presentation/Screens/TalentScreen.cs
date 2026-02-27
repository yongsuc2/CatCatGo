using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Services;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Data;
using CatCatGo.Domain.Entities;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Screens
{
    public class TalentScreen : BaseScreen
    {
        private const int VIEW_RANGE = 15;

        private static readonly Dictionary<StatType, string> STAT_ICON_ID = new Dictionary<StatType, string>
        {
            { StatType.ATK, "icon_atk" },
            { StatType.HP, "icon_hp" },
            { StatType.DEF, "icon_def" },
        };

        private static readonly Dictionary<StatType, string> STAT_NAME = new Dictionary<StatType, string>
        {
            { StatType.ATK, "\uacf5\uaca9\ub825" },
            { StatType.HP, "\uccb4\ub825" },
            { StatType.DEF, "\ubc29\uc5b4\ub825" },
        };

        private static readonly Dictionary<string, string> BONUS_STAT_LABEL = new Dictionary<string, string>
        {
            { "ATK", "\uacf5\uaca9\ub825" },
            { "DEF", "\ubc29\uc5b4\ub825" },
        };

        private static readonly Dictionary<HeritageRoute, string> ROUTE_LABEL = new Dictionary<HeritageRoute, string>
        {
            { HeritageRoute.SKULL, "\ud574\uace8" },
            { HeritageRoute.KNIGHT, "\uae30\uc0ac" },
            { HeritageRoute.RANGER, "\ub808\uc778\uc800" },
            { HeritageRoute.GHOST, "\uc720\ub839" },
        };

        private TextMeshProUGUI _gradeText;
        private TextMeshProUGUI _subGradeProgressText;
        private TextMeshProUGUI _totalLevelText;
        private TextMeshProUGUI _goldMultiplierText;
        private GameObject _goldMultiplierRow;
        private Slider _subGradeSlider;

        private Slider _rewardProgressSlider;
        private RectTransform _nodeContainer;
        private TextMeshProUGUI _lvStartText;
        private TextMeshProUGUI _lvEndText;
        private TextMeshProUGUI _unclaimedText;
        private Button _claimAllButton;
        private List<GameObject> _nodeObjects = new List<GameObject>();

        private GameObject _rewardPopup;
        private Image _rewardPopupIconImage;
        private TextMeshProUGUI _rewardPopupTitle;
        private TextMeshProUGUI _rewardPopupDesc;

        private Dictionary<StatType, TextMeshProUGUI> _statLevelTexts = new Dictionary<StatType, TextMeshProUGUI>();
        private Dictionary<StatType, TextMeshProUGUI> _statBonusTexts = new Dictionary<StatType, TextMeshProUGUI>();
        private Dictionary<StatType, TextMeshProUGUI> _statCostTexts = new Dictionary<StatType, TextMeshProUGUI>();
        private Dictionary<StatType, Button> _statButtons = new Dictionary<StatType, Button>();

        private GameObject _heritageCard;
        private TextMeshProUGUI _heritageRouteText;
        private TextMeshProUGUI _heritageLevelText;

        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            var scrollGo = new GameObject("ScrollView");
            scrollGo.transform.SetParent(transform, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();

            if (scrollRt == null) scrollRt = scrollGo.AddComponent<RectTransform>();
            UIManager.StretchFull(scrollRt);

            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRt = viewportGo.GetComponent<RectTransform>();

            if (viewportRt == null) viewportRt = viewportGo.AddComponent<RectTransform>();
            UIManager.StretchFull(viewportRt);
            viewportGo.AddComponent<RectMask2D>();

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = contentGo.GetComponent<RectTransform>();

            if (contentRt == null) contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.offsetMin = Vector2.zero;
            contentRt.offsetMax = Vector2.zero;

            var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 12;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.padding = new RectOffset(16, 16, 16, 16);

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;
            scrollRect.viewport = viewportRt;

            BuildHeader(contentGo.transform);
            BuildGradeInfoCard(contentGo.transform);
            BuildGradeRewardCard(contentGo.transform);
            BuildUpgradeSection(contentGo.transform);
            BuildHeritageCard(contentGo.transform);
            BuildRewardPopup();
        }

        private void BuildHeader(Transform parent)
        {
            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(parent, false);
            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 40;

            var headerText = headerGo.AddComponent<TextMeshProUGUI>();
            headerText.text = "\uc7ac\ub2a5";
            headerText.fontSize = 30f;
            headerText.color = ColorPalette.Text;
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.raycastTarget = false;
        }

        private void BuildGradeInfoCard(Transform parent)
        {
            var cardGo = new GameObject("GradeInfoCard");
            cardGo.transform.SetParent(parent, false);
            cardGo.AddComponent<Image>().color = ColorPalette.Card;

            var cardLayout = cardGo.AddComponent<VerticalLayoutGroup>();
            cardLayout.spacing = 6;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;
            cardLayout.childAlignment = TextAnchor.MiddleCenter;
            cardLayout.padding = new RectOffset(16, 16, 12, 12);

            var cardFitter = cardGo.AddComponent<ContentSizeFitter>();
            cardFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _gradeText = CreateInfoRow(cardGo.transform, "GradeRow");
            _subGradeProgressText = CreateInfoRow(cardGo.transform, "SubGradeRow");
            _totalLevelText = CreateInfoRow(cardGo.transform, "TotalLevelRow");

            _goldMultiplierRow = new GameObject("GoldMultiplierRow");
            _goldMultiplierRow.transform.SetParent(cardGo.transform, false);
            var goldLe = _goldMultiplierRow.AddComponent<LayoutElement>();
            goldLe.preferredHeight = 28f;
            _goldMultiplierText = _goldMultiplierRow.AddComponent<TextMeshProUGUI>();
            _goldMultiplierText.fontSize = 22f;
            _goldMultiplierText.color = ColorPalette.Gold;
            _goldMultiplierText.alignment = TextAlignmentOptions.Left;
            _goldMultiplierText.raycastTarget = false;

            var barGo = new GameObject("SubGradeBar");
            barGo.transform.SetParent(cardGo.transform, false);
            var barLe = barGo.AddComponent<LayoutElement>();
            barLe.preferredHeight = 28f;

            _subGradeSlider = UIManager.CreateSlider(barGo.transform, "BarSlider");
            _subGradeSlider.interactable = false;
            var sliderRt = _subGradeSlider.GetComponent<RectTransform>();
            UIManager.StretchFull(sliderRt);
        }

        private TextMeshProUGUI CreateInfoRow(Transform parent, string name)
        {
            var rowGo = new GameObject(name);
            rowGo.transform.SetParent(parent, false);
            var rowLe = rowGo.AddComponent<LayoutElement>();
            rowLe.preferredHeight = 28f;
            var tmp = rowGo.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 22f;
            tmp.color = ColorPalette.Text;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.raycastTarget = false;
            return tmp;
        }

        private void BuildGradeRewardCard(Transform parent)
        {
            var cardGo = new GameObject("GradeRewardCard");
            cardGo.transform.SetParent(parent, false);
            cardGo.AddComponent<Image>().color = ColorPalette.Card;

            var cardLayout = cardGo.AddComponent<VerticalLayoutGroup>();
            cardLayout.spacing = 8;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;
            cardLayout.padding = new RectOffset(12, 12, 12, 12);

            var cardFitter = cardGo.AddComponent<ContentSizeFitter>();
            cardFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var titleGo = new GameObject("RewardTitle");
            titleGo.transform.SetParent(cardGo.transform, false);
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 28f;
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "\ub4f1\uae09 \ubcf4\uc0c1";
            titleText.fontSize = 24f;
            titleText.color = ColorPalette.Text;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.raycastTarget = false;

            var nodeAreaGo = new GameObject("NodeArea");
            nodeAreaGo.transform.SetParent(cardGo.transform, false);
            var nodeAreaLe = nodeAreaGo.AddComponent<LayoutElement>();
            nodeAreaLe.preferredHeight = 80;
            _nodeContainer = nodeAreaGo.GetComponent<RectTransform>();

            if (_nodeContainer == null) _nodeContainer = nodeAreaGo.AddComponent<RectTransform>();

            var nodeLayout = nodeAreaGo.AddComponent<HorizontalLayoutGroup>();
            nodeLayout.spacing = 4;
            nodeLayout.childForceExpandWidth = true;
            nodeLayout.childForceExpandHeight = true;
            nodeLayout.childAlignment = TextAnchor.MiddleCenter;
            nodeLayout.padding = new RectOffset(4, 4, 4, 4);

            var barRowGo = new GameObject("ProgressBarRow");
            barRowGo.transform.SetParent(cardGo.transform, false);
            var barRowLe = barRowGo.AddComponent<LayoutElement>();
            barRowLe.preferredHeight = 20f;
            _rewardProgressSlider = UIManager.CreateSlider(barRowGo.transform, "RewardSlider");
            _rewardProgressSlider.interactable = false;
            var rSliderRt = _rewardProgressSlider.GetComponent<RectTransform>();
            UIManager.StretchFull(rSliderRt);

            var lvRowGo = new GameObject("LvLabels");
            lvRowGo.transform.SetParent(cardGo.transform, false);
            var lvRowLe = lvRowGo.AddComponent<LayoutElement>();
            lvRowLe.preferredHeight = 22f;
            var lvRowLayout = lvRowGo.AddComponent<HorizontalLayoutGroup>();
            lvRowLayout.spacing = 0;
            lvRowLayout.childForceExpandWidth = true;
            lvRowLayout.childForceExpandHeight = true;

            var lvStartGo = new GameObject("LvStart");
            lvStartGo.transform.SetParent(lvRowGo.transform, false);
            _lvStartText = lvStartGo.AddComponent<TextMeshProUGUI>();
            _lvStartText.fontSize = 18f;
            _lvStartText.color = ColorPalette.TextDim;
            _lvStartText.alignment = TextAlignmentOptions.Left;
            _lvStartText.raycastTarget = false;

            var lvEndGo = new GameObject("LvEnd");
            lvEndGo.transform.SetParent(lvRowGo.transform, false);
            _lvEndText = lvEndGo.AddComponent<TextMeshProUGUI>();
            _lvEndText.fontSize = 18f;
            _lvEndText.color = ColorPalette.TextDim;
            _lvEndText.alignment = TextAlignmentOptions.Right;
            _lvEndText.raycastTarget = false;

            var bottomRowGo = new GameObject("BottomRow");
            bottomRowGo.transform.SetParent(cardGo.transform, false);
            var bottomRowLe = bottomRowGo.AddComponent<LayoutElement>();
            bottomRowLe.preferredHeight = 36;

            var bottomLayout = bottomRowGo.AddComponent<HorizontalLayoutGroup>();
            bottomLayout.spacing = 8;
            bottomLayout.childForceExpandWidth = false;
            bottomLayout.childForceExpandHeight = true;
            bottomLayout.childAlignment = TextAnchor.MiddleCenter;

            var unclaimedGo = new GameObject("UnclaimedLabel");
            unclaimedGo.transform.SetParent(bottomRowGo.transform, false);
            var unclaimedLe = unclaimedGo.AddComponent<LayoutElement>();
            unclaimedLe.flexibleWidth = 1;
            _unclaimedText = unclaimedGo.AddComponent<TextMeshProUGUI>();
            _unclaimedText.fontSize = 22f;
            _unclaimedText.color = ColorPalette.Gold;
            _unclaimedText.alignment = TextAlignmentOptions.Left;
            _unclaimedText.raycastTarget = false;

            var claimBtnGo = new GameObject("ClaimAllBtn");
            claimBtnGo.transform.SetParent(bottomRowGo.transform, false);
            var claimBtnLe = claimBtnGo.AddComponent<LayoutElement>();
            claimBtnLe.preferredWidth = 140;
            var claimBtnBg = claimBtnGo.AddComponent<Image>();
            claimBtnBg.color = ColorPalette.Heal;
            _claimAllButton = claimBtnGo.AddComponent<Button>();
            _claimAllButton.targetGraphic = claimBtnBg;
            _claimAllButton.onClick.AddListener(OnClaimAll);

            var claimBtnTextGo = new GameObject("Label");
            claimBtnTextGo.transform.SetParent(claimBtnGo.transform, false);
            var claimBtnTextRt = claimBtnTextGo.GetComponent<RectTransform>();

            if (claimBtnTextRt == null) claimBtnTextRt = claimBtnTextGo.AddComponent<RectTransform>();
            UIManager.StretchFull(claimBtnTextRt);
            var claimBtnTmp = claimBtnTextGo.AddComponent<TextMeshProUGUI>();
            claimBtnTmp.text = "\ubaa8\ub450 \uc218\ub839";
            claimBtnTmp.fontSize = 22f;
            claimBtnTmp.color = Color.white;
            claimBtnTmp.alignment = TextAlignmentOptions.Center;
            claimBtnTmp.raycastTarget = false;
        }

        private void BuildUpgradeSection(Transform parent)
        {
            var sectionGo = new GameObject("UpgradeSection");
            sectionGo.transform.SetParent(parent, false);
            sectionGo.AddComponent<Image>().color = ColorPalette.Card;

            var sectionLayout = sectionGo.AddComponent<VerticalLayoutGroup>();
            sectionLayout.spacing = 8;
            sectionLayout.childForceExpandWidth = true;
            sectionLayout.childForceExpandHeight = false;
            sectionLayout.childAlignment = TextAnchor.UpperCenter;
            sectionLayout.padding = new RectOffset(12, 12, 12, 12);

            var sectionFitter = sectionGo.AddComponent<ContentSizeFitter>();
            sectionFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var btnRowGo = new GameObject("ButtonRow");
            btnRowGo.transform.SetParent(sectionGo.transform, false);
            var btnRowLe = btnRowGo.AddComponent<LayoutElement>();
            btnRowLe.preferredHeight = 120;

            var btnRowLayout = btnRowGo.AddComponent<HorizontalLayoutGroup>();
            btnRowLayout.spacing = 8;
            btnRowLayout.childForceExpandWidth = true;
            btnRowLayout.childForceExpandHeight = true;
            btnRowLayout.padding = new RectOffset(4, 4, 0, 0);

            BuildUpgradeCard(btnRowGo.transform, StatType.ATK);
            BuildUpgradeCard(btnRowGo.transform, StatType.HP);
            BuildUpgradeCard(btnRowGo.transform, StatType.DEF);
        }

        private void BuildUpgradeCard(Transform parent, StatType statType)
        {
            var go = new GameObject("Card_" + statType);
            go.transform.SetParent(parent, false);

            var bg = go.AddComponent<Image>();
            bg.color = ColorPalette.ButtonPrimary;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => OnUpgradeClicked(statType));
            _statButtons[statType] = btn;

            var innerLayout = go.AddComponent<VerticalLayoutGroup>();
            innerLayout.spacing = 2;
            innerLayout.childForceExpandWidth = true;
            innerLayout.childForceExpandHeight = false;
            innerLayout.childAlignment = TextAnchor.MiddleCenter;
            innerLayout.padding = new RectOffset(4, 4, 6, 6);

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(go.transform, false);
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.preferredHeight = 32f;
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = SpriteManager.Instance.GetIcon(STAT_ICON_ID[statType]);
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            var nameGo = new GameObject("Name");
            nameGo.transform.SetParent(go.transform, false);
            var nameLe = nameGo.AddComponent<LayoutElement>();
            nameLe.preferredHeight = 28f;
            var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
            nameTmp.text = STAT_NAME[statType];
            nameTmp.fontSize = 22f;
            nameTmp.color = Color.white;
            nameTmp.alignment = TextAlignmentOptions.Center;
            nameTmp.raycastTarget = false;

            var lvGo = new GameObject("Level");
            lvGo.transform.SetParent(go.transform, false);
            var lvLe = lvGo.AddComponent<LayoutElement>();
            lvLe.preferredHeight = 28f;
            var lvTmp = lvGo.AddComponent<TextMeshProUGUI>();
            lvTmp.fontSize = 22f;
            lvTmp.color = ColorPalette.Gold;
            lvTmp.alignment = TextAlignmentOptions.Center;
            lvTmp.raycastTarget = false;
            _statLevelTexts[statType] = lvTmp;

            var bonusGo = new GameObject("Bonus");
            bonusGo.transform.SetParent(go.transform, false);
            var bonusLe = bonusGo.AddComponent<LayoutElement>();
            bonusLe.preferredHeight = 28f;
            var bonusTmp = bonusGo.AddComponent<TextMeshProUGUI>();
            bonusTmp.fontSize = 22f;
            bonusTmp.color = ColorPalette.TextDim;
            bonusTmp.alignment = TextAlignmentOptions.Center;
            bonusTmp.raycastTarget = false;
            _statBonusTexts[statType] = bonusTmp;

            var costGo = new GameObject("Cost");
            costGo.transform.SetParent(go.transform, false);
            var costLe = costGo.AddComponent<LayoutElement>();
            costLe.preferredHeight = 28f;
            var costTmp = costGo.AddComponent<TextMeshProUGUI>();
            costTmp.fontSize = 22f;
            costTmp.color = ColorPalette.TextDim;
            costTmp.alignment = TextAlignmentOptions.Center;
            costTmp.raycastTarget = false;
            _statCostTexts[statType] = costTmp;
        }

        private void BuildHeritageCard(Transform parent)
        {
            _heritageCard = new GameObject("HeritageCard");
            _heritageCard.transform.SetParent(parent, false);
            _heritageCard.AddComponent<Image>().color = ColorPalette.Card;

            var layout = _heritageCard.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(16, 16, 12, 12);

            var fitter = _heritageCard.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var titleGo = new GameObject("HeritageTitle");
            titleGo.transform.SetParent(_heritageCard.transform, false);
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 28f;
            var titleTmp = titleGo.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "\uc720\uc0b0";
            titleTmp.fontSize = 24f;
            titleTmp.color = ColorPalette.Text;
            titleTmp.alignment = TextAlignmentOptions.Left;
            titleTmp.raycastTarget = false;

            var routeGo = new GameObject("Route");
            routeGo.transform.SetParent(_heritageCard.transform, false);
            var routeLe = routeGo.AddComponent<LayoutElement>();
            routeLe.preferredHeight = 28f;
            _heritageRouteText = routeGo.AddComponent<TextMeshProUGUI>();
            _heritageRouteText.fontSize = 22f;
            _heritageRouteText.color = ColorPalette.Text;
            _heritageRouteText.alignment = TextAlignmentOptions.Left;
            _heritageRouteText.raycastTarget = false;

            var levelGo = new GameObject("Level");
            levelGo.transform.SetParent(_heritageCard.transform, false);
            var levelLe = levelGo.AddComponent<LayoutElement>();
            levelLe.preferredHeight = 28f;
            _heritageLevelText = levelGo.AddComponent<TextMeshProUGUI>();
            _heritageLevelText.fontSize = 22f;
            _heritageLevelText.color = ColorPalette.TextDim;
            _heritageLevelText.alignment = TextAlignmentOptions.Left;
            _heritageLevelText.raycastTarget = false;

            _heritageCard.SetActive(false);
        }

        private void BuildRewardPopup()
        {
            _rewardPopup = new GameObject("RewardPopup");
            _rewardPopup.transform.SetParent(transform.root, false);
            var popupRt = _rewardPopup.GetComponent<RectTransform>();

            if (popupRt == null) popupRt = _rewardPopup.AddComponent<RectTransform>();
            popupRt.anchorMin = Vector2.zero;
            popupRt.anchorMax = Vector2.one;
            popupRt.offsetMin = Vector2.zero;
            popupRt.offsetMax = Vector2.zero;

            var overlayBg = _rewardPopup.AddComponent<Image>();
            overlayBg.color = new Color(0, 0, 0, 0.6f);
            var overlayBtn = _rewardPopup.AddComponent<Button>();
            overlayBtn.targetGraphic = overlayBg;
            overlayBtn.onClick.AddListener(HideRewardPopup);

            var panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(_rewardPopup.transform, false);
            var panelRt = panelGo.GetComponent<RectTransform>();

            if (panelRt == null) panelRt = panelGo.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.15f, 0.35f);
            panelRt.anchorMax = new Vector2(0.85f, 0.65f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            var panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.06f, 0.13f, 1f);

            var panelLayout = panelGo.AddComponent<VerticalLayoutGroup>();
            panelLayout.spacing = 8;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;
            panelLayout.childAlignment = TextAnchor.MiddleCenter;
            panelLayout.padding = new RectOffset(24, 24, 24, 24);

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(panelGo.transform, false);
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.preferredHeight = 48f;
            _rewardPopupIconImage = iconGo.AddComponent<Image>();
            _rewardPopupIconImage.preserveAspect = true;
            _rewardPopupIconImage.raycastTarget = false;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(panelGo.transform, false);
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 30f;
            _rewardPopupTitle = titleGo.AddComponent<TextMeshProUGUI>();
            _rewardPopupTitle.text = "\ubcf4\uc0c1 \uc218\ub839!";
            _rewardPopupTitle.fontSize = 26f;
            _rewardPopupTitle.color = ColorPalette.Gold;
            _rewardPopupTitle.alignment = TextAlignmentOptions.Center;
            _rewardPopupTitle.raycastTarget = false;

            var descGo = new GameObject("Desc");
            descGo.transform.SetParent(panelGo.transform, false);
            var descLe = descGo.AddComponent<LayoutElement>();
            descLe.preferredHeight = 28f;
            _rewardPopupDesc = descGo.AddComponent<TextMeshProUGUI>();
            _rewardPopupDesc.fontSize = 22f;
            _rewardPopupDesc.color = ColorPalette.Text;
            _rewardPopupDesc.alignment = TextAlignmentOptions.Center;
            _rewardPopupDesc.raycastTarget = false;

            var confirmBtnGo = new GameObject("ConfirmBtn");
            confirmBtnGo.transform.SetParent(panelGo.transform, false);
            var confirmBtnLe = confirmBtnGo.AddComponent<LayoutElement>();
            confirmBtnLe.preferredHeight = 44f;
            var confirmBtnBg = confirmBtnGo.AddComponent<Image>();
            confirmBtnBg.color = ColorPalette.ButtonPrimary;
            var confirmBtn = confirmBtnGo.AddComponent<Button>();
            confirmBtn.targetGraphic = confirmBtnBg;
            confirmBtn.onClick.AddListener(HideRewardPopup);

            var confirmTextGo = new GameObject("Text");
            confirmTextGo.transform.SetParent(confirmBtnGo.transform, false);
            var confirmTextRt = confirmTextGo.GetComponent<RectTransform>();

            if (confirmTextRt == null) confirmTextRt = confirmTextGo.AddComponent<RectTransform>();
            UIManager.StretchFull(confirmTextRt);
            var confirmTmp = confirmTextGo.AddComponent<TextMeshProUGUI>();
            confirmTmp.text = "\ud655\uc778";
            confirmTmp.fontSize = 24f;
            confirmTmp.color = Color.white;
            confirmTmp.alignment = TextAlignmentOptions.Center;
            confirmTmp.raycastTarget = false;

            _rewardPopup.SetActive(false);
        }

        private void OnUpgradeClicked(StatType statType)
        {
            if (Game == null || Game.Player == null) return;

            var talent = Game.Player.Talent;
            var result = talent.Upgrade(statType, (int)Game.Player.Resources.Gold);
            if (result.IsOk())
            {
                Game.Player.Resources.Spend(ResourceType.GOLD, result.Data.Cost);
                Game.SaveGame();
                UI.Refresh();
            }
        }

        private void OnClaimAll()
        {
            if (Game == null || Game.Player == null) return;

            var claimable = Game.Player.Talent.GetClaimableMilestones(Game.Player.ClaimedMilestones);
            if (claimable.Count == 0) return;

            foreach (var milestone in claimable)
            {
                string key = $"LV_{milestone.Level}";
                if (Game.Player.ClaimedMilestones.Contains(key)) continue;
                Game.Player.ClaimedMilestones.Add(key);

                if (milestone.RewardType != "GOLD_BOOST")
                {
                    Game.Player.Resources.Add(ResourceType.GOLD, milestone.RewardAmount);
                }
            }

            Game.SaveGame();
            UI.Refresh();
        }

        private void ClaimMilestone(int level, string rewardType, int rewardAmount)
        {
            if (Game == null || Game.Player == null) return;

            string key = $"LV_{level}";
            if (Game.Player.ClaimedMilestones.Contains(key)) return;
            Game.Player.ClaimedMilestones.Add(key);

            if (rewardType != "GOLD_BOOST")
            {
                Game.Player.Resources.Add(ResourceType.GOLD, rewardAmount);
            }

            Game.SaveGame();
            ShowRewardPopup(rewardType, rewardAmount);
            UI.Refresh();
        }

        private void ShowRewardPopup(string rewardType, int rewardAmount)
        {
            string label = rewardType == "GOLD_BOOST" ? "\uace8\ub4dc \ud68d\ub4dd\ub7c9" : "\uace8\ub4dc";
            string amountStr = rewardType == "GOLD_BOOST"
                ? $"+{rewardAmount}%"
                : $"+{NumberFormatter.FormatInt(rewardAmount)}";

            string iconId = rewardType == "GOLD_BOOST" ? "icon_gold_boost" : "icon_gold";
            _rewardPopupIconImage.sprite = SpriteManager.Instance.GetIcon(iconId);
            _rewardPopupDesc.text = $"{label} {amountStr}";
            _rewardPopup.SetActive(true);
        }

        private void HideRewardPopup()
        {
            _rewardPopup.SetActive(false);
        }

        public override void Refresh()
        {
            if (Game == null || Game.Player == null) return;

            var talent = Game.Player.Talent;
            int totalLevel = talent.GetTotalLevel();
            var gradeInfo = TalentTable.GetSubGradeInfo(totalLevel);

            RefreshGradeInfo(talent, totalLevel, gradeInfo);
            RefreshRewardWindow(totalLevel);
            RefreshUpgradeCards(talent);
            RefreshHeritage();
        }

        private void RefreshGradeInfo(Talent talent, int totalLevel, SubGradeInfo gradeInfo)
        {
            string subGradeLabel = TalentTable.GetSubGradeLabel(totalLevel);
            var gradeColor = ColorPalette.GetTalentGradeColor(gradeInfo.Grade);

            _gradeText.text = $"\ub4f1\uae09: <color=#{ColorUtility.ToHtmlStringRGB(gradeColor)}>{subGradeLabel}</color>";

            _subGradeProgressText.text = $"\uc11c\ube0c \ub4f1\uae09 \uc9c4\ud589: {gradeInfo.LevelInTier} / {gradeInfo.TierLevels}";

            int? nextThreshold = talent.GetNextGradeThreshold();
            string thresholdStr = nextThreshold.HasValue ? nextThreshold.Value.ToString() : "MAX";
            _totalLevelText.text = $"\ucd1d \ub808\ubca8: {totalLevel} / {thresholdStr}";

            float goldMult = Game.Player.GetGoldMultiplier();
            int goldPercent = Mathf.RoundToInt((goldMult - 1f) * 100f);
            if (goldPercent > 0)
            {
                _goldMultiplierRow.SetActive(true);
                _goldMultiplierText.text = $"\uace8\ub4dc \ud68d\ub4dd\ub7c9: +{goldPercent}%";
            }
            else
            {
                _goldMultiplierRow.SetActive(false);
            }

            if (gradeInfo.TierLevels > 0)
            {
                _subGradeSlider.maxValue = gradeInfo.TierLevels;
                _subGradeSlider.value = gradeInfo.LevelInTier;
            }
            else
            {
                _subGradeSlider.maxValue = 1;
                _subGradeSlider.value = 1;
            }
        }

        private string FormatRewardAmount(string rewardType, int amount)
        {
            if (rewardType == "GOLD_BOOST") return $"+{amount}%";
            return NumberFormatter.FormatInt(amount);
        }

        private string GetTransitionLabel(TransitionInfo t)
        {
            if (t.IsMainGrade)
                return TalentTable.GetGradeLabel(t.Grade);

            string statLabel;
            if (!BONUS_STAT_LABEL.TryGetValue(t.BonusStat, out statLabel))
                statLabel = t.BonusStat;
            return $"{statLabel}+{t.BonusAmount}";
        }

        private void RefreshRewardWindow(int totalLevel)
        {
            foreach (var obj in _nodeObjects)
                Destroy(obj);
            _nodeObjects.Clear();

            int maxLevel = TalentTable.GetMaxLevel();
            int halfRange = VIEW_RANGE / 2;
            int windowStart = Mathf.Max(0, totalLevel - halfRange);
            int windowEnd = Mathf.Min(maxLevel, windowStart + VIEW_RANGE);
            if (windowEnd - windowStart < VIEW_RANGE)
                windowStart = Mathf.Max(0, windowEnd - VIEW_RANGE);

            float windowSize = windowEnd - windowStart;
            if (windowSize > 0)
            {
                _rewardProgressSlider.maxValue = windowSize;
                _rewardProgressSlider.value = Mathf.Clamp(totalLevel - windowStart, 0, windowSize);
            }
            else
            {
                _rewardProgressSlider.maxValue = 1;
                _rewardProgressSlider.value = 1;
            }

            _lvStartText.text = $"Lv.{windowStart}";
            _lvEndText.text = $"Lv.{windowEnd}";

            var allMilestones = TalentTable.GetAllMilestones();
            var allTransitions = TalentTable.GetAllTransitions();

            var nodesInWindow = new List<NodeData>();

            foreach (var milestone in allMilestones)
            {
                if (milestone.Level < windowStart || milestone.Level > windowEnd) continue;
                string key = $"LV_{milestone.Level}";
                bool claimed = Game.Player.ClaimedMilestones.Contains(key);
                bool reached = totalLevel >= milestone.Level;
                string iconId = milestone.RewardType == "GOLD_BOOST" ? "icon_gold_boost" : "icon_gold";
                nodesInWindow.Add(new NodeData
                {
                    Level = milestone.Level,
                    IconId = iconId,
                    Description = FormatRewardAmount(milestone.RewardType, milestone.RewardAmount),
                    Claimed = claimed,
                    Reached = reached,
                    IsTransition = false,
                    RewardType = milestone.RewardType,
                    RewardAmount = milestone.RewardAmount,
                });
            }

            foreach (var transition in allTransitions)
            {
                if (transition.Level < windowStart || transition.Level > windowEnd) continue;
                bool reached = totalLevel >= transition.Level;
                string iconId = transition.IsMainGrade ? "icon_star" : "icon_upgrade";
                nodesInWindow.Add(new NodeData
                {
                    Level = transition.Level,
                    IconId = iconId,
                    Description = GetTransitionLabel(transition),
                    Claimed = false,
                    Reached = reached,
                    IsTransition = true,
                    RewardType = null,
                    RewardAmount = 0,
                });
            }

            nodesInWindow.Sort((a, b) => a.Level.CompareTo(b.Level));

            foreach (var node in nodesInWindow)
            {
                var nodeGo = new GameObject("Node_" + node.Level);
                nodeGo.transform.SetParent(_nodeContainer, false);

                var nodeBg = nodeGo.AddComponent<Image>();
                var nodeLayout = nodeGo.AddComponent<VerticalLayoutGroup>();
                nodeLayout.childForceExpandWidth = true;
                nodeLayout.childForceExpandHeight = false;
                nodeLayout.childAlignment = TextAnchor.MiddleCenter;
                nodeLayout.padding = new RectOffset(2, 2, 2, 2);

                bool isClaimable = node.Reached && !node.Claimed && !node.IsTransition;

                if (node.Claimed)
                {
                    nodeBg.color = ColorPalette.CardLight;
                }
                else if (isClaimable)
                {
                    nodeBg.color = ColorPalette.Gold;
                }
                else if (!node.Reached)
                {
                    nodeBg.color = new Color(
                        ColorPalette.CardLight.r,
                        ColorPalette.CardLight.g,
                        ColorPalette.CardLight.b,
                        0.4f);
                }
                else
                {
                    nodeBg.color = ColorPalette.CardLight;
                }

                if (isClaimable)
                {
                    var btn = nodeGo.AddComponent<Button>();
                    btn.targetGraphic = nodeBg;
                    int lvCapture = node.Level;
                    string rtCapture = node.RewardType;
                    int raCapture = node.RewardAmount;
                    btn.onClick.AddListener(() => ClaimMilestone(lvCapture, rtCapture, raCapture));
                }

                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(nodeGo.transform, false);
                var iconLe = iconGo.AddComponent<LayoutElement>();
                iconLe.preferredHeight = 36f;
                iconLe.preferredWidth = 36f;
                var iconImg = iconGo.AddComponent<Image>();
                iconImg.raycastTarget = false;
                if (SpriteManager.Instance != null)
                {
                    string spriteId = node.Claimed ? "icon_check" : node.IconId;
                    iconImg.sprite = SpriteManager.Instance.GetIcon(spriteId);
                }
                else
                {
                    iconImg.color = new Color(1f, 0.41f, 0.71f);
                }
                if (!node.Reached && !node.Claimed)
                    iconImg.color = new Color(1f, 1f, 1f, 0.4f);

                var descGo = new GameObject("Desc");
                descGo.transform.SetParent(nodeGo.transform, false);
                var descLe = descGo.AddComponent<LayoutElement>();
                descLe.preferredHeight = 26f;
                var descTmp = descGo.AddComponent<TextMeshProUGUI>();
                descTmp.text = node.Description;
                descTmp.fontSize = 18f;
                descTmp.color = node.Reached ? ColorPalette.Text : ColorPalette.TextDim;
                descTmp.alignment = TextAlignmentOptions.Center;
                descTmp.enableWordWrapping = false;
                descTmp.overflowMode = TextOverflowModes.Ellipsis;
                descTmp.raycastTarget = false;

                _nodeObjects.Add(nodeGo);
            }

            var claimable = Game.Player.Talent.GetClaimableMilestones(Game.Player.ClaimedMilestones);
            int claimableCount = claimable.Count;
            _unclaimedText.text = $"\ubbf8\uc218\ub839 \ubcf4\uc0c1: {claimableCount}\uac1c";
            _claimAllButton.interactable = claimableCount > 0;
        }

        private void RefreshUpgradeCards(Talent talent)
        {
            int gold = (int)Game.Player.Resources.Gold;

            foreach (StatType statType in new[] { StatType.ATK, StatType.HP, StatType.DEF })
            {
                int tierLevel = talent.GetStatLevelInTier(statType);
                int perLevel = TalentTable.GetStatPerLevel(statType);
                int cost = talent.GetUpgradeCost(statType);
                bool canUpgrade = talent.CanUpgradeStat(statType);
                bool isMax = tierLevel >= TalentTable.GetLevelsPerStat();

                _statLevelTexts[statType].text = $"Lv.{tierLevel}/10";
                _statBonusTexts[statType].text = $"+{perLevel}";

                if (isMax)
                {
                    _statCostTexts[statType].text = "MAX";
                    _statCostTexts[statType].color = ColorPalette.Gold;
                }
                else
                {
                    _statCostTexts[statType].text = $"{NumberFormatter.FormatInt(cost)} G";
                    _statCostTexts[statType].color = ColorPalette.TextDim;
                }

                _statButtons[statType].interactable = canUpgrade && gold >= cost;
            }
        }

        private void RefreshHeritage()
        {
            bool unlocked = Game.Player.IsHeritageUnlocked();
            _heritageCard.SetActive(unlocked);
            if (!unlocked) return;

            var heritage = Game.Player.Heritage;
            string routeLabel;
            if (!ROUTE_LABEL.TryGetValue(heritage.Route, out routeLabel))
                routeLabel = heritage.Route.ToString();

            _heritageRouteText.text = $"\uacbd\ub85c: {routeLabel}";
            _heritageLevelText.text = $"\ub808\ubca8: {heritage.Level}";
        }

        private struct NodeData
        {
            public int Level;
            public string IconId;
            public string Description;
            public bool Claimed;
            public bool Reached;
            public bool IsTransition;
            public string RewardType;
            public int RewardAmount;
        }
    }
}
