using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Data;
using CatCatGo.Infrastructure;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Screens
{
    public class DebugScreen : BaseScreen
    {
        private TMP_InputField _chapterInput;
        private TextMeshProUGUI _statusText;
        private TextMeshProUGUI _logText;
        private ScrollRect _logScrollRect;
        private LogLevel _logFilterLevel = LogLevel.Debug;

        private void Awake()
        {
            BuildUI();
        }

        private void OnEnable()
        {
            GameLog.OnLogAdded += OnLogAdded;
            RefreshLogView();
        }

        private void OnDisable()
        {
            GameLog.OnLogAdded -= OnLogAdded;
        }

        private void OnLogAdded(LogEntry entry)
        {
            if (entry.Level < _logFilterLevel) return;
            AppendLogLine(entry);
            Canvas.ForceUpdateCanvases();
            if (_logScrollRect != null)
                _logScrollRect.verticalNormalizedPosition = 0f;
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
            contentLayout.spacing = 8;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.padding = new RectOffset(16, 16, 16, 16);

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;
            scrollRect.viewport = viewportRt;

            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(contentGo.transform, false);
            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 40;

            var headerLayout = headerGo.AddComponent<HorizontalLayoutGroup>();
            headerLayout.childForceExpandWidth = true;
            headerLayout.childForceExpandHeight = true;

            var backGo = new GameObject("BackBtn");
            backGo.transform.SetParent(headerGo.transform, false);
            var backLe = backGo.AddComponent<LayoutElement>();
            backLe.preferredWidth = 60;
            var backBg = backGo.AddComponent<Image>();
            backBg.color = ColorPalette.ButtonSecondary;
            var backBtn = backGo.AddComponent<Button>();
            backBtn.targetGraphic = backBg;
            backBtn.onClick.AddListener(() => UI.ShowScreen(ScreenType.Settings));
            var backTextGo = new GameObject("Text");
            backTextGo.transform.SetParent(backGo.transform, false);
            var backText = backTextGo.AddComponent<TextMeshProUGUI>();
            backText.text = "<";
            backText.fontSize = 28;
            backText.color = ColorPalette.Text;
            backText.alignment = TextAlignmentOptions.Center;
            backText.raycastTarget = false;
            UIManager.StretchFull(backTextGo.GetComponent<RectTransform>());

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(headerGo.transform, false);
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "\ub514\ubc84\uadf8";
            titleText.fontSize = 32;
            titleText.color = ColorPalette.Hp;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.raycastTarget = false;

            var statusGo = new GameObject("Status");
            statusGo.transform.SetParent(contentGo.transform, false);
            var statusLe = statusGo.AddComponent<LayoutElement>();
            statusLe.preferredHeight = 24;
            _statusText = statusGo.AddComponent<TextMeshProUGUI>();
            _statusText.fontSize = 22;
            _statusText.color = ColorPalette.Gold;
            _statusText.alignment = TextAlignmentOptions.Center;
            _statusText.raycastTarget = false;

            CreateSectionHeader(contentGo.transform, "\uc790\uc6d0 \ucd94\uac00");

            CreateDebugButton(contentGo.transform, "+10000 \uace8\ub4dc", () =>
            {
                Game.Player.Resources.Add(ResourceType.GOLD, 10000);
                SetStatus("\uace8\ub4dc +10000");
            });

            CreateDebugButton(contentGo.transform, "+100 \ubcf4\uc11d", () =>
            {
                Game.Player.Resources.Add(ResourceType.GEMS, 100);
                SetStatus("\ubcf4\uc11d +100");
            });

            CreateDebugButton(contentGo.transform, "\uc2a4\ud0dc\ubbf8\ub098 \ucc44\uc6b0\uae30", () =>
            {
                Game.Player.Resources.SetAmount(ResourceType.STAMINA, Game.Player.Resources.GetStaminaMax());
                SetStatus("\uc2a4\ud0dc\ubbf8\ub098 MAX");
            });

            CreateDebugButton(contentGo.transform, "+10 \uac01\uc885 \ud1a0\ud070", () =>
            {
                Game.Player.Resources.Add(ResourceType.CHALLENGE_TOKEN, 10);
                Game.Player.Resources.Add(ResourceType.PICKAXE, 10);
                Game.Player.Resources.Add(ResourceType.EQUIPMENT_STONE, 10);
                Game.Player.Resources.Add(ResourceType.POWER_STONE, 10);
                Game.Player.Resources.Add(ResourceType.PET_EGG, 10);
                Game.Player.Resources.Add(ResourceType.PET_FOOD, 100);
                SetStatus("\ud1a0\ud070 +10, \uba39\uc774 +100");
            });

            CreateSectionHeader(contentGo.transform, "\ucc55\ud130 \uc124\uc815");

            var chapterRowGo = new GameObject("ChapterRow");
            chapterRowGo.transform.SetParent(contentGo.transform, false);
            var chapterRowLe = chapterRowGo.AddComponent<LayoutElement>();
            chapterRowLe.preferredHeight = 44;
            var chapterRowLayout = chapterRowGo.AddComponent<HorizontalLayoutGroup>();
            chapterRowLayout.spacing = 8;
            chapterRowLayout.childForceExpandWidth = false;
            chapterRowLayout.childForceExpandHeight = true;

            var inputGo = new GameObject("ChapterInput");
            inputGo.transform.SetParent(chapterRowGo.transform, false);
            var inputLe = inputGo.AddComponent<LayoutElement>();
            inputLe.preferredWidth = 120;
            inputGo.AddComponent<Image>().color = ColorPalette.CardLight;

            var inputTextArea = new GameObject("TextArea");
            inputTextArea.transform.SetParent(inputGo.transform, false);
            var inputTextAreaRt = inputTextArea.GetComponent<RectTransform>();

            if (inputTextAreaRt == null) inputTextAreaRt = inputTextArea.AddComponent<RectTransform>();
            UIManager.StretchFull(inputTextAreaRt);
            inputTextAreaRt.offsetMin = new Vector2(8, 4);
            inputTextAreaRt.offsetMax = new Vector2(-8, -4);

            var inputTextGo = new GameObject("Text");
            inputTextGo.transform.SetParent(inputTextArea.transform, false);
            var inputText = inputTextGo.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 24;
            inputText.color = ColorPalette.Text;
            inputText.alignment = TextAlignmentOptions.Left;
            var inputTextRt = inputTextGo.GetComponent<RectTransform>();
            UIManager.StretchFull(inputTextRt);

            var placeholderGo = new GameObject("Placeholder");
            placeholderGo.transform.SetParent(inputTextArea.transform, false);
            var placeholder = placeholderGo.AddComponent<TextMeshProUGUI>();
            placeholder.text = "\ucc55\ud130 ID";
            placeholder.fontSize = 24;
            placeholder.color = ColorPalette.TextDim;
            placeholder.fontStyle = FontStyles.Italic;
            placeholder.alignment = TextAlignmentOptions.Left;
            var placeholderRt = placeholderGo.GetComponent<RectTransform>();
            UIManager.StretchFull(placeholderRt);

            _chapterInput = inputGo.AddComponent<TMP_InputField>();
            _chapterInput.textViewport = inputTextAreaRt;
            _chapterInput.textComponent = inputText;
            _chapterInput.placeholder = placeholder;
            _chapterInput.contentType = TMP_InputField.ContentType.IntegerNumber;

            var setChapterGo = new GameObject("SetChapterBtn");
            setChapterGo.transform.SetParent(chapterRowGo.transform, false);
            var setChapterLe = setChapterGo.AddComponent<LayoutElement>();
            setChapterLe.flexibleWidth = 1;
            var setChapterBg = setChapterGo.AddComponent<Image>();
            setChapterBg.color = ColorPalette.ButtonPrimary;
            var setChapterBtn = setChapterGo.AddComponent<Button>();
            setChapterBtn.targetGraphic = setChapterBg;
            setChapterBtn.onClick.AddListener(OnSetChapter);
            var setChapterTextGo = new GameObject("Text");
            setChapterTextGo.transform.SetParent(setChapterGo.transform, false);
            var setChapterText = setChapterTextGo.AddComponent<TextMeshProUGUI>();
            setChapterText.text = "\ucc55\ud130 \uc124\uc815";
            setChapterText.fontSize = 24;
            setChapterText.color = Color.white;
            setChapterText.alignment = TextAlignmentOptions.Center;
            setChapterText.raycastTarget = false;
            UIManager.StretchFull(setChapterTextGo.GetComponent<RectTransform>());

            CreateSectionHeader(contentGo.transform, "\uae30\ud0c0");

            CreateDebugButton(contentGo.transform, "\ub79c\ub364 \uc7a5\ube44 \uc0dd\uc131", () =>
            {
                var pullResult = Game.EquipmentChestSystem.Pull(Game.Rng);
                if (pullResult.Equipment != null)
                {
                    Game.Player.AddToInventory(pullResult.Equipment);
                    SetStatus($"\uc7a5\ube44 \uc0dd\uc131: {pullResult.Equipment.Name} ({pullResult.Equipment.Grade})");
                }
            });

            CreateDebugButton(contentGo.transform, "\ubaa8\ub4e0 \ud034\uc2a4\ud2b8 \uc644\ub8cc", () =>
            {
                var events = Game.EventManagerSystem.GetActiveEvents();
                foreach (var evt in events)
                {
                    foreach (var mission in evt.Missions)
                    {
                        mission.Current = mission.Target;
                    }
                }
                SetStatus("\ubaa8\ub4e0 \ud034\uc2a4\ud2b8 \uc644\ub8cc \ucc98\ub9ac\ub428");
            });

            CreateDebugButton(contentGo.transform, "\ucd9c\uc11d +1\uc77c", () =>
            {
                var result = Game.ClaimAttendance();
                if (result != null)
                    SetStatus($"\ucd9c\uc11d Day {result.Day} \uc644\ub8cc");
                else
                    SetStatus("\ucd9c\uc11d \ubd88\uac00 (\uc774\ubbf8 \uccb4\ud06c\uc778 \ub610\ub294 \uc644\ub8cc)");
            });

            CreateDebugButton(contentGo.transform, "\uc800\uc7a5 \uc0ad\uc81c (\ub9ac\uc14b)", () =>
            {
                Game.ResetToNewGame();
                SetStatus("\ub9ac\uc14b \uc644\ub8cc");
            });

            BuildLogConsole(contentGo.transform);
        }

        private void BuildLogConsole(Transform parent)
        {
            CreateSectionHeader(parent, "\ub85c\uadf8 \ucf58\uc194");

            var filterRowGo = new GameObject("LogFilterRow");
            filterRowGo.transform.SetParent(parent, false);
            var filterRowLe = filterRowGo.AddComponent<LayoutElement>();
            filterRowLe.preferredHeight = 36;
            var filterRowLayout = filterRowGo.AddComponent<HorizontalLayoutGroup>();
            filterRowLayout.spacing = 6;
            filterRowLayout.childForceExpandWidth = true;
            filterRowLayout.childForceExpandHeight = true;

            CreateFilterButton(filterRowGo.transform, "ALL", LogLevel.Debug);
            CreateFilterButton(filterRowGo.transform, "INFO", LogLevel.Info);
            CreateFilterButton(filterRowGo.transform, "WARN", LogLevel.Warn);
            CreateFilterButton(filterRowGo.transform, "ERR", LogLevel.Error);

            var clearBtnGo = new GameObject("ClearLogBtn");
            clearBtnGo.transform.SetParent(filterRowGo.transform, false);
            var clearBtnBg = clearBtnGo.AddComponent<Image>();
            clearBtnBg.color = ColorPalette.Hp;
            var clearBtn = clearBtnGo.AddComponent<Button>();
            clearBtn.targetGraphic = clearBtnBg;
            clearBtn.onClick.AddListener(() =>
            {
                GameLog.Clear();
                RefreshLogView();
            });
            var clearTextGo = new GameObject("Text");
            clearTextGo.transform.SetParent(clearBtnGo.transform, false);
            var clearText = clearTextGo.AddComponent<TextMeshProUGUI>();
            clearText.text = "CLR";
            clearText.fontSize = 18;
            clearText.color = Color.white;
            clearText.alignment = TextAlignmentOptions.Center;
            clearText.raycastTarget = false;
            UIManager.StretchFull(clearTextGo.GetComponent<RectTransform>());

            var logContainerGo = new GameObject("LogContainer");
            logContainerGo.transform.SetParent(parent, false);
            var logContainerLe = logContainerGo.AddComponent<LayoutElement>();
            logContainerLe.preferredHeight = 400;
            logContainerGo.AddComponent<Image>().color = new Color(0.05f, 0.08f, 0.12f, 1f);

            var logScrollGo = new GameObject("LogScroll");
            logScrollGo.transform.SetParent(logContainerGo.transform, false);
            var logScrollRt = logScrollGo.GetComponent<RectTransform>();
            if (logScrollRt == null) logScrollRt = logScrollGo.AddComponent<RectTransform>();
            UIManager.StretchFull(logScrollRt);
            _logScrollRect = logScrollGo.AddComponent<ScrollRect>();
            _logScrollRect.horizontal = false;
            _logScrollRect.vertical = true;

            var logViewportGo = new GameObject("Viewport");
            logViewportGo.transform.SetParent(logScrollGo.transform, false);
            var logViewportRt = logViewportGo.GetComponent<RectTransform>();
            if (logViewportRt == null) logViewportRt = logViewportGo.AddComponent<RectTransform>();
            UIManager.StretchFull(logViewportRt);
            logViewportGo.AddComponent<RectMask2D>();

            var logContentGo = new GameObject("LogContent");
            logContentGo.transform.SetParent(logViewportGo.transform, false);
            var logContentRt = logContentGo.GetComponent<RectTransform>();
            if (logContentRt == null) logContentRt = logContentGo.AddComponent<RectTransform>();
            logContentRt.anchorMin = new Vector2(0, 1);
            logContentRt.anchorMax = new Vector2(1, 1);
            logContentRt.pivot = new Vector2(0.5f, 1);
            logContentRt.offsetMin = Vector2.zero;
            logContentRt.offsetMax = Vector2.zero;

            var logContentFitter = logContentGo.AddComponent<ContentSizeFitter>();
            logContentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _logScrollRect.content = logContentRt;
            _logScrollRect.viewport = logViewportRt;

            _logText = logContentGo.AddComponent<TextMeshProUGUI>();
            _logText.fontSize = 16;
            _logText.color = ColorPalette.Text;
            _logText.alignment = TextAlignmentOptions.TopLeft;
            _logText.raycastTarget = false;
            _logText.textWrappingMode = TextWrappingModes.Normal;
            _logText.overflowMode = TextOverflowModes.Truncate;
            _logText.margin = new Vector4(8, 4, 8, 4);
        }

        private void CreateFilterButton(Transform parent, string label, LogLevel level)
        {
            var go = new GameObject("Filter_" + label);
            go.transform.SetParent(parent, false);
            var bg = go.AddComponent<Image>();
            bg.color = _logFilterLevel == level ? ColorPalette.ButtonPrimary : ColorPalette.ButtonSecondary;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() =>
            {
                _logFilterLevel = level;
                RefreshLogView();
            });
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 18;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            UIManager.StretchFull(textGo.GetComponent<RectTransform>());
        }

        private void RefreshLogView()
        {
            if (_logText == null) return;
            var sb = new StringBuilder();
            foreach (var entry in GameLog.Entries)
            {
                if (entry.Level < _logFilterLevel) continue;
                sb.AppendLine(FormatEntry(entry));
            }
            _logText.text = sb.ToString();
        }

        private void AppendLogLine(LogEntry entry)
        {
            if (_logText == null) return;
            _logText.text += FormatEntry(entry) + "\n";
        }

        private static string FormatEntry(LogEntry entry)
        {
            string levelColor = entry.Level switch
            {
                LogLevel.Debug => "#a0a0b0",
                LogLevel.Info => "#f0f0f0",
                LogLevel.Warn => "#ffd700",
                LogLevel.Error => "#e74c3c",
                _ => "#f0f0f0",
            };
            int seconds = (int)entry.Time;
            int m = seconds / 60;
            int s = seconds % 60;
            return $"<color={levelColor}>{m:D2}:{s:D2} [{entry.Tag}] {entry.Message}</color>";
        }

        private void CreateSectionHeader(Transform parent, string text)
        {
            var go = new GameObject("Section_" + text);
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 30;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.color = ColorPalette.Gold;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.raycastTarget = false;
        }

        private void CreateDebugButton(Transform parent, string label, Action onClick)
        {
            var go = new GameObject("DebugBtn_" + label);
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 44;
            var bg = go.AddComponent<Image>();
            bg.color = ColorPalette.ButtonSecondary;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() =>
            {
                onClick();
                Game.SaveGame();
                UI.Refresh();
            });
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

        private void OnSetChapter()
        {
            if (_chapterInput == null || string.IsNullOrEmpty(_chapterInput.text)) return;
            if (int.TryParse(_chapterInput.text, out int chapterId))
            {
                Game.Player.ClearedChapterMax = chapterId;
                SetStatus($"\ucd5c\uace0 \ucc55\ud130: {chapterId}");
                Game.SaveGame();
                UI.Refresh();
            }
        }

        private void SetStatus(string text)
        {
            _statusText.text = text;
        }

        public override void Refresh()
        {
            RefreshLogView();
        }
    }
}
