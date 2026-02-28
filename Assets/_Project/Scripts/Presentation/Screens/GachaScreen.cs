using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Economy;
using CatCatGo.Domain.Data;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Components;
using CatCatGo.Presentation.Popups;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Screens
{
    public class GachaScreen : BaseScreen
    {
        private static readonly Dictionary<ChestType, string> CHEST_LABELS = new Dictionary<ChestType, string>
        {
            { ChestType.EQUIPMENT, "\uc7a5\ube44 \ubb51\uae30" },
            { ChestType.PET, "\ud3ab \ubb51\uae30" },
            { ChestType.GEM, "\ubcf4\uc11d \ubb51\uae30" },
        };

        private TabBarView _tabBar;
        private ChestType _activeChest = ChestType.EQUIPMENT;

        private TextMeshProUGUI _showcaseTitle;
        private Image _showcaseIcon;

        private TextMeshProUGUI _gemsText;
        private TextMeshProUGUI _cost1Text;
        private TextMeshProUGUI _cost10Text;
        private TextMeshProUGUI _pityText;
        private ProgressBarView _pityBar;

        private Button _pull1Button;
        private Button _pull10Button;
        private TextMeshProUGUI _pull1Label;
        private TextMeshProUGUI _pull10Label;

        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            var mainLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            mainLayout.spacing = 10;
            mainLayout.childForceExpandWidth = true;
            mainLayout.childForceExpandHeight = false;
            mainLayout.padding = new RectOffset(16, 16, 12, 12);

            BuildTabBar(transform);
            BuildShowcase(transform);
            BuildInfoCard(transform);
            BuildButtons(transform);
        }

        private void BuildTabBar(Transform parent)
        {
            var tabGo = new GameObject("TabBar");
            tabGo.transform.SetParent(parent, false);
            var tabLe = tabGo.AddComponent<LayoutElement>();
            tabLe.preferredHeight = 44;
            tabLe.flexibleHeight = 0;
            tabGo.AddComponent<Image>().color = ColorPalette.Card;
            _tabBar = tabGo.AddComponent<TabBarView>();
            _tabBar.Initialize(new[] { "\uc7a5\ube44", "\ud3ab", "\ubcf4\uc11d" });
            _tabBar.OnTabSelected += OnTabChanged;
        }

        private void BuildShowcase(Transform parent)
        {
            var showcaseGo = new GameObject("Showcase");
            showcaseGo.transform.SetParent(parent, false);
            var showcaseLe = showcaseGo.AddComponent<LayoutElement>();
            showcaseLe.flexibleHeight = 1;
            showcaseGo.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.14f);

            var showcaseLayout = showcaseGo.AddComponent<VerticalLayoutGroup>();
            showcaseLayout.spacing = 8;
            showcaseLayout.childForceExpandWidth = true;
            showcaseLayout.childForceExpandHeight = false;
            showcaseLayout.childAlignment = TextAnchor.MiddleCenter;
            showcaseLayout.padding = new RectOffset(20, 20, 20, 20);

            var spacerTop = new GameObject("SpacerTop");
            spacerTop.transform.SetParent(showcaseGo.transform, false);
            var spacerTopLe = spacerTop.AddComponent<LayoutElement>();
            spacerTopLe.flexibleHeight = 1;

            var iconRowGo = new GameObject("IconRow");
            iconRowGo.transform.SetParent(showcaseGo.transform, false);
            var iconRowLe = iconRowGo.AddComponent<LayoutElement>();
            iconRowLe.preferredHeight = 120;
            iconRowLe.flexibleHeight = 0;
            var iconRowLayout = iconRowGo.AddComponent<HorizontalLayoutGroup>();
            iconRowLayout.childForceExpandWidth = false;
            iconRowLayout.childForceExpandHeight = false;
            iconRowLayout.childAlignment = TextAnchor.MiddleCenter;

            var iconBgGo = new GameObject("ChestIcon");
            iconBgGo.transform.SetParent(iconRowGo.transform, false);
            var iconBgLe = iconBgGo.AddComponent<LayoutElement>();
            iconBgLe.preferredWidth = 120;
            iconBgLe.preferredHeight = 120;
            _showcaseIcon = iconBgGo.AddComponent<Image>();
            _showcaseIcon.color = ColorPalette.GradeLegendary;
            _showcaseIcon.preserveAspect = true;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(showcaseGo.transform, false);
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 36;
            titleLe.flexibleHeight = 0;
            _showcaseTitle = titleGo.AddComponent<TextMeshProUGUI>();
            _showcaseTitle.fontSize = 30;
            _showcaseTitle.color = ColorPalette.Text;
            _showcaseTitle.fontStyle = FontStyles.Bold;
            _showcaseTitle.alignment = TextAlignmentOptions.Center;
            _showcaseTitle.raycastTarget = false;

            var spacerBottom = new GameObject("SpacerBottom");
            spacerBottom.transform.SetParent(showcaseGo.transform, false);
            var spacerBottomLe = spacerBottom.AddComponent<LayoutElement>();
            spacerBottomLe.flexibleHeight = 1;
        }

        private void BuildInfoCard(Transform parent)
        {
            var cardGo = new GameObject("InfoCard");
            cardGo.transform.SetParent(parent, false);
            cardGo.AddComponent<Image>().color = ColorPalette.Card;
            var cardLe = cardGo.AddComponent<LayoutElement>();
            cardLe.preferredHeight = 160;
            cardLe.flexibleHeight = 0;

            var cardLayout = cardGo.AddComponent<VerticalLayoutGroup>();
            cardLayout.spacing = 6;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;
            cardLayout.childAlignment = TextAnchor.UpperCenter;
            cardLayout.padding = new RectOffset(16, 16, 12, 12);

            _gemsText = CreateInfoRow(cardGo.transform, "GemsRow", 26);
            _gemsText.color = ColorPalette.Gems;

            var costRowGo = new GameObject("CostRow");
            costRowGo.transform.SetParent(cardGo.transform, false);
            var costRowLe = costRowGo.AddComponent<LayoutElement>();
            costRowLe.preferredHeight = 26;
            costRowLe.flexibleHeight = 0;
            var costRowLayout = costRowGo.AddComponent<HorizontalLayoutGroup>();
            costRowLayout.spacing = 12;
            costRowLayout.childForceExpandWidth = true;
            costRowLayout.childForceExpandHeight = true;
            costRowLayout.childAlignment = TextAnchor.MiddleCenter;

            var cost1Go = new GameObject("Cost1");
            cost1Go.transform.SetParent(costRowGo.transform, false);
            _cost1Text = cost1Go.AddComponent<TextMeshProUGUI>();
            _cost1Text.fontSize = 22;
            _cost1Text.color = ColorPalette.Text;
            _cost1Text.alignment = TextAlignmentOptions.Center;
            _cost1Text.raycastTarget = false;

            var cost10Go = new GameObject("Cost10");
            cost10Go.transform.SetParent(costRowGo.transform, false);
            _cost10Text = cost10Go.AddComponent<TextMeshProUGUI>();
            _cost10Text.fontSize = 22;
            _cost10Text.color = ColorPalette.Text;
            _cost10Text.alignment = TextAlignmentOptions.Center;
            _cost10Text.raycastTarget = false;

            var pityGo = new GameObject("PityText");
            pityGo.transform.SetParent(cardGo.transform, false);
            var pityLe = pityGo.AddComponent<LayoutElement>();
            pityLe.preferredHeight = 22;
            pityLe.flexibleHeight = 0;
            _pityText = pityGo.AddComponent<TextMeshProUGUI>();
            _pityText.fontSize = 20;
            _pityText.color = ColorPalette.TextDim;
            _pityText.alignment = TextAlignmentOptions.Center;
            _pityText.raycastTarget = false;

            var barGo = new GameObject("PityBar");
            barGo.transform.SetParent(cardGo.transform, false);
            var barLe = barGo.AddComponent<LayoutElement>();
            barLe.preferredHeight = 20;
            barLe.flexibleHeight = 0;
            _pityBar = barGo.AddComponent<ProgressBarView>();
            _pityBar.Initialize(400, 20);
            _pityBar.SetColor(ColorPalette.GradeLegendary);
        }

        private TextMeshProUGUI CreateInfoRow(Transform parent, string name, float fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = fontSize + 4;
            le.flexibleHeight = 0;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.color = ColorPalette.Text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return tmp;
        }

        private void BuildButtons(Transform parent)
        {
            var btnRowGo = new GameObject("ButtonRow");
            btnRowGo.transform.SetParent(parent, false);
            var btnRowLe = btnRowGo.AddComponent<LayoutElement>();
            btnRowLe.minHeight = UISize.NormalButtonMinHeight;
            btnRowLe.preferredHeight = UISize.NormalButtonHeight;
            btnRowLe.flexibleHeight = 0;

            var btnRowLayout = btnRowGo.AddComponent<HorizontalLayoutGroup>();
            btnRowLayout.spacing = 12;
            btnRowLayout.childForceExpandWidth = true;
            btnRowLayout.childForceExpandHeight = true;

            var pull1Go = new GameObject("Pull1Btn");
            pull1Go.transform.SetParent(btnRowGo.transform, false);
            var pull1Bg = pull1Go.AddComponent<Image>();
            pull1Bg.color = ColorPalette.ButtonPrimary;
            _pull1Button = pull1Go.AddComponent<Button>();
            _pull1Button.targetGraphic = pull1Bg;
            _pull1Button.onClick.AddListener(OnPull1);
            var pull1TextGo = new GameObject("Text");
            pull1TextGo.transform.SetParent(pull1Go.transform, false);
            _pull1Label = pull1TextGo.AddComponent<TextMeshProUGUI>();
            _pull1Label.fontSize = 22;
            _pull1Label.color = Color.white;
            _pull1Label.fontStyle = FontStyles.Bold;
            _pull1Label.alignment = TextAlignmentOptions.Center;
            _pull1Label.raycastTarget = false;
            UIManager.StretchFull(pull1TextGo.GetComponent<RectTransform>());

            var pull10Go = new GameObject("Pull10Btn");
            pull10Go.transform.SetParent(btnRowGo.transform, false);
            var pull10Bg = pull10Go.AddComponent<Image>();
            pull10Bg.color = ColorPalette.GradeLegendary;
            _pull10Button = pull10Go.AddComponent<Button>();
            _pull10Button.targetGraphic = pull10Bg;
            _pull10Button.onClick.AddListener(OnPull10);
            var pull10TextGo = new GameObject("Text");
            pull10TextGo.transform.SetParent(pull10Go.transform, false);
            _pull10Label = pull10TextGo.AddComponent<TextMeshProUGUI>();
            _pull10Label.fontSize = 22;
            _pull10Label.color = Color.white;
            _pull10Label.fontStyle = FontStyles.Bold;
            _pull10Label.alignment = TextAlignmentOptions.Center;
            _pull10Label.raycastTarget = false;
            UIManager.StretchFull(pull10TextGo.GetComponent<RectTransform>());
        }

        private void OnTabChanged(int index)
        {
            switch (index)
            {
                case 0: _activeChest = ChestType.EQUIPMENT; break;
                case 1: _activeChest = ChestType.PET; break;
                case 2: _activeChest = ChestType.GEM; break;
            }
            Refresh();
        }

        private TreasureChest GetActiveChest()
        {
            return Game.EquipmentChestSystem;
        }

        private void OnPull1()
        {
            var result = Game.PullGacha();
            if (result == null) return;

            if (result.Equipment != null)
                Game.Player.AddToInventory(result.Equipment);
            foreach (var r in result.Resources)
                Game.Player.Resources.Add(r.Type, r.Amount);

            Game.SaveGame();
            UI.Refresh();
            ShowRewardPopup(new List<PullResult> { result }, OnPull1);
        }

        private void OnPull10()
        {
            var results = Game.PullGacha10();
            if (results == null) return;

            foreach (var result in results)
            {
                if (result.Equipment != null)
                    Game.Player.AddToInventory(result.Equipment);
                foreach (var r in result.Resources)
                    Game.Player.Resources.Add(r.Type, r.Amount);
            }

            Game.SaveGame();
            UI.Refresh();
            ShowRewardPopup(results, OnPull10);
        }

        private void ShowRewardPopup(List<PullResult> results, System.Action pullAgainAction)
        {
            UI.ShowPopupFromType<GachaRewardPopup>(new GachaRewardPopupData
            {
                Results = results,
                OnPullAgain = pullAgainAction
            });
        }

        private Color GetChestIconColor()
        {
            switch (_activeChest)
            {
                case ChestType.EQUIPMENT: return ColorPalette.GradeLegendary;
                case ChestType.PET: return ColorPalette.Heal;
                case ChestType.GEM: return ColorPalette.Gems;
                default: return ColorPalette.GradeLegendary;
            }
        }

        public override void Refresh()
        {
            if (Game == null || Game.Player == null) return;

            var chest = GetActiveChest();
            int cost1 = chest.GetCostPerPull();
            int cost10 = chest.GetPull10Cost();
            int gems = (int)Game.Player.Resources.Gems;

            string chestLabel;
            CHEST_LABELS.TryGetValue(_activeChest, out chestLabel);
            if (chestLabel == null) chestLabel = _activeChest.ToString();
            _showcaseTitle.text = chestLabel;
            _showcaseIcon.color = GetChestIconColor();

            _gemsText.text = $"\ubcf4\uc11d: {NumberFormatter.FormatInt(gems)}";
            _cost1Text.text = $"1\ud68c: {cost1}";
            _cost10Text.text = $"10\ud68c: {cost10}";

            int remaining = chest.GetRemainingToPity();
            int pityThreshold = chest.GetPityThreshold();
            _pityText.text = remaining >= 0 ? $"\ucc9c\uc7a5: {chest.PityCount}/{pityThreshold}" : "";
            if (pityThreshold > 0)
                _pityBar.SetProgress(chest.PityCount, pityThreshold);

            _pull1Label.text = $"1\ud68c ({cost1})";
            _pull10Label.text = $"10\ud68c ({cost10})";
            _pull1Button.interactable = gems >= cost1;
            _pull10Button.interactable = gems >= cost10;
        }
    }
}
