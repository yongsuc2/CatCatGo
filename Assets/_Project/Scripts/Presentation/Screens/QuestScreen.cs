using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Meta;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Components;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Screens
{
    public class QuestScreen : BaseScreen
    {
        private TabBarView _tabBar;
        private RectTransform _questContent;
        private Button _claimAllButton;
        private TextMeshProUGUI _claimAllText;
        private List<GameObject> _questEntries = new List<GameObject>();
        private int _activeTab;

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

            var tabGo = new GameObject("TabBar");
            tabGo.transform.SetParent(transform, false);
            var tabLe = tabGo.AddComponent<LayoutElement>();
            tabLe.preferredHeight = 40;
            tabGo.AddComponent<Image>().color = ColorPalette.Card;
            _tabBar = tabGo.AddComponent<TabBarView>();
            _tabBar.Initialize(new[] { "\uc77c\uc77c", "\uc8fc\uac04" });
            _tabBar.OnTabSelected += OnTabChanged;

            var scrollGo = new GameObject("ScrollView");
            scrollGo.transform.SetParent(transform, false);
            var scrollLe = scrollGo.AddComponent<LayoutElement>();
            scrollLe.flexibleHeight = 1;

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
            _questContent = contentGo.GetComponent<RectTransform>();

            if (_questContent == null) _questContent = contentGo.AddComponent<RectTransform>();
            _questContent.anchorMin = new Vector2(0, 1);
            _questContent.anchorMax = new Vector2(1, 1);
            _questContent.pivot = new Vector2(0.5f, 1);
            _questContent.offsetMin = Vector2.zero;
            _questContent.offsetMax = Vector2.zero;

            var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 6;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.padding = new RectOffset(12, 12, 8, 8);

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = _questContent;
            scrollRect.viewport = viewportRt;

            var claimAllGo = new GameObject("ClaimAllBtn");
            claimAllGo.transform.SetParent(transform, false);
            var claimAllLe = claimAllGo.AddComponent<LayoutElement>();
            claimAllLe.preferredHeight = 50;
            var claimAllBg = claimAllGo.AddComponent<Image>();
            claimAllBg.color = ColorPalette.Heal;
            _claimAllButton = claimAllGo.AddComponent<Button>();
            _claimAllButton.targetGraphic = claimAllBg;
            _claimAllButton.onClick.AddListener(OnClaimAll);

            var claimAllTextGo = new GameObject("Text");
            claimAllTextGo.transform.SetParent(claimAllGo.transform, false);
            _claimAllText = claimAllTextGo.AddComponent<TextMeshProUGUI>();
            _claimAllText.text = "\ubaa8\ub450 \uc218\ub839";
            _claimAllText.fontSize = 26;
            _claimAllText.color = Color.white;
            _claimAllText.alignment = TextAlignmentOptions.Center;
            _claimAllText.raycastTarget = false;
            UIManager.StretchFull(claimAllTextGo.GetComponent<RectTransform>());
        }

        private void OnTabChanged(int index)
        {
            _activeTab = index;
            Refresh();
        }

        private GameEvent GetCurrentEvent()
        {
            var events = Game.EventManagerSystem.GetActiveEvents();
            string prefix = _activeTab == 0 ? "daily_" : "weekly_";
            return events.FirstOrDefault(e => e.Id.StartsWith(prefix));
        }

        public override void Refresh()
        {
            if (Game == null || Game.Player == null) return;

            foreach (var go in _questEntries)
                Destroy(go);
            _questEntries.Clear();

            var evt = GetCurrentEvent();
            if (evt == null)
            {
                CreateEmptyRow("\ud034\uc2a4\ud2b8\uac00 \uc5c6\uc2b5\ub2c8\ub2e4");
                _claimAllButton.interactable = false;
                return;
            }

            bool hasClaimable = false;
            foreach (var mission in evt.Missions)
            {
                bool completed = mission.Current >= mission.Target;
                bool claimed = mission.Claimed;
                if (completed && !claimed) hasClaimable = true;

                var go = new GameObject("Quest_" + mission.Id);
                go.transform.SetParent(_questContent, false);
                var le = go.AddComponent<LayoutElement>();
                le.preferredHeight = 80;
                go.AddComponent<Image>().color = claimed ? new Color(0.15f, 0.2f, 0.15f, 1f) : ColorPalette.Card;

                var layout = go.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 4;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                layout.padding = new RectOffset(12, 12, 6, 6);

                var descGo = new GameObject("Description");
                descGo.transform.SetParent(go.transform, false);
                var descLe = descGo.AddComponent<LayoutElement>();
                descLe.preferredHeight = 22;
                var descText = descGo.AddComponent<TextMeshProUGUI>();
                descText.text = mission.Description;
                descText.fontSize = 22;
                descText.color = claimed ? ColorPalette.TextDim : ColorPalette.Text;
                descText.alignment = TextAlignmentOptions.Left;
                descText.raycastTarget = false;

                var progressRowGo = new GameObject("ProgressRow");
                progressRowGo.transform.SetParent(go.transform, false);
                var progressRowLe = progressRowGo.AddComponent<LayoutElement>();
                progressRowLe.preferredHeight = 20;
                var progressBar = progressRowGo.AddComponent<ProgressBarView>();
                progressBar.Initialize(400, 20);
                progressBar.SetProgress(mission.Current, mission.Target, $"{mission.Current}/{mission.Target}");
                progressBar.SetColor(completed ? ColorPalette.Heal : ColorPalette.ProgressBarFill);

                var rewardRowGo = new GameObject("RewardRow");
                rewardRowGo.transform.SetParent(go.transform, false);
                var rewardRowLe = rewardRowGo.AddComponent<LayoutElement>();
                rewardRowLe.preferredHeight = 24;

                var rewardRowLayout = rewardRowGo.AddComponent<HorizontalLayoutGroup>();
                rewardRowLayout.spacing = 8;
                rewardRowLayout.childForceExpandWidth = false;
                rewardRowLayout.childForceExpandHeight = true;

                var rewardTextGo = new GameObject("RewardText");
                rewardTextGo.transform.SetParent(rewardRowGo.transform, false);
                var rewardTextLe = rewardTextGo.AddComponent<LayoutElement>();
                rewardTextLe.flexibleWidth = 1;
                var rewardText = rewardTextGo.AddComponent<TextMeshProUGUI>();
                var rewardStr = "";
                if (mission.Reward != null)
                {
                    foreach (var r in mission.Reward.Resources)
                        rewardStr += $"{NumberFormatter.FormatResourceType(r.Type)}: {r.Amount}  ";
                }
                rewardText.text = rewardStr;
                rewardText.fontSize = 20;
                rewardText.color = ColorPalette.Gold;
                rewardText.alignment = TextAlignmentOptions.Left;
                rewardText.raycastTarget = false;

                if (completed && !claimed)
                {
                    var claimGo = new GameObject("ClaimBtn");
                    claimGo.transform.SetParent(rewardRowGo.transform, false);
                    var claimLe2 = claimGo.AddComponent<LayoutElement>();
                    claimLe2.preferredWidth = 70;
                    var claimBg = claimGo.AddComponent<Image>();
                    claimBg.color = ColorPalette.Heal;
                    var claimBtn = claimGo.AddComponent<Button>();
                    claimBtn.targetGraphic = claimBg;
                    string missionId = mission.Id;
                    claimBtn.onClick.AddListener(() => OnClaimMission(missionId));
                    var claimTextGo = new GameObject("Text");
                    claimTextGo.transform.SetParent(claimGo.transform, false);
                    var claimText = claimTextGo.AddComponent<TextMeshProUGUI>();
                    claimText.text = "\uc218\ub839";
                    claimText.fontSize = 20;
                    claimText.color = Color.white;
                    claimText.alignment = TextAlignmentOptions.Center;
                    claimText.raycastTarget = false;
                    UIManager.StretchFull(claimTextGo.GetComponent<RectTransform>());
                }
                else if (claimed)
                {
                    var doneGo = new GameObject("Done");
                    doneGo.transform.SetParent(rewardRowGo.transform, false);
                    var doneLe = doneGo.AddComponent<LayoutElement>();
                    doneLe.preferredWidth = 70;
                    var doneText = doneGo.AddComponent<TextMeshProUGUI>();
                    doneText.text = "V";
                    doneText.fontSize = 26;
                    doneText.color = ColorPalette.Heal;
                    doneText.alignment = TextAlignmentOptions.Center;
                    doneText.raycastTarget = false;
                }

                _questEntries.Add(go);
            }

            _claimAllButton.interactable = hasClaimable;
        }

        private void OnClaimMission(string missionId)
        {
            var evt = GetCurrentEvent();
            if (evt == null) return;

            var reward = evt.ClaimMissionReward(missionId);
            if (reward == null) return;

            foreach (var r in reward.Resources)
                Game.Player.Resources.Add(r.Type, r.Amount);

            Game.SaveGame();
            UI.Refresh();
        }

        private void OnClaimAll()
        {
            var evt = GetCurrentEvent();
            if (evt == null) return;

            foreach (var mission in evt.Missions)
            {
                if (mission.Current >= mission.Target && !mission.Claimed)
                {
                    var reward = evt.ClaimMissionReward(mission.Id);
                    if (reward != null)
                    {
                        foreach (var r in reward.Resources)
                            Game.Player.Resources.Add(r.Type, r.Amount);
                    }
                }
            }

            Game.SaveGame();
            UI.Refresh();
        }

        private void CreateEmptyRow(string text)
        {
            var go = new GameObject("Empty");
            go.transform.SetParent(_questContent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 40;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.color = ColorPalette.TextDim;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            _questEntries.Add(go);
        }
    }
}
