using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Components
{
    public class PlayerStatsBarView : MonoBehaviour
    {
        private RectTransform _root;
        private Slider _hpSlider;
        private Image _hpFill;
        private TextMeshProUGUI _hpText;
        private TextMeshProUGUI _atkText;
        private TextMeshProUGUI _defText;

        private void BuildUI()
        {
            _root = gameObject.GetComponent<RectTransform>();
            if (_root == null)
                _root = gameObject.GetComponent<RectTransform>();

                if (_root == null) _root = gameObject.AddComponent<RectTransform>();
            _root.sizeDelta = new Vector2(0f, 36f);

            var bgImage = gameObject.AddComponent<Image>();
            bgImage.color = ColorPalette.Card;

            var layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 4, 4);
            layout.spacing = 2f;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var topRow = new GameObject("TopRow");
            topRow.transform.SetParent(transform, false);
            var topRt = topRow.GetComponent<RectTransform>();

            if (topRt == null) topRt = topRow.AddComponent<RectTransform>();
            topRt.sizeDelta = new Vector2(0f, 14f);
            var topLayout = topRow.AddComponent<HorizontalLayoutGroup>();
            topLayout.spacing = 8f;
            topLayout.childForceExpandWidth = false;
            topLayout.childForceExpandHeight = false;
            topLayout.childAlignment = TextAnchor.MiddleLeft;

            var hpBarGo = new GameObject("HpBar");
            hpBarGo.transform.SetParent(topRow.transform, false);
            var hpBarRt = hpBarGo.GetComponent<RectTransform>();

            if (hpBarRt == null) hpBarRt = hpBarGo.AddComponent<RectTransform>();
            hpBarRt.sizeDelta = new Vector2(0f, 12f);
            var hpBarFlex = hpBarGo.AddComponent<LayoutElement>();
            hpBarFlex.flexibleWidth = 1f;
            hpBarFlex.preferredHeight = 12f;

            var hpBg = new GameObject("HpBg");
            hpBg.transform.SetParent(hpBarGo.transform, false);
            var hpBgRt = hpBg.GetComponent<RectTransform>();

            if (hpBgRt == null) hpBgRt = hpBg.AddComponent<RectTransform>();
            hpBgRt.anchorMin = Vector2.zero;
            hpBgRt.anchorMax = Vector2.one;
            hpBgRt.offsetMin = Vector2.zero;
            hpBgRt.offsetMax = Vector2.zero;
            var hpBgImg = hpBg.AddComponent<Image>();
            hpBgImg.color = ColorPalette.ProgressBarBackground;

            var hpFillArea = new GameObject("FillArea");
            hpFillArea.transform.SetParent(hpBarGo.transform, false);
            var hpFillAreaRt = hpFillArea.GetComponent<RectTransform>();

            if (hpFillAreaRt == null) hpFillAreaRt = hpFillArea.AddComponent<RectTransform>();
            hpFillAreaRt.anchorMin = Vector2.zero;
            hpFillAreaRt.anchorMax = Vector2.one;
            hpFillAreaRt.offsetMin = Vector2.zero;
            hpFillAreaRt.offsetMax = Vector2.zero;

            var hpFillGo = new GameObject("Fill");
            hpFillGo.transform.SetParent(hpFillArea.transform, false);
            var hpFillRt = hpFillGo.GetComponent<RectTransform>();

            if (hpFillRt == null) hpFillRt = hpFillGo.AddComponent<RectTransform>();
            hpFillRt.anchorMin = Vector2.zero;
            hpFillRt.anchorMax = Vector2.one;
            hpFillRt.offsetMin = Vector2.zero;
            hpFillRt.offsetMax = Vector2.zero;
            _hpFill = hpFillGo.AddComponent<Image>();
            _hpFill.color = ColorPalette.Hp;

            _hpSlider = hpBarGo.AddComponent<Slider>();
            _hpSlider.fillRect = hpFillRt;
            _hpSlider.targetGraphic = _hpFill;
            _hpSlider.direction = Slider.Direction.LeftToRight;
            _hpSlider.interactable = false;

            var hpTextGo = new GameObject("HpText");
            hpTextGo.transform.SetParent(hpBarGo.transform, false);
            var hpTextRt = hpTextGo.GetComponent<RectTransform>();

            if (hpTextRt == null) hpTextRt = hpTextGo.AddComponent<RectTransform>();
            hpTextRt.anchorMin = Vector2.zero;
            hpTextRt.anchorMax = Vector2.one;
            hpTextRt.offsetMin = Vector2.zero;
            hpTextRt.offsetMax = Vector2.zero;
            _hpText = hpTextGo.AddComponent<TextMeshProUGUI>();
            _hpText.fontSize = 22f;
            _hpText.color = Color.white;
            _hpText.alignment = TextAlignmentOptions.Center;
            _hpText.enableWordWrapping = false;
            _hpText.raycastTarget = false;

            var statsRow = new GameObject("StatsRow");
            statsRow.transform.SetParent(transform, false);
            var statsRt = statsRow.GetComponent<RectTransform>();

            if (statsRt == null) statsRt = statsRow.AddComponent<RectTransform>();
            statsRt.sizeDelta = new Vector2(0f, 28f);
            var statsLayout = statsRow.AddComponent<HorizontalLayoutGroup>();
            statsLayout.spacing = 12f;
            statsLayout.childForceExpandWidth = false;
            statsLayout.childForceExpandHeight = false;
            statsLayout.childAlignment = TextAnchor.MiddleCenter;

            _atkText = CreateStatLabel("ATK", statsRow.transform, new Color(1f, 0.5f, 0.5f));
            _defText = CreateStatLabel("DEF", statsRow.transform, new Color(0.5f, 0.7f, 1f));
        }

        private TextMeshProUGUI CreateStatLabel(string prefix, Transform parent, Color color)
        {
            var go = new GameObject(prefix);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 28f;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 22f;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
            return tmp;
        }

        public void SetStats(int hp, int maxHp, int atk, int def)
        {
            if (_root == null)
                BuildUI();

            _hpSlider.maxValue = maxHp;
            _hpSlider.value = Mathf.Max(0, hp);
            _hpText.text = $"{NumberFormatter.FormatInt(hp)}/{NumberFormatter.FormatInt(maxHp)}";

            float ratio = maxHp > 0 ? (float)hp / maxHp : 0f;
            if (ratio > 0.5f)
                _hpFill.color = ColorPalette.Hp;
            else if (ratio > 0.2f)
                _hpFill.color = ColorPalette.Gold;
            else
                _hpFill.color = new Color(0.8f, 0.1f, 0.1f);

            _atkText.text = $"ATK {NumberFormatter.FormatInt(atk)}";
            _defText.text = $"DEF {NumberFormatter.FormatInt(def)}";
        }
    }
}
