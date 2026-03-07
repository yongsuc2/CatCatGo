using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Data;
using CatCatGo.Domain.ValueObjects;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Components;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Screens
{
    public class ChapterTreasureScreen : BaseScreen
    {
        private RectTransform _chapterListContent;
        private List<GameObject> _chapterEntries = new List<GameObject>();

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

            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(transform, false);
            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 88;
            headerLe.flexibleHeight = 0;
            headerGo.AddComponent<Image>().color = ColorPalette.Card;

            var headerLayout = headerGo.AddComponent<HorizontalLayoutGroup>();
            headerLayout.childForceExpandWidth = true;
            headerLayout.childForceExpandHeight = true;
            headerLayout.padding = new RectOffset(16, 16, 12, 12);

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(headerGo.transform, false);
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "\ucc55\ud130 \ubcf4\ubb3c";
            titleText.fontSize = 48;
            titleText.color = ColorPalette.Text;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.raycastTarget = false;

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
            _chapterListContent = contentGo.GetComponent<RectTransform>();

            if (_chapterListContent == null) _chapterListContent = contentGo.AddComponent<RectTransform>();
            _chapterListContent.anchorMin = new Vector2(0, 1);
            _chapterListContent.anchorMax = new Vector2(1, 1);
            _chapterListContent.pivot = new Vector2(0.5f, 1);
            _chapterListContent.offsetMin = Vector2.zero;
            _chapterListContent.offsetMax = Vector2.zero;

            var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 16;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.padding = new RectOffset(12, 12, 12, 12);

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = _chapterListContent;
            scrollRect.viewport = viewportRt;
        }

        public override void Refresh()
        {
            if (Game == null || Game.Player == null) return;

            foreach (var go in _chapterEntries)
                Destroy(go);
            _chapterEntries.Clear();

            var chapterIds = ChapterTreasureTable.GetAvailableChapterIds(Game.Player.ClearedChapterMax);

            foreach (int chapterId in chapterIds)
            {
                var chapterGo = new GameObject($"Chapter_{chapterId}");
                chapterGo.transform.SetParent(_chapterListContent, false);
                chapterGo.AddComponent<Image>().color = ColorPalette.Card;

                var chapterLayout = chapterGo.AddComponent<VerticalLayoutGroup>();
                chapterLayout.spacing = 8;
                chapterLayout.childForceExpandWidth = true;
                chapterLayout.childForceExpandHeight = false;
                chapterLayout.padding = new RectOffset(16, 16, 12, 12);

                var chapterFitter = chapterGo.AddComponent<ContentSizeFitter>();
                chapterFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var chapterHeaderGo = new GameObject("ChapterHeader");
                chapterHeaderGo.transform.SetParent(chapterGo.transform, false);
                var chapterHeaderLe = chapterHeaderGo.AddComponent<LayoutElement>();
                chapterHeaderLe.preferredHeight = 60;
                var chapterHeaderText = chapterHeaderGo.AddComponent<TextMeshProUGUI>();

                int bestDay = Game.Player.BestSurvivalDays.TryGetValue(chapterId, out var val) ? val : 0;
                int totalDays = ChapterTreasureTable.GetTotalDays(chapterId);
                int clearSentinel = ChapterTreasureTable.GetClearSentinelDay(chapterId);
                bool cleared = bestDay >= clearSentinel;

                chapterHeaderText.text = cleared
                    ? $"\ucc55\ud130 {chapterId} - \ud074\ub9ac\uc5b4!"
                    : $"\ucc55\ud130 {chapterId} - \ucd5c\uace0 {bestDay}\uc77c/{totalDays}\uc77c";
                chapterHeaderText.fontSize = 40;
                chapterHeaderText.color = cleared ? ColorPalette.Heal : ColorPalette.Text;
                chapterHeaderText.fontStyle = FontStyles.Bold;
                chapterHeaderText.alignment = TextAlignmentOptions.Left;
                chapterHeaderText.raycastTarget = false;

                var progressGo = new GameObject("ProgressBar");
                progressGo.transform.SetParent(chapterGo.transform, false);
                var progressLe = progressGo.AddComponent<LayoutElement>();
                progressLe.preferredHeight = 28;
                var progressBar = progressGo.AddComponent<ProgressBarView>();
                progressBar.Initialize(400, 28);
                float progressValue = cleared ? totalDays : Mathf.Min(bestDay, totalDays);
                progressBar.SetProgress(progressValue, totalDays);
                progressBar.SetColor(cleared ? ColorPalette.Heal : ColorPalette.ProgressBarFill);

                var milestones = ChapterTreasureTable.GetMilestonesForChapter(chapterId);

                foreach (var milestone in milestones)
                {
                    string status = Game.ChapterTreasureSystem.GetMilestoneStatus(milestone, Game.Player);
                    CreateMilestoneRow(chapterGo.transform, milestone, status);
                }

                _chapterEntries.Add(chapterGo);
            }
        }

        private void CreateMilestoneRow(Transform parent, ChapterMilestone milestone, string status)
        {
            var rowGo = new GameObject("Milestone_" + milestone.Id);
            rowGo.transform.SetParent(parent, false);
            var rowLe = rowGo.AddComponent<LayoutElement>();
            rowLe.preferredHeight = 72;

            Color bgColor;
            if (status == "claimed") bgColor = new Color(0.15f, 0.25f, 0.15f, 1f);
            else if (status == "claimable") bgColor = new Color(0.2f, 0.2f, 0.35f, 1f);
            else bgColor = ColorPalette.CardLight;

            rowGo.AddComponent<Image>().color = bgColor;

            var rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 12;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;
            rowLayout.padding = new RectOffset(12, 12, 4, 4);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(rowGo.transform, false);
            var labelLe = labelGo.AddComponent<LayoutElement>();
            labelLe.preferredWidth = 140;
            var labelText = labelGo.AddComponent<TextMeshProUGUI>();
            labelText.text = milestone.Label;
            labelText.fontSize = 36;
            labelText.color = status == "claimed" ? ColorPalette.TextDim : ColorPalette.Text;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.raycastTarget = false;

            var rewardGo = new GameObject("Reward");
            rewardGo.transform.SetParent(rowGo.transform, false);
            var rewardLe = rewardGo.AddComponent<LayoutElement>();
            rewardLe.flexibleWidth = 1;
            var rewardText = rewardGo.AddComponent<TextMeshProUGUI>();
            var sb = new System.Text.StringBuilder();
            foreach (var r in milestone.MilestoneReward.Resources)
                sb.Append($"{NumberFormatter.FormatResourceType(r.Type)}: {r.Amount}  ");
            rewardText.text = sb.ToString();
            rewardText.fontSize = 32;
            rewardText.color = status == "claimed" ? ColorPalette.TextDim : ColorPalette.Gold;
            rewardText.alignment = TextAlignmentOptions.Left;
            rewardText.raycastTarget = false;

            if (status == "claimable")
            {
                var claimGo = new GameObject("ClaimBtn");
                claimGo.transform.SetParent(rowGo.transform, false);
                var claimLe2 = claimGo.AddComponent<LayoutElement>();
                claimLe2.preferredWidth = 110;
                var claimBg = claimGo.AddComponent<Image>();
                claimBg.color = ColorPalette.Heal;
                var claimBtn = claimGo.AddComponent<Button>();
                claimBtn.targetGraphic = claimBg;
                string milestoneId = milestone.Id;
                claimBtn.onClick.AddListener(() => OnClaimMilestone(milestoneId));
                var claimTextGo = new GameObject("Text");
                claimTextGo.transform.SetParent(claimGo.transform, false);
                var claimText = claimTextGo.AddComponent<TextMeshProUGUI>();
                claimText.text = "\uc218\ub839";
                claimText.fontSize = 34;
                claimText.color = Color.white;
                claimText.fontStyle = FontStyles.Bold;
                claimText.alignment = TextAlignmentOptions.Center;
                claimText.raycastTarget = false;
                UIManager.StretchFull(claimTextGo.GetComponent<RectTransform>());
            }
            else if (status == "claimed")
            {
                var checkGo = new GameObject("Check");
                checkGo.transform.SetParent(rowGo.transform, false);
                var checkLe = checkGo.AddComponent<LayoutElement>();
                checkLe.preferredWidth = 50;
                var checkText = checkGo.AddComponent<TextMeshProUGUI>();
                checkText.text = "V";
                checkText.fontSize = 40;
                checkText.color = ColorPalette.Heal;
                checkText.alignment = TextAlignmentOptions.Center;
                checkText.raycastTarget = false;
            }
        }

        private bool _isRequestPending;

        private void OnClaimMilestone(string milestoneId)
        {
            if (_isRequestPending) return;
            _isRequestPending = true;
            Game.ClaimChapterTreasureAsync(milestoneId, result =>
            {
                _isRequestPending = false;
                if (result.IsOk())
                    UI.Refresh();
            });
        }
    }
}
