using System;
using System.Collections.Generic;
using System.Linq;
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
    public class EquipmentScreen : BaseScreen
    {
        private static readonly Dictionary<SlotType, string> SlotLabels = new Dictionary<SlotType, string>
        {
            { SlotType.WEAPON, "\ubb34\uae30" },
            { SlotType.ARMOR, "\ubc29\uc5b4\uad6c" },
            { SlotType.RING, "\ubc18\uc9c0" },
            { SlotType.NECKLACE, "\ubaa9\uac78\uc774" },
            { SlotType.SHOES, "\uc2e0\ubc1c" },
            { SlotType.GLOVES, "\uc7a5\uac11" },
            { SlotType.HAT, "\ubaa8\uc790" },
        };

        private static readonly SlotType[] FilterSlotOrder = {
            SlotType.WEAPON, SlotType.ARMOR, SlotType.RING,
            SlotType.NECKLACE, SlotType.SHOES, SlotType.GLOVES, SlotType.HAT
        };

        private int _activeTab;
        private Image _equipTabBg;
        private Image _forgeTabBg;
        private TextMeshProUGUI _equipTabText;
        private TextMeshProUGUI _forgeTabText;

        private RectTransform _equipmentTabPanel;
        private RectTransform _forgeTabPanel;

        private Dictionary<string, GameObject> _paperDollSlots = new Dictionary<string, GameObject>();
        private RectTransform _detailPanel;
        private TextMeshProUGUI _detailNameText;
        private TextMeshProUGUI _detailStatsText;
        private Button _upgradeButton;
        private TextMeshProUGUI _upgradeButtonText;
        private Button _unequipButton;
        private GameObject _equippedButtonRow;
        private GameObject _inventoryButtonRow;
        private Button _equipButton;
        private Button _sellButton;
        private TextMeshProUGUI _sellButtonText;
        private Equipment _selectedEquipment;
        private bool _selectedIsEquipped;
        private SlotType _selectedSlotType;
        private int _selectedSlotIndex;

        private TextMeshProUGUI _totalStatsText;
        private TextMeshProUGUI _inventoryHeaderText;
        private TextMeshProUGUI _stonesHeaderText;

        private int _activeFilterIndex;
        private Image[] _filterBgs;
        private TextMeshProUGUI[] _filterTexts;

        private RectTransform _inventoryGrid;
        private List<GameObject> _inventoryItems = new List<GameObject>();

        private RectTransform _forgeGridContent;
        private List<GameObject> _forgeEntries = new List<GameObject>();

        private Equipment _forgeSelectedEquipment;
        private Image _forgeResultIcon;
        private Image _forgeResultIconSprite;
        private TextMeshProUGUI _forgeResultNameText;
        private TextMeshProUGUI _forgeResultStatsText;
        private GameObject _forgeResultArea;
        private Image _forgeStarIcon;
        private Image _forgeStarSprite;
        private GameObject[] _forgeMaterialSlots;
        private Image[] _forgeMaterialBgs;
        private Image[] _forgeMaterialSprites;
        private TextMeshProUGUI[] _forgeMaterialTexts;
        private Button _forgeMergeButton;
        private TextMeshProUGUI _forgeMergeButtonText;
        private Button _forgeBulkMergeButton;
        private TextMeshProUGUI _forgeBulkMergeText;
        private RectTransform _forgeInventoryGrid;
        private List<GameObject> _forgeInventoryItems = new List<GameObject>();

        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            var mainLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            mainLayout.spacing = 0;
            mainLayout.childForceExpandWidth = true;
            mainLayout.childForceExpandHeight = false;

            BuildTabBar();
            BuildEquipmentTab();
            BuildForgeTab();

            _forgeTabPanel.gameObject.SetActive(false);
        }

        private void BuildTabBar()
        {
            var tabGo = new GameObject("TabBar");
            tabGo.transform.SetParent(transform, false);
            var tabLe = tabGo.AddComponent<LayoutElement>();
            tabLe.preferredHeight = 22;
            tabLe.flexibleHeight = 0;

            var tabLayout = tabGo.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 2;
            tabLayout.childForceExpandWidth = true;
            tabLayout.childForceExpandHeight = true;
            tabLayout.padding = new RectOffset(2, 2, 1, 1);

            var equipTabGo = new GameObject("Tab_Equip");
            equipTabGo.transform.SetParent(tabGo.transform, false);
            _equipTabBg = equipTabGo.AddComponent<Image>();
            _equipTabBg.color = ColorPalette.ButtonPrimary;
            var equipBtn = equipTabGo.AddComponent<Button>();
            equipBtn.targetGraphic = _equipTabBg;
            equipBtn.onClick.AddListener(() => OnTabChanged(0));

            var equipTextGo = new GameObject("Text");
            equipTextGo.transform.SetParent(equipTabGo.transform, false);
            _equipTabText = equipTextGo.AddComponent<TextMeshProUGUI>();
            _equipTabText.text = "\uc7a5\ube44";
            _equipTabText.fontSize = 13;
            _equipTabText.color = Color.white;
            _equipTabText.alignment = TextAlignmentOptions.Center;
            _equipTabText.raycastTarget = false;
            var equipTextRt = equipTextGo.GetComponent<RectTransform>();
            UIManager.StretchFull(equipTextRt);

            var forgeTabGo = new GameObject("Tab_Forge");
            forgeTabGo.transform.SetParent(tabGo.transform, false);
            _forgeTabBg = forgeTabGo.AddComponent<Image>();
            _forgeTabBg.color = ColorPalette.ButtonSecondary;
            var forgeBtn = forgeTabGo.AddComponent<Button>();
            forgeBtn.targetGraphic = _forgeTabBg;
            forgeBtn.onClick.AddListener(() => OnTabChanged(1));

            var forgeTextGo = new GameObject("Text");
            forgeTextGo.transform.SetParent(forgeTabGo.transform, false);
            _forgeTabText = forgeTextGo.AddComponent<TextMeshProUGUI>();
            _forgeTabText.text = "\ud569\uc131 (0)";
            _forgeTabText.fontSize = 13;
            _forgeTabText.color = ColorPalette.TextDim;
            _forgeTabText.alignment = TextAlignmentOptions.Center;
            _forgeTabText.raycastTarget = false;
            var forgeTextRt = forgeTextGo.GetComponent<RectTransform>();
            UIManager.StretchFull(forgeTextRt);
        }

        private void BuildEquipmentTab()
        {
            var panelGo = new GameObject("EquipmentTab");
            panelGo.transform.SetParent(transform, false);
            _equipmentTabPanel = panelGo.GetComponent<RectTransform>();

            if (_equipmentTabPanel == null) _equipmentTabPanel = panelGo.AddComponent<RectTransform>();
            var panelLe = panelGo.AddComponent<LayoutElement>();
            panelLe.flexibleHeight = 1;

            var scrollGo = new GameObject("ScrollView");
            scrollGo.transform.SetParent(panelGo.transform, false);
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
            contentLayout.spacing = 10;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.padding = new RectOffset(12, 12, 12, 12);

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;
            scrollRect.viewport = viewportRt;

            BuildPaperDoll(contentGo.transform);
            BuildDetailPanel(contentGo.transform);
            BuildTotalStats(contentGo.transform);
            BuildInventoryHeader(contentGo.transform);
            BuildFilterBar(contentGo.transform);
            BuildInventoryGrid(contentGo.transform);
        }

        private void BuildPaperDoll(Transform parent)
        {
            var dollGo = new GameObject("PaperDoll");
            dollGo.transform.SetParent(parent, false);
            var dollLe = dollGo.AddComponent<LayoutElement>();
            dollLe.preferredHeight = 440;
            dollGo.AddComponent<Image>().color = ColorPalette.Card;

            var dollLayout = dollGo.AddComponent<VerticalLayoutGroup>();
            dollLayout.spacing = 4;
            dollLayout.childForceExpandWidth = true;
            dollLayout.childForceExpandHeight = false;
            dollLayout.childAlignment = TextAnchor.UpperCenter;
            dollLayout.padding = new RectOffset(8, 8, 8, 8);

            var gridGo = new GameObject("Grid");
            gridGo.transform.SetParent(dollGo.transform, false);
            var gridLe = gridGo.AddComponent<LayoutElement>();
            gridLe.preferredHeight = 420;

            var grid = gridGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(96, 96);
            grid.spacing = new Vector2(10, 10);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;

            CreatePaperDollCell(gridGo.transform, SlotType.NECKLACE, 0);
            CreatePaperDollCell(gridGo.transform, SlotType.HAT, 0);
            CreatePaperDollCell(gridGo.transform, SlotType.WEAPON, 0);

            CreatePaperDollCell(gridGo.transform, SlotType.RING, 0);
            CreatePaperDollPlaceholder(gridGo.transform, true);
            CreatePaperDollCell(gridGo.transform, SlotType.ARMOR, 0);

            CreatePaperDollCell(gridGo.transform, SlotType.RING, 1);
            CreatePaperDollPlaceholder(gridGo.transform, false);
            CreatePaperDollCell(gridGo.transform, SlotType.GLOVES, 0);

            CreatePaperDollEmptyCell(gridGo.transform);
            CreatePaperDollCell(gridGo.transform, SlotType.SHOES, 0);
            CreatePaperDollEmptyCell(gridGo.transform);
        }

        private void CreatePaperDollCell(Transform parent, SlotType slotType, int index)
        {
            var cellGo = new GameObject($"Slot_{slotType}_{index}");
            cellGo.transform.SetParent(parent, false);

            var borderImg = cellGo.AddComponent<Image>();
            borderImg.color = ColorPalette.CardLight;

            var innerGo = new GameObject("Inner");
            innerGo.transform.SetParent(cellGo.transform, false);
            var innerImg = innerGo.AddComponent<Image>();
            innerImg.color = ColorPalette.Card;
            var innerRt = innerGo.GetComponent<RectTransform>();
            innerRt.anchorMin = Vector2.zero;
            innerRt.anchorMax = Vector2.one;
            innerRt.offsetMin = new Vector2(3, 3);
            innerRt.offsetMax = new Vector2(-3, -3);

            var slotLabelGo = new GameObject("SlotLabel");
            slotLabelGo.transform.SetParent(cellGo.transform, false);
            var slotLabel = slotLabelGo.AddComponent<TextMeshProUGUI>();
            string label;
            SlotLabels.TryGetValue(slotType, out label);
            if (slotType == SlotType.RING)
                label = index == 0 ? "\ubc18\uc9c01" : "\ubc18\uc9c02";
            slotLabel.text = label ?? "";
            slotLabel.fontSize = 26;
            slotLabel.color = ColorPalette.TextDim;
            slotLabel.alignment = TextAlignmentOptions.Center;
            slotLabel.raycastTarget = false;
            var slotLabelRt = slotLabelGo.GetComponent<RectTransform>();
            slotLabelRt.anchorMin = Vector2.zero;
            slotLabelRt.anchorMax = Vector2.one;
            slotLabelRt.offsetMin = Vector2.zero;
            slotLabelRt.offsetMax = Vector2.zero;

            var btn = cellGo.AddComponent<Button>();
            btn.targetGraphic = borderImg;
            SlotType capturedSlot = slotType;
            int capturedIndex = index;
            btn.onClick.AddListener(() => OnPaperDollSlotClicked(capturedSlot, capturedIndex));

            string key = $"{slotType}_{index}";
            _paperDollSlots[key] = cellGo;
        }

        private void CreatePaperDollPlaceholder(Transform parent, bool isTop)
        {
            var cellGo = new GameObject(isTop ? "CharTop" : "CharBottom");
            cellGo.transform.SetParent(parent, false);
            cellGo.AddComponent<Image>().color = ColorPalette.CardLight;

            if (isTop)
            {
                var charText = UIManager.CreateText(cellGo.transform, "CAT", 28f, ColorPalette.Text, "CharIcon");
                charText.alignment = TextAlignmentOptions.Center;
            }
        }

        private void CreatePaperDollEmptyCell(Transform parent)
        {
            var cellGo = new GameObject("Empty");
            cellGo.transform.SetParent(parent, false);
            var img = cellGo.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
        }

        private void BuildDetailPanel(Transform parent)
        {
            var detailGo = new GameObject("DetailPanel");
            detailGo.transform.SetParent(parent, false);
            _detailPanel = detailGo.GetComponent<RectTransform>();

            if (_detailPanel == null) _detailPanel = detailGo.AddComponent<RectTransform>();
            var detailLe = detailGo.AddComponent<LayoutElement>();
            detailLe.preferredHeight = 160;
            detailGo.AddComponent<Image>().color = ColorPalette.CardLight;

            var layout = detailGo.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(12, 12, 8, 8);

            var nameGo = new GameObject("Name");
            nameGo.transform.SetParent(detailGo.transform, false);
            var nameLe = nameGo.AddComponent<LayoutElement>();
            nameLe.preferredHeight = 32;
            _detailNameText = nameGo.AddComponent<TextMeshProUGUI>();
            _detailNameText.fontSize = 26;
            _detailNameText.color = ColorPalette.Text;
            _detailNameText.alignment = TextAlignmentOptions.Left;
            _detailNameText.raycastTarget = false;

            var statsGo = new GameObject("Stats");
            statsGo.transform.SetParent(detailGo.transform, false);
            var statsLe = statsGo.AddComponent<LayoutElement>();
            statsLe.preferredHeight = 60;
            _detailStatsText = statsGo.AddComponent<TextMeshProUGUI>();
            _detailStatsText.fontSize = 22;
            _detailStatsText.color = ColorPalette.Text;
            _detailStatsText.alignment = TextAlignmentOptions.Left;
            _detailStatsText.raycastTarget = false;

            _equippedButtonRow = new GameObject("EquippedButtonRow");
            _equippedButtonRow.transform.SetParent(detailGo.transform, false);
            var eqBtnRowLe = _equippedButtonRow.AddComponent<LayoutElement>();
            eqBtnRowLe.preferredHeight = 40;
            var eqBtnRowLayout = _equippedButtonRow.AddComponent<HorizontalLayoutGroup>();
            eqBtnRowLayout.spacing = 8;
            eqBtnRowLayout.childForceExpandWidth = true;
            eqBtnRowLayout.childForceExpandHeight = true;

            var upgradeGo = new GameObject("UpgradeBtn");
            upgradeGo.transform.SetParent(_equippedButtonRow.transform, false);
            var upgradeBg = upgradeGo.AddComponent<Image>();
            upgradeBg.color = ColorPalette.ButtonPrimary;
            _upgradeButton = upgradeGo.AddComponent<Button>();
            _upgradeButton.targetGraphic = upgradeBg;
            _upgradeButton.onClick.AddListener(OnUpgradeClicked);

            var upgradeTextGo = new GameObject("Text");
            upgradeTextGo.transform.SetParent(upgradeGo.transform, false);
            _upgradeButtonText = upgradeTextGo.AddComponent<TextMeshProUGUI>();
            _upgradeButtonText.text = "\uac15\ud654";
            _upgradeButtonText.fontSize = 22;
            _upgradeButtonText.color = Color.white;
            _upgradeButtonText.alignment = TextAlignmentOptions.Center;
            _upgradeButtonText.raycastTarget = false;
            var upgradeTextRt = upgradeTextGo.GetComponent<RectTransform>();
            UIManager.StretchFull(upgradeTextRt);

            var unequipGo = new GameObject("UnequipBtn");
            unequipGo.transform.SetParent(_equippedButtonRow.transform, false);
            var unequipBg = unequipGo.AddComponent<Image>();
            unequipBg.color = ColorPalette.Hp;
            _unequipButton = unequipGo.AddComponent<Button>();
            _unequipButton.targetGraphic = unequipBg;
            _unequipButton.onClick.AddListener(OnUnequipClicked);

            var unequipTextGo = new GameObject("Text");
            unequipTextGo.transform.SetParent(unequipGo.transform, false);
            var unequipText = unequipTextGo.AddComponent<TextMeshProUGUI>();
            unequipText.text = "\ud574\uc81c";
            unequipText.fontSize = 22;
            unequipText.color = Color.white;
            unequipText.alignment = TextAlignmentOptions.Center;
            unequipText.raycastTarget = false;
            var unequipTextRt = unequipTextGo.GetComponent<RectTransform>();
            UIManager.StretchFull(unequipTextRt);

            _inventoryButtonRow = new GameObject("InventoryButtonRow");
            _inventoryButtonRow.transform.SetParent(detailGo.transform, false);
            var invBtnRowLe = _inventoryButtonRow.AddComponent<LayoutElement>();
            invBtnRowLe.preferredHeight = 40;
            var invBtnRowLayout = _inventoryButtonRow.AddComponent<HorizontalLayoutGroup>();
            invBtnRowLayout.spacing = 8;
            invBtnRowLayout.childForceExpandWidth = true;
            invBtnRowLayout.childForceExpandHeight = true;

            var equipGo = new GameObject("EquipBtn");
            equipGo.transform.SetParent(_inventoryButtonRow.transform, false);
            var equipBg = equipGo.AddComponent<Image>();
            equipBg.color = ColorPalette.ButtonPrimary;
            _equipButton = equipGo.AddComponent<Button>();
            _equipButton.targetGraphic = equipBg;
            _equipButton.onClick.AddListener(OnEquipClicked);

            var equipTextGo = new GameObject("Text");
            equipTextGo.transform.SetParent(equipGo.transform, false);
            var equipText = equipTextGo.AddComponent<TextMeshProUGUI>();
            equipText.text = "\uc7a5\ucc29";
            equipText.fontSize = 22;
            equipText.color = Color.white;
            equipText.alignment = TextAlignmentOptions.Center;
            equipText.raycastTarget = false;
            UIManager.StretchFull(equipTextGo.GetComponent<RectTransform>());

            var sellGo = new GameObject("SellBtn");
            sellGo.transform.SetParent(_inventoryButtonRow.transform, false);
            var sellBg = sellGo.AddComponent<Image>();
            sellBg.color = ColorPalette.Hp;
            _sellButton = sellGo.AddComponent<Button>();
            _sellButton.targetGraphic = sellBg;
            _sellButton.onClick.AddListener(OnSellClicked);

            var sellTextGo = new GameObject("Text");
            sellTextGo.transform.SetParent(sellGo.transform, false);
            _sellButtonText = sellTextGo.AddComponent<TextMeshProUGUI>();
            _sellButtonText.text = "\ud310\ub9e4";
            _sellButtonText.fontSize = 22;
            _sellButtonText.color = Color.white;
            _sellButtonText.alignment = TextAlignmentOptions.Center;
            _sellButtonText.raycastTarget = false;
            UIManager.StretchFull(sellTextGo.GetComponent<RectTransform>());

            var cancelGo = new GameObject("CancelBtn");
            cancelGo.transform.SetParent(_inventoryButtonRow.transform, false);
            var cancelBg = cancelGo.AddComponent<Image>();
            cancelBg.color = ColorPalette.ButtonSecondary;
            var cancelBtn = cancelGo.AddComponent<Button>();
            cancelBtn.targetGraphic = cancelBg;
            cancelBtn.onClick.AddListener(OnDetailCancelClicked);

            var cancelTextGo = new GameObject("Text");
            cancelTextGo.transform.SetParent(cancelGo.transform, false);
            var cancelText = cancelTextGo.AddComponent<TextMeshProUGUI>();
            cancelText.text = "\ub2eb\uae30";
            cancelText.fontSize = 22;
            cancelText.color = Color.white;
            cancelText.alignment = TextAlignmentOptions.Center;
            cancelText.raycastTarget = false;
            UIManager.StretchFull(cancelTextGo.GetComponent<RectTransform>());

            _detailPanel.gameObject.SetActive(false);
        }

        private void BuildTotalStats(Transform parent)
        {
            var statsGo = new GameObject("TotalStats");
            statsGo.transform.SetParent(parent, false);
            var statsLe = statsGo.AddComponent<LayoutElement>();
            statsLe.preferredHeight = 32;
            statsGo.AddComponent<Image>().color = ColorPalette.Card;

            var statsLayout = statsGo.AddComponent<HorizontalLayoutGroup>();
            statsLayout.padding = new RectOffset(12, 12, 4, 4);
            statsLayout.childForceExpandWidth = true;
            statsLayout.childForceExpandHeight = true;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(statsGo.transform, false);
            _totalStatsText = textGo.AddComponent<TextMeshProUGUI>();
            _totalStatsText.fontSize = 22;
            _totalStatsText.color = ColorPalette.Text;
            _totalStatsText.alignment = TextAlignmentOptions.Center;
            _totalStatsText.raycastTarget = false;
        }

        private void BuildInventoryHeader(Transform parent)
        {
            var headerGo = new GameObject("InvHeader");
            headerGo.transform.SetParent(parent, false);
            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 30;

            var headerLayout = headerGo.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 12;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = true;

            var invLabelGo = new GameObject("InvLabel");
            invLabelGo.transform.SetParent(headerGo.transform, false);
            var invLabelLe = invLabelGo.AddComponent<LayoutElement>();
            invLabelLe.flexibleWidth = 1;
            _inventoryHeaderText = invLabelGo.AddComponent<TextMeshProUGUI>();
            _inventoryHeaderText.text = "\ubcf4\uad00\ud568 (0)";
            _inventoryHeaderText.fontSize = 24;
            _inventoryHeaderText.color = ColorPalette.Text;
            _inventoryHeaderText.alignment = TextAlignmentOptions.Left;
            _inventoryHeaderText.raycastTarget = false;

            var stonesGo = new GameObject("Stones");
            stonesGo.transform.SetParent(headerGo.transform, false);
            var stonesLe = stonesGo.AddComponent<LayoutElement>();
            stonesLe.preferredWidth = 200;
            _stonesHeaderText = stonesGo.AddComponent<TextMeshProUGUI>();
            _stonesHeaderText.text = "\uac15\ud654\uc11d: 0";
            _stonesHeaderText.fontSize = 22;
            _stonesHeaderText.color = ColorPalette.Gold;
            _stonesHeaderText.alignment = TextAlignmentOptions.Right;
            _stonesHeaderText.raycastTarget = false;

            var forgeBtnGo = new GameObject("ForgeBtn");
            forgeBtnGo.transform.SetParent(headerGo.transform, false);
            var forgeBtnLe = forgeBtnGo.AddComponent<LayoutElement>();
            forgeBtnLe.preferredWidth = 50;
            var forgeBtnBg = forgeBtnGo.AddComponent<Image>();
            forgeBtnBg.color = ColorPalette.GradeEpic;
            var forgeBtn = forgeBtnGo.AddComponent<Button>();
            forgeBtn.targetGraphic = forgeBtnBg;
            forgeBtn.onClick.AddListener(() => OnTabChanged(1));
            var forgeBtnTextGo = new GameObject("Text");
            forgeBtnTextGo.transform.SetParent(forgeBtnGo.transform, false);
            var forgeBtnText = forgeBtnTextGo.AddComponent<TextMeshProUGUI>();
            forgeBtnText.text = "\uD569\uC131";
            forgeBtnText.fontSize = 18;
            forgeBtnText.color = Color.white;
            forgeBtnText.fontStyle = FontStyles.Bold;
            forgeBtnText.alignment = TextAlignmentOptions.Center;
            forgeBtnText.raycastTarget = false;
            UIManager.StretchFull(forgeBtnTextGo.GetComponent<RectTransform>());
        }

        private void BuildFilterBar(Transform parent)
        {
            var filterGo = new GameObject("FilterBar");
            filterGo.transform.SetParent(parent, false);
            var filterLe = filterGo.AddComponent<LayoutElement>();
            filterLe.preferredHeight = 30;

            var filterLayout = filterGo.AddComponent<HorizontalLayoutGroup>();
            filterLayout.spacing = 2;
            filterLayout.childForceExpandWidth = true;
            filterLayout.childForceExpandHeight = true;
            filterLayout.padding = new RectOffset(2, 2, 2, 2);

            string[] filterLabels = { "\uc804\uccb4", "\ubb34\uae30", "\ubc29\uc5b4\uad6c", "\ubc18\uc9c0", "\ubaa9\uac78\uc774", "\uc2e0\ubc1c", "\uc7a5\uac11", "\ubaa8\uc790" };
            _filterBgs = new Image[filterLabels.Length];
            _filterTexts = new TextMeshProUGUI[filterLabels.Length];

            for (int i = 0; i < filterLabels.Length; i++)
            {
                int idx = i;
                var btnGo = new GameObject("Filter_" + filterLabels[i]);
                btnGo.transform.SetParent(filterGo.transform, false);
                _filterBgs[i] = btnGo.AddComponent<Image>();
                _filterBgs[i].color = i == 0 ? ColorPalette.ButtonPrimary : ColorPalette.ButtonSecondary;
                var btn = btnGo.AddComponent<Button>();
                btn.targetGraphic = _filterBgs[i];
                btn.onClick.AddListener(() => OnFilterChanged(idx));

                var textGo = new GameObject("Text");
                textGo.transform.SetParent(btnGo.transform, false);
                _filterTexts[i] = textGo.AddComponent<TextMeshProUGUI>();
                _filterTexts[i].text = filterLabels[i];
                _filterTexts[i].fontSize = 16;
                _filterTexts[i].color = i == 0 ? Color.white : ColorPalette.TextDim;
                _filterTexts[i].alignment = TextAlignmentOptions.Center;
                _filterTexts[i].raycastTarget = false;
                var textRt = textGo.GetComponent<RectTransform>();
                UIManager.StretchFull(textRt);
            }
        }

        private void BuildInventoryGrid(Transform parent)
        {
            var invGo = new GameObject("InventoryGrid");
            invGo.transform.SetParent(parent, false);
            _inventoryGrid = invGo.GetComponent<RectTransform>();

            if (_inventoryGrid == null) _inventoryGrid = invGo.AddComponent<RectTransform>();
            var invLe = invGo.AddComponent<LayoutElement>();
            invLe.preferredHeight = 400;

            var grid = invGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(120, 160);
            grid.spacing = new Vector2(8, 8);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
        }

        private void BuildForgeTab()
        {
            var panelGo = new GameObject("ForgeTab");
            panelGo.transform.SetParent(transform, false);
            _forgeTabPanel = panelGo.GetComponent<RectTransform>();

            if (_forgeTabPanel == null) _forgeTabPanel = panelGo.AddComponent<RectTransform>();
            var panelLe = panelGo.AddComponent<LayoutElement>();
            panelLe.flexibleHeight = 1;

            var scrollGo = new GameObject("ScrollView");
            scrollGo.transform.SetParent(panelGo.transform, false);
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
            _forgeGridContent = contentGo.GetComponent<RectTransform>();

            if (_forgeGridContent == null) _forgeGridContent = contentGo.AddComponent<RectTransform>();
            _forgeGridContent.anchorMin = new Vector2(0, 1);
            _forgeGridContent.anchorMax = new Vector2(1, 1);
            _forgeGridContent.pivot = new Vector2(0.5f, 1);
            _forgeGridContent.offsetMin = Vector2.zero;
            _forgeGridContent.offsetMax = Vector2.zero;

            var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 10;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.padding = new RectOffset(12, 12, 12, 12);

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = _forgeGridContent;
            scrollRect.viewport = viewportRt;

            BuildForgeResultPreview(contentGo.transform);
            BuildForgeMaterialRow(contentGo.transform);
            BuildForgeMergeButtons(contentGo.transform);
            BuildForgeInventoryGrid(contentGo.transform);
        }

        private void BuildForgeResultPreview(Transform parent)
        {
            _forgeResultArea = new GameObject("ResultPreview");
            _forgeResultArea.transform.SetParent(parent, false);
            var resultLe = _forgeResultArea.AddComponent<LayoutElement>();
            resultLe.preferredHeight = 120;
            _forgeResultArea.AddComponent<Image>().color = ColorPalette.Card;

            var resultLayout = _forgeResultArea.AddComponent<HorizontalLayoutGroup>();
            resultLayout.spacing = 12;
            resultLayout.padding = new RectOffset(16, 16, 12, 12);
            resultLayout.childForceExpandWidth = false;
            resultLayout.childForceExpandHeight = true;
            resultLayout.childAlignment = TextAnchor.MiddleCenter;

            var iconGo = new GameObject("ResultIcon");
            iconGo.transform.SetParent(_forgeResultArea.transform, false);
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 90;
            _forgeResultIcon = iconGo.AddComponent<Image>();
            _forgeResultIcon.color = ColorPalette.CardLight;

            var innerGo = new GameObject("Inner");
            innerGo.transform.SetParent(iconGo.transform, false);
            var innerRt = innerGo.AddComponent<RectTransform>();
            innerRt.anchorMin = Vector2.zero;
            innerRt.anchorMax = Vector2.one;
            innerRt.offsetMin = new Vector2(3, 3);
            innerRt.offsetMax = new Vector2(-3, -3);
            var innerImg = innerGo.AddComponent<Image>();
            innerImg.color = ColorPalette.Card;

            var spriteGo = new GameObject("Sprite");
            spriteGo.transform.SetParent(iconGo.transform, false);
            _forgeResultIconSprite = spriteGo.AddComponent<Image>();
            _forgeResultIconSprite.preserveAspect = true;
            _forgeResultIconSprite.raycastTarget = false;
            _forgeResultIconSprite.gameObject.SetActive(false);
            var spriteRt = spriteGo.GetComponent<RectTransform>();
            spriteRt.anchorMin = new Vector2(0.1f, 0.1f);
            spriteRt.anchorMax = new Vector2(0.9f, 0.9f);
            spriteRt.offsetMin = Vector2.zero;
            spriteRt.offsetMax = Vector2.zero;

            var infoGo = new GameObject("ResultInfo");
            infoGo.transform.SetParent(_forgeResultArea.transform, false);
            var infoLe = infoGo.AddComponent<LayoutElement>();
            infoLe.flexibleWidth = 1;
            var infoLayout = infoGo.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 4;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;
            infoLayout.childAlignment = TextAnchor.MiddleLeft;

            var nameGo = new GameObject("Name");
            nameGo.transform.SetParent(infoGo.transform, false);
            var nameLe = nameGo.AddComponent<LayoutElement>();
            nameLe.preferredHeight = 30;
            _forgeResultNameText = nameGo.AddComponent<TextMeshProUGUI>();
            _forgeResultNameText.text = "\uC7A5\uBE44\uB97C \uC120\uD0DD\uD558\uC138\uC694";
            _forgeResultNameText.fontSize = 24;
            _forgeResultNameText.color = ColorPalette.TextDim;
            _forgeResultNameText.alignment = TextAlignmentOptions.Left;
            _forgeResultNameText.raycastTarget = false;

            var statsGo = new GameObject("Stats");
            statsGo.transform.SetParent(infoGo.transform, false);
            var statsLe = statsGo.AddComponent<LayoutElement>();
            statsLe.preferredHeight = 24;
            _forgeResultStatsText = statsGo.AddComponent<TextMeshProUGUI>();
            _forgeResultStatsText.text = "";
            _forgeResultStatsText.fontSize = 20;
            _forgeResultStatsText.color = ColorPalette.TextDim;
            _forgeResultStatsText.alignment = TextAlignmentOptions.Left;
            _forgeResultStatsText.raycastTarget = false;

            _forgeResultArea.SetActive(false);
        }

        private void BuildForgeMaterialRow(Transform parent)
        {
            var rowGo = new GameObject("MaterialRow");
            rowGo.transform.SetParent(parent, false);
            var rowLe = rowGo.AddComponent<LayoutElement>();
            rowLe.preferredHeight = 110;

            var rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8;
            rowLayout.padding = new RectOffset(16, 16, 8, 8);
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;

            var starGo = new GameObject("StarSlot");
            starGo.transform.SetParent(rowGo.transform, false);
            var starLe = starGo.AddComponent<LayoutElement>();
            starLe.preferredWidth = 80;
            _forgeStarIcon = starGo.AddComponent<Image>();
            _forgeStarIcon.color = ColorPalette.CardLight;

            var starInner = new GameObject("Inner");
            starInner.transform.SetParent(starGo.transform, false);
            var starInnerRt = starInner.AddComponent<RectTransform>();
            starInnerRt.anchorMin = Vector2.zero;
            starInnerRt.anchorMax = Vector2.one;
            starInnerRt.offsetMin = new Vector2(3, 3);
            starInnerRt.offsetMax = new Vector2(-3, -3);
            starInner.AddComponent<Image>().color = ColorPalette.Card;

            var starSpriteGo = new GameObject("Sprite");
            starSpriteGo.transform.SetParent(starGo.transform, false);
            _forgeStarSprite = starSpriteGo.AddComponent<Image>();
            _forgeStarSprite.preserveAspect = true;
            _forgeStarSprite.raycastTarget = false;
            _forgeStarSprite.gameObject.SetActive(false);
            var starSpriteRt = starSpriteGo.GetComponent<RectTransform>();
            starSpriteRt.anchorMin = new Vector2(0.1f, 0.1f);
            starSpriteRt.anchorMax = new Vector2(0.9f, 0.9f);
            starSpriteRt.offsetMin = Vector2.zero;
            starSpriteRt.offsetMax = Vector2.zero;

            var labelGo = new GameObject("StarLabel");
            labelGo.transform.SetParent(starGo.transform, false);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = "\u2605";
            labelTmp.fontSize = 28;
            labelTmp.color = ColorPalette.Gold;
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.raycastTarget = false;
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            _forgeMaterialSlots = new GameObject[2];
            _forgeMaterialBgs = new Image[2];
            _forgeMaterialSprites = new Image[2];
            _forgeMaterialTexts = new TextMeshProUGUI[2];

            for (int i = 0; i < 2; i++)
            {
                var plusGo = new GameObject("Plus" + i);
                plusGo.transform.SetParent(rowGo.transform, false);
                var plusLe = plusGo.AddComponent<LayoutElement>();
                plusLe.preferredWidth = 30;
                var plusText = plusGo.AddComponent<TextMeshProUGUI>();
                plusText.text = "+";
                plusText.fontSize = 30;
                plusText.color = ColorPalette.TextDim;
                plusText.alignment = TextAlignmentOptions.Center;
                plusText.raycastTarget = false;

                var matGo = new GameObject("Material" + i);
                matGo.transform.SetParent(rowGo.transform, false);
                var matLe = matGo.AddComponent<LayoutElement>();
                matLe.preferredWidth = 80;
                _forgeMaterialBgs[i] = matGo.AddComponent<Image>();
                _forgeMaterialBgs[i].color = ColorPalette.CardLight;

                var matInner = new GameObject("Inner");
                matInner.transform.SetParent(matGo.transform, false);
                var matInnerRt = matInner.AddComponent<RectTransform>();
                matInnerRt.anchorMin = Vector2.zero;
                matInnerRt.anchorMax = Vector2.one;
                matInnerRt.offsetMin = new Vector2(3, 3);
                matInnerRt.offsetMax = new Vector2(-3, -3);
                matInner.AddComponent<Image>().color = ColorPalette.Card;

                var matSpriteGo = new GameObject("Sprite");
                matSpriteGo.transform.SetParent(matGo.transform, false);
                _forgeMaterialSprites[i] = matSpriteGo.AddComponent<Image>();
                _forgeMaterialSprites[i].preserveAspect = true;
                _forgeMaterialSprites[i].raycastTarget = false;
                _forgeMaterialSprites[i].gameObject.SetActive(false);
                var matSpriteRt = matSpriteGo.GetComponent<RectTransform>();
                matSpriteRt.anchorMin = new Vector2(0.1f, 0.1f);
                matSpriteRt.anchorMax = new Vector2(0.9f, 0.9f);
                matSpriteRt.offsetMin = Vector2.zero;
                matSpriteRt.offsetMax = Vector2.zero;

                var matTextGo = new GameObject("Label");
                matTextGo.transform.SetParent(matGo.transform, false);
                _forgeMaterialTexts[i] = matTextGo.AddComponent<TextMeshProUGUI>();
                _forgeMaterialTexts[i].text = "?";
                _forgeMaterialTexts[i].fontSize = 24;
                _forgeMaterialTexts[i].color = ColorPalette.TextDim;
                _forgeMaterialTexts[i].alignment = TextAlignmentOptions.Center;
                _forgeMaterialTexts[i].raycastTarget = false;
                var matTextRt = matTextGo.GetComponent<RectTransform>();
                matTextRt.anchorMin = Vector2.zero;
                matTextRt.anchorMax = Vector2.one;
                matTextRt.offsetMin = Vector2.zero;
                matTextRt.offsetMax = Vector2.zero;

                _forgeMaterialSlots[i] = matGo;
            }
        }

        private void BuildForgeMergeButtons(Transform parent)
        {
            var btnRowGo = new GameObject("ForgeBtnRow");
            btnRowGo.transform.SetParent(parent, false);
            var btnRowLe = btnRowGo.AddComponent<LayoutElement>();
            btnRowLe.preferredHeight = 44;
            var btnRowLayout = btnRowGo.AddComponent<HorizontalLayoutGroup>();
            btnRowLayout.spacing = 8;
            btnRowLayout.childForceExpandWidth = true;
            btnRowLayout.childForceExpandHeight = true;

            var mergeGo = new GameObject("MergeBtn");
            mergeGo.transform.SetParent(btnRowGo.transform, false);
            var mergeBg = mergeGo.AddComponent<Image>();
            mergeBg.color = ColorPalette.GradeLegendary;
            _forgeMergeButton = mergeGo.AddComponent<Button>();
            _forgeMergeButton.targetGraphic = mergeBg;
            _forgeMergeButton.onClick.AddListener(OnForgeSelectedMerge);
            _forgeMergeButton.interactable = false;
            var mergeTextGo = new GameObject("Text");
            mergeTextGo.transform.SetParent(mergeGo.transform, false);
            _forgeMergeButtonText = mergeTextGo.AddComponent<TextMeshProUGUI>();
            _forgeMergeButtonText.text = "\uD569\uC131";
            _forgeMergeButtonText.fontSize = 24;
            _forgeMergeButtonText.color = Color.white;
            _forgeMergeButtonText.fontStyle = FontStyles.Bold;
            _forgeMergeButtonText.alignment = TextAlignmentOptions.Center;
            _forgeMergeButtonText.raycastTarget = false;
            UIManager.StretchFull(mergeTextGo.GetComponent<RectTransform>());

            var bulkGo = new GameObject("BulkMergeBtn");
            bulkGo.transform.SetParent(btnRowGo.transform, false);
            var bulkBg = bulkGo.AddComponent<Image>();
            bulkBg.color = ColorPalette.ButtonPrimary;
            _forgeBulkMergeButton = bulkGo.AddComponent<Button>();
            _forgeBulkMergeButton.targetGraphic = bulkBg;
            _forgeBulkMergeButton.onClick.AddListener(OnBulkMergeClicked);
            var bulkTextGo = new GameObject("Text");
            bulkTextGo.transform.SetParent(bulkGo.transform, false);
            _forgeBulkMergeText = bulkTextGo.AddComponent<TextMeshProUGUI>();
            _forgeBulkMergeText.text = "\uC77C\uAD04 \uD569\uC131 (0)";
            _forgeBulkMergeText.fontSize = 22;
            _forgeBulkMergeText.color = Color.white;
            _forgeBulkMergeText.alignment = TextAlignmentOptions.Center;
            _forgeBulkMergeText.raycastTarget = false;
            UIManager.StretchFull(bulkTextGo.GetComponent<RectTransform>());
        }

        private void BuildForgeInventoryGrid(Transform parent)
        {
            var gridGo = new GameObject("ForgeInvGrid");
            gridGo.transform.SetParent(parent, false);
            _forgeInventoryGrid = gridGo.GetComponent<RectTransform>();
            if (_forgeInventoryGrid == null) _forgeInventoryGrid = gridGo.AddComponent<RectTransform>();
            var gridLe = gridGo.AddComponent<LayoutElement>();
            gridLe.preferredHeight = 400;

            var grid = gridGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(120, 160);
            grid.spacing = new Vector2(8, 8);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;
        }

        private void OnTabChanged(int index)
        {
            _activeTab = index;

            _equipTabBg.color = index == 0 ? ColorPalette.ButtonPrimary : ColorPalette.ButtonSecondary;
            _equipTabText.color = index == 0 ? Color.white : ColorPalette.TextDim;
            _forgeTabBg.color = index == 1 ? ColorPalette.ButtonPrimary : ColorPalette.ButtonSecondary;
            _forgeTabText.color = index == 1 ? Color.white : ColorPalette.TextDim;

            _equipmentTabPanel.gameObject.SetActive(index == 0);
            _forgeTabPanel.gameObject.SetActive(index == 1);

            if (index == 1) RefreshForge();
        }

        private void OnFilterChanged(int index)
        {
            _activeFilterIndex = index;
            for (int i = 0; i < _filterBgs.Length; i++)
            {
                _filterBgs[i].color = i == index ? ColorPalette.ButtonPrimary : ColorPalette.ButtonSecondary;
                _filterTexts[i].color = i == index ? Color.white : ColorPalette.TextDim;
            }
            RefreshInventory();
        }

        private void OnPaperDollSlotClicked(SlotType slotType, int index)
        {
            var slot = Game.Player.GetEquipmentSlot(slotType);
            if (index >= slot.Equipped.Length) return;
            var eq = slot.Equipped[index];
            if (eq == null) return;

            _selectedEquipment = eq;
            _selectedIsEquipped = true;
            _selectedSlotType = slotType;
            _selectedSlotIndex = index;
            ShowDetail(eq);
        }

        private void OnInventoryItemClicked(Equipment equipment)
        {
            _selectedEquipment = equipment;
            _selectedIsEquipped = false;
            ShowDetail(equipment);
        }

        private bool _isRequestPending;

        private void OnEquipClicked()
        {
            if (_isRequestPending || _selectedEquipment == null || _selectedIsEquipped) return;
            _isRequestPending = true;
            Game.EquipItemAsync(_selectedEquipment.Id, result =>
            {
                _isRequestPending = false;
                if (result.IsFail()) return;
                _selectedEquipment = null;
                _detailPanel.gameObject.SetActive(false);
                UI.Refresh();
            });
        }

        private void OnSellClicked()
        {
            if (_isRequestPending || _selectedEquipment == null || _selectedIsEquipped) return;
            _isRequestPending = true;
            Game.SellEquipmentAsync(_selectedEquipment.Id, result =>
            {
                _isRequestPending = false;
                if (result.IsFail()) return;
                _selectedEquipment = null;
                _detailPanel.gameObject.SetActive(false);
                UI.Refresh();
            });
        }

        private void OnDetailCancelClicked()
        {
            _selectedEquipment = null;
            _detailPanel.gameObject.SetActive(false);
        }

        private void ShowDetail(Equipment equipment)
        {
            _detailPanel.gameObject.SetActive(true);

            Color gradeColor = ColorPalette.GetEquipmentGradeColor(equipment.Grade);
            string gradeLabel = EquipmentDataTable.GetGradeLabel(equipment.Grade);
            string levelStr = equipment.Level > 0 ? $" +{equipment.Level}" : "";
            _detailNameText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(gradeColor)}>{gradeLabel} {equipment.Name}</color>{levelStr}";

            var stats = equipment.GetStats();
            var sb = new System.Text.StringBuilder();
            if (stats.Atk > 0) sb.Append($"ATK +{stats.Atk}  ");
            if (stats.MaxHp > 0) sb.Append($"HP +{stats.MaxHp}  ");
            if (stats.Def > 0) sb.Append($"DEF +{stats.Def}  ");
            if (stats.Crit > 0) sb.Append($"CRIT +{stats.Crit:F1}%");
            _detailStatsText.text = sb.ToString();

            _equippedButtonRow.SetActive(_selectedIsEquipped);
            _inventoryButtonRow.SetActive(!_selectedIsEquipped);

            if (_selectedIsEquipped)
            {
                int upgradeCost = equipment.GetUpgradeCost();
                _upgradeButtonText.text = $"\uac15\ud654 {NumberFormatter.FormatInt(upgradeCost)}\uc11d";
                int stones = (int)Game.Player.Resources.EquipmentStones;
                _upgradeButton.interactable = stones >= upgradeCost && !equipment.NeedsPromote();
                _unequipButton.interactable = true;
            }
            else
            {
                int sellPrice = EquipmentDataTable.GetSellPrice(equipment.Grade);
                _sellButtonText.text = $"\ud310\ub9e4 {NumberFormatter.FormatInt(sellPrice)}G";
            }
        }

        private void OnUpgradeClicked()
        {
            if (_isRequestPending || _selectedEquipment == null || !_selectedIsEquipped) return;
            _isRequestPending = true;
            var captured = _selectedEquipment;
            Game.UpgradeEquipmentAsync(_selectedEquipment.Id, result =>
            {
                _isRequestPending = false;
                if (result.IsOk())
                {
                    UI.Refresh();
                    ShowDetail(captured);
                }
            });
        }

        private void OnUnequipClicked()
        {
            if (_isRequestPending || _selectedEquipment == null || !_selectedIsEquipped) return;
            _isRequestPending = true;
            Game.UnequipItemAsync(_selectedSlotType, _selectedSlotIndex, result =>
            {
                _isRequestPending = false;
                if (result.IsFail()) return;
                _selectedEquipment = null;
                _detailPanel.gameObject.SetActive(false);
                UI.Refresh();
            });
        }

        private void OnBulkMergeClicked()
        {
            if (_isRequestPending || Game == null) return;
            _isRequestPending = true;
            Game.BulkForgeAsync(result =>
            {
                _isRequestPending = false;
                if (result.IsOk() && result.Data.MergedCount > 0)
                {
                    UI.Refresh();
                    RefreshForge();
                }
            });
        }

        private void OnMergeClicked(List<Equipment> group)
        {
            if (_isRequestPending) return;
            _isRequestPending = true;
            var ids = group.ConvertAll(eq => eq.Id);
            Game.ForgeEquipmentAsync(ids, result =>
            {
                _isRequestPending = false;
                if (result.IsOk())
                {
                    UI.Refresh();
                    RefreshForge();
                }
            });
        }

        public override void Refresh()
        {
            if (Game == null || Game.Player == null) return;

            RefreshTabLabels();
            RefreshPaperDoll();
            RefreshTotalStats();
            RefreshInventoryHeader();
            RefreshInventory();

            if (_selectedEquipment != null && _selectedIsEquipped)
                ShowDetail(_selectedEquipment);

            if (_activeTab == 1)
                RefreshForge();
        }

        private void RefreshTabLabels()
        {
            var candidates = Game.ForgeService.FindMergeCandidates(Game.Player.Inventory);
            _forgeTabText.text = $"\ud569\uc131 ({candidates.Count})";
        }

        private void RefreshPaperDoll()
        {
            foreach (var kv in _paperDollSlots)
            {
                string[] parts = kv.Key.Split('_');
                var slotType = (SlotType)Enum.Parse(typeof(SlotType), parts[0]);
                int index = int.Parse(parts[1]);

                var slot = Game.Player.GetEquipmentSlot(slotType);
                var cellGo = kv.Value;
                var borderImg = cellGo.GetComponent<Image>();
                var innerGo = cellGo.transform.Find("Inner");
                var innerImg = innerGo != null ? innerGo.GetComponent<Image>() : null;
                var slotLabel = cellGo.transform.Find("SlotLabel")?.GetComponent<TextMeshProUGUI>();

                var spriteImg = cellGo.transform.Find("EquipSprite")?.GetComponent<Image>();
                if (spriteImg == null)
                {
                    var spriteGo = new GameObject("EquipSprite");
                    spriteGo.transform.SetParent(cellGo.transform, false);
                    spriteImg = spriteGo.AddComponent<Image>();
                    spriteImg.preserveAspect = true;
                    spriteImg.raycastTarget = false;
                    var spriteRt = spriteGo.GetComponent<RectTransform>();
                    spriteRt.anchorMin = new Vector2(0.1f, 0.1f);
                    spriteRt.anchorMax = new Vector2(0.9f, 0.9f);
                    spriteRt.offsetMin = Vector2.zero;
                    spriteRt.offsetMax = Vector2.zero;
                }

                if (index < slot.Equipped.Length && slot.Equipped[index] != null)
                {
                    var eq = slot.Equipped[index];
                    Color gradeColor = ColorPalette.GetEquipmentGradeColor(eq.Grade);
                    borderImg.color = gradeColor;
                    if (innerImg != null)
                        innerImg.color = new Color(gradeColor.r * 0.3f, gradeColor.g * 0.3f, gradeColor.b * 0.3f, 1f);

                    string levelStr = eq.Level > 0 ? $"+{eq.Level}" : "";
                    if (slotLabel != null)
                    {
                        slotLabel.text = levelStr;
                        slotLabel.color = gradeColor;
                        slotLabel.alignment = TextAlignmentOptions.Bottom;
                    }

                    spriteImg.gameObject.SetActive(true);
                    spriteImg.color = Color.white;
                    if (SpriteManager.Instance != null)
                        spriteImg.sprite = SpriteManager.Instance.GetEquipmentIcon(eq.Slot, eq.Grade);
                }
                else
                {
                    borderImg.color = ColorPalette.CardLight;
                    if (innerImg != null)
                        innerImg.color = ColorPalette.Card;

                    string label;
                    SlotLabels.TryGetValue(slotType, out label);
                    if (slotType == SlotType.RING)
                        label = index == 0 ? "\ubc18\uc9c01" : "\ubc18\uc9c02";
                    if (slotLabel != null)
                    {
                        slotLabel.text = label ?? "";
                        slotLabel.color = ColorPalette.TextDim;
                        slotLabel.alignment = TextAlignmentOptions.Center;
                    }

                    spriteImg.gameObject.SetActive(false);
                }
            }
        }

        private void RefreshTotalStats()
        {
            var totalStats = Game.Player.ComputeStats();
            _totalStatsText.text = $"ATK {totalStats.Atk}  /  HP {totalStats.MaxHp}  /  DEF {totalStats.Def}  /  CRIT {totalStats.Crit:F1}%";
        }

        private void RefreshInventoryHeader()
        {
            int count = Game.Player.Inventory.Count;
            int stones = (int)Game.Player.Resources.EquipmentStones;
            _inventoryHeaderText.text = $"\ubcf4\uad00\ud568 ({count})";
            _stonesHeaderText.text = $"\uac15\ud654\uc11d: {NumberFormatter.FormatInt(stones)}";
        }

        private void RefreshInventory()
        {
            foreach (var go in _inventoryItems)
                Destroy(go);
            _inventoryItems.Clear();

            IEnumerable<Equipment> filtered = Game.Player.Inventory;

            if (_activeFilterIndex > 0 && _activeFilterIndex - 1 < FilterSlotOrder.Length)
            {
                SlotType filterSlot = FilterSlotOrder[_activeFilterIndex - 1];
                filtered = filtered.Where(e => e.Slot == filterSlot);
            }

            var sorted = filtered
                .OrderByDescending(e => e.GetGradeIndex())
                .ThenBy(e => e.Slot)
                .ThenByDescending(e => e.MergeLevel)
                .ToList();

            foreach (var eq in sorted)
            {
                var itemGo = new GameObject("InvItem");
                itemGo.transform.SetParent(_inventoryGrid, false);

                var itemLayout = itemGo.AddComponent<VerticalLayoutGroup>();
                itemLayout.spacing = 0;
                itemLayout.childForceExpandWidth = true;
                itemLayout.childForceExpandHeight = false;
                itemLayout.childAlignment = TextAnchor.UpperCenter;

                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(itemGo.transform, false);
                var iconLe = iconGo.AddComponent<LayoutElement>();
                iconLe.preferredHeight = 120;

                Color gradeColor = ColorPalette.GetEquipmentGradeColor(eq.Grade);

                var borderImg = iconGo.AddComponent<Image>();
                borderImg.color = gradeColor;

                var innerGo = new GameObject("Inner");
                innerGo.transform.SetParent(iconGo.transform, false);
                var innerImg = innerGo.AddComponent<Image>();
                innerImg.color = new Color(gradeColor.r * 0.3f, gradeColor.g * 0.3f, gradeColor.b * 0.3f, 1f);
                var innerRt = innerGo.GetComponent<RectTransform>();
                innerRt.anchorMin = Vector2.zero;
                innerRt.anchorMax = Vector2.one;
                innerRt.offsetMin = new Vector2(2, 2);
                innerRt.offsetMax = new Vector2(-2, -2);

                var spriteGo = new GameObject("Sprite");
                spriteGo.transform.SetParent(iconGo.transform, false);
                var spriteImg = spriteGo.AddComponent<Image>();
                spriteImg.preserveAspect = true;
                spriteImg.raycastTarget = false;
                if (SpriteManager.Instance != null)
                    spriteImg.sprite = SpriteManager.Instance.GetEquipmentIcon(eq.Slot, eq.Grade);
                var spriteRt = spriteGo.GetComponent<RectTransform>();
                spriteRt.anchorMin = new Vector2(0.1f, 0.1f);
                spriteRt.anchorMax = new Vector2(0.9f, 0.9f);
                spriteRt.offsetMin = Vector2.zero;
                spriteRt.offsetMax = Vector2.zero;

                if (eq.MergeLevel > 0)
                {
                    var badgeGo = new GameObject("MergeBadge");
                    badgeGo.transform.SetParent(iconGo.transform, false);
                    var badgeBg = badgeGo.AddComponent<Image>();
                    badgeBg.color = ColorPalette.GradeLegendary;
                    var badgeRt = badgeGo.GetComponent<RectTransform>();
                    badgeRt.anchorMin = new Vector2(1, 1);
                    badgeRt.anchorMax = new Vector2(1, 1);
                    badgeRt.pivot = new Vector2(1, 1);
                    badgeRt.sizeDelta = new Vector2(36, 28);

                    var badgeTextGo = new GameObject("Text");
                    badgeTextGo.transform.SetParent(badgeGo.transform, false);
                    var badgeText = badgeTextGo.AddComponent<TextMeshProUGUI>();
                    badgeText.text = $"+{eq.MergeLevel}";
                    badgeText.fontSize = 20;
                    badgeText.color = Color.white;
                    badgeText.alignment = TextAlignmentOptions.Center;
                    badgeText.raycastTarget = false;
                    var badgeTextRt = badgeTextGo.GetComponent<RectTransform>();
                    UIManager.StretchFull(badgeTextRt);
                }

                string slotAbbr = EquipmentDataTable.GetSlotLabel(eq.Slot);
                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(itemGo.transform, false);
                var labelLe = labelGo.AddComponent<LayoutElement>();
                labelLe.preferredHeight = 36;
                var labelText = labelGo.AddComponent<TextMeshProUGUI>();
                string gradeStr = EquipmentDataTable.GetGradeLabel(eq.Grade);
                string lvStr = eq.Level > 0 ? $" +{eq.Level}" : "";
                labelText.text = $"{slotAbbr}{lvStr}";
                labelText.fontSize = 22;
                labelText.color = ColorPalette.GetEquipmentGradeColor(eq.Grade);
                labelText.alignment = TextAlignmentOptions.Center;
                labelText.raycastTarget = false;

                var btn = iconGo.AddComponent<Button>();
                btn.targetGraphic = borderImg;
                var capturedEq = eq;
                btn.onClick.AddListener(() => OnInventoryItemClicked(capturedEq));

                _inventoryItems.Add(itemGo);
            }

            int rows = Mathf.CeilToInt(sorted.Count / 4f);
            float gridHeight = Mathf.Max(200, rows * 168 + 12);
            var gridLe = _inventoryGrid.GetComponent<LayoutElement>();
            gridLe.preferredHeight = gridHeight;
        }

        private void RefreshForge()
        {
            _forgeSelectedEquipment = null;
            RefreshForgeMaterialDisplay();
            RefreshForgeInventory();

            var candidates = Game.ForgeService.FindMergeCandidates(Game.Player.Inventory);
            _forgeBulkMergeText.text = $"\uC77C\uAD04 \uD569\uC131 ({candidates.Count})";
            _forgeBulkMergeButton.interactable = candidates.Count > 0;
        }

        private void OnForgeItemClicked(Equipment equipment)
        {
            _forgeSelectedEquipment = equipment;
            RefreshForgeMaterialDisplay();
        }

        private void RefreshForgeMaterialDisplay()
        {
            if (_forgeSelectedEquipment == null)
            {
                _forgeResultArea.SetActive(false);
                _forgeStarIcon.color = ColorPalette.CardLight;
                _forgeStarSprite.gameObject.SetActive(false);
                _forgeStarIcon.transform.Find("StarLabel").gameObject.SetActive(true);
                _forgeMergeButton.interactable = false;

                for (int i = 0; i < 2; i++)
                {
                    _forgeMaterialBgs[i].color = ColorPalette.CardLight;
                    _forgeMaterialSprites[i].gameObject.SetActive(false);
                    _forgeMaterialTexts[i].text = "?";
                    _forgeMaterialTexts[i].color = ColorPalette.TextDim;
                }
                return;
            }

            var eq = _forgeSelectedEquipment;
            Color gradeColor = ColorPalette.GetEquipmentGradeColor(eq.Grade);
            int required = Game.ForgeService.GetMergeRequirement(eq.Grade);

            _forgeStarIcon.color = gradeColor;
            _forgeStarSprite.gameObject.SetActive(true);
            _forgeStarIcon.transform.Find("StarLabel").gameObject.SetActive(false);
            if (SpriteManager.Instance != null)
                _forgeStarSprite.sprite = SpriteManager.Instance.GetEquipmentIcon(eq.Slot, eq.Grade);

            var subKey = eq.Slot == SlotType.WEAPON ? eq.WeaponSubTypeValue : null;
            int mlKey = EquipmentTable.IsHighGradeMerge(eq.Grade) ? eq.MergeLevel : -1;
            var sameItems = Game.Player.Inventory
                .Where(e => e.Id != eq.Id && e.Grade == eq.Grade && e.Slot == eq.Slot && !e.IsS
                    && (eq.Slot != SlotType.WEAPON || e.WeaponSubTypeValue == subKey)
                    && (mlKey < 0 || e.MergeLevel == mlKey))
                .ToList();

            int materialsNeeded = required > 0 ? required - 1 : 2;
            bool canMerge = sameItems.Count >= materialsNeeded && required > 0;

            for (int i = 0; i < 2; i++)
            {
                if (i < materialsNeeded)
                {
                    _forgeMaterialSlots[i].SetActive(true);
                    if (i < sameItems.Count)
                    {
                        _forgeMaterialBgs[i].color = gradeColor;
                        _forgeMaterialSprites[i].gameObject.SetActive(true);
                        if (SpriteManager.Instance != null)
                            _forgeMaterialSprites[i].sprite = SpriteManager.Instance.GetEquipmentIcon(sameItems[i].Slot, sameItems[i].Grade);
                        _forgeMaterialTexts[i].text = "";
                    }
                    else
                    {
                        _forgeMaterialBgs[i].color = new Color(gradeColor.r * 0.3f, gradeColor.g * 0.3f, gradeColor.b * 0.3f, 1f);
                        _forgeMaterialSprites[i].gameObject.SetActive(false);
                        string slotLabel = EquipmentDataTable.GetSlotLabel(eq.Slot);
                        _forgeMaterialTexts[i].text = slotLabel;
                        _forgeMaterialTexts[i].color = gradeColor;
                    }
                }
                else
                {
                    _forgeMaterialSlots[i].SetActive(false);
                }
            }

            _forgeMergeButton.interactable = canMerge;

            _forgeResultArea.SetActive(true);
            if (canMerge)
            {
                EquipmentGrade resultGrade;
                int resultMergeLevel = 0;
                if (EquipmentTable.IsHighGradeMerge(eq.Grade) && eq.MergeLevel < EquipmentTable.GetMergeEnhanceMax())
                {
                    resultGrade = eq.Grade;
                    resultMergeLevel = eq.MergeLevel + 1;
                }
                else
                {
                    var next = EquipmentTable.GetNextGrade(eq.Grade);
                    resultGrade = next.HasValue ? next.Value : eq.Grade;
                }

                Color resultColor = ColorPalette.GetEquipmentGradeColor(resultGrade);
                _forgeResultIcon.color = resultColor;
                _forgeResultIconSprite.gameObject.SetActive(true);
                if (SpriteManager.Instance != null)
                    _forgeResultIconSprite.sprite = SpriteManager.Instance.GetEquipmentIcon(eq.Slot, resultGrade);

                string resultLabel = EquipmentDataTable.GetGradeLabel(resultGrade);
                string slotName = EquipmentDataTable.GetSlotLabel(eq.Slot);
                string mlStr = resultMergeLevel > 0 ? $" +{resultMergeLevel}" : "";
                _forgeResultNameText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(resultColor)}>{resultLabel}</color> {slotName}{mlStr}";
                _forgeResultNameText.color = ColorPalette.Text;

                var simStats = EquipmentTable.GetBaseStats(eq.Slot, resultGrade);
                var sb = new System.Text.StringBuilder();
                if (simStats.Atk > 0) sb.Append($"ATK +{simStats.Atk}  ");
                if (simStats.MaxHp > 0) sb.Append($"HP +{simStats.MaxHp}  ");
                if (simStats.Def > 0) sb.Append($"DEF +{simStats.Def}  ");
                _forgeResultStatsText.text = sb.ToString();
            }
            else
            {
                _forgeResultIcon.color = new Color(gradeColor.r * 0.3f, gradeColor.g * 0.3f, gradeColor.b * 0.3f, 1f);
                _forgeResultIconSprite.gameObject.SetActive(false);
                string need = $"{EquipmentDataTable.GetSlotLabel(eq.Slot)} {materialsNeeded - sameItems.Count}\uAC1C \uBD80\uC871";
                _forgeResultNameText.text = need;
                _forgeResultNameText.color = ColorPalette.TextDim;
                _forgeResultStatsText.text = "";
            }
        }

        private void OnForgeSelectedMerge()
        {
            if (_forgeSelectedEquipment == null) return;

            var eq = _forgeSelectedEquipment;
            int required = Game.ForgeService.GetMergeRequirement(eq.Grade);
            if (required <= 0) return;

            var subKey = eq.Slot == SlotType.WEAPON ? eq.WeaponSubTypeValue : null;
            int mlKey = EquipmentTable.IsHighGradeMerge(eq.Grade) ? eq.MergeLevel : -1;
            var sameItems = Game.Player.Inventory
                .Where(e => e.Id != eq.Id && e.Grade == eq.Grade && e.Slot == eq.Slot && !e.IsS
                    && (eq.Slot != SlotType.WEAPON || e.WeaponSubTypeValue == subKey)
                    && (mlKey < 0 || e.MergeLevel == mlKey))
                .Take(required - 1)
                .ToList();

            if (sameItems.Count < required - 1) return;

            var mergeGroup = new List<Equipment> { eq };
            mergeGroup.AddRange(sameItems);

            OnMergeClicked(mergeGroup);
        }

        private void RefreshForgeInventory()
        {
            foreach (var go in _forgeInventoryItems)
                Destroy(go);
            _forgeInventoryItems.Clear();

            var sorted = Game.Player.Inventory
                .Where(e => !e.IsS)
                .OrderByDescending(e => e.GetGradeIndex())
                .ThenBy(e => e.Slot)
                .ThenByDescending(e => e.MergeLevel)
                .ToList();

            foreach (var eq in sorted)
            {
                var itemGo = new GameObject("ForgeInvItem");
                itemGo.transform.SetParent(_forgeInventoryGrid, false);
                Color gradeColor = ColorPalette.GetEquipmentGradeColor(eq.Grade);

                var itemLayout = itemGo.AddComponent<VerticalLayoutGroup>();
                itemLayout.spacing = 0;
                itemLayout.childForceExpandWidth = true;
                itemLayout.childForceExpandHeight = false;
                itemLayout.childAlignment = TextAnchor.UpperCenter;

                var iconGo = new GameObject("Icon");
                iconGo.transform.SetParent(itemGo.transform, false);
                var iconLe = iconGo.AddComponent<LayoutElement>();
                iconLe.preferredHeight = 120;

                var borderImg = iconGo.AddComponent<Image>();
                borderImg.color = gradeColor;

                var innerGo = new GameObject("Inner");
                innerGo.transform.SetParent(iconGo.transform, false);
                var innerImg = innerGo.AddComponent<Image>();
                innerImg.color = new Color(gradeColor.r * 0.3f, gradeColor.g * 0.3f, gradeColor.b * 0.3f, 1f);
                var innerRt = innerGo.GetComponent<RectTransform>();
                innerRt.anchorMin = Vector2.zero;
                innerRt.anchorMax = Vector2.one;
                innerRt.offsetMin = new Vector2(2, 2);
                innerRt.offsetMax = new Vector2(-2, -2);

                var spriteGo = new GameObject("Sprite");
                spriteGo.transform.SetParent(iconGo.transform, false);
                var spriteImg = spriteGo.AddComponent<Image>();
                spriteImg.preserveAspect = true;
                spriteImg.raycastTarget = false;
                if (SpriteManager.Instance != null)
                    spriteImg.sprite = SpriteManager.Instance.GetEquipmentIcon(eq.Slot, eq.Grade);
                var spriteRt = spriteGo.GetComponent<RectTransform>();
                spriteRt.anchorMin = new Vector2(0.1f, 0.1f);
                spriteRt.anchorMax = new Vector2(0.9f, 0.9f);
                spriteRt.offsetMin = Vector2.zero;
                spriteRt.offsetMax = Vector2.zero;

                string slotAbbr = EquipmentDataTable.GetSlotLabel(eq.Slot);
                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(itemGo.transform, false);
                var labelLe = labelGo.AddComponent<LayoutElement>();
                labelLe.preferredHeight = 36;
                var labelText = labelGo.AddComponent<TextMeshProUGUI>();
                string lvStr = eq.Level > 0 ? $" +{eq.Level}" : "";
                labelText.text = $"{slotAbbr}{lvStr}";
                labelText.fontSize = 22;
                labelText.color = gradeColor;
                labelText.alignment = TextAlignmentOptions.Center;
                labelText.raycastTarget = false;

                var btn = iconGo.AddComponent<Button>();
                btn.targetGraphic = borderImg;
                var capturedEq = eq;
                btn.onClick.AddListener(() => OnForgeItemClicked(capturedEq));

                _forgeInventoryItems.Add(itemGo);
            }

            int rows = Mathf.CeilToInt(sorted.Count / 4f);
            float gridHeight = Mathf.Max(200, rows * 168 + 12);
            var gridLe2 = _forgeInventoryGrid.GetComponent<LayoutElement>();
            gridLe2.preferredHeight = gridHeight;
        }

        public override void OnScreenEnter()
        {
            _selectedEquipment = null;
            _detailPanel.gameObject.SetActive(false);
            OnTabChanged(0);
            OnFilterChanged(0);
            base.OnScreenEnter();
        }
    }
}
