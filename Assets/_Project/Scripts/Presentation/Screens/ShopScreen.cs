using System;
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
    public class ShopScreen : BaseScreen
    {
        private enum ShopTab { Equipment, Pet, Package, Charge }

        private TabBarView _tabBar;
        private ShopTab _activeTab = ShopTab.Equipment;
        private RectTransform _scrollContent;
        private readonly List<GameObject> _productCards = new List<GameObject>();

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
            mainLayout.padding = new RectOffset(0, 0, 0, 0);

            BuildScrollArea(transform);
            BuildTabBar(transform);
        }

        private void BuildScrollArea(Transform parent)
        {
            var scrollGo = new GameObject("ScrollArea");
            scrollGo.transform.SetParent(parent, false);
            var scrollLe = scrollGo.AddComponent<LayoutElement>();
            scrollLe.flexibleHeight = 1;

            var scrollImage = scrollGo.AddComponent<Image>();
            scrollImage.color = ColorPalette.Background;

            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            var mask = scrollGo.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            var viewportRt = scrollGo.GetComponent<RectTransform>();

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(scrollGo.transform, false);
            _scrollContent = contentGo.AddComponent<RectTransform>();
            _scrollContent.anchorMin = new Vector2(0, 1);
            _scrollContent.anchorMax = new Vector2(1, 1);
            _scrollContent.pivot = new Vector2(0.5f, 1);
            _scrollContent.offsetMin = new Vector2(0, 0);
            _scrollContent.offsetMax = new Vector2(0, 0);

            var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 12;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.padding = new RectOffset(16, 16, 12, 12);

            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = _scrollContent;
            scrollRect.viewport = viewportRt;
        }

        private void BuildTabBar(Transform parent)
        {
            var tabGo = new GameObject("ShopTabBar");
            tabGo.transform.SetParent(parent, false);
            var tabLe = tabGo.AddComponent<LayoutElement>();
            tabLe.preferredHeight = 50;
            tabLe.flexibleHeight = 0;
            tabGo.AddComponent<Image>().color = ColorPalette.NavBarBackground;
            _tabBar = tabGo.AddComponent<TabBarView>();
            _tabBar.Initialize(new[] { "\uC7A5\uBE44\uC0C1\uC810", "\uD3AB\uC0C1\uC810", "\uD328\uD0A4\uC9C0\uC0C1\uC810", "\uCDA9\uC804" });
            _tabBar.OnTabSelected += OnTabChanged;
        }

        private void OnTabChanged(int index)
        {
            _activeTab = (ShopTab)index;
            RebuildProducts();
        }

        private void ClearProducts()
        {
            foreach (var card in _productCards)
                Destroy(card);
            _productCards.Clear();
        }

        private void RebuildProducts()
        {
            ClearProducts();

            switch (_activeTab)
            {
                case ShopTab.Equipment:
                    BuildEquipmentShopProducts();
                    break;
                case ShopTab.Pet:
                    BuildPetShopProducts();
                    break;
                case ShopTab.Package:
                    BuildPackageShopProducts();
                    break;
                case ShopTab.Charge:
                    BuildChargeProducts();
                    break;
            }
        }

        private void BuildEquipmentShopProducts()
        {
            BuildProductCard(
                "S\uAE09 \uC7A5\uBE44\uBF51\uAE30",
                "\uC774\uBCA4\uD2B8\uB85C \uD2B9\uC815 S\uAE09 \uC7A5\uBE44 \uBF51\uAE30 \uD655\uB960\uC774 \uB192\uC544\uC9D1\uB2C8\uB2E4",
                ColorPalette.GradeMythic,
                "\uBCF4\uC11D",
                GetEquipmentCost1(),
                GetEquipmentCost10(),
                OnSGradePull1,
                OnSGradePull10
            );

            BuildProductCard(
                "\uC7A5\uBE44\uBF51\uAE30",
                "\uC5D0\uD53D \uC774\uC0C1 \uC7A5\uBE44\uB97C \uBF51\uC744 \uD655\uB960\uC774 \uB192\uC740 \uC0C1\uC790",
                ColorPalette.GradeLegendary,
                "\uBCF4\uC11D",
                GetEquipmentCost1(),
                GetEquipmentCost10(),
                OnEquipmentPull1,
                OnEquipmentPull10
            );

            BuildProductCard(
                "\uBAA8\uD5D8\uAC00 \uC0C1\uC790 \uBF51\uAE30",
                "\uC77C\uBC18~\uC6B0\uC218 \uC7A5\uBE44\uBF51\uAE30 \uC0C1\uC790 (\uC740\uC5F4\uC1E0\uB85C \uBF51\uAE30)",
                ColorPalette.GradeUncommon,
                "\uC740\uC5F4\uC1E0",
                0,
                0,
                null,
                null
            );

            BuildProductCard(
                "\uC601\uC6C5\uC0C1\uC790 \uBF51\uAE30",
                "\uC6B0\uC218~\uC5D0\uD53D \uC7A5\uBE44 \uD3EC\uD568 (\uAE08\uC5F4\uC1E0\uB85C \uBF51\uAE30)",
                ColorPalette.GradeEpic,
                "\uAE08\uC5F4\uC1E0",
                0,
                0,
                null,
                null
            );
        }

        private void BuildPetShopProducts()
        {
            BuildProductCard(
                "\uC6B0\uC218 \uD3AB\uBF51\uAE30",
                "\uC5D0\uD53D \uC774\uC0C1 \uD3AB \uBF51\uAE30 \uD655\uB960\uC774 \uB192\uC740 \uBF51\uAE30",
                ColorPalette.GradeEpic,
                "\uBCF4\uC11D",
                GetEquipmentCost1(),
                GetEquipmentCost10(),
                OnPremiumPetPull1,
                OnPremiumPetPull10
            );

            BuildProductCard(
                "\uC77C\uBC18 \uD3AB \uBF51\uAE30",
                "\uC77C\uBC18\uBD80\uD130 \uB4DC\uBB3C\uAC8C \uC5D0\uD53D\uD3AB\uB3C4 \uB098\uC624\uB294 \uBF51\uAE30",
                ColorPalette.GradeCommon,
                "\uD3AB \uC54C",
                0,
                0,
                null,
                null
            );
        }

        private void BuildPackageShopProducts()
        {
            BuildPlaceholderContent("\uD328\uD0A4\uC9C0\uC0C1\uC810 \uC900\uBE44 \uC911...");
        }

        private void BuildChargeProducts()
        {
            BuildPlaceholderContent("\uCDA9\uC804 \uC900\uBE44 \uC911...");
        }

        private void BuildPlaceholderContent(string message)
        {
            var go = new GameObject("Placeholder");
            go.transform.SetParent(_scrollContent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 200;
            le.flexibleHeight = 0;

            go.AddComponent<Image>().color = ColorPalette.Card;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = message;
            text.fontSize = 28;
            text.color = ColorPalette.TextDim;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            UIManager.StretchFull(textGo.GetComponent<RectTransform>());

            _productCards.Add(go);
        }

        private void BuildProductCard(
            string title,
            string description,
            Color accentColor,
            string currencyName,
            int cost1,
            int cost10,
            Action onPull1,
            Action onPull10)
        {
            var cardGo = new GameObject("Card_" + title);
            cardGo.transform.SetParent(_scrollContent, false);
            var cardLe = cardGo.AddComponent<LayoutElement>();
            cardLe.preferredHeight = 200;
            cardLe.flexibleHeight = 0;

            cardGo.AddComponent<Image>().color = ColorPalette.Card;

            var cardLayout = cardGo.AddComponent<VerticalLayoutGroup>();
            cardLayout.spacing = 8;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;
            cardLayout.childAlignment = TextAnchor.UpperCenter;
            cardLayout.padding = new RectOffset(16, 16, 14, 14);

            var accentGo = new GameObject("Accent");
            accentGo.transform.SetParent(cardGo.transform, false);
            var accentLe = accentGo.AddComponent<LayoutElement>();
            accentLe.preferredHeight = 4;
            accentLe.flexibleHeight = 0;
            accentGo.AddComponent<Image>().color = accentColor;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(cardGo.transform, false);
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 34;
            titleLe.flexibleHeight = 0;
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 28;
            titleText.color = accentColor;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.raycastTarget = false;

            var descGo = new GameObject("Description");
            descGo.transform.SetParent(cardGo.transform, false);
            var descLe = descGo.AddComponent<LayoutElement>();
            descLe.preferredHeight = 24;
            descLe.flexibleHeight = 0;
            var descText = descGo.AddComponent<TextMeshProUGUI>();
            descText.text = description;
            descText.fontSize = 20;
            descText.color = ColorPalette.TextDim;
            descText.alignment = TextAlignmentOptions.Left;
            descText.raycastTarget = false;

            var spacer = new GameObject("Spacer");
            spacer.transform.SetParent(cardGo.transform, false);
            var spacerLe = spacer.AddComponent<LayoutElement>();
            spacerLe.flexibleHeight = 1;

            var currencyGo = new GameObject("Currency");
            currencyGo.transform.SetParent(cardGo.transform, false);
            var currencyLe = currencyGo.AddComponent<LayoutElement>();
            currencyLe.preferredHeight = 22;
            currencyLe.flexibleHeight = 0;
            var currencyText = currencyGo.AddComponent<TextMeshProUGUI>();
            currencyText.fontSize = 18;
            currencyText.color = ColorPalette.Gems;
            currencyText.alignment = TextAlignmentOptions.Left;
            currencyText.raycastTarget = false;

            if (onPull1 != null)
                currencyText.text = $"\uD544\uC694: {currencyName}";
            else
                currencyText.text = $"\uD544\uC694: {currencyName} (\uC900\uBE44 \uC911)";

            var btnRowGo = new GameObject("ButtonRow");
            btnRowGo.transform.SetParent(cardGo.transform, false);
            var btnRowLe = btnRowGo.AddComponent<LayoutElement>();
            btnRowLe.preferredHeight = 44;
            btnRowLe.flexibleHeight = 0;

            var btnRowLayout = btnRowGo.AddComponent<HorizontalLayoutGroup>();
            btnRowLayout.spacing = 10;
            btnRowLayout.childForceExpandWidth = true;
            btnRowLayout.childForceExpandHeight = true;

            CreatePullButton(btnRowGo.transform, cost1 > 0 ? $"1\uD68C ({cost1})" : "1\uD68C", onPull1, ColorPalette.ButtonPrimary);
            CreatePullButton(btnRowGo.transform, cost10 > 0 ? $"10\uD68C ({cost10})" : "10\uD68C", onPull10, ColorPalette.GradeLegendary);

            _productCards.Add(cardGo);
        }

        private void CreatePullButton(Transform parent, string label, Action onClick, Color bgColor)
        {
            var btnGo = new GameObject("PullBtn");
            btnGo.transform.SetParent(parent, false);
            var btnBg = btnGo.AddComponent<Image>();
            btnBg.color = bgColor;
            var button = btnGo.AddComponent<Button>();
            button.targetGraphic = btnBg;

            if (onClick != null)
                button.onClick.AddListener(() => onClick());
            else
                button.interactable = false;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 20;
            text.color = Color.white;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            UIManager.StretchFull(textGo.GetComponent<RectTransform>());
        }

        private int GetEquipmentCost1()
        {
            if (Game == null) return 0;
            return Game.EquipmentChestSystem.GetCostPerPull();
        }

        private int GetEquipmentCost10()
        {
            if (Game == null) return 0;
            return Game.EquipmentChestSystem.GetPull10Cost();
        }

        private void OnSGradePull1()
        {
            ExecuteEquipmentPull(false);
        }

        private void OnSGradePull10()
        {
            ExecuteEquipmentPull(true);
        }

        private void OnEquipmentPull1()
        {
            ExecuteEquipmentPull(false);
        }

        private void OnEquipmentPull10()
        {
            ExecuteEquipmentPull(true);
        }

        private void OnPremiumPetPull1()
        {
            ExecuteEquipmentPull(false);
        }

        private void OnPremiumPetPull10()
        {
            ExecuteEquipmentPull(true);
        }

        private void ExecuteEquipmentPull(bool isTenPull)
        {
            Debug.Log($"[ShopScreen] ExecuteEquipmentPull called. isTenPull={isTenPull}");
            if (Game == null || Game.Player == null)
            {
                Debug.Log("[ShopScreen] Game or Player is null!");
                return;
            }

            int cost = isTenPull ? GetEquipmentCost10() : GetEquipmentCost1();
            int gems = (int)Game.Player.Resources.Gems;
            Debug.Log($"[ShopScreen] cost={cost}, gems={gems}");
            if (gems < cost)
            {
                ShowInsufficientPopup(cost, gems);
                return;
            }

            if (isTenPull)
            {
                var results = Game.PullGacha10();
                Debug.Log($"[ShopScreen] PullGacha10 result: {(results == null ? "null" : results.Count.ToString())}");
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
                UI.ShowPopupFromType<GachaRewardPopup>(new GachaRewardPopupData
                {
                    Results = results,
                    OnPullAgain = () => ExecuteEquipmentPull(true)
                });
            }
            else
            {
                var result = Game.PullGacha();
                Debug.Log($"[ShopScreen] PullGacha result: {(result == null ? "null" : "ok")}");
                if (result == null) return;

                if (result.Equipment != null)
                    Game.Player.AddToInventory(result.Equipment);
                foreach (var r in result.Resources)
                    Game.Player.Resources.Add(r.Type, r.Amount);

                Game.SaveGame();
                UI.Refresh();
                UI.ShowPopupFromType<GachaRewardPopup>(new GachaRewardPopupData
                {
                    Results = new List<PullResult> { result },
                    OnPullAgain = () => ExecuteEquipmentPull(false)
                });
            }
        }

        private void ShowInsufficientPopup(int cost, int current)
        {
            UI.ShowPopupFromType<ShopInsufficientPopup>(new ShopInsufficientPopupData
            {
                RequiredGems = cost,
                CurrentGems = current
            });
        }

        public override void Refresh()
        {
            if (Game == null || Game.Player == null) return;
            RebuildProducts();
        }
    }
}
