using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Screens
{
    public class SettingsScreen : BaseScreen
    {
        private TextMeshProUGUI _lastSaveText;
        private Button _saveButton;
        private Button _loadButton;
        private Button _deleteButton;
        private Button _exportButton;
        private Button _importButton;
        private Button _debugButton;

        private RectTransform _exportPanel;
        private TextMeshProUGUI _exportText;
        private TMP_InputField _importInput;

        private RectTransform _confirmPanel;
        private TextMeshProUGUI _confirmMessage;
        private Action _confirmAction;

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
            contentLayout.spacing = 10;
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
            headerLe.preferredHeight = 36;
            var headerText = headerGo.AddComponent<TextMeshProUGUI>();
            headerText.text = "\uc124\uc815";
            headerText.fontSize = 32;
            headerText.color = ColorPalette.Text;
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.raycastTarget = false;

            var saveTimeGo = new GameObject("SaveTime");
            saveTimeGo.transform.SetParent(contentGo.transform, false);
            var saveTimeLe = saveTimeGo.AddComponent<LayoutElement>();
            saveTimeLe.preferredHeight = 28;
            _lastSaveText = saveTimeGo.AddComponent<TextMeshProUGUI>();
            _lastSaveText.fontSize = 22;
            _lastSaveText.color = ColorPalette.TextDim;
            _lastSaveText.alignment = TextAlignmentOptions.Center;
            _lastSaveText.raycastTarget = false;

            _saveButton = CreateSettingsButton(contentGo.transform, "\uc800\uc7a5", ColorPalette.ButtonPrimary, OnSave);
            _loadButton = CreateSettingsButton(contentGo.transform, "\ubd88\ub7ec\uc624\uae30", ColorPalette.ButtonPrimary, OnLoad);
            _deleteButton = CreateSettingsButton(contentGo.transform, "\uc800\uc7a5 \uc0ad\uc81c", ColorPalette.Hp, () => ShowConfirm("\uc815\ub9d0 \uc800\uc7a5\uc744 \uc0ad\uc81c\ud558\uc2dc\uaca0\uc2b5\ub2c8\uae4c?", OnDelete));

            CreateSeparator(contentGo.transform);

            _exportButton = CreateSettingsButton(contentGo.transform, "\ub0b4\ubcf4\ub0b4\uae30 (Base64)", ColorPalette.ButtonSecondary, OnExport);

            var exportPanelGo = new GameObject("ExportPanel");
            exportPanelGo.transform.SetParent(contentGo.transform, false);
            _exportPanel = exportPanelGo.AddComponent<RectTransform>();
            var exportPanelLe = exportPanelGo.AddComponent<LayoutElement>();
            exportPanelLe.preferredHeight = 100;
            exportPanelGo.AddComponent<Image>().color = ColorPalette.CardLight;

            var exportLayout = exportPanelGo.AddComponent<VerticalLayoutGroup>();
            exportLayout.spacing = 4;
            exportLayout.childForceExpandWidth = true;
            exportLayout.childForceExpandHeight = true;
            exportLayout.padding = new RectOffset(8, 8, 8, 8);

            var exportTextGo = new GameObject("ExportText");
            exportTextGo.transform.SetParent(exportPanelGo.transform, false);
            _exportText = exportTextGo.AddComponent<TextMeshProUGUI>();
            _exportText.fontSize = 16;
            _exportText.color = ColorPalette.Text;
            _exportText.alignment = TextAlignmentOptions.TopLeft;
            _exportText.raycastTarget = true;
            _exportText.enableWordWrapping = true;
            _exportText.overflowMode = TextOverflowModes.Ellipsis;
            _exportPanel.gameObject.SetActive(false);

            CreateSeparator(contentGo.transform);

            var importLabelGo = new GameObject("ImportLabel");
            importLabelGo.transform.SetParent(contentGo.transform, false);
            var importLabelLe = importLabelGo.AddComponent<LayoutElement>();
            importLabelLe.preferredHeight = 24;
            var importLabel = importLabelGo.AddComponent<TextMeshProUGUI>();
            importLabel.text = "\uac00\uc838\uc624\uae30 (Base64 \ubd99\uc5ec\ub123\uae30)";
            importLabel.fontSize = 22;
            importLabel.color = ColorPalette.TextDim;
            importLabel.alignment = TextAlignmentOptions.Left;
            importLabel.raycastTarget = false;

            var importFieldGo = new GameObject("ImportField");
            importFieldGo.transform.SetParent(contentGo.transform, false);
            var importFieldLe = importFieldGo.AddComponent<LayoutElement>();
            importFieldLe.preferredHeight = 80;
            importFieldGo.AddComponent<Image>().color = ColorPalette.CardLight;

            var inputTextArea = new GameObject("TextArea");
            inputTextArea.transform.SetParent(importFieldGo.transform, false);
            var inputTextAreaRt = inputTextArea.AddComponent<RectTransform>();
            UIManager.StretchFull(inputTextAreaRt);
            inputTextAreaRt.offsetMin = new Vector2(8, 4);
            inputTextAreaRt.offsetMax = new Vector2(-8, -4);

            var inputTextGo = new GameObject("Text");
            inputTextGo.transform.SetParent(inputTextArea.transform, false);
            var inputText = inputTextGo.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 16;
            inputText.color = ColorPalette.Text;
            inputText.alignment = TextAlignmentOptions.TopLeft;
            inputText.enableWordWrapping = true;
            var inputTextRt = inputTextGo.GetComponent<RectTransform>();
            UIManager.StretchFull(inputTextRt);

            var placeholderGo = new GameObject("Placeholder");
            placeholderGo.transform.SetParent(inputTextArea.transform, false);
            var placeholder = placeholderGo.AddComponent<TextMeshProUGUI>();
            placeholder.text = "Base64 \ubb38\uc790\uc5f4\uc744 \uc785\ub825\ud558\uc138\uc694...";
            placeholder.fontSize = 16;
            placeholder.color = ColorPalette.TextDim;
            placeholder.alignment = TextAlignmentOptions.TopLeft;
            placeholder.fontStyle = FontStyles.Italic;
            var placeholderRt = placeholderGo.GetComponent<RectTransform>();
            UIManager.StretchFull(placeholderRt);

            _importInput = importFieldGo.AddComponent<TMP_InputField>();
            _importInput.textViewport = inputTextAreaRt;
            _importInput.textComponent = inputText;
            _importInput.placeholder = placeholder;
            _importInput.lineType = TMP_InputField.LineType.MultiLineNewline;

            _importButton = CreateSettingsButton(contentGo.transform, "\uac00\uc838\uc624\uae30 \ud655\uc778", ColorPalette.ButtonPrimary, () => ShowConfirm("\uac00\uc838\uc628 \ub370\uc774\ud130\ub85c \ub36e\uc5b4\uc501\ub2c8\ub2e4. \uacc4\uc18d?", OnImport));

            CreateSeparator(contentGo.transform);

            _debugButton = CreateSettingsButton(contentGo.transform, "\ub514\ubc84\uadf8 \ud654\uba74", ColorPalette.ButtonSecondary, () => UI.ShowScreen(ScreenType.Debug));

            var confirmPanelGo = new GameObject("ConfirmPanel");
            confirmPanelGo.transform.SetParent(contentGo.transform, false);
            _confirmPanel = confirmPanelGo.AddComponent<RectTransform>();
            var confirmLe = confirmPanelGo.AddComponent<LayoutElement>();
            confirmLe.preferredHeight = 80;
            confirmPanelGo.AddComponent<Image>().color = new Color(0.3f, 0.1f, 0.1f, 1f);

            var confirmLayout = confirmPanelGo.AddComponent<VerticalLayoutGroup>();
            confirmLayout.spacing = 4;
            confirmLayout.childForceExpandWidth = true;
            confirmLayout.childForceExpandHeight = false;
            confirmLayout.padding = new RectOffset(12, 12, 8, 8);

            var confirmMsgGo = new GameObject("Message");
            confirmMsgGo.transform.SetParent(confirmPanelGo.transform, false);
            var confirmMsgLe = confirmMsgGo.AddComponent<LayoutElement>();
            confirmMsgLe.preferredHeight = 24;
            _confirmMessage = confirmMsgGo.AddComponent<TextMeshProUGUI>();
            _confirmMessage.fontSize = 22;
            _confirmMessage.color = ColorPalette.Hp;
            _confirmMessage.alignment = TextAlignmentOptions.Center;
            _confirmMessage.raycastTarget = false;

            var confirmBtnRow = new GameObject("BtnRow");
            confirmBtnRow.transform.SetParent(confirmPanelGo.transform, false);
            var confirmBtnRowLe = confirmBtnRow.AddComponent<LayoutElement>();
            confirmBtnRowLe.preferredHeight = 36;
            var confirmBtnRowLayout = confirmBtnRow.AddComponent<HorizontalLayoutGroup>();
            confirmBtnRowLayout.spacing = 8;
            confirmBtnRowLayout.childForceExpandWidth = true;
            confirmBtnRowLayout.childForceExpandHeight = true;

            var yesGo = new GameObject("YesBtn");
            yesGo.transform.SetParent(confirmBtnRow.transform, false);
            var yesBg = yesGo.AddComponent<Image>();
            yesBg.color = ColorPalette.Hp;
            var yesBtn = yesGo.AddComponent<Button>();
            yesBtn.targetGraphic = yesBg;
            yesBtn.onClick.AddListener(() => { _confirmAction?.Invoke(); _confirmPanel.gameObject.SetActive(false); });
            var yesTextGo = new GameObject("Text");
            yesTextGo.transform.SetParent(yesGo.transform, false);
            var yesText = yesTextGo.AddComponent<TextMeshProUGUI>();
            yesText.text = "\ud655\uc778";
            yesText.fontSize = 22;
            yesText.color = Color.white;
            yesText.alignment = TextAlignmentOptions.Center;
            yesText.raycastTarget = false;
            UIManager.StretchFull(yesTextGo.GetComponent<RectTransform>());

            var noGo = new GameObject("NoBtn");
            noGo.transform.SetParent(confirmBtnRow.transform, false);
            var noBg = noGo.AddComponent<Image>();
            noBg.color = ColorPalette.ButtonSecondary;
            var noBtn = noGo.AddComponent<Button>();
            noBtn.targetGraphic = noBg;
            noBtn.onClick.AddListener(() => _confirmPanel.gameObject.SetActive(false));
            var noTextGo = new GameObject("Text");
            noTextGo.transform.SetParent(noGo.transform, false);
            var noText = noTextGo.AddComponent<TextMeshProUGUI>();
            noText.text = "\ucde8\uc18c";
            noText.fontSize = 22;
            noText.color = Color.white;
            noText.alignment = TextAlignmentOptions.Center;
            noText.raycastTarget = false;
            UIManager.StretchFull(noTextGo.GetComponent<RectTransform>());

            _confirmPanel.gameObject.SetActive(false);
        }

        private Button CreateSettingsButton(Transform parent, string label, Color color, Action onClick)
        {
            var go = new GameObject("Btn_" + label);
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 48;

            var bg = go.AddComponent<Image>();
            bg.color = color;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => onClick());

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 26;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            UIManager.StretchFull(textGo.GetComponent<RectTransform>());

            return btn;
        }

        private void CreateSeparator(Transform parent)
        {
            var go = new GameObject("Separator");
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 2;
            go.AddComponent<Image>().color = ColorPalette.CardLight;
        }

        private void ShowConfirm(string message, Action onConfirm)
        {
            _confirmMessage.text = message;
            _confirmAction = onConfirm;
            _confirmPanel.gameObject.SetActive(true);
        }

        private void OnSave()
        {
            Game.SaveGame();
            Refresh();
        }

        private void OnLoad()
        {
            Game.LoadGame();
            UI.Refresh();
        }

        private void OnDelete()
        {
            Game.DeleteSave();
            Refresh();
        }

        private void OnExport()
        {
            string encoded = Game.ExportSave();
            _exportText.text = encoded;
            _exportPanel.gameObject.SetActive(true);
        }

        private void OnImport()
        {
            if (_importInput == null || string.IsNullOrEmpty(_importInput.text)) return;
            bool success = Game.ImportSave(_importInput.text);
            if (success)
            {
                _importInput.text = "";
                UI.Refresh();
            }
        }

        public override void Refresh()
        {
            if (Game == null) return;

            long? lastSave = Game.GetLastSaveTime();
            if (lastSave.HasValue && lastSave.Value > 0)
            {
                var dt = DateTimeOffset.FromUnixTimeMilliseconds(lastSave.Value).LocalDateTime;
                _lastSaveText.text = $"\ub9c8\uc9c0\ub9c9 \uc800\uc7a5: {dt:yyyy-MM-dd HH:mm:ss}";
            }
            else
            {
                _lastSaveText.text = "\uc800\uc7a5 \ub0b4\uc5ed \uc5c6\uc74c";
            }
        }
    }
}
