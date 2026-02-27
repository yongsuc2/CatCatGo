using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Data;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Screens
{
    public class EventScreen : BaseScreen
    {
        private TextMeshProUGUI _headerText;
        private RectTransform _calendarGrid;
        private Button _claimTodayButton;
        private TextMeshProUGUI _claimButtonLabel;
        private TextMeshProUGUI _statusText;
        private List<DayCell> _dayCells = new List<DayCell>();

        private struct DayCell
        {
            public GameObject Root;
            public Image Background;
            public TextMeshProUGUI DayText;
            public TextMeshProUGUI RewardText;
            public TextMeshProUGUI CheckMark;
        }

        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            var scrollGo = new GameObject("ScrollView");
            scrollGo.transform.SetParent(transform, false);
            var scrollRt = scrollGo.AddComponent<RectTransform>();
            UIManager.StretchFull(scrollRt);
            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(scrollGo.transform, false);
            var viewportRt = viewportGo.AddComponent<RectTransform>();
            UIManager.StretchFull(viewportRt);
            viewportGo.AddComponent<RectMask2D>();

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = contentGo.AddComponent<RectTransform>();
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

            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(contentGo.transform, false);
            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 36;
            _headerText = headerGo.AddComponent<TextMeshProUGUI>();
            _headerText.fontSize = 30;
            _headerText.color = ColorPalette.Text;
            _headerText.alignment = TextAlignmentOptions.Center;
            _headerText.raycastTarget = false;

            var gridContainerGo = new GameObject("GridContainer");
            gridContainerGo.transform.SetParent(contentGo.transform, false);
            _calendarGrid = gridContainerGo.AddComponent<RectTransform>();
            var gridContainerLe = gridContainerGo.AddComponent<LayoutElement>();
            gridContainerLe.preferredHeight = 300;

            var grid = gridContainerGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(240, 120);
            grid.spacing = new Vector2(10, 10);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;

            var statusGo = new GameObject("Status");
            statusGo.transform.SetParent(contentGo.transform, false);
            var statusLe = statusGo.AddComponent<LayoutElement>();
            statusLe.preferredHeight = 30;
            _statusText = statusGo.AddComponent<TextMeshProUGUI>();
            _statusText.fontSize = 24;
            _statusText.color = ColorPalette.TextDim;
            _statusText.alignment = TextAlignmentOptions.Center;
            _statusText.raycastTarget = false;

            var claimGo = new GameObject("ClaimTodayBtn");
            claimGo.transform.SetParent(contentGo.transform, false);
            var claimLe = claimGo.AddComponent<LayoutElement>();
            claimLe.preferredHeight = 56;
            var claimBg = claimGo.AddComponent<Image>();
            claimBg.color = ColorPalette.Heal;
            _claimTodayButton = claimGo.AddComponent<Button>();
            _claimTodayButton.targetGraphic = claimBg;
            _claimTodayButton.onClick.AddListener(OnClaimToday);

            var claimTextGo = new GameObject("Text");
            claimTextGo.transform.SetParent(claimGo.transform, false);
            _claimButtonLabel = claimTextGo.AddComponent<TextMeshProUGUI>();
            _claimButtonLabel.text = "\ucd9c\uc11d \uccb4\ud06c";
            _claimButtonLabel.fontSize = 28;
            _claimButtonLabel.color = Color.white;
            _claimButtonLabel.alignment = TextAlignmentOptions.Center;
            _claimButtonLabel.raycastTarget = false;
            UIManager.StretchFull(claimTextGo.GetComponent<RectTransform>());
        }

        private void OnClaimToday()
        {
            if (Game == null) return;
            Game.ClaimAttendance();
            UI.Refresh();
        }

        public override void Refresh()
        {
            if (Game == null || Game.Player == null) return;

            var attendance = Game.AttendanceSystem;
            var allRewards = AttendanceDataTable.GetAllRewards();
            int totalDays = allRewards.Count;

            int checkedCount = 0;
            for (int i = 0; i < attendance.CheckedDays.Length; i++)
            {
                if (attendance.CheckedDays[i]) checkedCount++;
            }

            _headerText.text = $"\ucd9c\uc11d\ubd80 ({checkedCount}/{totalDays})";

            foreach (var cell in _dayCells)
                Destroy(cell.Root);
            _dayCells.Clear();

            for (int i = 0; i < totalDays; i++)
            {
                bool checked_ = i < attendance.CheckedDays.Length && attendance.CheckedDays[i];
                bool isToday = i == attendance.GetCurrentDay();
                var rewardDef = allRewards.Count > i ? allRewards[i] : null;

                var cellGo = new GameObject($"Day_{i + 1}");
                cellGo.transform.SetParent(_calendarGrid, false);

                Color bgColor;
                if (checked_) bgColor = new Color(0.15f, 0.3f, 0.15f, 1f);
                else if (isToday) bgColor = new Color(0.2f, 0.2f, 0.4f, 1f);
                else bgColor = ColorPalette.Card;

                var bg = cellGo.AddComponent<Image>();
                bg.color = bgColor;

                var cellLayout = cellGo.AddComponent<VerticalLayoutGroup>();
                cellLayout.spacing = 2;
                cellLayout.childForceExpandWidth = true;
                cellLayout.childForceExpandHeight = false;
                cellLayout.childAlignment = TextAnchor.MiddleCenter;
                cellLayout.padding = new RectOffset(4, 4, 4, 4);

                var dayGo = new GameObject("Day");
                dayGo.transform.SetParent(cellGo.transform, false);
                var dayLe = dayGo.AddComponent<LayoutElement>();
                dayLe.preferredHeight = 30;
                var dayText = dayGo.AddComponent<TextMeshProUGUI>();
                dayText.text = $"Day {i + 1}";
                dayText.fontSize = 26;
                dayText.fontStyle = FontStyles.Bold;
                dayText.color = isToday ? ColorPalette.Gold : ColorPalette.Text;
                dayText.alignment = TextAlignmentOptions.Center;
                dayText.raycastTarget = false;

                var rewardGo = new GameObject("Reward");
                rewardGo.transform.SetParent(cellGo.transform, false);
                var rewardLe = rewardGo.AddComponent<LayoutElement>();
                rewardLe.flexibleHeight = 1;
                var rewardText = rewardGo.AddComponent<TextMeshProUGUI>();
                rewardText.text = rewardDef != null ? rewardDef.Description : "";
                rewardText.fontSize = 22;
                rewardText.color = ColorPalette.TextDim;
                rewardText.alignment = TextAlignmentOptions.Center;
                rewardText.raycastTarget = false;
                rewardText.enableWordWrapping = true;

                var checkGo = new GameObject("Check");
                checkGo.transform.SetParent(cellGo.transform, false);
                var checkLe = checkGo.AddComponent<LayoutElement>();
                checkLe.preferredHeight = 28;
                var checkText = checkGo.AddComponent<TextMeshProUGUI>();
                checkText.text = checked_ ? "\u2713" : "";
                checkText.fontSize = 30;
                checkText.color = ColorPalette.Heal;
                checkText.alignment = TextAlignmentOptions.Center;
                checkText.raycastTarget = false;

                _dayCells.Add(new DayCell
                {
                    Root = cellGo,
                    Background = bg,
                    DayText = dayText,
                    RewardText = rewardText,
                    CheckMark = checkText,
                });
            }

            bool canCheckIn = attendance.CanCheckIn();
            _claimTodayButton.interactable = canCheckIn;
            _claimButtonLabel.text = canCheckIn ? "\ucd9c\uc11d \uccb4\ud06c" : "\uc624\ub298 \ucd9c\uc11d \uc644\ub8cc";
            _statusText.text = attendance.IsComplete() ? "\ubaa8\ub4e0 \ucd9c\uc11d \uc644\ub8cc!" : $"\ub2e4\uc74c \ubcf4\uc0c1: Day {attendance.GetCurrentDay() + 1}";
        }
    }
}
