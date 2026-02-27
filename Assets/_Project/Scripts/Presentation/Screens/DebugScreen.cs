using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Enums;
using CatCatGo.Domain.Entities;
using CatCatGo.Domain.Data;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Screens
{
    public class DebugScreen : BaseScreen
    {
        private TMP_InputField _chapterInput;
        private TextMeshProUGUI _statusText;

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
                Game.Player.Resources.Add(ResourceType.ARENA_TICKET, 10);
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
                Game.DeleteSave();
                SetStatus("\uc800\uc7a5 \uc0ad\uc81c\ub428. \uc7ac\uc2dc\uc791 \ud544\uc694");
            });
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
        }
    }
}
