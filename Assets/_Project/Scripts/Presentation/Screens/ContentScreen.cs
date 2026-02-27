using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Battle;
using CatCatGo.Domain.Content;
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
            Arena,
            Travel,
            GoblinMine,
            Catacomb,
        }

        private ContentView _currentView = ContentView.Menu;

        private RectTransform _menuPanel;
        private RectTransform _subPanel;

        private TextMeshProUGUI _towerInfo;
        private TextMeshProUGUI _dungeonInfo;
        private TextMeshProUGUI _arenaInfo;
        private TextMeshProUGUI _travelInfo;
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
                ("\uc544\ub808\ub098", "\uc544\ub808\ub098", ContentView.Arena),
                ("\uc5ec\ud589", "\uc5ec\ud589", ContentView.Travel),
                ("\uad11\uc0b0", "\uace0\ube14\ub9b0 \uad11\uc0b0", ContentView.GoblinMine),
                ("\uc9c0\ud558\ubb18\uc9c0", "\uce74\ud0c0\ucf64", ContentView.Catacomb),
            };

            _towerInfo = CreateContentCard(menuScroll, cards[0].Item1, cards[0].Item2, cards[0].Item3);
            _dungeonInfo = CreateContentCard(menuScroll, cards[1].Item1, cards[1].Item2, cards[1].Item3);
            _arenaInfo = CreateContentCard(menuScroll, cards[2].Item1, cards[2].Item2, cards[2].Item3);
            _travelInfo = CreateContentCard(menuScroll, cards[3].Item1, cards[3].Item2, cards[3].Item3);
            _goblinInfo = CreateContentCard(menuScroll, cards[4].Item1, cards[4].Item2, cards[4].Item3);
            _catacombInfo = CreateContentCard(menuScroll, cards[5].Item1, cards[5].Item2, cards[5].Item3);

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
            backText.text = "\u2190";
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
            _subContent = (subContentGo.GetComponent<RectTransform>() ?? subContentGo.AddComponent<RectTransform>());
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
            _resultText.enableWordWrapping = true;
        }

        private Transform CreateScrollableContent(RectTransform parent)
        {
            var scrollGo = new GameObject("Scroll");
            scrollGo.transform.SetParent(parent, false);
            var scrollRt = (scrollGo.GetComponent<RectTransform>() ?? scrollGo.AddComponent<RectTransform>());
            UIManager.StretchFull(scrollRt);
            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRt = (viewportGo.GetComponent<RectTransform>() ?? viewportGo.AddComponent<RectTransform>());
            UIManager.StretchFull(viewportRt);
            viewportGo.AddComponent<RectMask2D>();

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = (contentGo.GetComponent<RectTransform>() ?? contentGo.AddComponent<RectTransform>());
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
            cardGo.AddComponent<Image>().color = ColorPalette.Card;

            var cardBtn = cardGo.AddComponent<Button>();
            cardBtn.targetGraphic = cardGo.GetComponent<Image>();
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
            iconText.enableWordWrapping = false;
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
                case ContentView.Arena: BuildArenaSub(); break;
                case ContentView.Travel: BuildTravelSub(); break;
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
                if (tokens < 1) return;
                var stats = Game.Player.ComputeStats();
                var playerUnit = Game.BattleManagerService.CreatePlayerUnit(Game.Player, null, new Domain.Entities.PassiveSkill[0]);
                var result = tower.Challenge(playerUnit, tokens);
                if (result.IsFail()) return;

                var battle = result.Data.Battle;
                battle.RunToCompletion(50);

                var battleResult = tower.OnBattleResult(battle.State);
                if (battleResult.TokenConsumed)
                    Game.Player.Resources.Spend(ResourceType.CHALLENGE_TOKEN, 1);

                string rewardStr = "";
                if (battleResult.Advanced && battleResult.Reward != null)
                {
                    foreach (var r in battleResult.Reward.Resources)
                    {
                        Game.Player.Resources.Add(r.Type, r.Amount);
                        rewardStr += $"{NumberFormatter.FormatResourceType(r.Type)}: +{r.Amount}  ";
                    }
                }

                _resultText.text = battle.State == BattleState.VICTORY
                    ? $"\uc2b9\ub9ac! {rewardStr}"
                    : "\ud328\ubc30...";
                Game.SaveGame();
                ShowSubPanel(ContentView.Tower);
            });
        }

        private void BuildDungeonSub()
        {
            _subTitle.text = "\ub358\uc804";
            int remaining = Game.DungeonManager.GetRemainingCount();
            CreateInfoRow(_subContent, $"\ub0a8\uc740 \uc785\uc7a5: {remaining}/{Game.DungeonManager.DailyLimit}");

            var dungeonTypes = new[] { DungeonType.DRAGON_NEST, DungeonType.CELESTIAL_TREE, DungeonType.SKY_ISLAND };
            var dungeonNames = new Dictionary<DungeonType, string>
            {
                { DungeonType.DRAGON_NEST, "\ub4dc\ub798\uace4 \ub465\uc9c0" },
                { DungeonType.CELESTIAL_TREE, "\uc138\uacc4\uc218" },
                { DungeonType.SKY_ISLAND, "\ud558\ub298\uc12c" },
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
                    var result = Game.ChallengeDungeon(capturedType);
                    if (result.IsFail()) { _resultText.text = result.Message; return; }

                    var battle = result.Data.Battle;
                    battle.RunToCompletion(50);
                    var reward = Game.OnDungeonBattleResult(capturedType, battle.State);

                    string rewardStr = "";
                    if (reward != null)
                    {
                        foreach (var r in reward.Resources)
                            rewardStr += $"{NumberFormatter.FormatResourceType(r.Type)}: +{r.Amount}  ";
                    }

                    _resultText.text = battle.State == BattleState.VICTORY
                        ? $"\uc2b9\ub9ac! {rewardStr}"
                        : "\ud328\ubc30...";
                    Game.SaveGame();
                    ShowSubPanel(ContentView.Dungeon);
                });

                if (dungeon.ClearedStage > 0)
                {
                    CreateActionButton(_subContent, $"{name} \uc18c\ud0d5", () =>
                    {
                        var sweepResult = Game.SweepDungeon(capturedType);
                        if (sweepResult.IsFail()) { _resultText.text = sweepResult.Message; return; }

                        string rewardStr = "";
                        foreach (var r in sweepResult.Data.Reward.Resources)
                            rewardStr += $"{NumberFormatter.FormatResourceType(r.Type)}: +{r.Amount}  ";
                        _resultText.text = $"\uc18c\ud0d5 \uc644\ub8cc! {rewardStr}";
                        Game.SaveGame();
                        ShowSubPanel(ContentView.Dungeon);
                    });
                }
            }
        }

        private void BuildArenaSub()
        {
            _subTitle.text = "\uc544\ub808\ub098";
            var arena = Game.ArenaSystem;
            int tickets = (int)Game.Player.Resources.ArenaTickets;

            CreateInfoRow(_subContent, $"\ub4f1\uae09: {arena.Tier}  \ud3ec\uc778\ud2b8: {arena.Points}");
            CreateInfoRow(_subContent, $"\ub0a8\uc740 \uc785\uc7a5: {arena.GetRemainingEntries()}  \ud2f0\ucf13: {tickets}");

            CreateActionButton(_subContent, "\ub300\uc804 (\ud2f0\ucf13 1\uac1c)", () =>
            {
                var playerUnit = Game.BattleManagerService.CreatePlayerUnit(Game.Player, null, new Domain.Entities.PassiveSkill[0]);
                var result = arena.Fight(playerUnit, tickets, Game.Rng);
                if (result.IsFail()) { _resultText.text = result.Message; return; }

                Game.Player.Resources.Spend(ResourceType.ARENA_TICKET, 1);
                int wins = 0;
                for (int i = 0; i < result.Data.Results.Count; i++)
                {
                    if (result.Data.Results[i] == BattleState.VICTORY) wins++;
                }

                var reward = arena.GetReward();
                if (wins > 0)
                {
                    foreach (var r in reward.Resources)
                        Game.Player.Resources.Add(r.Type, r.Amount);
                }

                _resultText.text = $"\uacb0\uacfc: {wins}\uc2b9 {4 - wins}\ud328  \ub4f1\uae09: {arena.Tier}  \ud3ec\uc778\ud2b8: {arena.Points}";
                Game.SaveGame();
                ShowSubPanel(ContentView.Arena);
            });
        }

        private void BuildTravelSub()
        {
            _subTitle.text = "\uc5ec\ud589";
            var travel = Game.TravelSystem;
            travel.MaxClearedChapter = Mathf.Max(1, Game.Player.ClearedChapterMax);
            int stamina = (int)Game.Player.Resources.Stamina;

            CreateInfoRow(_subContent, $"\ucd5c\uace0 \ud074\ub9ac\uc5b4 \ucc55\ud130: {travel.MaxClearedChapter}  \uc2a4\ud0dc\ubbf8\ub098: {stamina}");

            var multipliers = travel.GetAvailableMultipliers();
            foreach (int mult in multipliers)
            {
                int goldPreview = travel.GetGoldPreview(mult);
                int capturedMult = mult;
                CreateActionButton(_subContent, $"\uc5ec\ud589 x{mult} (\uace8\ub4dc: {NumberFormatter.FormatInt(goldPreview)})", () =>
                {
                    var result = Game.TravelRun(capturedMult);
                    if (result.IsFail()) { _resultText.text = result.Message; return; }

                    string rewardStr = "";
                    foreach (var r in result.Data.Reward.Resources)
                        rewardStr += $"{NumberFormatter.FormatResourceType(r.Type)}: +{r.Amount}  ";
                    _resultText.text = $"\uc5ec\ud589 \uc644\ub8cc! {rewardStr}";
                    Game.SaveGame();
                    ShowSubPanel(ContentView.Travel);
                });
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
                var result = miner.Mine(pickaxes);
                if (result.IsFail()) { _resultText.text = result.Message; return; }
                Game.Player.Resources.Spend(ResourceType.PICKAXE, 1);
                _resultText.text = $"\uad11\uc11d +{result.Data.OreGained}  (\ucd1d: {miner.OreCount})";
                Game.SaveGame();
                ShowSubPanel(ContentView.GoblinMine);
            });

            if (miner.CanUseCart())
            {
                CreateActionButton(_subContent, "\uc218\ub808 \ubcf4\ub0b4\uae30 (30 \uad11\uc11d)", () =>
                {
                    var result = miner.UseCart(Game.Rng);
                    if (result.IsFail()) { _resultText.text = result.Message; return; }
                    foreach (var r in result.Data.Reward.Resources)
                        Game.Player.Resources.Add(r.Type, r.Amount);

                    string rewardStr = "";
                    foreach (var r in result.Data.Reward.Resources)
                        rewardStr += $"{NumberFormatter.FormatResourceType(r.Type)}: +{r.Amount}  ";
                    _resultText.text = $"\uc218\ub808 \ubcf4\uc0c1: {rewardStr}";
                    Game.SaveGame();
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
                    catacomb.StartRun();
                    Game.SaveGame();
                    ShowSubPanel(ContentView.Catacomb);
                });
            }
            else
            {
                CreateActionButton(_subContent, "\ub2e4\uc74c \uc804\ud22c", () =>
                {
                    var playerUnit = Game.BattleManagerService.CreatePlayerUnit(Game.Player, null, new Domain.Entities.PassiveSkill[0]);
                    var battle = catacomb.GetNextBattle(playerUnit);
                    if (battle == null) return;
                    battle.RunToCompletion(50);
                    var result = catacomb.OnBattleResult(battle.State);

                    if (!result.ContinueRun)
                    {
                        string rewardStr = "";
                        foreach (var r in result.Reward.Resources)
                        {
                            Game.Player.Resources.Add(r.Type, r.Amount);
                            rewardStr += $"{NumberFormatter.FormatResourceType(r.Type)}: +{r.Amount}  ";
                        }
                        _resultText.text = $"\ud328\ubc30. \ubcf4\uc0c1: {rewardStr}";
                    }
                    else
                    {
                        _resultText.text = $"\uc2b9\ub9ac! \uce35: {catacomb.CurrentRunFloor}  \uc804\ud22c: {catacomb.CurrentBattleIndex}/{catacomb.BattlesPerFloor}";
                    }
                    Game.SaveGame();
                    ShowSubPanel(ContentView.Catacomb);
                });

                CreateActionButton(_subContent, "\ud0d0\ud5d8 \uc885\ub8cc", () =>
                {
                    var reward = catacomb.EndRun();
                    foreach (var r in reward.Resources)
                        Game.Player.Resources.Add(r.Type, r.Amount);

                    string rewardStr = "";
                    foreach (var r in reward.Resources)
                        rewardStr += $"{NumberFormatter.FormatResourceType(r.Type)}: +{r.Amount}  ";
                    _resultText.text = $"\ud0d0\ud5d8 \uc885\ub8cc. \ubcf4\uc0c1: {rewardStr}";
                    Game.SaveGame();
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

                _arenaInfo.text = $"{Game.ArenaSystem.Tier}  \ub0a8\uc740: {Game.ArenaSystem.GetRemainingEntries()}";

                _travelInfo.text = $"\uc2a4\ud0dc\ubbf8\ub098: {(int)Game.Player.Resources.Stamina}";

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
