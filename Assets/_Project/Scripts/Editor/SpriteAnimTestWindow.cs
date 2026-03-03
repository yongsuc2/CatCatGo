using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace CatCatGo.Editor
{
    public class SpriteAnimTestWindow : EditorWindow
    {
        private string[] _spriteNames;
        private int _selectedIndex;
        private string _loadedName;
        private Texture2D _texture;

        private float _zoom = 1f;
        private Vector2 _scrollPos;
        private bool _showMotionEffects = true;

        [MenuItem("CatCatGo/Sprite Anim Test %#a")]
        public static void ShowWindow()
        {
            var w = GetWindow<SpriteAnimTestWindow>("Sprite Preview");
            w.minSize = new Vector2(400, 500);
        }

        private void OnEnable()
        {
            RefreshSpriteList();
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void RefreshSpriteList()
        {
            var charsDir = "Assets/_Project/Resources/Chars";
            var names = new HashSet<string>();

            if (Directory.Exists(charsDir))
            {
                foreach (var f in Directory.GetFiles(charsDir, "*.png"))
                    names.Add(Path.GetFileNameWithoutExtension(f));

                foreach (var d in Directory.GetDirectories(charsDir))
                    names.Add(Path.GetFileName(d));
            }

            var sorted = new List<string>(names);
            sorted.Sort();
            _spriteNames = sorted.ToArray();
            _selectedIndex = 0;
            ClearLoaded();
        }

        private void ClearLoaded()
        {
            _loadedName = null;
            _texture = null;
        }

        private void LoadSprite(string name)
        {
            if (_loadedName == name && _texture != null) return;

            ClearLoaded();
            _loadedName = name;

            var charsDir = "Assets/_Project/Resources/Chars";

            var spritePath = $"{charsDir}/{name}/sprite.png";
            if (File.Exists(spritePath))
            {
                _texture = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePath);
                if (_texture != null) return;
            }

            var idlePath = $"{charsDir}/{name}/idle_0.png";
            if (File.Exists(idlePath))
            {
                _texture = AssetDatabase.LoadAssetAtPath<Texture2D>(idlePath);
                if (_texture != null) return;
            }

            var sheetPath = $"{charsDir}/{name}.png";
            if (File.Exists(sheetPath))
                _texture = AssetDatabase.LoadAssetAtPath<Texture2D>(sheetPath);
        }

        private void OnEditorUpdate()
        {
            if (_showMotionEffects && _texture != null)
                Repaint();
        }

        private void OnGUI()
        {
            if (_spriteNames == null || _spriteNames.Length == 0)
            {
                EditorGUILayout.HelpBox("Resources/Chars/ 폴더에 스프라이트가 없습니다.", MessageType.Warning);
                if (GUILayout.Button("Refresh"))
                    RefreshSpriteList();
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawSpriteSelector();

            if (_selectedIndex >= 0 && _selectedIndex < _spriteNames.Length)
                LoadSprite(_spriteNames[_selectedIndex]);

            if (_texture == null)
            {
                EditorGUILayout.HelpBox("로드할 수 없습니다.", MessageType.Error);
                EditorGUILayout.EndScrollView();
                return;
            }

            DrawControls();
            DrawPreview();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSpriteSelector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sprite", EditorStyles.boldLabel, GUILayout.Width(50));
            int newIndex = EditorGUILayout.Popup(_selectedIndex, _spriteNames);
            if (newIndex != _selectedIndex)
            {
                _selectedIndex = newIndex;
                ClearLoaded();
            }
            if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                RefreshSpriteList();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawControls()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(
                $"[{_loadedName}] {_texture.width}x{_texture.height}px",
                EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Zoom", GUILayout.Width(36));
            _zoom = EditorGUILayout.Slider(_zoom, 0.25f, 3f);
            EditorGUILayout.EndHorizontal();

            _showMotionEffects = EditorGUILayout.ToggleLeft("Motion Effects (Bob + Breathe)", _showMotionEffects);
        }

        private void DrawPreview()
        {
            EditorGUILayout.Space(2);

            float time = (float)EditorApplication.timeSinceStartup;
            float motionBobY = 0f;
            float motionScale = 1f;

            if (_showMotionEffects)
            {
                motionBobY = Mathf.Sin(time * 2.5f) * 4f * _zoom;
                motionScale = Mathf.Lerp(0.97f, 1.03f, (Mathf.Sin(time * 2f) + 1f) * 0.5f);
            }

            float baseW = _texture.width * _zoom;
            float baseH = _texture.height * _zoom;
            float padding = 20f * _zoom;
            float areaH = baseH + padding * 2f;

            Rect areaRect = GUILayoutUtility.GetRect(baseW + padding * 2f, areaH, GUILayout.ExpandWidth(false));
            EditorGUI.DrawRect(areaRect, new Color(0.12f, 0.12f, 0.12f));

            float displayW = baseW * motionScale;
            float displayH = baseH * motionScale;
            float drawX = areaRect.x + (areaRect.width - displayW) * 0.5f;
            float drawY = areaRect.y + (areaRect.height - displayH) * 0.5f - motionBobY;

            Rect drawRect = new Rect(drawX, drawY, displayW, displayH);
            GUI.DrawTexture(drawRect, _texture, ScaleMode.StretchToFill);
            DrawRectOutline(areaRect, Color.green, 2f);

            EditorGUILayout.LabelField(
                $"Display: {(int)displayW}x{(int)displayH}",
                EditorStyles.miniLabel);
        }

        private static void DrawRectOutline(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }
    }
}
