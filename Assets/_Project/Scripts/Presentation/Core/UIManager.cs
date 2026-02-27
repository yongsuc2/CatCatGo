using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TextCore.LowLevel;
using TMPro;
using CatCatGo.Services;
using CatCatGo.Presentation.Components;
using CatCatGo.Presentation.Screens;
using CatCatGo.Presentation.Utils;

namespace CatCatGo.Presentation.Core
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private Canvas _canvas;
        private CanvasScaler _canvasScaler;
        private GraphicRaycaster _raycaster;

        private RectTransform _safeAreaContainer;
        private RectTransform _resourceBarArea;
        private RectTransform _screenContainer;
        private RectTransform _navBarArea;
        private RectTransform _popupLayer;
        private RectTransform _popupOverlay;
        private RectTransform _popupContainer;

        private ResourceBarView _resourceBar;
        private NavBarView _navBar;

        private Dictionary<ScreenType, BaseScreen> _screens = new Dictionary<ScreenType, BaseScreen>();
        private ScreenType _activeScreenType = ScreenType.Main;
        private BaseScreen _activeScreen;
        private BasePopup _activePopup;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            SetupKoreanFallbackFont();
            BuildCanvasHierarchy();
            CreateResourceBar();
            CreateNavBar();
            RegisterScreens();
        }

        private void Start()
        {
            ShowScreen(ScreenType.Main);
            Refresh();
        }

        private void Update()
        {
        }

        private void SetupKoreanFallbackFont()
        {
            var defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont == null) return;

            var bundledFont = Resources.Load<Font>("Fonts/MalgunGothic");
            if (bundledFont == null)
                bundledFont = Font.CreateDynamicFontFromOSFont("Malgun Gothic", 36);

            if (bundledFont == null) return;

            var koreanFont = TMP_FontAsset.CreateFontAsset(
                bundledFont, 48, 6,
                GlyphRenderMode.SDFAA,
                4096, 4096,
                AtlasPopulationMode.Dynamic);

            if (koreanFont == null) return;

            if (defaultFont.fallbackFontAssetTable == null)
                defaultFont.fallbackFontAssetTable = new List<TMP_FontAsset>();

            defaultFont.fallbackFontAssetTable.Add(koreanFont);
        }

        private void BuildCanvasHierarchy()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 0;

            _canvasScaler = gameObject.AddComponent<CanvasScaler>();
            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvasScaler.referenceResolution = new Vector2(1080, 1920);
            _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _canvasScaler.matchWidthOrHeight = 1f;

            _raycaster = gameObject.AddComponent<GraphicRaycaster>();

            _safeAreaContainer = CreateRectTransform("SafeAreaContainer", transform);
            StretchFull(_safeAreaContainer);
            _safeAreaContainer.gameObject.AddComponent<SafeAreaFitter>();

            _resourceBarArea = CreateRectTransform("ResourceBarArea", _safeAreaContainer);
            _resourceBarArea.anchorMin = new Vector2(0, 1);
            _resourceBarArea.anchorMax = new Vector2(1, 1);
            _resourceBarArea.pivot = new Vector2(0.5f, 1);
            _resourceBarArea.offsetMin = new Vector2(0, -80);
            _resourceBarArea.offsetMax = Vector2.zero;

            _navBarArea = CreateRectTransform("NavBarArea", _safeAreaContainer);
            _navBarArea.anchorMin = new Vector2(0, 0);
            _navBarArea.anchorMax = new Vector2(1, 0);
            _navBarArea.pivot = new Vector2(0.5f, 0);
            _navBarArea.offsetMin = Vector2.zero;
            _navBarArea.offsetMax = new Vector2(0, 120);

            _screenContainer = CreateRectTransform("ScreenContainer", _safeAreaContainer);
            _screenContainer.anchorMin = new Vector2(0, 0);
            _screenContainer.anchorMax = new Vector2(1, 1);
            _screenContainer.offsetMin = new Vector2(0, 120);
            _screenContainer.offsetMax = new Vector2(0, -80);

            var screenBg = _screenContainer.gameObject.AddComponent<Image>();
            screenBg.color = ColorPalette.Background;

            _popupLayer = CreateRectTransform("PopupLayer", _safeAreaContainer);
            StretchFull(_popupLayer);
            _popupLayer.gameObject.SetActive(false);

            _popupOverlay = CreateRectTransform("PopupOverlay", _popupLayer);
            StretchFull(_popupOverlay);
            var overlayImage = _popupOverlay.gameObject.AddComponent<Image>();
            overlayImage.color = ColorPalette.PopupOverlay;
            var overlayButton = _popupOverlay.gameObject.AddComponent<Button>();
            overlayButton.targetGraphic = overlayImage;
            overlayButton.onClick.AddListener(ClosePopup);

            _popupContainer = CreateRectTransform("PopupContainer", _popupLayer);
            _popupContainer.anchorMin = new Vector2(0.05f, 0.1f);
            _popupContainer.anchorMax = new Vector2(0.95f, 0.9f);
            _popupContainer.offsetMin = Vector2.zero;
            _popupContainer.offsetMax = Vector2.zero;
        }

        private void CreateResourceBar()
        {
            _resourceBar = _resourceBarArea.gameObject.AddComponent<ResourceBarView>();
            _resourceBar.Initialize();
        }

        private void CreateNavBar()
        {
            _navBar = _navBarArea.gameObject.AddComponent<NavBarView>();
            _navBar.Initialize();
        }

        private void RegisterScreens()
        {
            RegisterScreen<MainScreen>(ScreenType.Main, "MainScreen");
            RegisterScreen<ChapterScreen>(ScreenType.Chapter, "ChapterScreen");
            RegisterScreen<TalentScreen>(ScreenType.Talent, "TalentScreen");
            RegisterScreen<EquipmentScreen>(ScreenType.Equipment, "EquipmentScreen");
            RegisterScreen<PetScreen>(ScreenType.Pet, "PetScreen");
            RegisterScreen<ContentScreen>(ScreenType.Content, "ContentScreen");
            RegisterScreen<GachaScreen>(ScreenType.Gacha, "GachaScreen");
            RegisterScreen<QuestScreen>(ScreenType.Quest, "QuestScreen");
            RegisterScreen<EventScreen>(ScreenType.Event, "EventScreen");
            RegisterScreen<SettingsScreen>(ScreenType.Settings, "SettingsScreen");
            RegisterScreen<DebugScreen>(ScreenType.Debug, "DebugScreen");
            RegisterScreen<ChapterTreasureScreen>(ScreenType.ChapterTreasure, "ChapterTreasureScreen");
        }

        private void RegisterScreen<T>(ScreenType type, string name) where T : BaseScreen
        {
            var screenGo = new GameObject(name);
            screenGo.transform.SetParent(_screenContainer, false);
            var rt = screenGo.AddComponent<RectTransform>();
            StretchFull(rt);
            var screen = screenGo.AddComponent<T>();
            screenGo.SetActive(false);
            _screens[type] = screen;
        }

        public void ShowScreen(ScreenType type)
        {
            if (!_screens.ContainsKey(type)) return;

            if (_activeScreen != null)
            {
                _activeScreen.OnScreenExit();
                _activeScreen.gameObject.SetActive(false);
            }

            _activeScreenType = type;
            _activeScreen = _screens[type];
            _activeScreen.gameObject.SetActive(true);
            _activeScreen.OnScreenEnter();
            _navBar.SetActiveScreen(type);
        }

        public void ShowPopup(GameObject popupPrefab, object data = null)
        {
            ClosePopup();

            _popupLayer.gameObject.SetActive(true);
            var popupGo = Instantiate(popupPrefab, _popupContainer);
            var rt = popupGo.GetComponent<RectTransform>();
            if (rt != null) StretchFull(rt);

            _activePopup = popupGo.GetComponent<BasePopup>();
            if (_activePopup != null)
                _activePopup.Show(data);
        }

        public void ShowPopupFromType<T>(object data = null) where T : BasePopup
        {
            ClosePopup();

            _popupLayer.gameObject.SetActive(true);
            var popupGo = new GameObject(typeof(T).Name);
            popupGo.transform.SetParent(_popupContainer, false);
            var rt = popupGo.AddComponent<RectTransform>();
            StretchFull(rt);

            _activePopup = popupGo.AddComponent<T>();
            _activePopup.Show(data);
        }

        public void ClosePopup()
        {
            if (_activePopup != null)
            {
                Destroy(_activePopup.gameObject);
                _activePopup = null;
            }

            _popupLayer.gameObject.SetActive(false);
        }

        public void Refresh()
        {
            _resourceBar.Refresh();
            _navBar.Refresh();

            if (_activeScreen != null)
                _activeScreen.Refresh();
        }

        public ScreenType ActiveScreenType => _activeScreenType;

        public void SetNavBarVisible(bool visible)
        {
            _navBarArea.gameObject.SetActive(visible);
            _screenContainer.offsetMin = new Vector2(0, visible ? 120 : 0);
        }

        public static RectTransform CreateRectTransform(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            return rt;
        }

        public static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static Image CreatePanel(Transform parent, Color color, string name = "Panel")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            StretchFull(rt);
            var image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static TextMeshProUGUI CreateText(Transform parent, string text, float fontSize, Color color, string name = "Text")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            StretchFull(rt);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            return tmp;
        }

        public static Button CreateButton(Transform parent, string label, Action onClick, string name = "Button")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            StretchFull(rt);

            var image = go.AddComponent<Image>();
            image.color = ColorPalette.ButtonPrimary;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            if (onClick != null)
                button.onClick.AddListener(() => onClick());

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            StretchFull(textRt);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 30;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return button;
        }

        public static Slider CreateSlider(Transform parent, string name = "Slider")
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 20);

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(go.transform, false);
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = ColorPalette.ProgressBarBackground;
            var bgRt = bgGo.GetComponent<RectTransform>();
            StretchFull(bgRt);

            var fillAreaGo = new GameObject("FillArea");
            fillAreaGo.transform.SetParent(go.transform, false);
            var fillAreaRt = fillAreaGo.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = new Vector2(0, 0.25f);
            fillAreaRt.anchorMax = new Vector2(1, 0.75f);
            fillAreaRt.offsetMin = new Vector2(5, 0);
            fillAreaRt.offsetMax = new Vector2(-5, 0);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillImage = fillGo.AddComponent<Image>();
            fillImage.color = ColorPalette.ProgressBarFill;
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.sizeDelta = Vector2.zero;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.targetGraphic = fillImage;

            return slider;
        }
    }
}
