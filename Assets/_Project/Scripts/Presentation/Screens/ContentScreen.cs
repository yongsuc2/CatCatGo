using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Screens
{
    public class ContentScreen : BaseScreen
    {
        private enum ContentView
        {
            Menu,
            Tower,
            Dungeon,
            GoblinMine,
            Catacomb,
        }

        private ContentView _currentView = ContentView.Menu;

        private RectTransform _menuPanel;
        private RectTransform _subPanel;

        private TextMeshProUGUI _towerInfo;
        private TextMeshProUGUI _dungeonInfo;
        private TextMeshProUGUI _goblinInfo;
        private TextMeshProUGUI _catacombInfo;

        private TextMeshProUGUI _subTitle;
        private RectTransform _subContent;
        private TextMeshProUGUI _resultText;

        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            _menuPanel = UIManager.CreateRectTransform("MenuPanel", transform);
            UIManager.StretchFull(_menuPanel);

            var menuScroll = CreateScrollableContent(_menuPanel);

            var cards = new[]
            {
                ("\ud0d1", "\ud1a0\uc804", ContentView.Tower),
                ("\ub358\uc804", "\uc77c\uc77c \ub358\uc804", ContentView.Dungeon),
                ("\uad11\uc0b0", "\uace0\ube14\ub9b0 \uad11\uc0b0", ContentView.GoblinMine),
                ("\uc9c0\ud558\ubb18\uc9c0", "\uce74\ud0c0\ucf64", ContentView.Catacomb),
            };

            _towerInfo = CreateContentCard(menuScroll, cards[0].Item1, cards[0].Item2, cards[0].Item3);
            _dungeonInfo = CreateContentCard(menuScroll, cards[1].Item1, cards[1].Item2, cards[1].Item3);
            _goblinInfo = CreateContentCard(menuScroll, cards[2].Item1, cards[2].Item2, cards[2].Item3);
            _catacombInfo = CreateContentCard(menuScroll, cards[3].Item1, cards[3].Item2, cards[3].Item3);

            _subPanel = UIManager.CreateRectTransform("SubPanel", transform);
            UIManager.StretchFull(_subPanel);
            _subPanel.gameObject.SetActive(false);

            var subLayout = _subPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            subLayout.spacing = 8;
            subLayout.childForceExpandWidth = true;
            subLayout.childForceExpandHeight = false;
            subLayout.padding = new RectOffset(16, 16, 12, 12);

            var headerRowGo = new GameObject("HeaderRow");
            headerRowGo.transform.SetParent(_subPanel, false);
            var headerRowLe = headerRowGo.AddComponent<LayoutElement>();
            headerRowLe.preferredHeight = 40;
            var headerRowLayout = headerRowGo.AddComponent<HorizontalLayoutGroup>();
            headerRowLayout.childForceExpandWidth = true;
            headerRowLayout.childForceExpandHeight = true;

            var backGo = new GameObject("BackBtn");
            backGo.transform.SetParent(headerRowGo.transform, false);
            var backLe = backGo.AddComponent<LayoutElement>();
            backLe.preferredWidth = 60;
            var backBg = backGo.AddComponent<Image>();
            backBg.color = ColorPalette.ButtonSecondary;
            var backBtn = backGo.AddComponent<Button>();
            backBtn.targetGraphic = backBg;
            backBtn.onClick.AddListener(ShowMenu);
            var backTextGo = new GameObject("Text");
            backTextGo.transform.SetParent(backGo.transform, false);
            var backText = backTextGo.AddComponent<TextMeshProUGUI>();
            backText.text = "<";
            backText.fontSize = 28;
            backText.color = ColorPalette.Text;
            backText.alignment = TextAlignmentOptions.Center;
            backText.raycastTarget = false;
            UIManager.StretchFull(backTextGo.GetComponent<RectTransform>());

            var titleGo = new GameObject("SubTitle");
            titleGo.transform.SetParent(headerRowGo.transform, false);
            _subTitle = titleGo.AddComponent<TextMeshProUGUI>();
            _subTitle.fontSize = 30;
            _subTitle.color = ColorPalette.Text;
            _subTitle.alignment = TextAlignmentOptions.Center;
            _subTitle.raycastTarget = false;

            var subContentGo = new GameObject("SubContent");
            subContentGo.transform.SetParent(_subPanel, false);
            _subContent = subContentGo.GetComponent<RectTransform>();

            if (_subContent == null) _subContent = subContentGo.AddComponent<RectTransform>();
            var subContentLe = subContentGo.AddComponent<LayoutElement>();
            subContentLe.flexibleHeight = 1;

            var subContentLayout = subContentGo.AddComponent<VerticalLayoutGroup>();
            subContentLayout.spacing = 8;
            subContentLayout.childForceExpandWidth = true;
            subContentLayout.childForceExpandHeight = false;
            subContentLayout.padding = new RectOffset(0, 0, 8, 8);

            var resultGo = new GameObject("ResultText");
            resultGo.transform.SetParent(_subPanel, false);
            var resultLe = resultGo.AddComponent<LayoutElement>();
            resultLe.preferredHeight = 60;
            _resultText = resultGo.AddComponent<TextMeshProUGUI>();
            _resultText.fontSize = 22;
            _resultText.color = ColorPalette.Gold;
            _resultText.alignment = TextAlignmentOptions.Center;
            _resultText.raycastTarget = false;
            _resultText.textWrappingMode = TextWrappingModes.Normal;
        }

        private Transform CreateScrollableContent(RectTransform parent)
        {
            var scrollGo = new GameObject("Scroll");
            scrollGo.transform.SetParent(parent, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();

            if (scrollRt == null) scrollRt = scrollGo.AddComponent<RectTransform>();
            UIManager.StretchFull(scrollRt);
            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

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
            contentLayout.padding = new RectOffset(16, 16, 12, 12);

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;
            scrollRect.viewport = viewportRt;

            return contentGo.transform;
        }

        private TextMeshProUGUI CreateContentCard(Transform parent, string icon, string title, ContentView view)
        {
            var cardGo = new GameObject("Card_" + title);
            cardGo.transform.SetParent(parent, false);
            var cardLe = cardGo.AddComponent<LayoutElement>();
            cardLe.preferredHeight = 80;
            var cardImage = cardGo.AddComponent<Image>();
            cardImage.color = ColorPalette.Card;

            var cardBtn = cardGo.AddComponent<Button>();
            cardBtn.targetGraphic = cardImage;
            cardBtn.onClick.AddListener(() => ShowSubPanel(view));

            var cardLayout = cardGo.AddComponent<HorizontalLayoutGroup>();
            cardLayout.spacing = 12;
            cardLayout.childForceExpandWidth = false;
            cardLayout.childForceExpandHeight = true;
            cardLayout.padding = new RectOffset(16, 16, 8, 8);

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(cardGo.transform, false);
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 50;
            var iconText = iconGo.AddComponent<TextMeshProUGUI>();
            iconText.text = icon.Length > 1 ? icon.Substring(0, 1) : icon;
            iconText.fontSize = 38;
            iconText.color = ColorPalette.Gold;
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.textWrappingMode = TextWrappingModes.NoWrap;
            iconText.overflowMode = TextOverflowModes.Ellipsis;
            iconText.raycastTarget = false;

            var infoGo = new GameObject("Info");
            infoGo.transform.SetParent(cardGo.transform, false);
            var infoLe = infoGo.AddComponent<LayoutElement>();
            infoLe.flexibleWidth = 1;
            var infoLayout = infoGo.AddComponent<VerticalLayoutGroup>();
            infoLayout.spacing = 4;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;
            infoLayout.childAlignment = TextAnchor.MiddleLeft;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(infoGo.transform, false);
            var titleLe2 = titleGo.AddComponent<LayoutElement>();
            titleLe2.preferredHeight = 28;
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 28;
            titleText.color = ColorPalette.Text;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.raycastTarget = false;

            var detailGo = new GameObject("Detail");
            detailGo.transform.SetParent(infoGo.transform, false);
            var detailLe = detailGo.AddComponent<LayoutElement>();
            detailLe.preferredHeight = 22;
            var detailText = detailGo.AddComponent<TextMeshProUGUI>();
            detailText.fontSize = 22;
            detailText.color = ColorPalette.TextDim;
            detailText.alignment = TextAlignmentOptions.Left;
            detailText.raycastTarget = false;

            return detailText;
        }

        private void ShowMenu()
        {
            _currentView = ContentView.Menu;
            _menuPanel.gameObject.SetActive(true);
            _subPanel.gameObject.SetActive(false);
            Refresh();
        }

        private void ShowSubPanel(ContentView view)
        {
            _currentView = view;
            _menuPanel.gameObject.SetActive(false);
            _subPanel.gameObject.SetActive(true);
            _resultText.text = "";

            ClearSubContent();
            switch (view)
            {
                case ContentView.Tower: BuildTowerSub(); break;
                case ContentView.Dungeon: BuildDungeonSub(); break;
                case ContentView.GoblinMine: BuildGoblinMineSub(); break;
                case ContentView.Catacomb: BuildCatacombSub(); break;
            }
        }

        private void ClearSubContent()
        {
            for (int i = _subContent.childCount - 1; i >= 0; i--)
                Destroy(_subContent.GetChild(i).gameObject);
        }

        private void BuildTowerSub()
        {
            _subTitle.text = "\ud0d1";
            var tower = Game.Tower;
            int tokens = (int)Game.Player.Resources.ChallengeTokens;

            CreateInfoRow(_subContent, $"\ud604\uc7ac \uce35: {tower.CurrentFloor}F  \ubc30\ud2c0: {tower.CurrentStage}/{tower.StagesPerFloor}");
            CreateInfoRow(_subContent, $"\ub3c4\uc804 \ud1a0\ud070: {tokens}");

            CreateActionButton(_subContent, $"\ub3c4\uc804 (\ud1a0\ud070 1\uac1c)", () =>
            {
                var result = Game.TowerChallenge();
                if (result.IsFail()) return;

                string rewardStr = "";
                if (result.Data.Advanced && result.Data.Reward != null)
                {
                    foreach (var r in result.Data.Reward.Resources)
                        rewardStr += $"{NumberFormatter.FormatResourceType(r.Type)}: +{r.Amount}  ";
                }

                _resultText.text = result.Data.BattleState == BattleState.VICTORY
                    ? $"\uc2b9\ub9ac! {rewardStr}"
                    : "\ud328\ubc30...";
                ShowSubPanel(ContentView.Tower);
            });
        }

        private void BuildDungeonSub()
        {
            _subTitle.text = "\ub358\uc804";
            int remaining = Game.DungeonManager.GetRemainingCount();
            CreateInfoRow(_subContent, $"\ub0a8\uc740 \uc785\uc7a5: {remaining}/{Game.DungeonManager.DailyLimit}");

            var dungeonTypes = new[] { DungeonType.GIANT_BEEHIVE, DungeonType.ANCIENT_TREE, DungeonType.TIGER_CLIFF };
            var dungeonNames = new Dictionary<DungeonType, string>
            {
                { DungeonType.GIANT_BEEHIVE, "\uac70\ub300 \ubc8c\uc9d1" },
                { DungeonType.ANCIENT_TREE, "\uc218\ucc9c\ub144 \uace0\ubaa9" },
                { DungeonType.TIGER_CLIFF, "\ud638\ub791\uc774 \uc808\ubcbd" },
            };

            foreach (var type in dungeonTypes)
            {
                var dungeon = Game.DungeonManager.GetDungeon(type);
                string name;
                dungeonNames.TryGetValue(type, out name);
                CreateInfoRow(_subContent, $"{name} - \ud074\ub9ac\uc5b4: {dungeon.ClearedStage}  \ub2e4\uc74c: {dungeon.GetNextStage()}");

                var capturedType = type;
                CreateActionButton(_subContent, $"{name} \uc804\ud22c", () =>
                {
                    var result = Game.DungeonChallenge(capturedType);
                    if (result.IsFail()) { _resultText.text = result.Message; return; }

                    string rewardStr = "";
                    if (result.Data.Reward != null)
                    {
                        foreach (var r in result.Data.Reward.Resources)
                            rewardStr += $"{NumberFormatter.FormatResourceType(r.Type)}: +{r.Amount}  ";
                    }

                    _resultText.text = result.Data.BattleState == BattleState.VICTORY
                        ? $"\uc2b9\ub9ac! {rewardStr}"
                        : "\ud328\ubc30...";
                    ShowSubPanel(ContentView.Dungeon);
                });

                if (dungeon.ClearedStage > 0)
                {
                    CreateActionButton(_subContent, $"{name} \uc18c\ud0d5", () =>
                    {
                        var sweepResult = Game.DungeonSweep(capturedType);
                        if (sweepResult.IsFail()) { _resultText.text = sweepResult.Message; return; }

                        string rewardStr = "";
                        foreach (var r in sweepResult.Data.Reward.Resources)
                            rewardStr += $"{NumberFormatter.FormatResourceType(r.Type)}: +{r.Amount}  ";
                        _resultText.text = $"\uc18c\ud0d5 \uc644\ub8cc! {rewardStr}";
                        ShowSubPanel(ContentView.Dungeon);
                    });
                }
            }
        }

        private void BuildGoblinMineSub()
        {
            _subTitle.text = "\uace0\ube14\ub9b0 \uad11\uc0b0";
            var miner = Game.GoblinMinerSystem;
            int pickaxes = (int)Game.Player.Resources.Pickaxes;

            CreateInfoRow(_subContent, $"\uad11\uc11d: {miner.OreCount}/30  \uace1\uad2d\uc774: {pickaxes}");

            CreateActionButton(_subContent, $"\ucc44\uad74 (\uace1\uad2d\uc774 1\uac1c)", () =>
            {
                var result = Game.GoblinMine();
                if (result.IsFail()) { _resultText.text = result.Message; return; }
                _resultText.text = $"\uad11\uc11d +{result.Data.OreGained}  (\ucd1d: {result.Data.TotalOre})";
                ShowSubPanel(ContentView.GoblinMine);
            });

            if (miner.CanUseCart())
            {
                CreateActionButton(_subContent, "\uc218\ub808 \ubcf4\ub0b4\uae30 (30 \uad11\uc11d)", () =>
                {
                    var result = Game.GoblinCart();
                    if (result.IsFail()) { _resultText.text = result.Message; return; }

                    string rewardStr = "";
                    foreach (var r in result.Data.Resources)
                        rewardStr += $"{NumberFormatter.FormatResourceType(r.Type)}: +{r.Amount}  ";
                    _resultText.text = $"\uc218\ub808 \ubcf4\uc0c1: {rewardStr}";
                    ShowSubPanel(ContentView.GoblinMine);
                });
            }
        }

        private void BuildCatacombSub()
        {
            _subTitle.text = "\uce74\ud0c0\ucf64";
            var catacomb = Game.Catacomb;

            CreateInfoRow(_subContent, $"\ucd5c\uace0 \uce35: {catacomb.HighestFloor}  \ud604\uc7ac: {catacomb.CurrentRunFloor}  \uc804\ud22c: {catacomb.CurrentBattleIndex}/{catacomb.BattlesPerFloor}");

            if (!catacomb.IsRunning)
            {
                CreateActionButton(_subContent, "\ub3c4\uc804 \uc2dc\uc791", () =>
                {
                    var result = Game.CatacombStart();
                    if (result.IsFail()) return;
                    ShowSubPanel(ContentView.Catacomb);
                });
            }
            else
            {
                CreateActionButton(_subContent, "\ub2e4\uc74c \uc804\ud22c", () =>
                {
                    var result = Game.CatacombBattle();
                    if (result.IsFail()) return;

                    if (!result.Data.ContinueRun)
                    {
                        string rewardStr = "";
                        foreach (var r in result.Data.Reward.Resources)
                            rewardStr += $"{NumberFormatter.FormatResourceType(r.Type)}: +{r.Amount}  ";
                        _resultText.text = $"\ud328\ubc30. \ubcf4\uc0c1: {rewardStr}";
                    }
                    else
                    {
                        _resultText.text = $"\uc2b9\ub9ac! \uce35: {result.Data.CurrentFloor}  \uc804\ud22c: {result.Data.BattleIndex}/{catacomb.BattlesPerFloor}";
                    }
                    ShowSubPanel(ContentView.Catacomb);
                });

                CreateActionButton(_subContent, "\ud0d0\ud5d8 \uc885\ub8cc", () =>
                {
                    var result = Game.CatacombEnd();
                    if (result.IsFail()) return;

                    string rewardStr = "";
                    foreach (var r in result.Data.Resources)
                        rewardStr += $"{NumberFormatter.FormatResourceType(r.Type)}: +{r.Amount}  ";
                    _resultText.text = $"\ud0d0\ud5d8 \uc885\ub8cc. \ubcf4\uc0c1: {rewardStr}";
                    ShowSubPanel(ContentView.Catacomb);
                });
            }
        }

        private void CreateInfoRow(RectTransform parent, string text)
        {
            var go = new GameObject("InfoRow");
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 36;
            go.AddComponent<Image>().color = ColorPalette.CardLight;
            var tmp = UIManager.CreateText(go.transform, text, 22, ColorPalette.Text, "Text");
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.margin = new Vector4(8, 0, 8, 0);
        }

        private void CreateActionButton(RectTransform parent, string label, Action onClick)
        {
            var go = new GameObject("ActionBtn");
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 44;

            var bg = go.AddComponent<Image>();
            bg.color = ColorPalette.ButtonPrimary;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => onClick());

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 24;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            UIManager.StretchFull(textGo.GetComponent<RectTransform>());
        }

        public override void Refresh()
        {
            if (Game == null || Game.Player == null) return;

            if (_currentView == ContentView.Menu)
            {
                int tokens = (int)Game.Player.Resources.ChallengeTokens;
                _towerInfo.text = $"{Game.Tower.CurrentFloor}F-{Game.Tower.CurrentStage}  \ud1a0\ud070: {tokens}";

                int dungeonRemaining = Game.DungeonManager.GetRemainingCount();
                _dungeonInfo.text = $"\ub0a8\uc740 \uc785\uc7a5: {dungeonRemaining}/{Game.DungeonManager.DailyLimit}";

                _goblinInfo.text = $"\uad11\uc11d: {Game.GoblinMinerSystem.OreCount}/30  \uace1\uad2d\uc774: {(int)Game.Player.Resources.Pickaxes}";

                _catacombInfo.text = $"\ucd5c\uace0\uce35: {Game.Catacomb.HighestFloor}  {(Game.Catacomb.IsRunning ? "\ud0d0\ud5d8\uc911" : "\ub300\uae30")}";
            }
        }

        public override void OnScreenEnter()
        {
            ShowMenu();
        }
    }
}
