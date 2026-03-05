using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Popups
{
    public class MailPopup : BasePopup
    {
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
            layout.spacing = 8;
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;

            var headerGo = new GameObject("Header");
            headerGo.transform.SetParent(transform, false);
            var headerLayout = headerGo.AddComponent<HorizontalLayoutGroup>();
            headerLayout.childForceExpandWidth = true;
            headerLayout.childForceExpandHeight = true;
            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.preferredHeight = 40;

            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(headerGo.transform, false);
            var titleText = titleGo.AddComponent<TextMeshProUGUI>();
            titleText.text = "\uC6B0\uD3B8";
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

            var emptyGo = new GameObject("EmptyMessage");
            emptyGo.transform.SetParent(transform, false);
            var emptyLe = emptyGo.AddComponent<LayoutElement>();
            emptyLe.preferredHeight = 200;
            var emptyText = emptyGo.AddComponent<TextMeshProUGUI>();
            emptyText.text = "\uC6B0\uD3B8\uD568\uC774 \uBE44\uC5B4 \uC788\uC2B5\uB2C8\uB2E4.";
            emptyText.fontSize = 28f;
            emptyText.color = ColorPalette.TextDim;
            emptyText.alignment = TextAlignmentOptions.Center;
            emptyText.raycastTarget = false;
        }
    }
}
