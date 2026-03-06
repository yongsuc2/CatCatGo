using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Popups
{
    public class SettingsPopup : BasePopup
    {
        private const string BGM_KEY = "Settings_BGM";
        private const string SFX_KEY = "Settings_SFX";

        private TextMeshProUGUI _bgmStatusText;
        private TextMeshProUGUI _sfxStatusText;

        public override void Show(object data = null)
        {
            base.Show(data);
            BuildUI();
        }

        private void BuildUI()
        {
            var bg = gameObject.AddComponent<Image>();
            bg.color = ColorPalette.Card;

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12;
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;

            BuildHeader(transform);

            _bgmStatusText = CreateToggleRow(transform, "\uBC30\uACBD\uC74C", IsBgmOn(), OnBgmToggle);
            _sfxStatusText = CreateToggleRow(transform, "\uD6A8\uACFC\uC74C", IsSfxOn(), OnSfxToggle);

            var debugBtnGo = new GameObject("DebugBtn");
            debugBtnGo.transform.SetParent(transform, false);
            var debugBtnLe = debugBtnGo.AddComponent<LayoutElement>();
            debugBtnLe.preferredHeight = 48;
            var debugBtnBg = debugBtnGo.AddComponent<Image>();
            debugBtnBg.color = ColorPalette.ButtonSecondary;
            var debugBtn = debugBtnGo.AddComponent<Button>();
            debugBtn.targetGraphic = debugBtnBg;
            debugBtn.onClick.AddListener(() =>
            {
                UIManager.Instance.ClosePopup();
                UIManager.Instance.ShowScreen(ScreenType.Debug);
            });
            var debugTextGo = new GameObject("Text");
            debugTextGo.transform.SetParent(debugBtnGo.transform, false);
            var debugText = debugTextGo.AddComponent<TextMeshProUGUI>();
            debugText.text = "\uB514\uBC84\uADF8 \uD654\uBA74";
            debugText.fontSize = 26;
            debugText.color = Color.white;
            debugText.alignment = TextAlignmentOptions.Center;
            debugText.raycastTarget = false;
            UIManager.StretchFull(debugTextGo.GetComponent<RectTransform>());
        }

        private void BuildHeader(Transform parent)
        {
            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(parent, false);
            var headerLayout = headerGo.AddComponent<HorizontalLayoutGroup>();
            headerLayout.childForceExpandWidth = true;
            headerLayout.childForceExpandHeight = true;
            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 40;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(headerGo.transform, false);
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "\uC124\uC815";
            titleText.fontSize = 32f;
            titleText.color = ColorPalette.Text;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.raycastTarget = false;

            var closeBtnGo = new GameObject("CloseButton");
            closeBtnGo.transform.SetParent(headerGo.transform, false);
            var closeBtnLe = closeBtnGo.AddComponent<LayoutElement>();
            closeBtnLe.preferredWidth = 40;
            var closeBtnImage = closeBtnGo.AddComponent<Image>();
            closeBtnImage.color = ColorPalette.ButtonSecondary;
            var closeBtn = closeBtnGo.AddComponent<Button>();
            closeBtn.targetGraphic = closeBtnImage;
            closeBtn.onClick.AddListener(Hide);
            var closeBtnText = new GameObject("X");
            closeBtnText.transform.SetParent(closeBtnGo.transform, false);
            var cbt = closeBtnText.AddComponent<TextMeshProUGUI>();
            cbt.text = "X";
            cbt.fontSize = 28f;
            cbt.color = ColorPalette.Text;
            cbt.alignment = TextAlignmentOptions.Center;
            cbt.raycastTarget = false;
            UIManager.StretchFull(cbt.rectTransform);
        }

        private TextMeshProUGUI CreateToggleRow(Transform parent, string label, bool isOn, UnityEngine.Events.UnityAction onClick)
        {
            var rowGo = new GameObject("Toggle_" + label);
            rowGo.transform.SetParent(parent, false);
            var rowBg = rowGo.AddComponent<Image>();
            rowBg.color = ColorPalette.CardLight;

            var rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;
            rowLayout.padding = new RectOffset(16, 16, 0, 0);
            var rowLe = rowGo.AddComponent<LayoutElement>();
            rowLe.preferredHeight = 56;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(rowGo.transform, false);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.fontSize = 28f;
            labelTmp.color = ColorPalette.Text;
            labelTmp.alignment = TextAlignmentOptions.Left;
            labelTmp.raycastTarget = false;

            var btnGo = new GameObject("ToggleBtn");
            btnGo.transform.SetParent(rowGo.transform, false);
            var btnLe = btnGo.AddComponent<LayoutElement>();
            btnLe.preferredWidth = 80;
            var btnImage = btnGo.AddComponent<Image>();
            btnImage.color = isOn ? ColorPalette.ButtonPrimary : ColorPalette.ButtonSecondary;
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnImage;
            btn.onClick.AddListener(onClick);

            var statusGo = new GameObject("Status");
            statusGo.transform.SetParent(btnGo.transform, false);
            var statusTmp = statusGo.AddComponent<TextMeshProUGUI>();
            statusTmp.text = isOn ? "ON" : "OFF";
            statusTmp.fontSize = 24f;
            statusTmp.color = Color.white;
            statusTmp.alignment = TextAlignmentOptions.Center;
            statusTmp.raycastTarget = false;
            UIManager.StretchFull(statusTmp.rectTransform);

            return statusTmp;
        }

        private void OnBgmToggle()
        {
            bool current = IsBgmOn();
            bool next = !current;
            PlayerPrefs.SetInt(BGM_KEY, next ? 1 : 0);
            PlayerPrefs.Save();
            UpdateToggleVisual(_bgmStatusText, next);
        }

        private void OnSfxToggle()
        {
            bool current = IsSfxOn();
            bool next = !current;
            PlayerPrefs.SetInt(SFX_KEY, next ? 1 : 0);
            PlayerPrefs.Save();
            UpdateToggleVisual(_sfxStatusText, next);
        }

        private void UpdateToggleVisual(TextMeshProUGUI statusText, bool isOn)
        {
            statusText.text = isOn ? "ON" : "OFF";
            var btnImage = statusText.transform.parent.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = isOn ? ColorPalette.ButtonPrimary : ColorPalette.ButtonSecondary;
        }

        private bool IsBgmOn()
        {
            return PlayerPrefs.GetInt(BGM_KEY, 1) == 1;
        }

        private bool IsSfxOn()
        {
            return PlayerPrefs.GetInt(SFX_KEY, 1) == 1;
        }
    }
}
