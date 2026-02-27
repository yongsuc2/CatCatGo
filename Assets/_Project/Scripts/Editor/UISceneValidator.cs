using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using CatCatGo.Presentation.Core;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Editor
{
    public static class UISceneValidator
    {
        [MenuItem("Tools/Validate UI Buttons")]
        public static void ValidateScene()
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            if (canvases.Length == 0)
            {
                Debug.Log("[UIValidator] No Canvas found in scene.");
                return;
            }

            int totalButtons = 0;
            int totalViolations = 0;

            foreach (var canvas in canvases)
            {
                var buttons = canvas.GetComponentsInChildren<Button>(true);
                totalButtons += buttons.Length;

                var violations = UIValidator.ValidateButtons(canvas.transform);
                totalViolations += violations.Count;

                foreach (var v in violations)
                    Debug.LogWarning($"[UIValidator] {v}", canvas.gameObject);
            }

            if (totalViolations == 0)
                Debug.Log($"[UIValidator] All {totalButtons} buttons passed validation.");
            else
                Debug.LogWarning($"[UIValidator] {totalViolations} violations found in {totalButtons} buttons.");
        }

        [MenuItem("Tools/Validate UI Buttons (Battle Controls)")]
        public static void ValidateSceneBattleControls()
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            if (canvases.Length == 0)
            {
                Debug.Log("[UIValidator] No Canvas found in scene.");
                return;
            }

            foreach (var canvas in canvases)
            {
                var violations = UIValidator.ValidateButtons(
                    canvas.transform, UIConstants.MIN_BATTLE_CONTROL_HEIGHT);

                foreach (var v in violations)
                    Debug.LogWarning($"[UIValidator-Battle] {v}", canvas.gameObject);

                if (violations.Count == 0)
                    Debug.Log($"[UIValidator-Battle] Canvas '{canvas.name}' passed.");
            }
        }
    }
}
