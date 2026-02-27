using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CatCatGo.Presentation.Core;

namespace CatCatGo.Presentation.Utils
{
    public struct UIViolation
    {
        public string Path;
        public string Rule;
        public float Actual;
        public float Expected;

        public override string ToString()
        {
            return $"[{Rule}] {Path}: {Actual} (min {Expected})";
        }
    }

    public static class UIValidator
    {
        public static List<UIViolation> ValidateAll(Transform root, float minButtonHeight = UIConstants.MIN_BUTTON_HEIGHT)
        {
            var violations = new List<UIViolation>();
            ValidateRecursive(root, violations, minButtonHeight);
            return violations;
        }

        public static List<UIViolation> ValidateButtons(Transform root, float minHeight = UIConstants.MIN_BUTTON_HEIGHT)
        {
            var violations = new List<UIViolation>();
            ValidateButtonsRecursive(root, violations, minHeight);
            return violations;
        }

        public static List<UIViolation> ValidateFontSizes(Transform root, float minFontSize = UIConstants.MIN_FONT_SIZE)
        {
            var violations = new List<UIViolation>();
            ValidateFontSizesRecursive(root, violations, minFontSize);
            return violations;
        }

        private static void ValidateRecursive(Transform node, List<UIViolation> violations, float minButtonHeight)
        {
            var button = node.GetComponent<Button>();
            if (button != null)
            {
                ValidateButtonHeight(node, violations, minButtonHeight);
                ValidateButtonFontSize(node, violations);
            }

            var tmp = node.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
                ValidateFontSize(node, tmp, violations, UIConstants.MIN_FONT_SIZE);

            for (int i = 0; i < node.childCount; i++)
                ValidateRecursive(node.GetChild(i), violations, minButtonHeight);
        }

        private static void ValidateButtonsRecursive(Transform node, List<UIViolation> violations, float minHeight)
        {
            var button = node.GetComponent<Button>();
            if (button != null)
            {
                ValidateButtonHeight(node, violations, minHeight);
                ValidateButtonFontSize(node, violations);
            }

            for (int i = 0; i < node.childCount; i++)
                ValidateButtonsRecursive(node.GetChild(i), violations, minHeight);
        }

        private static void ValidateFontSizesRecursive(Transform node, List<UIViolation> violations, float minFontSize)
        {
            var tmp = node.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
                ValidateFontSize(node, tmp, violations, minFontSize);

            for (int i = 0; i < node.childCount; i++)
                ValidateFontSizesRecursive(node.GetChild(i), violations, minFontSize);
        }

        private static void ValidateButtonHeight(Transform node, List<UIViolation> violations, float minHeight)
        {
            var le = node.GetComponent<LayoutElement>();
            if (le == null) return;

            float height = le.preferredHeight;
            if (height < 0) return;

            if (height < minHeight)
            {
                violations.Add(new UIViolation
                {
                    Path = GetPath(node),
                    Rule = "ButtonHeight",
                    Actual = height,
                    Expected = minHeight,
                });
            }
        }

        private static void ValidateButtonFontSize(Transform node, List<UIViolation> violations)
        {
            var tmp = node.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp == null) return;

            if (tmp.fontSize < UIConstants.MIN_BUTTON_FONT_SIZE)
            {
                violations.Add(new UIViolation
                {
                    Path = GetPath(node),
                    Rule = "ButtonFontSize",
                    Actual = tmp.fontSize,
                    Expected = UIConstants.MIN_BUTTON_FONT_SIZE,
                });
            }
        }

        private static void ValidateFontSize(Transform node, TextMeshProUGUI tmp, List<UIViolation> violations, float minFontSize)
        {
            if (tmp.fontSize < minFontSize)
            {
                violations.Add(new UIViolation
                {
                    Path = GetPath(node),
                    Rule = "FontSize",
                    Actual = tmp.fontSize,
                    Expected = minFontSize,
                });
            }
        }

        public static string GetPath(Transform t)
        {
            var parts = new List<string>();
            while (t != null)
            {
                parts.Add(t.name);
                t = t.parent;
            }
            parts.Reverse();
            return string.Join("/", parts);
        }
    }
}
