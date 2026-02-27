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
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Screens
{
    public class GachaScreen : BaseScreen
    {
        private TabBarView _tabBar;
        private ChestType _activeChest = ChestType.EQUIPMENT;

        private TextMeshProUGUI _costText;
        private TextMeshProUGUI _pityText;
        private ProgressBarView _pityBar;
        private Button _pull1Button;
        private Button _pull10Button;
        private TextMeshProUGUI _pull1Label;
        private TextMeshProUGUI _pull10Label;

        private RectTransform _resultPanel;
        private RectTransform _resultContent;
        private List<GameObject> _resultItems = new List<GameObject>();

        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            var mainLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            mainLayout.spacing = 8;
            mainLayout.childForceExpandWidth = true;
            mainLayout.childForceExpandHeight = false;
            mainLayout.padding = new RectOffset(16, 16, 12, 12);

            var tabGo = new GameObject("TabBar");
            tabGo.transform.SetParent(transform, false);
            var tabLe = tabGo.AddComponent<LayoutElement>();
            tabLe.preferredHeight = 40;
            tabGo.AddComponent<Image>().color = ColorPalette.Card;
            _tabBar = tabGo.AddComponent<TabBarView>();
            _tabBar.Initialize(new[] { "\uc7a5\ube44", "\ud3ab", "\ubcf4\uc11d" });
            _tabBar.OnTabSelected += OnTabChanged;

            var costGo = new GameObject("CostDisplay");
            costGo.transform.SetParent(transform, false);
            var costLe = costGo.AddComponent<LayoutElement>();
            costLe.preferredHeight = 60;
            costGo.AddComponent<Image>().color = ColorPalette.Card;

            var costLayout = costGo.AddComponent<VerticalLayoutGroup>();
            costLayout.spacing = 4;
            costLayout.childForceExpandWidth = true;
            costLayout.childForceExpandHeight = false;
            costLayout.childAlignment = TextAnchor.MiddleCenter;
            costLayout.padding = new RectOffset(12, 12, 8, 8);

            var costTextGo = new GameObject("CostText");
            costTextGo.transform.SetParent(costGo.transform, false);
            var costTextLe = costTextGo.AddComponent<LayoutElement>();
            costTextLe.preferredHeight = 24;
            _costText = costTextGo.AddComponent<TextMeshProUGUI>();
            _costText.fontSize = 24;
            _costText.color = ColorPalette.Gems;
            _costText.alignment = TextAlignmentOptions.Center;
            _costText.raycastTarget = false;

            var pityTextGo = new GameObject("PityText");
            pityTextGo.transform.SetParent(costGo.transform, false);
            var pityTextLe = pityTextGo.AddComponent<LayoutElement>();
            pityTextLe.preferredHeight = 20;
            _pityText = pityTextGo.AddComponent<TextMeshProUGUI>();
            _pityText.fontSize = 20;
            _pityText.color = ColorPalette.TextDim;
            _pityText.alignment = TextAlignmentOptions.Center;
            _pityText.raycastTarget = false;

            var barGo = new GameObject("PityBar");
            barGo.transform.SetParent(transform, false);
            var barLe = barGo.AddComponent<LayoutElement>();
            barLe.preferredHeight = 16;
            _pityBar = barGo.AddComponent<ProgressBarView>();
            _pityBar.Initialize(400, 16);
            _pityBar.SetColor(ColorPalette.GradeLegendary);

            var btnRowGo = new GameObject("ButtonRow");
            btnRowGo.transform.SetParent(transform, false);
            var btnRowLe = btnRowGo.AddComponent<LayoutElement>();
            btnRowLe.preferredHeight = 56;

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
            _pull1Label.fontSize = 26;
            _pull1Label.color = Color.white;
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
            _pull10Label.fontSize = 26;
            _pull10Label.color = Color.white;
            _pull10Label.alignment = TextAlignmentOptions.Center;
            _pull10Label.raycastTarget = false;
            UIManager.StretchFull(pull10TextGo.GetComponent<RectTransform>());

            var resultHeaderGo = new GameObject("ResultHeader");
            resultHeaderGo.transform.SetParent(transform, false);
            var resultHeaderLe = resultHeaderGo.AddComponent<LayoutElement>();
            resultHeaderLe.preferredHeight = 26;
            var resultHeaderText = resultHeaderGo.AddComponent<TextMeshProUGUI>();
            resultHeaderText.text = "\uacb0\uacfc";
            resultHeaderText.fontSize = 26;
            resultHeaderText.color = ColorPalette.Text;
            resultHeaderText.alignment = TextAlignmentOptions.Left;
            resultHeaderText.raycastTarget = false;

            var resultGo = new GameObject("ResultPanel");
            resultGo.transform.SetParent(transform, false);
            _resultPanel = (resultGo.GetComponent<RectTransform>() ?? resultGo.AddComponent<RectTransform>());
            var resultPanelLe = resultGo.AddComponent<LayoutElement>();
            resultPanelLe.flexibleHeight = 1;

            var resultScrollGo = new GameObject("ResultScroll");
            resultScrollGo.transform.SetParent(resultGo.transform, false);
            var resultScrollRt = (resultScrollGo.GetComponent<RectTransform>() ?? resultScrollGo.AddComponent<RectTransform>());
            UIManager.StretchFull(resultScrollRt);
            var resultScrollRect = resultScrollGo.AddComponent<ScrollRect>();
            resultScrollRect.horizontal = false;
            resultScrollRect.vertical = true;

            var resultViewport = new GameObject("Viewport");
            resultViewport.transform.SetParent(resultScrollGo.transform, false);
            var resultViewportRt = (resultViewport.GetComponent<RectTransform>() ?? resultViewport.AddComponent<RectTransform>());
            UIManager.StretchFull(resultViewportRt);
            resultViewport.AddComponent<RectMask2D>();

            var resultContentGo = new GameObject("Content");
            resultContentGo.transform.SetParent(resultViewport.transform, false);
            _resultContent = (resultContentGo.GetComponent<RectTransform>() ?? resultContentGo.AddComponent<RectTransform>());
            _resultContent.anchorMin = new Vector2(0, 1);
            _resultContent.anchorMax = new Vector2(1, 1);
            _resultContent.pivot = new Vector2(0.5f, 1);
            _resultContent.offsetMin = Vector2.zero;
            _resultContent.offsetMax = Vector2.zero;

            var resultContentLayout = resultContentGo.AddComponent<VerticalLayoutGroup>();
            resultContentLayout.spacing = 4;
            resultContentLayout.childForceExpandWidth = true;
            resultContentLayout.childForceExpandHeight = false;

            var resultContentFitter = resultContentGo.AddComponent<ContentSizeFitter>();
            resultContentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            resultScrollRect.content = _resultContent;
            resultScrollRect.viewport = resultViewportRt;
        }

        private void OnTabChanged(int index)
        {
            switch (index)
            {
                case 0: _activeChest = ChestType.EQUIPMENT; break;
                case 1: _activeChest = ChestType.PET; break;
                case 2: _activeChest = ChestType.GEM; break;
            }
            ClearResults();
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
            ShowResults(new List<PullResult> { result });
            UI.Refresh();
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
            ShowResults(results);
            UI.Refresh();
        }

        private void ClearResults()
        {
            foreach (var go in _resultItems)
                Destroy(go);
            _resultItems.Clear();
        }

        private void ShowResults(List<PullResult> results)
        {
            ClearResults();

            foreach (var result in results)
            {
                var go = new GameObject("ResultItem");
                go.transform.SetParent(_resultContent, false);
                var le = go.AddComponent<LayoutElement>();
                le.preferredHeight = 36;
                go.AddComponent<Image>().color = ColorPalette.CardLight;

                var layout = go.AddComponent<HorizontalLayoutGroup>();
                layout.spacing = 8;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = true;
                layout.padding = new RectOffset(8, 8, 4, 4);

                if (result.Equipment != null)
                {
                    Color gradeColor = ColorPalette.GetEquipmentGradeColor(result.Equipment.Grade);
                    var textGo = new GameObject("Text");
                    textGo.transform.SetParent(go.transform, false);
                    var tmp = textGo.AddComponent<TextMeshProUGUI>();
                    string pityMark = result.IsPity ? " [PITY]" : "";
                    string sMark = result.Equipment.IsS ? " [S]" : "";
                    tmp.text = $"<color=#{ColorUtility.ToHtmlStringRGB(gradeColor)}>{result.Equipment.Name}{sMark}{pityMark}</color>";
                    tmp.fontSize = 22;
                    tmp.color = ColorPalette.Text;
                    tmp.alignment = TextAlignmentOptions.Left;
                    tmp.raycastTarget = false;
                }

                foreach (var r in result.Resources)
                {
                    var textGo = new GameObject("Resource");
                    textGo.transform.SetParent(go.transform, false);
                    var tmp = textGo.AddComponent<TextMeshProUGUI>();
                    tmp.text = $"{NumberFormatter.FormatResourceType(r.Type)}: +{r.Amount}";
                    tmp.fontSize = 22;
                    tmp.color = ColorPalette.Gold;
                    tmp.alignment = TextAlignmentOptions.Left;
                    tmp.raycastTarget = false;
                }

                _resultItems.Add(go);
            }
        }

        public override void Refresh()
        {
            if (Game == null || Game.Player == null) return;

            var chest = GetActiveChest();
            int cost1 = chest.GetCostPerPull();
            int cost10 = chest.GetPull10Cost();
            int gems = (int)Game.Player.Resources.Gems;

            _costText.text = $"\ubcf4\uc11d: {NumberFormatter.FormatInt(gems)}  1\ud68c: {cost1}  10\ud68c: {cost10}";

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
