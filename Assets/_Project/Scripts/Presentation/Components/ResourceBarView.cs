using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Services;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Components
{
    public class ResourceBarView : MonoBehaviour
    {
        private TextMeshProUGUI _goldText;
        private TextMeshProUGUI _gemsText;
        private TextMeshProUGUI _staminaText;

        public void Initialize()
        {
            var bg = gameObject.AddComponent<Image>();
            bg.color = ColorPalette.ResourceBarBackground;

            var layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 16;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(20, 20, 4, 4);

            _goldText = CreateResourceEntry("G", ColorPalette.Gold);
            _gemsText = CreateResourceEntry("D", ColorPalette.Gems);
            _staminaText = CreateResourceEntry("S", ColorPalette.Stamina);
        }

        public void Refresh()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Player == null) return;

            var resources = gm.Player.Resources;
            _goldText.text = NumberFormatter.Format(resources.Gold);
            _gemsText.text = NumberFormatter.Format(resources.Gems);
            _staminaText.text = $"{(int)resources.Stamina}/{resources.GetStaminaMax()}";
        }

        private TextMeshProUGUI CreateResourceEntry(string iconLabel, Color color)
        {
            var entryGo = new GameObject("ResourceEntry");
            entryGo.transform.SetParent(transform, false);

            var entryLayout = entryGo.AddComponent<HorizontalLayoutGroup>();
            entryLayout.spacing = 4;
            entryLayout.childForceExpandWidth = false;
            entryLayout.childForceExpandHeight = true;
            entryLayout.childAlignment = TextAnchor.MiddleCenter;

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(entryGo.transform, false);
            var iconText = iconGo.AddComponent<TextMeshProUGUI>();
            iconText.text = iconLabel;
            iconText.fontSize = 32;
            iconText.color = color;
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.raycastTarget = false;
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 36;

            var valueGo = new GameObject("Value");
            valueGo.transform.SetParent(entryGo.transform, false);
            var valueText = valueGo.AddComponent<TextMeshProUGUI>();
            valueText.text = "0";
            valueText.fontSize = 30;
            valueText.color = color;
            valueText.alignment = TextAlignmentOptions.Left;
            valueText.enableWordWrapping = false;
            valueText.overflowMode = TextOverflowModes.Ellipsis;
            valueText.raycastTarget = false;
            var valueLe = valueGo.AddComponent<LayoutElement>();
            valueLe.flexibleWidth = 1;

            return valueText;
        }
    }
}
