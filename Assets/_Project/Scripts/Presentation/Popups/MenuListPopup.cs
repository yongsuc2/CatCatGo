using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Popups
{
    public class MenuListPopup : BasePopup
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

            BuildHeader(transform);

            CreateMenuItem(transform, "\uAC00\uBC29", OnBagClicked);
            CreateMenuItem(transform, "\uC6B0\uD3B8", OnMailClicked);
            CreateMenuItem(transform, "\uD018\uC2A4\uD2B8", OnQuestClicked);
            CreateMenuItem(transform, "\uCD9C\uC11D\uCCB4\uD06C", OnAttendanceClicked);
            CreateMenuItem(transform, "\uC124\uC815", OnSettingsClicked);
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
            titleText.text = "\uBA54\uB274";
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

        private void CreateMenuItem(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Menu_" + label);
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 56;

            var bgImage = go.AddComponent<Image>();
            bgImage.color = ColorPalette.CardLight;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bgImage;
            btn.onClick.AddListener(onClick);

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 28f;
            tmp.color = ColorPalette.Text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            UIManager.StretchFull(tmp.rectTransform);
        }

        private void OnBagClicked()
        {
            Hide();
            UIManager.Instance.ShowPopupFromType<InventoryPopup>();
        }

        private void OnMailClicked()
        {
            Hide();
            UIManager.Instance.ShowPopupFromType<MailPopup>();
        }

        private void OnQuestClicked()
        {
            Hide();
            UIManager.Instance.ShowScreen(ScreenType.Quest);
        }

        private void OnAttendanceClicked()
        {
            Hide();
            UIManager.Instance.ShowScreen(ScreenType.Event);
        }

        private void OnSettingsClicked()
        {
            Hide();
            UIManager.Instance.ShowPopupFromType<SettingsPopup>();
        }
    }
}
