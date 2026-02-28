using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace CatCatGo.Editor
{
    public class SpriteAnimTestWindow : EditorWindow
    {
        private const int FRAME_COUNT = 4;

        private enum SpriteFormat { None, Sheet, Individual }

        private string[] _spriteNames;
        private int _selectedIndex;
        private string _loadedName;

        private SpriteFormat _format;
        private Texture2D _sheetTexture;
        private Texture2D[] _idleFrames;
        private Texture2D[] _attackFrames;

        private int _currentFrame;
        private double _lastFrameTime;
        private float _frameInterval = 0.15f;
        private int _animRow;
        private bool _playing = true;
        private bool _showAllFrames = true;
        private float _zoom = 1f;
        private Vector2 _scrollPos;
        private bool _showMotionEffects = true;

        [MenuItem("CatCatGo/Sprite Anim Test %#a")]
        public static void ShowWindow()
        {
            var w = GetWindow<SpriteAnimTestWindow>("Sprite Anim Test");
            w.minSize = new Vector2(500, 600);
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
                {
                    var idleCheck = Path.Combine(d, "idle_0.png");
                    if (File.Exists(idleCheck))
                        names.Add(Path.GetFileName(d));
                }
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
            _format = SpriteFormat.None;
            _sheetTexture = null;
            _idleFrames = null;
            _attackFrames = null;
            _currentFrame = 0;
        }

        private void LoadSprite(string name)
        {
            if (_loadedName == name && _format != SpriteFormat.None) return;

            ClearLoaded();
            _loadedName = name;

            var charsDir = "Assets/_Project/Resources/Chars";
            var individualDir = Path.Combine(charsDir, name);
            var idleCheck = Path.Combine(individualDir, "idle_0.png");

            if (File.Exists(idleCheck))
            {
                _format = SpriteFormat.Individual;
                _idleFrames = new Texture2D[FRAME_COUNT];
                _attackFrames = new Texture2D[FRAME_COUNT];

                for (int i = 0; i < FRAME_COUNT; i++)
                {
                    _idleFrames[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        $"{individualDir}/idle_{i}.png");
                    _attackFrames[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        $"{individualDir}/attack_{i}.png");
                }

                for (int i = 0; i < FRAME_COUNT; i++)
                {
                    if (_idleFrames[i] == null && _idleFrames[0] != null)
                        _idleFrames[i] = _idleFrames[0];
                    if (_attackFrames[i] == null)
                        _attackFrames[i] = _attackFrames[0] != null ? _attackFrames[0] : _idleFrames[i];
                }
                return;
            }

            var sheetPath = $"{charsDir}/{name}.png";
            if (File.Exists(sheetPath))
            {
                _sheetTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(sheetPath);
                if (_sheetTexture != null)
                    _format = SpriteFormat.Sheet;
            }
        }

        private void OnEditorUpdate()
        {
            if (_format == SpriteFormat.None) return;

            bool needRepaint = false;

            if (_playing && EditorApplication.timeSinceStartup - _lastFrameTime >= _frameInterval)
            {
                _lastFrameTime = EditorApplication.timeSinceStartup;
                _currentFrame = (_currentFrame + 1) % FRAME_COUNT;
                needRepaint = true;
            }

            if (_showMotionEffects)
                needRepaint = true;

            if (needRepaint)
                Repaint();
        }

        private void OnGUI()
        {
            if (_spriteNames == null || _spriteNames.Length == 0)
            {
                EditorGUILayout.HelpBox("Resources/Chars/ \ud3f4\ub354\uc5d0 \uc2a4\ud504\ub77c\uc774\ud2b8\uac00 \uc5c6\uc2b5\ub2c8\ub2e4.", MessageType.Warning);
                if (GUILayout.Button("Refresh"))
                    RefreshSpriteList();
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawSpriteSelector();

            if (_selectedIndex >= 0 && _selectedIndex < _spriteNames.Length)
                LoadSprite(_spriteNames[_selectedIndex]);

            if (_format == SpriteFormat.None)
            {
                EditorGUILayout.HelpBox("\ub85c\ub4dc\ud560 \uc218 \uc5c6\uc2b5\ub2c8\ub2e4.", MessageType.Error);
                EditorGUILayout.EndScrollView();
                return;
            }

            DrawFormatInfo();
            DrawAnimControls();
            DrawPreview();

            EditorGUILayout.Space(8);
            _showAllFrames = EditorGUILayout.Foldout(_showAllFrames, "All Frames", true);
            if (_showAllFrames)
                DrawAllFrames();

            if (_format == SpriteFormat.Sheet)
            {
                EditorGUILayout.Space(8);
                DrawFullSheet();
            }

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

        private void DrawFormatInfo()
        {
            if (_format == SpriteFormat.Sheet)
            {
                EditorGUILayout.LabelField(
                    $"[Sheet] {_sheetTexture.width}x{_sheetTexture.height}  |  Frame: {_sheetTexture.width / 4}x{_sheetTexture.height / 3}");
            }
            else
            {
                var tex = _idleFrames[0];
                string size = tex != null ? $"{tex.width}x{tex.height}" : "?";
                int idleCount = 0, atkCount = 0;
                for (int i = 0; i < FRAME_COUNT; i++)
                {
                    if (_idleFrames[i] != null) idleCount++;
                    if (_attackFrames[i] != null) atkCount++;
                }
                EditorGUILayout.LabelField(
                    $"[Individual] {size}  |  idle: {idleCount}  attack: {atkCount}");
            }
        }

        private void DrawAnimControls()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Animation Preview", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = _animRow == 0 ? Color.cyan : Color.white;
            if (GUILayout.Button("Idle", GUILayout.Height(28)))
            {
                _animRow = 0;
                _currentFrame = 0;
            }
            GUI.backgroundColor = _animRow == 1 ? Color.cyan : Color.white;
            if (GUILayout.Button("Attack", GUILayout.Height(28)))
            {
                _animRow = 1;
                _currentFrame = 0;
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _playing = GUILayout.Toggle(_playing, _playing ? "Playing" : "Paused", "Button", GUILayout.Width(80));
            EditorGUILayout.LabelField("Speed", GUILayout.Width(40));
            _frameInterval = EditorGUILayout.Slider(_frameInterval, 0.05f, 0.5f);
            EditorGUILayout.LabelField($"F{_currentFrame}", GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            if (!_playing)
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < FRAME_COUNT; i++)
                {
                    GUI.backgroundColor = i == _currentFrame ? Color.yellow : Color.white;
                    if (GUILayout.Button($"F{i}", GUILayout.Width(40), GUILayout.Height(22)))
                        _currentFrame = i;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Zoom", GUILayout.Width(36));
            _zoom = EditorGUILayout.Slider(_zoom, 0.25f, 3f);
            EditorGUILayout.EndHorizontal();

            _showMotionEffects = EditorGUILayout.ToggleLeft("Motion Effects (Bob + Breathe + Attack Scale)", _showMotionEffects);
        }

        private void DrawPreview()
        {
            EditorGUILayout.Space(2);

            Texture2D frameTex = GetCurrentFrameTexture();
            if (frameTex == null)
            {
                EditorGUILayout.HelpBox("No frame texture", MessageType.Warning);
                return;
            }

            float time = (float)EditorApplication.timeSinceStartup;
            float motionBobY = 0f;
            float motionScale = 1f;

            if (_showMotionEffects)
            {
                if (_animRow == 0)
                {
                    motionBobY = Mathf.Sin(time * 2.5f) * 4f * _zoom;
                    motionScale = Mathf.Lerp(0.97f, 1.03f, (Mathf.Sin(time * 2f) + 1f) * 0.5f);
                }
                else
                {
                    float attackCycle = (time * 3f) % 1f;
                    if (attackCycle < 0.3f)
                    {
                        float t = attackCycle / 0.3f;
                        motionScale = Mathf.Lerp(1f, 1.15f, t * t);
                    }
                    else if (attackCycle < 0.5f)
                    {
                        float t = (attackCycle - 0.3f) / 0.2f;
                        motionScale = Mathf.Lerp(1.15f, 0.9f, t);
                        motionBobY = Mathf.Sin(t * Mathf.PI * 4f) * 6f * _zoom * (1f - t);
                    }
                    else
                    {
                        float t = (attackCycle - 0.5f) / 0.5f;
                        motionScale = Mathf.Lerp(0.9f, 1f, t);
                    }
                }
            }

            if (_format == SpriteFormat.Individual)
            {
                float baseW = frameTex.width * _zoom;
                float baseH = frameTex.height * _zoom;
                float padding = 20f * _zoom;
                float areaH = baseH + padding * 2f;

                Rect areaRect = GUILayoutUtility.GetRect(baseW + padding * 2f, areaH, GUILayout.ExpandWidth(false));
                EditorGUI.DrawRect(areaRect, new Color(0.12f, 0.12f, 0.12f));

                float displayW = baseW * motionScale;
                float displayH = baseH * motionScale;
                float drawX = areaRect.x + (areaRect.width - displayW) * 0.5f;
                float drawY = areaRect.y + (areaRect.height - displayH) * 0.5f - motionBobY;

                Rect drawRect = new Rect(drawX, drawY, displayW, displayH);
                GUI.DrawTexture(drawRect, frameTex, ScaleMode.StretchToFill);
                DrawRectOutline(areaRect, Color.green, 2f);

                EditorGUILayout.LabelField(
                    $"{frameTex.width}x{frameTex.height}px  |  Display: {(int)displayW}x{(int)displayH}",
                    EditorStyles.miniLabel);
            }
            else
            {
                int frameW = _sheetTexture.width / 4;
                int frameH = _sheetTexture.height / 3;
                float baseW = frameW * _zoom;
                float baseH = frameH * _zoom;
                float padding = 20f * _zoom;
                float areaH = baseH + padding * 2f;

                Rect areaRect = GUILayoutUtility.GetRect(baseW + padding * 2f, areaH, GUILayout.ExpandWidth(false));
                Rect uvRect = GetSheetUVRect(_currentFrame, _animRow);

                EditorGUI.DrawRect(areaRect, new Color(0.12f, 0.12f, 0.12f));

                float displayW = baseW * motionScale;
                float displayH = baseH * motionScale;
                float drawX = areaRect.x + (areaRect.width - displayW) * 0.5f;
                float drawY = areaRect.y + (areaRect.height - displayH) * 0.5f - motionBobY;

                Rect drawRect = new Rect(drawX, drawY, displayW, displayH);
                GUI.DrawTextureWithTexCoords(drawRect, _sheetTexture, uvRect);
                DrawRectOutline(areaRect, Color.green, 2f);

                int pixelX = _currentFrame * frameW;
                int pixelY = _animRow * frameH;
                EditorGUILayout.LabelField(
                    $"{frameW}x{frameH}px  |  Pixel: ({pixelX},{pixelY})~({pixelX + frameW - 1},{pixelY + frameH - 1})",
                    EditorStyles.miniLabel);
            }
        }

        private void DrawAllFrames()
        {
            string[] rowNames = { "Idle", "Attack" };

            for (int row = 0; row < 2; row++)
            {
                EditorGUILayout.LabelField(rowNames[row], EditorStyles.miniLabel);
                EditorGUILayout.BeginHorizontal();

                for (int col = 0; col < FRAME_COUNT; col++)
                {
                    Texture2D tex = GetFrameTexture(col, row);
                    if (tex == null) continue;

                    float thumbW, thumbH;
                    if (_format == SpriteFormat.Individual)
                    {
                        float maxThumb = Mathf.Min((position.width - 60) / FRAME_COUNT, 120);
                        thumbW = maxThumb;
                        thumbH = maxThumb * ((float)tex.height / tex.width);
                    }
                    else
                    {
                        int frameW = _sheetTexture.width / 4;
                        int frameH = _sheetTexture.height / 3;
                        float maxThumb = Mathf.Min((position.width - 60) / FRAME_COUNT, 120);
                        thumbW = maxThumb;
                        thumbH = maxThumb * ((float)frameH / frameW);
                    }

                    Rect thumbRect = GUILayoutUtility.GetRect(thumbW, thumbH, GUILayout.ExpandWidth(false));

                    bool isSelected = row == _animRow && col == _currentFrame;
                    EditorGUI.DrawRect(thumbRect,
                        isSelected ? new Color(0.2f, 0.5f, 0.9f, 0.3f) : new Color(0.12f, 0.12f, 0.12f));

                    if (_format == SpriteFormat.Individual)
                        GUI.DrawTexture(thumbRect, tex, ScaleMode.ScaleToFit);
                    else
                        GUI.DrawTextureWithTexCoords(thumbRect, _sheetTexture, GetSheetUVRect(col, row));

                    if (isSelected)
                        DrawRectOutline(thumbRect, Color.green, 1f);

                    if (Event.current.type == EventType.MouseDown && thumbRect.Contains(Event.current.mousePosition))
                    {
                        _animRow = row;
                        _currentFrame = col;
                        _playing = false;
                        Event.current.Use();
                    }
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(4);
            }
        }

        private void DrawFullSheet()
        {
            EditorGUILayout.LabelField("Full Sheet (Grid Overlay)", EditorStyles.boldLabel);

            float sheetAspect = (float)_sheetTexture.height / _sheetTexture.width;
            float sheetDisplayW = Mathf.Min(position.width - 30, 500);
            float sheetDisplayH = sheetDisplayW * sheetAspect;

            Rect sheetRect = GUILayoutUtility.GetRect(sheetDisplayW, sheetDisplayH, GUILayout.ExpandWidth(false));
            EditorGUI.DrawRect(sheetRect, new Color(0.12f, 0.12f, 0.12f));
            GUI.DrawTexture(sheetRect, _sheetTexture, ScaleMode.StretchToFill);

            float cellW = sheetRect.width / 4f;
            float cellH = sheetRect.height / 3f;

            for (int r = 0; r <= 3; r++)
            {
                float y = sheetRect.y + r * cellH;
                EditorGUI.DrawRect(new Rect(sheetRect.x, y, sheetRect.width, 1), new Color(1f, 1f, 0f, 0.6f));
            }
            for (int c = 0; c <= 4; c++)
            {
                float x = sheetRect.x + c * cellW;
                EditorGUI.DrawRect(new Rect(x, sheetRect.y, 1, sheetRect.height), new Color(1f, 1f, 0f, 0.6f));
            }

            float selX = sheetRect.x + _currentFrame * cellW;
            float selY = sheetRect.y + _animRow * cellH;
            DrawRectOutline(new Rect(selX, selY, cellW, cellH), Color.green, 2f);

            string[] rowNames = { "Idle", "Attack", "Empty" };
            for (int r = 0; r < 3; r++)
            {
                var labelRect = new Rect(sheetRect.x + 4, sheetRect.y + r * cellH + 2, 80, 16);
                EditorGUI.DropShadowLabel(labelRect, rowNames[r], EditorStyles.miniLabel);
            }
        }

        private Texture2D GetCurrentFrameTexture()
        {
            return GetFrameTexture(_currentFrame, _animRow);
        }

        private Texture2D GetFrameTexture(int col, int row)
        {
            if (_format == SpriteFormat.Individual)
            {
                var frames = row == 0 ? _idleFrames : _attackFrames;
                if (frames != null && col < frames.Length)
                    return frames[col];
                return null;
            }
            return _sheetTexture;
        }

        private Rect GetSheetUVRect(int col, int row)
        {
            float frameW = _sheetTexture.width / 4f;
            float frameH = _sheetTexture.height / 3f;
            float uvX = col * frameW / _sheetTexture.width;
            float uvW = frameW / _sheetTexture.width;
            float uvY = 1f - (row + 1) * frameH / _sheetTexture.height;
            float uvH = frameH / _sheetTexture.height;
            return new Rect(uvX, uvY, uvW, uvH);
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
