using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Editor
{
    public static class UISceneValidator
    {
        [MenuItem("Tools/UI Validator/All (Buttons + Fonts)")]
        public static void ValidateAll()
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            if (canvases.Length == 0)
            {
                Debug.Log("[UIValidator] No Canvas found in scene.");
                return;
            }

            int totalViolations = 0;
            foreach (var canvas in canvases)
            {
                var violations = UIValidator.ValidateAll(canvas.transform);
                totalViolations += violations.Count;
                foreach (var v in violations)
                    Debug.LogWarning($"[UIValidator] {v}", canvas.gameObject);
            }

            Debug.Log(totalViolations == 0
                ? "[UIValidator] All checks passed."
                : $"[UIValidator] {totalViolations} violations found.");
        }

        [MenuItem("Tools/UI Validator/Buttons Only")]
        public static void ValidateButtons()
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            if (canvases.Length == 0)
            {
                Debug.Log("[UIValidator] No Canvas found in scene.");
                return;
            }

            int totalViolations = 0;
            foreach (var canvas in canvases)
            {
                var violations = UIValidator.ValidateButtons(canvas.transform);
                totalViolations += violations.Count;
                foreach (var v in violations)
                    Debug.LogWarning($"[UIValidator] {v}", canvas.gameObject);
            }

            Debug.Log(totalViolations == 0
                ? "[UIValidator] All buttons passed."
                : $"[UIValidator] {totalViolations} button violations found.");
        }

        [MenuItem("Tools/UI Validator/Font Sizes Only")]
        public static void ValidateFontSizes()
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            if (canvases.Length == 0)
            {
                Debug.Log("[UIValidator] No Canvas found in scene.");
                return;
            }

            int totalViolations = 0;
            foreach (var canvas in canvases)
            {
                var violations = UIValidator.ValidateFontSizes(canvas.transform);
                totalViolations += violations.Count;
                foreach (var v in violations)
                    Debug.LogWarning($"[UIValidator] {v}", canvas.gameObject);
            }

            Debug.Log(totalViolations == 0
                ? "[UIValidator] All fonts passed."
                : $"[UIValidator] {totalViolations} font violations found.");
        }

        [MenuItem("Tools/UI Validator/Battle Controls")]
        public static void ValidateBattleControls()
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            if (canvases.Length == 0)
            {
                Debug.Log("[UIValidator] No Canvas found in scene.");
                return;
            }

            int totalViolations = 0;
            foreach (var canvas in canvases)
            {
                var violations = UIValidator.ValidateButtons(
                    canvas.transform, UIConstants.MIN_BATTLE_CONTROL_HEIGHT);
                totalViolations += violations.Count;
                foreach (var v in violations)
                    Debug.LogWarning($"[UIValidator-Battle] {v}", canvas.gameObject);
            }

            Debug.Log(totalViolations == 0
                ? "[UIValidator-Battle] All passed."
                : $"[UIValidator-Battle] {totalViolations} violations found.");
        }
    }
}
