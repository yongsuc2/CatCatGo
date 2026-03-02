---
name: unity-ui-guide
description: This skill should be used when creating or modifying Unity UI code, building screens/popups with code-based UI (no prefabs), setting up ScrollRect/ScrollView, using TextMeshPro text, loading sprites, or working with layout groups (VerticalLayoutGroup, HorizontalLayoutGroup, GridLayoutGroup). Covers CatCatGo project's Presentation layer patterns and verified pitfalls.
---

# Unity Code-Based UI Guide

This skill provides verified rules and patterns for building Unity UI entirely in C# code (no prefabs/UXML). All rules below are confirmed through real bugs encountered and fixed in the CatCatGo project.

## Critical Rules (Must Follow)

### 1. ScrollRect Viewport: Use RectMask2D, Never Mask + Transparent Image

`Mask` component uses stencil buffer based on the attached Image's alpha. If the Image has alpha=0 (`Color.clear`), the stencil test fails and **all children become invisible**.

**Wrong (children invisible):**
```csharp
viewportGo.AddComponent<Image>().color = Color.clear;
viewportGo.AddComponent<Mask>().showMaskGraphic = false;
```

**Correct:**
```csharp
var viewportRt = viewportGo.GetComponent<RectTransform>();
if (viewportRt == null) viewportRt = viewportGo.AddComponent<RectTransform>();
UIManager.StretchFull(viewportRt);
viewportGo.AddComponent<RectMask2D>();
```

`RectMask2D` uses simple rect clipping — no Image component needed, no stencil issues.

### 2. TextMeshPro: No Emoji or Special Unicode

LiberationSans SDF (TMP default font) does not contain emoji or special Unicode glyphs. Using them causes `Font Atlas` warnings and renders blank squares.

**Forbidden characters (confirmed failures):**
- Emoji: `\u2764` (heart), `\u26A1` (lightning), `\u2699` (gear), `\uD83D\uDC31` (cat)
- Symbols: `\u2713` (checkmark), `\u2190` (arrow), `\u2500` (box drawing), `\u2605` (star)

**Replacement strategy:**
| Forbidden | Replacement |
|-----------|-------------|
| Checkmark | "V" |
| Arrow left | "<" |
| Line/dash | "-" |
| Star | Korean text or Image |
| Any emoji | Korean text, or display as Image/Sprite |

For skill/equipment icons, always use `Image` component with sprite from `SpriteManager`, never embed icon text in TMP strings.

### 3. Unity Object Null Check: Never Use `??` Operator

Unity overloads `==` to detect "fake null" (destroyed objects). The C# `??` operator bypasses this and only checks C# null.

**Wrong (MissingComponentException):**
```csharp
var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
```

**Correct:**
```csharp
var rt = go.GetComponent<RectTransform>();
if (rt == null) rt = go.AddComponent<RectTransform>();
```

This applies to all UnityEngine.Object derivatives: `GetComponent`, `Find`, `Resources.Load`, etc.

### 4. LayoutElement Fixed Size: Must Set flexibleHeight = 0

`LayoutElement`의 `flexibleHeight` 기본값은 `-1` (unspecified). `VerticalLayoutGroup`/`HorizontalLayoutGroup`은 unspecified인 요소에도 남은 공간을 분배할 수 있다. `preferredHeight`만 설정하면 고정 크기가 보장되지 않는다.

**Wrong (button/title expands to fill remaining space):**
```csharp
var le = go.AddComponent<LayoutElement>();
le.preferredHeight = UISize.NormalButtonHeight;
```

**Correct (guaranteed fixed size):**
```csharp
var le = go.AddComponent<LayoutElement>();
le.minHeight = UISize.NormalButtonMinHeight;
le.preferredHeight = UISize.NormalButtonHeight;
le.flexibleHeight = 0;
```

고정 크기여야 하는 모든 요소(버튼, 타이틀, 구분선 등)에 반드시 `flexibleHeight = 0`을 명시할 것. `flexibleHeight = 1`은 남은 공간을 채워야 하는 요소(스크롤 영역 등)에만 사용.

### 5. Sprite Loading: Always Provide Fallback Chain

`Resources.Load<Sprite>()` may fail even when the file exists (import settings mismatch). Always implement a fallback chain: Sprite → Texture2D → Placeholder. Never return null from sprite getters.

### 6. UI.Refresh() vs Local Refresh()

After modifying game state (save, delete, resource changes), call `UI.Refresh()` (UIManager global) to update all visible screens including ResourceBar. Calling local `Refresh()` only updates the current screen.

## Standard Patterns

### ScrollRect Setup (Verified Working Pattern)

```csharp
var scrollGo = new GameObject("ScrollView");
scrollGo.transform.SetParent(transform, false);
var scrollLe = scrollGo.AddComponent<LayoutElement>();
scrollLe.flexibleHeight = 1;

var scrollRect = scrollGo.AddComponent<ScrollRect>();
scrollRect.horizontal = false;
scrollRect.vertical = true;

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

var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

scrollRect.content = contentRt;
scrollRect.viewport = viewportRt;
```

### Button Creation Pattern

```csharp
var btnGo = new GameObject(label);
btnGo.transform.SetParent(parent, false);
var le = btnGo.AddComponent<LayoutElement>();
le.minHeight = UISize.NormalButtonMinHeight;
le.preferredHeight = UISize.NormalButtonHeight;
le.flexibleHeight = 0;

var bg = btnGo.AddComponent<Image>();
bg.color = buttonColor;
var btn = btnGo.AddComponent<Button>();
btn.targetGraphic = bg;
btn.onClick.AddListener(() => onClick());

var textGo = new GameObject("Text");
textGo.transform.SetParent(btnGo.transform, false);
UIManager.StretchFull(textGo.GetComponent<RectTransform>());
var tmp = textGo.AddComponent<TextMeshProUGUI>();
tmp.text = label;
tmp.color = Color.white;
tmp.alignment = TextAlignmentOptions.Center;
tmp.raycastTarget = false;
```

### Icon Display in Layout (Image, Not TMP Text)

When displaying icons (skill, equipment, pet), always use `Image` component with `SpriteManager`. Never put icon characters (emoji) in label strings — use a separate Image alongside the text.

## UI Sizing System

All UI element sizes are managed through `UISize` (`Presentation/Utils/UISize.cs`).

Size properties per element type:
- **MinHeight** — LayoutElement.minHeight (layout system enforces)
- **Height** — LayoutElement.preferredHeight (default/preferred)
- **MaxHeight** — code-level upper bound (referenced when element may grow)

Currently defined types:
- **NormalButton** — standard button used in popups, dialogs, screen actions

New element types should follow the same Min/Height/Max pattern.

All colors are managed through `ColorPalette` (`Presentation/Utils/ColorPalette.cs`).

## Quick Checklist Before Completing UI Code

1. All ScrollRect viewports use `RectMask2D` (not `Mask`)
2. No emoji or special Unicode in any TMP text
3. No `??` with Unity objects
4. Fixed-size LayoutElements have `flexibleHeight = 0` explicitly set
5. All sprite getters have fallback (never return null)
6. State-changing actions call `UI.Refresh()` not local `Refresh()`
7. Button heights use `UISize` constants (not hardcoded values)
8. Icons displayed as `Image` components, not TMP text
9. Colors reference `ColorPalette` constants

## Additional Resources

For layout hierarchy and element relationships, consult:
- **`references/layout-reference.md`** — Canvas hierarchy, popup structure, element types
