using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Services;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Data;
using CatCatGo.Domain.Entities;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Components;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Screens
{
    public class PetScreen : BaseScreen
    {
        private static readonly Dictionary<PetGrade, string> PET_GRADE_LABELS = new Dictionary<PetGrade, string>
        {
            { PetGrade.COMMON, "일반" },
            { PetGrade.RARE, "희귀" },
            { PetGrade.EPIC, "영웅" },
            { PetGrade.LEGENDARY, "전설" },
            { PetGrade.IMMORTAL, "불멸" },
        };

        private TextMeshProUGUI _eggCountText;
        private TextMeshProUGUI _foodCountText;
        private Button _hatchButton;

        private GameObject _emptyState;

        private GameObject _showcaseSection;
        private TextMeshProUGUI _showcaseInitial;
        private Image _showcaseIconBg;
        private TextMeshProUGUI _showcaseName;
        private TextMeshProUGUI _showcaseTierGrade;
        private GameObject _activeBadge;

        private GameObject _statsSection;
        private TextMeshProUGUI _levelText;
        private TextMeshProUGUI _statsText;
        private TextMeshProUGUI _abilityText;
        private TextMeshProUGUI _levelUpPreviewText;
        private ProgressBarView _expBar;
        private TextMeshProUGUI _feedInfoText;
        private Button _deployButton;
        private TextMeshProUGUI _deployButtonLabel;
        private Button _feedButton;
        private Button _maxLevelButton;
        private TextMeshProUGUI _maxLevelButtonLabel;

        private RectTransform _gridContent;
        private List<GridSlot> _gridSlots = new List<GridSlot>();

        private Pet _selectedPet;

        private struct GridSlot
        {
            public GameObject Root;
            public Image Background;
            public Image Border;
            public TextMeshProUGUI Initial;
            public Pet Pet;
        }

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
            BuildResourceCard(contentGo.transform);
            BuildEmptyState(contentGo.transform);
            BuildShowcase(contentGo.transform);
            BuildStatsCard(contentGo.transform);
            BuildPetGrid(contentGo.transform);
        }

        private void BuildHeader(Transform parent)
        {
            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(parent, false);
            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 36;
            var headerText = headerGo.AddComponent<TextMeshProUGUI>();
            headerText.text = "펫";
            headerText.fontSize = 30;
            headerText.color = ColorPalette.Text;
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.raycastTarget = false;
        }

        private void BuildResourceCard(Transform parent)
        {
            var cardGo = new GameObject("ResourceCard");
            cardGo.transform.SetParent(parent, false);
            cardGo.AddComponent<Image>().color = ColorPalette.Card;
            var cardLe = cardGo.AddComponent<LayoutElement>();
            cardLe.preferredHeight = 120;

            var cardLayout = cardGo.AddComponent<VerticalLayoutGroup>();
            cardLayout.spacing = 6;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;
            cardLayout.childAlignment = TextAnchor.MiddleCenter;
            cardLayout.padding = new RectOffset(16, 16, 10, 10);

            var eggRowGo = new GameObject("EggRow");
            eggRowGo.transform.SetParent(cardGo.transform, false);
            var eggRowLe = eggRowGo.AddComponent<LayoutElement>();
            eggRowLe.preferredHeight = 28;
            _eggCountText = eggRowGo.AddComponent<TextMeshProUGUI>();
            _eggCountText.fontSize = 24;
            _eggCountText.color = ColorPalette.Text;
            _eggCountText.alignment = TextAlignmentOptions.Left;
            _eggCountText.raycastTarget = false;

            var foodRowGo = new GameObject("FoodRow");
            foodRowGo.transform.SetParent(cardGo.transform, false);
            var foodRowLe = foodRowGo.AddComponent<LayoutElement>();
            foodRowLe.preferredHeight = 28;
            _foodCountText = foodRowGo.AddComponent<TextMeshProUGUI>();
            _foodCountText.fontSize = 24;
            _foodCountText.color = ColorPalette.Text;
            _foodCountText.alignment = TextAlignmentOptions.Left;
            _foodCountText.raycastTarget = false;

            var hatchRowGo = new GameObject("HatchRow");
            hatchRowGo.transform.SetParent(cardGo.transform, false);
            var hatchRowLe = hatchRowGo.AddComponent<LayoutElement>();
            hatchRowLe.preferredHeight = 40;
            _hatchButton = UIManager.CreateButton(hatchRowGo.transform, "알 부화", OnHatchClicked, "HatchBtn");
            _hatchButton.GetComponent<Image>().color = ColorPalette.Heal;
            var hatchBtnLabel = _hatchButton.GetComponentInChildren<TextMeshProUGUI>();
            hatchBtnLabel.fontSize = 24;
        }

        private void BuildEmptyState(Transform parent)
        {
            _emptyState = new GameObject("EmptyState");
            _emptyState.transform.SetParent(parent, false);
            var emptyLe = _emptyState.AddComponent<LayoutElement>();
            emptyLe.preferredHeight = 60;
            var emptyText = _emptyState.AddComponent<TextMeshProUGUI>();
            emptyText.text = "보유한 펫이 없습니다";
            emptyText.fontSize = 24;
            emptyText.color = ColorPalette.TextDim;
            emptyText.alignment = TextAlignmentOptions.Center;
            emptyText.raycastTarget = false;
        }

        private void BuildShowcase(Transform parent)
        {
            _showcaseSection = new GameObject("Showcase");
            _showcaseSection.transform.SetParent(parent, false);
            _showcaseSection.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.14f);
            var showcaseLe = _showcaseSection.AddComponent<LayoutElement>();
            showcaseLe.preferredHeight = 160;

            var showcaseLayout = _showcaseSection.AddComponent<VerticalLayoutGroup>();
            showcaseLayout.spacing = 4;
            showcaseLayout.childForceExpandWidth = true;
            showcaseLayout.childForceExpandHeight = false;
            showcaseLayout.childAlignment = TextAnchor.MiddleCenter;
            showcaseLayout.padding = new RectOffset(16, 16, 12, 12);

            var iconRowGo = new GameObject("IconRow");
            iconRowGo.transform.SetParent(_showcaseSection.transform, false);
            var iconRowLe = iconRowGo.AddComponent<LayoutElement>();
            iconRowLe.preferredHeight = 96;
            var iconRowLayout = iconRowGo.AddComponent<HorizontalLayoutGroup>();
            iconRowLayout.childForceExpandWidth = false;
            iconRowLayout.childForceExpandHeight = false;
            iconRowLayout.childAlignment = TextAnchor.MiddleCenter;

            var iconBgGo = new GameObject("IconBg");
            iconBgGo.transform.SetParent(iconRowGo.transform, false);
            var iconBgLe = iconBgGo.AddComponent<LayoutElement>();
            iconBgLe.preferredWidth = 96;
            iconBgLe.preferredHeight = 96;
            _showcaseIconBg = iconBgGo.AddComponent<Image>();
            _showcaseIconBg.color = ColorPalette.GradeCommon;

            var initialGo = new GameObject("Initial");
            initialGo.transform.SetParent(iconBgGo.transform, false);
            var initialRt = initialGo.GetComponent<RectTransform>();

            if (initialRt == null) initialRt = initialGo.AddComponent<RectTransform>();
            UIManager.StretchFull(initialRt);
            _showcaseInitial = initialGo.AddComponent<TextMeshProUGUI>();
            _showcaseInitial.fontSize = 48;
            _showcaseInitial.color = Color.white;
            _showcaseInitial.alignment = TextAlignmentOptions.Center;
            _showcaseInitial.raycastTarget = false;

            var nameGo = new GameObject("Name");
            nameGo.transform.SetParent(_showcaseSection.transform, false);
            var nameLe = nameGo.AddComponent<LayoutElement>();
            nameLe.preferredHeight = 28;
            _showcaseName = nameGo.AddComponent<TextMeshProUGUI>();
            _showcaseName.fontSize = 26;
            _showcaseName.color = ColorPalette.Text;
            _showcaseName.alignment = TextAlignmentOptions.Center;
            _showcaseName.raycastTarget = false;

            var tierGradeGo = new GameObject("TierGrade");
            tierGradeGo.transform.SetParent(_showcaseSection.transform, false);
            var tierGradeLe = tierGradeGo.AddComponent<LayoutElement>();
            tierGradeLe.preferredHeight = 28;
            _showcaseTierGrade = tierGradeGo.AddComponent<TextMeshProUGUI>();
            _showcaseTierGrade.fontSize = 22;
            _showcaseTierGrade.color = ColorPalette.TextDim;
            _showcaseTierGrade.alignment = TextAlignmentOptions.Center;
            _showcaseTierGrade.raycastTarget = false;

            _activeBadge = new GameObject("ActiveBadge");
            _activeBadge.transform.SetParent(_showcaseSection.transform, false);
            var badgeLe = _activeBadge.AddComponent<LayoutElement>();
            badgeLe.preferredHeight = 28;
            var badgeText = _activeBadge.AddComponent<TextMeshProUGUI>();
            badgeText.text = "출전 중";
            badgeText.fontSize = 22;
            badgeText.color = new Color(0.91f, 0.27f, 0.38f);
            badgeText.alignment = TextAlignmentOptions.Center;
            badgeText.raycastTarget = false;
        }

        private void BuildStatsCard(Transform parent)
        {
            _statsSection = new GameObject("StatsCard");
            _statsSection.transform.SetParent(parent, false);
            _statsSection.AddComponent<Image>().color = ColorPalette.Card;
            var statsLe = _statsSection.AddComponent<LayoutElement>();
            statsLe.preferredHeight = 320;

            var statsLayout = _statsSection.AddComponent<VerticalLayoutGroup>();
            statsLayout.spacing = 6;
            statsLayout.childForceExpandWidth = true;
            statsLayout.childForceExpandHeight = false;
            statsLayout.childAlignment = TextAnchor.UpperCenter;
            statsLayout.padding = new RectOffset(16, 16, 12, 12);

            var levelGo = new GameObject("Level");
            levelGo.transform.SetParent(_statsSection.transform, false);
            var levelLe = levelGo.AddComponent<LayoutElement>();
            levelLe.preferredHeight = 28;
            _levelText = levelGo.AddComponent<TextMeshProUGUI>();
            _levelText.fontSize = 26;
            _levelText.color = ColorPalette.Text;
            _levelText.alignment = TextAlignmentOptions.Left;
            _levelText.raycastTarget = false;

            var statsTextGo = new GameObject("Stats");
            statsTextGo.transform.SetParent(_statsSection.transform, false);
            var statsTextLe = statsTextGo.AddComponent<LayoutElement>();
            statsTextLe.preferredHeight = 28;
            _statsText = statsTextGo.AddComponent<TextMeshProUGUI>();
            _statsText.fontSize = 22;
            _statsText.color = ColorPalette.Text;
            _statsText.alignment = TextAlignmentOptions.Left;
            _statsText.raycastTarget = false;

            var abilityGo = new GameObject("Ability");
            abilityGo.transform.SetParent(_statsSection.transform, false);
            var abilityLe = abilityGo.AddComponent<LayoutElement>();
            abilityLe.preferredHeight = 28;
            _abilityText = abilityGo.AddComponent<TextMeshProUGUI>();
            _abilityText.fontSize = 22;
            _abilityText.color = ColorPalette.Gold;
            _abilityText.alignment = TextAlignmentOptions.Left;
            _abilityText.raycastTarget = false;

            var previewGo = new GameObject("LevelUpPreview");
            previewGo.transform.SetParent(_statsSection.transform, false);
            var previewLe = previewGo.AddComponent<LayoutElement>();
            previewLe.preferredHeight = 28;
            _levelUpPreviewText = previewGo.AddComponent<TextMeshProUGUI>();
            _levelUpPreviewText.fontSize = 22;
            _levelUpPreviewText.color = ColorPalette.TextDim;
            _levelUpPreviewText.alignment = TextAlignmentOptions.Left;
            _levelUpPreviewText.raycastTarget = false;

            var barGo = new GameObject("ExpBar");
            barGo.transform.SetParent(_statsSection.transform, false);
            var barLe = barGo.AddComponent<LayoutElement>();
            barLe.preferredHeight = 24;
            _expBar = barGo.AddComponent<ProgressBarView>();
            _expBar.Initialize(400, 24);
            _expBar.SetColor(ColorPalette.Heal);

            var feedInfoGo = new GameObject("FeedInfo");
            feedInfoGo.transform.SetParent(_statsSection.transform, false);
            var feedInfoLe = feedInfoGo.AddComponent<LayoutElement>();
            feedInfoLe.preferredHeight = 28;
            _feedInfoText = feedInfoGo.AddComponent<TextMeshProUGUI>();
            _feedInfoText.fontSize = 22;
            _feedInfoText.color = ColorPalette.TextDim;
            _feedInfoText.alignment = TextAlignmentOptions.Center;
            _feedInfoText.raycastTarget = false;

            var btnRowGo = new GameObject("ButtonRow");
            btnRowGo.transform.SetParent(_statsSection.transform, false);
            var btnRowLe = btnRowGo.AddComponent<LayoutElement>();
            btnRowLe.preferredHeight = 48;
            var btnRowLayout = btnRowGo.AddComponent<HorizontalLayoutGroup>();
            btnRowLayout.spacing = 8;
            btnRowLayout.childForceExpandWidth = true;
            btnRowLayout.childForceExpandHeight = true;

            _deployButton = UIManager.CreateButton(btnRowGo.transform, "출전", OnDeployClicked, "DeployBtn");
            _deployButton.GetComponent<Image>().color = ColorPalette.GradeLegendary;
            _deployButtonLabel = _deployButton.GetComponentInChildren<TextMeshProUGUI>();
            _deployButtonLabel.fontSize = 22;

            _feedButton = UIManager.CreateButton(btnRowGo.transform, "먹이주기", OnFeedClicked, "FeedBtn");
            _feedButton.GetComponent<Image>().color = ColorPalette.ButtonPrimary;
            _feedButton.GetComponentInChildren<TextMeshProUGUI>().fontSize = 22;

            _maxLevelButton = UIManager.CreateButton(btnRowGo.transform, "⬆Lv.?", OnMaxLevelClicked, "MaxLevelBtn");
            _maxLevelButton.GetComponent<Image>().color = ColorPalette.Heal;
            _maxLevelButtonLabel = _maxLevelButton.GetComponentInChildren<TextMeshProUGUI>();
            _maxLevelButtonLabel.fontSize = 22;
        }

        private void BuildPetGrid(Transform parent)
        {
            var gridGo = new GameObject("PetGrid");
            gridGo.transform.SetParent(parent, false);
            _gridContent = gridGo.GetComponent<RectTransform>();

            if (_gridContent == null) _gridContent = gridGo.AddComponent<RectTransform>();
            var gridLe = gridGo.AddComponent<LayoutElement>();
            gridLe.preferredHeight = 300;

            var grid = gridGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(56, 56);
            grid.spacing = new Vector2(8, 8);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
        }

        private bool _isRequestPending;

        private void OnHatchClicked()
        {
            if (_isRequestPending || Game == null) return;
            _isRequestPending = true;
            Game.HatchPetAsync(result =>
            {
                _isRequestPending = false;
                if (result.IsFail()) return;
                _selectedPet = result.Data;
                UI.Refresh();
            });
        }

        private void OnDeployClicked()
        {
            if (_isRequestPending || _selectedPet == null || Game == null) return;
            _isRequestPending = true;
            Game.DeployPetAsync(_selectedPet.Id, result =>
            {
                _isRequestPending = false;
                if (result.IsOk())
                    UI.Refresh();
            });
        }

        private void OnFeedClicked()
        {
            if (_isRequestPending || _selectedPet == null || Game == null) return;
            _isRequestPending = true;
            Game.FeedPetAsync(_selectedPet.Id, 1, result =>
            {
                _isRequestPending = false;
                if (result.IsOk())
                    UI.Refresh();
            });
        }

        private void OnMaxLevelClicked()
        {
            if (_isRequestPending || _selectedPet == null || Game == null) return;
            int food = (int)Game.Player.Resources.Get(ResourceType.PET_FOOD);
            if (food < 1) return;
            _isRequestPending = true;
            Game.FeedPetAsync(_selectedPet.Id, food, result =>
            {
                _isRequestPending = false;
                if (result.IsOk())
                    UI.Refresh();
            });
        }

        private void SelectPet(Pet pet)
        {
            _selectedPet = pet;
            Refresh();
        }

        public override void OnScreenEnter()
        {
            _selectedPet = Game.Player.ActivePet;
            base.OnScreenEnter();
        }

        public override void Refresh()
        {
            if (Game == null || Game.Player == null) return;

            if (_selectedPet == null)
                _selectedPet = Game.Player.ActivePet;

            RefreshResourceCard();
            RefreshVisibility();
            RefreshShowcase();
            RefreshStatsCard();
            RefreshGrid();
        }

        private void RefreshResourceCard()
        {
            int eggs = (int)Game.Player.Resources.Get(ResourceType.PET_EGG);
            int food = (int)Game.Player.Resources.Get(ResourceType.PET_FOOD);
            _eggCountText.text = $"펫 알: {eggs}";
            _foodCountText.text = $"펫 먹이: {food}";
            _hatchButton.interactable = eggs > 0;
        }

        private void RefreshVisibility()
        {
            bool hasPets = Game.Player.OwnedPets.Count > 0;
            _emptyState.SetActive(!hasPets);
            _showcaseSection.SetActive(hasPets);
            _statsSection.SetActive(hasPets && _selectedPet != null);
        }

        private void RefreshShowcase()
        {
            if (_selectedPet == null) return;

            _showcaseInitial.text = string.IsNullOrEmpty(_selectedPet.Name)
                ? "?"
                : _selectedPet.Name.Substring(0, 1);
            _showcaseIconBg.color = GetTierColor(_selectedPet.Tier);
            _showcaseName.text = _selectedPet.Name;

            string gradeLabel;
            PET_GRADE_LABELS.TryGetValue(_selectedPet.Grade, out gradeLabel);
            if (gradeLabel == null) gradeLabel = _selectedPet.Grade.ToString();
            _showcaseTierGrade.text = $"[{_selectedPet.Tier}] {gradeLabel}";

            bool isActive = _selectedPet == Game.Player.ActivePet;
            _activeBadge.SetActive(isActive);
        }

        private void RefreshStatsCard()
        {
            if (_selectedPet == null) return;

            _levelText.text = $"Lv.{_selectedPet.Level}";

            var bonus = _selectedPet.GetGlobalBonus();
            _statsText.text = $"HP +{bonus.MaxHp}, 공격력 +{bonus.Atk}, 방어력 +{bonus.Def}";

            string abilityDesc = PetTable.GetAbilityDescription(_selectedPet.Id, _selectedPet.Grade);
            if (string.IsNullOrEmpty(abilityDesc))
            {
                var templates = PetTable.GetAllTemplates();
                var match = templates.Find(t => t.Name == _selectedPet.Name);
                if (match != null)
                    abilityDesc = PetTable.GetAbilityDescription(match.Id, _selectedPet.Grade);
            }
            _abilityText.text = string.IsNullOrEmpty(abilityDesc) ? "" : $"특수능력: {abilityDesc}";

            int nextLevelHp = (_selectedPet.Level + 1) * 2 * 2;
            int currentLevelHp = _selectedPet.Level * 2 * 2;
            int nextLevelAtk = (_selectedPet.Level + 1) * 2;
            int currentLevelAtk = _selectedPet.Level * 2;
            int hpGain = nextLevelHp - currentLevelHp;
            int atkGain = nextLevelAtk - currentLevelAtk;
            _levelUpPreviewText.text = $"레벨업 시: HP +{hpGain}, ATK +{atkGain}";

            int expNeeded = _selectedPet.GetExpToNextLevel();
            _expBar.SetProgress(_selectedPet.Exp, expNeeded, $"{_selectedPet.Exp}/{expNeeded}");

            int food = (int)Game.Player.Resources.Get(ResourceType.PET_FOOD);
            int expPerFood = 10;
            int foodForNextLevel = Mathf.Max(1, Mathf.CeilToInt((float)(expNeeded - _selectedPet.Exp) / expPerFood));
            _feedInfoText.text = $"먹이 {foodForNextLevel}개 → 레벨업";

            bool isActive = _selectedPet == Game.Player.ActivePet;
            _deployButton.interactable = !isActive;
            _deployButtonLabel.text = isActive ? "출전 중" : "출전";

            _feedButton.interactable = food >= 1;

            int maxReachableLevel = CalculateMaxReachableLevel(_selectedPet, food);
            _maxLevelButton.interactable = food >= 1 && maxReachableLevel > _selectedPet.Level;
            _maxLevelButtonLabel.text = $"⬆Lv.{maxReachableLevel}";
        }

        private int CalculateMaxReachableLevel(Pet pet, int foodAvailable)
        {
            int level = pet.Level;
            int exp = pet.Exp;
            int totalExpGain = foodAvailable * 10;
            exp += totalExpGain;

            int expToNext = 100 + (level - 1) * 20;
            while (exp >= expToNext)
            {
                exp -= expToNext;
                level++;
                expToNext = 100 + (level - 1) * 20;
            }

            return level;
        }

        private void RefreshGrid()
        {
            foreach (var slot in _gridSlots)
                Destroy(slot.Root);
            _gridSlots.Clear();

            foreach (var pet in Game.Player.OwnedPets)
            {
                var cellGo = new GameObject("Pet_" + pet.Name);
                cellGo.transform.SetParent(_gridContent, false);

                var cellBg = cellGo.AddComponent<Image>();
                cellBg.color = GetPetGradeColor(pet.Grade);

                var borderGo = new GameObject("Border");
                borderGo.transform.SetParent(cellGo.transform, false);
                var borderRt = borderGo.GetComponent<RectTransform>();

                if (borderRt == null) borderRt = borderGo.AddComponent<RectTransform>();
                UIManager.StretchFull(borderRt);
                var borderImg = borderGo.AddComponent<Image>();
                borderImg.color = Color.clear;

                Outline borderOutline = borderGo.AddComponent<Outline>();
                borderOutline.effectColor = Color.clear;
                borderOutline.effectDistance = new Vector2(2, 2);

                var textGo = new GameObject("Initial");
                textGo.transform.SetParent(cellGo.transform, false);
                var textRt = textGo.GetComponent<RectTransform>();

                if (textRt == null) textRt = textGo.AddComponent<RectTransform>();
                UIManager.StretchFull(textRt);
                var initialText = textGo.AddComponent<TextMeshProUGUI>();
                initialText.text = string.IsNullOrEmpty(pet.Name) ? "?" : pet.Name.Substring(0, 1);
                initialText.fontSize = 28;
                initialText.color = Color.white;
                initialText.alignment = TextAlignmentOptions.Center;
                initialText.raycastTarget = false;

                var btn = cellGo.AddComponent<Button>();
                btn.targetGraphic = cellBg;
                var capturedPet = pet;
                btn.onClick.AddListener(() => SelectPet(capturedPet));

                bool isSelected = pet == _selectedPet;
                bool isActive = pet == Game.Player.ActivePet;

                if (isActive)
                {
                    borderOutline.effectColor = new Color(0.91f, 0.27f, 0.38f);
                }
                else if (isSelected)
                {
                    borderOutline.effectColor = Color.white;
                }

                _gridSlots.Add(new GridSlot
                {
                    Root = cellGo,
                    Background = cellBg,
                    Border = borderImg,
                    Initial = initialText,
                    Pet = pet,
                });
            }

            int rowCount = Mathf.CeilToInt(Game.Player.OwnedPets.Count / 5f);
            var gridLe = _gridContent.GetComponent<LayoutElement>();
            gridLe.preferredHeight = Mathf.Max(64, rowCount * 64);
        }

        private Color GetPetGradeColor(PetGrade grade)
        {
            switch (grade)
            {
                case PetGrade.COMMON: return new Color(0.33f, 0.33f, 0.33f);
                case PetGrade.RARE: return new Color(0.08f, 0.40f, 0.75f);
                case PetGrade.EPIC: return new Color(0.42f, 0.11f, 0.60f);
                case PetGrade.LEGENDARY: return new Color(0.90f, 0.32f, 0.0f);
                case PetGrade.IMMORTAL: return new Color(0.72f, 0.11f, 0.11f);
                default: return Color.gray;
            }
        }

        private Color GetTierColor(PetTier tier)
        {
            switch (tier)
            {
                case PetTier.S: return ColorPalette.Gold;
                case PetTier.A: return ColorPalette.GradeEpic;
                case PetTier.B: return ColorPalette.Heal;
                default: return ColorPalette.GradeCommon;
            }
        }
    }
}
