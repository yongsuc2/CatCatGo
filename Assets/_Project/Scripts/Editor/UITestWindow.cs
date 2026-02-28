using UnityEngine;
using UnityEditor;
using CatCatGo.Domain.Enums;
using CatCatGo.Presentation.Core;
using CatCatGo.Services;

namespace CatCatGo.Editor
{
    public class UITestWindow : EditorWindow
    {
        private Vector2 _scrollPos;

        [MenuItem("CatCatGo/UI Test Window %#u")]
        public static void ShowWindow()
        {
            GetWindow<UITestWindow>("UI Test");
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Play Mode\uc5d0\uc11c\ub9cc \uc0ac\uc6a9 \uac00\ub2a5\ud569\ub2c8\ub2e4.", MessageType.Warning);
                if (GUILayout.Button("Play", GUILayout.Height(40)))
                    EditorApplication.isPlaying = true;
                return;
            }

            if (UIManager.Instance == null)
            {
                EditorGUILayout.HelpBox("UIManager\uac00 \ucd08\uae30\ud654\ub418\uc9c0 \uc54a\uc558\uc2b5\ub2c8\ub2e4.", MessageType.Warning);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.LabelField("Screen", EditorStyles.boldLabel);
            DrawScreenButtons();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Popup", EditorStyles.boldLabel);
            DrawPopupButtons();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Resource", EditorStyles.boldLabel);
            DrawResourceButtons();

            EditorGUILayout.EndScrollView();
        }

        private void DrawScreenButtons()
        {
            var screens = new[]
            {
                ("Main", ScreenType.Main),
                ("Chapter", ScreenType.Chapter),
                ("Talent", ScreenType.Talent),
                ("Equipment", ScreenType.Equipment),
                ("Pet", ScreenType.Pet),
                ("Content", ScreenType.Content),
                ("Gacha", ScreenType.Gacha),
                ("Quest", ScreenType.Quest),
                ("Event", ScreenType.Event),
                ("Settings", ScreenType.Settings),
                ("Debug", ScreenType.Debug),
                ("ChapterTreasure", ScreenType.ChapterTreasure),
            };

            int cols = 3;
            for (int i = 0; i < screens.Length; i += cols)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < cols && i + j < screens.Length; j++)
                {
                    var (label, type) = screens[i + j];
                    bool isActive = UIManager.Instance.ActiveScreenType == type;
                    GUI.backgroundColor = isActive ? Color.cyan : Color.white;
                    if (GUILayout.Button(label, GUILayout.Height(30)))
                        UIManager.Instance.ShowScreen(type);
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawPopupButtons()
        {
            if (GUILayout.Button("GachaRewardPopup (Mock 10\ud68c)", GUILayout.Height(28)))
            {
                var game = GameManager.Instance;
                if (game == null) return;
                var results = game.PullGacha10();
                if (results == null) return;

                foreach (var result in results)
                {
                    if (result.Equipment != null)
                        game.Player.AddToInventory(result.Equipment);
                    foreach (var r in result.Resources)
                        game.Player.Resources.Add(r.Type, r.Amount);
                }
                game.SaveGame();
                UIManager.Instance.Refresh();

                UIManager.Instance.ShowPopupFromType<CatCatGo.Presentation.Popups.GachaRewardPopup>(
                    new CatCatGo.Presentation.Popups.GachaRewardPopupData
                    {
                        Results = results,
                        OnPullAgain = null,
                    });
            }

            if (GUILayout.Button("Close Popup", GUILayout.Height(28)))
                UIManager.Instance.ClosePopup();
        }

        private void DrawResourceButtons()
        {
            var game = GameManager.Instance;
            if (game == null || game.Player == null) return;

            EditorGUILayout.LabelField(
                $"Gold: {game.Player.Resources.Gold}  Gems: {game.Player.Resources.Gems}  Stamina: {game.Player.Resources.Stamina}");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+1K Gold", GUILayout.Height(24)))
            {
                game.Player.Resources.Add(ResourceType.GOLD, 1000);
                UIManager.Instance.Refresh();
            }
            if (GUILayout.Button("+100 Gems", GUILayout.Height(24)))
            {
                game.Player.Resources.Add(ResourceType.GEMS, 100);
                UIManager.Instance.Refresh();
            }
            if (GUILayout.Button("+50 Stamina", GUILayout.Height(24)))
            {
                game.Player.Resources.Add(ResourceType.STAMINA, 50);
                UIManager.Instance.Refresh();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+5 Pet Egg", GUILayout.Height(24)))
            {
                game.Player.Resources.Add(ResourceType.PET_EGG, 5);
                UIManager.Instance.Refresh();
            }
            if (GUILayout.Button("+50 Pet Food", GUILayout.Height(24)))
            {
                game.Player.Resources.Add(ResourceType.PET_FOOD, 50);
                UIManager.Instance.Refresh();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Refresh UI", GUILayout.Height(28)))
                UIManager.Instance.Refresh();
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
                Repaint();
        }
    }
}
