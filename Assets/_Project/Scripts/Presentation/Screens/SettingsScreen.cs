using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Network;
using CatCatGo.Services;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Screens
{
    public class SettingsScreen : BaseScreen
    {
        private TextMeshProUGUI _accountIdText;
        private TextMeshProUGUI _deviceIdText;
        private TextMeshProUGUI _connectionStatusText;
        private TextMeshProUGUI _statusMessageText;

        private Button _deleteAccountButton;
        private Button _debugButton;

        private RectTransform _confirmPanel;
        private TextMeshProUGUI _confirmMessage;
        private Action _confirmAction;

        private bool _isRequestPending;

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
            contentLayout.spacing = 10;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.padding = new RectOffset(16, 16, 16, 16);

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;
            scrollRect.viewport = viewportRt;

            // Header
            CreateLabel(contentGo.transform, "Header", "\uc124\uc815", 32, ColorPalette.Text, TextAlignmentOptions.Center, 36);

            CreateSeparator(contentGo.transform);

            // Account Info Section
            CreateLabel(contentGo.transform, "AccountSection", "\uacc4\uc815 \uc815\ubcf4", 26, ColorPalette.Gold, TextAlignmentOptions.Left, 30);

            _accountIdText = CreateLabel(contentGo.transform, "AccountId", "", 20, ColorPalette.TextDim, TextAlignmentOptions.Left, 24);
            _deviceIdText = CreateLabel(contentGo.transform, "DeviceId", "", 20, ColorPalette.TextDim, TextAlignmentOptions.Left, 24);
            _connectionStatusText = CreateLabel(contentGo.transform, "ConnectionStatus", "", 20, ColorPalette.TextDim, TextAlignmentOptions.Left, 24);

            CreateSeparator(contentGo.transform);

            // Status message
            _statusMessageText = CreateLabel(contentGo.transform, "StatusMessage", "", 20, ColorPalette.Heal, TextAlignmentOptions.Center, 24);
            _statusMessageText.gameObject.SetActive(false);

            // Delete Account Button
            _deleteAccountButton = CreateSettingsButton(contentGo.transform, "\uacc4\uc815 \ud0c8\ud1f4", ColorPalette.Hp,
                () => ShowConfirm("\uc815\ub9d0 \uacc4\uc815\uc744 \ud0c8\ud1f4\ud558\uc2dc\uaca0\uc2b5\ub2c8\uae4c?\n\ubaa8\ub4e0 \ub370\uc774\ud130\uac00 \uc0ad\uc81c\ub418\uace0 \uc0c8 \uacc4\uc815\uc73c\ub85c \uc2dc\uc791\ud569\ub2c8\ub2e4.", OnDeleteAccount));

            CreateSeparator(contentGo.transform);

            // Debug Button
            _debugButton = CreateSettingsButton(contentGo.transform, "\ub514\ubc84\uadf8 \ud654\uba74", ColorPalette.ButtonSecondary,
                () => UI.ShowScreen(ScreenType.Debug));

            // Confirm Panel
            BuildConfirmPanel(contentGo.transform);
        }

        private void BuildConfirmPanel(Transform parent)
        {
            var confirmPanelGo = new GameObject("ConfirmPanel");
            confirmPanelGo.transform.SetParent(parent, false);
            _confirmPanel = confirmPanelGo.GetComponent<RectTransform>();
            if (_confirmPanel == null) _confirmPanel = confirmPanelGo.AddComponent<RectTransform>();
            var confirmLe = confirmPanelGo.AddComponent<LayoutElement>();
            confirmLe.preferredHeight = 100;
            confirmPanelGo.AddComponent<Image>().color = new Color(0.3f, 0.1f, 0.1f, 1f);

            var confirmLayout = confirmPanelGo.AddComponent<VerticalLayoutGroup>();
            confirmLayout.spacing = 4;
            confirmLayout.childForceExpandWidth = true;
            confirmLayout.childForceExpandHeight = false;
            confirmLayout.padding = new RectOffset(12, 12, 8, 8);

            var confirmMsgGo = new GameObject("Message");
            confirmMsgGo.transform.SetParent(confirmPanelGo.transform, false);
            var confirmMsgLe = confirmMsgGo.AddComponent<LayoutElement>();
            confirmMsgLe.preferredHeight = 48;
            _confirmMessage = confirmMsgGo.AddComponent<TextMeshProUGUI>();
            _confirmMessage.fontSize = 20;
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

        private TextMeshProUGUI CreateLabel(Transform parent, string name, string text, int fontSize, Color color, TextAlignmentOptions alignment, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            return tmp;
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

        private void ShowStatus(string message)
        {
            _statusMessageText.text = message;
            _statusMessageText.gameObject.SetActive(true);
        }

        private void OnDeleteAccount()
        {
            if (_isRequestPending) return;
            _isRequestPending = true;
            _deleteAccountButton.interactable = false;
            ShowStatus("\uacc4\uc815 \ud0c8\ud1f4 \ucc98\ub9ac\uc911...");

            Game.DeleteAccountAsync(success =>
            {
                if (success)
                {
                    ShowStatus("\uacc4\uc815 \uc0ad\uc81c \uc644\ub8cc. \uc0c8 \uacc4\uc815 \ub4f1\ub85d\uc911...");
                    AuthApi.AutoLogin((loginSuccess, isNew) =>
                    {
                        _isRequestPending = false;
                        _deleteAccountButton.interactable = true;

                        if (loginSuccess)
                        {
                            if (ServerSyncService.Instance != null)
                                ServerSyncService.Instance.RetryConnection();
                            ShowStatus("\uc0c8 \uacc4\uc815\uc73c\ub85c \uc2dc\uc791\ud569\ub2c8\ub2e4!");
                        }
                        else
                        {
                            ShowStatus("\uc0c8 \uacc4\uc815 \ub4f1\ub85d \uc2e4\ud328. \uc571\uc744 \uc7ac\uc2dc\uc791\ud574\uc8fc\uc138\uc694.");
                        }
                        UI.Refresh();
                    });
                }
                else
                {
                    _isRequestPending = false;
                    _deleteAccountButton.interactable = true;
                    ShowStatus("\uacc4\uc815 \ud0c8\ud1f4 \uc2e4\ud328. \ub124\ud2b8\uc6cc\ud06c\ub97c \ud655\uc778\ud574\uc8fc\uc138\uc694.");
                }
            });
        }

        public override void Refresh()
        {
            if (Game == null) return;

            // Account ID
            string accountId = "";
            if (ApiClient.Instance != null)
                accountId = ApiClient.Instance.TokenStore.AccountId ?? "";
            _accountIdText.text = string.IsNullOrEmpty(accountId)
                ? "\uacc4\uc815 ID: \ubbf8\ub85c\uadf8\uc778"
                : $"\uacc4\uc815 ID: {accountId}";

            // Device ID
            string deviceId = AuthApi.GetDeviceId();
            if (deviceId.Length > 16)
                deviceId = deviceId.Substring(0, 16) + "...";
            _deviceIdText.text = $"\ub514\ubc14\uc774\uc2a4 ID: {deviceId}";

            // Connection Status
            var syncState = ServerSyncService.Instance != null
                ? ServerSyncService.Instance.State
                : ConnectionState.Offline;
            string stateText;
            Color stateColor;
            switch (syncState)
            {
                case ConnectionState.Online:
                    stateText = "\uc628\ub77c\uc778";
                    stateColor = ColorPalette.Heal;
                    break;
                case ConnectionState.Connecting:
                    stateText = "\uc5f0\uacb0\uc911...";
                    stateColor = ColorPalette.Gold;
                    break;
                default:
                    stateText = "\uc624\ud504\ub77c\uc778";
                    stateColor = ColorPalette.Hp;
                    break;
            }
            _connectionStatusText.text = $"\uc11c\ubc84 \uc0c1\ud0dc: {stateText}";
            _connectionStatusText.color = stateColor;
        }
    }
}
