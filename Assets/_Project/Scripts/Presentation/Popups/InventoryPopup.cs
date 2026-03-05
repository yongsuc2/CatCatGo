using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Domain.Enums;
using CatCatGo.Services;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Popups
{
    public class InventoryPopup : BasePopup
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
            BuildScrollableContent(transform);
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
            titleText.text = "\uAC00\uBC29";
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

        private void BuildScrollableContent(Transform parent)
        {
            var scrollGo = new GameObject("ScrollView");
            scrollGo.transform.SetParent(parent, false);
            var scrollLe = scrollGo.AddComponent<LayoutElement>();
            scrollLe.flexibleHeight = 1;
            var scrollRect = scrollGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

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
            contentLayout.spacing = 6;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.padding = new RectOffset(4, 4, 4, 4);

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRt;
            scrollRect.viewport = viewportRt;

            var player = GameManager.Instance?.Player;
            if (player == null) return;

            var res = player.Resources;
            CreateResourceRow(contentGo.transform, "\uACE8\uB4DC", NumberFormatter.Format(res.Gold), ColorPalette.Gold);
            CreateResourceRow(contentGo.transform, "\uBCF4\uC11D", NumberFormatter.Format(res.Gems), ColorPalette.Gems);
            CreateResourceRow(contentGo.transform, "\uC2A4\uD0DC\uBBF8\uB098", $"{(int)res.Stamina}/{res.GetStaminaMax()}", ColorPalette.Stamina);
            CreateResourceRow(contentGo.transform, "\uB3C4\uC804 \uD1A0\uD070", NumberFormatter.Format(res.ChallengeTokens), ColorPalette.Text);
            CreateResourceRow(contentGo.transform, "\uACE1\uAD2D\uC774", NumberFormatter.Format(res.Pickaxes), ColorPalette.Text);
            CreateResourceRow(contentGo.transform, "\uC7A5\uBE44\uC11D", NumberFormatter.Format(res.EquipmentStones), ColorPalette.Text);
            CreateResourceRow(contentGo.transform, "\uD30C\uC6CC \uC2A4\uD1A4", NumberFormatter.Format(res.PowerStones), ColorPalette.Text);
            CreateResourceRow(contentGo.transform, "\uD3AB \uC54C", NumberFormatter.Format(res.Get(ResourceType.PET_EGG)), ColorPalette.Text);
            CreateResourceRow(contentGo.transform, "\uD3AB \uC0AC\uB8CC", NumberFormatter.Format(res.Get(ResourceType.PET_FOOD)), ColorPalette.Text);
            CreateResourceRow(contentGo.transform, "\uC2A4\uCEEC \uC11C\uC801", NumberFormatter.Format(res.Get(ResourceType.SKULL_BOOK)), ColorPalette.Text);
            CreateResourceRow(contentGo.transform, "\uB098\uC774\uD2B8 \uC11C\uC801", NumberFormatter.Format(res.Get(ResourceType.KNIGHT_BOOK)), ColorPalette.Text);
            CreateResourceRow(contentGo.transform, "\uB808\uC778\uC800 \uC11C\uC801", NumberFormatter.Format(res.Get(ResourceType.RANGER_BOOK)), ColorPalette.Text);
            CreateResourceRow(contentGo.transform, "\uACE0\uC2A4\uD2B8 \uC11C\uC801", NumberFormatter.Format(res.Get(ResourceType.GHOST_BOOK)), ColorPalette.Text);
        }

        private void CreateResourceRow(Transform parent, string label, string value, Color valueColor)
        {
            var rowGo = new GameObject("Row_" + label);
            rowGo.transform.SetParent(parent, false);
            var rowBg = rowGo.AddComponent<Image>();
            rowBg.color = ColorPalette.CardLight;

            var rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;
            rowLayout.padding = new RectOffset(12, 12, 0, 0);
            var rowLe = rowGo.AddComponent<LayoutElement>();
            rowLe.preferredHeight = 40;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(rowGo.transform, false);
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.fontSize = 26f;
            labelTmp.color = ColorPalette.TextDim;
            labelTmp.alignment = TextAlignmentOptions.Left;
            labelTmp.raycastTarget = false;

            var valueGo = new GameObject("Value");
            valueGo.transform.SetParent(rowGo.transform, false);
            var valueTmp = valueGo.AddComponent<TextMeshProUGUI>();
            valueTmp.text = value;
            valueTmp.fontSize = 26f;
            valueTmp.color = valueColor;
            valueTmp.alignment = TextAlignmentOptions.Right;
            valueTmp.raycastTarget = false;
        }
    }
}
