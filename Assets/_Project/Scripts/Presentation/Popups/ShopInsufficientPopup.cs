using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Popups
{
    public class ShopInsufficientPopupData
    {
        public int RequiredGems;
        public int CurrentGems;
    }

    public class ShopInsufficientPopup : BasePopup
    {
        public override void Show(object data = null)
        {
            base.Show(data);
            BuildUI();
        }

        private void BuildUI()
        {
            var popupData = PopupData as ShopInsufficientPopupData;
            if (popupData == null) return;

            var bg = gameObject.AddComponent<Image>();
            bg.color = ColorPalette.Card;

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 16;
            layout.padding = new RectOffset(30, 30, 30, 30);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.MiddleCenter;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(transform, false);
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.preferredHeight = 40;
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "\uBCF4\uC11D\uC774 \uBD80\uC871\uD569\uB2C8\uB2E4";
            titleText.fontSize = 30;
            titleText.color = ColorPalette.Hp;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.raycastTarget = false;

            var infoGo = new GameObject("Info");
            infoGo.transform.SetParent(transform, false);
            var infoLe = infoGo.AddComponent<LayoutElement>();
            infoLe.preferredHeight = 60;
            var infoText = infoGo.AddComponent<TextMeshProUGUI>();
            infoText.text = $"\uD544\uC694: {popupData.RequiredGems}  /  \uBCF4\uC720: {popupData.CurrentGems}";
            infoText.fontSize = 24;
            infoText.color = ColorPalette.Gems;
            infoText.alignment = TextAlignmentOptions.Center;
            infoText.raycastTarget = false;

            var closeBtnGo = new GameObject("CloseBtn");
            closeBtnGo.transform.SetParent(transform, false);
            var closeBtnLe = closeBtnGo.AddComponent<LayoutElement>();
            closeBtnLe.preferredHeight = 48;
            var closeBtnBg = closeBtnGo.AddComponent<Image>();
            closeBtnBg.color = ColorPalette.ButtonSecondary;
            var closeBtn = closeBtnGo.AddComponent<Button>();
            closeBtn.targetGraphic = closeBtnBg;
            closeBtn.onClick.AddListener(Hide);
            var closeTextGo = new GameObject("Text");
            closeTextGo.transform.SetParent(closeBtnGo.transform, false);
            var closeText = closeTextGo.AddComponent<TextMeshProUGUI>();
            closeText.text = "\uB2EB\uAE30";
            closeText.fontSize = 26;
            closeText.color = Color.white;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.raycastTarget = false;
            UIManager.StretchFull(closeTextGo.GetComponent<RectTransform>());
        }
    }
}
