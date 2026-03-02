# Layout Reference

## Canvas Hierarchy

```
UICanvas (ScreenSpaceOverlay, ScaleWithScreenSize, match height)
├── SafeAreaContainer
│   ├── ResourceBarArea (top)
│   ├── ScreenContainer (middle, between bars)
│   ├── NavBarArea (bottom)
│   └── PopupLayer (full overlay)
│       ├── PopupOverlay (dim background, click to close)
│       └── PopupContainer (centered, with margin on all sides)
```

## Element Type Size Pattern

Each UI element type defines three constants in `UISize`:
- `{Type}MinHeight` — minimum constraint
- `{Type}Height` — default/preferred height
- `{Type}MaxHeight` — upper bound

### Currently Defined Types

| Type | Constant Prefix | Purpose |
|------|----------------|---------|
| NormalButton | `NormalButton` | Standard buttons in popups, dialogs, actions |

### Adding New Types

Follow the same pattern:
```csharp
public const float {Type}MinHeight = ...;
public const float {Type}Height = ...;
public const float {Type}MaxHeight = ...;
```

## Color System

All colors live in `ColorPalette` (`Presentation/Utils/ColorPalette.cs`).

### Color Categories

| Category | Constants | Purpose |
|----------|-----------|---------|
| Grade | `GradeCommon` ~ `GradeMythic` | Equipment grade indication |
| Talent | `TalentDisciple` ~ `TalentHero` | Talent grade indication |
| Layout | `Background`, `Card`, `CardLight` | Panel/container backgrounds |
| Text | `Text`, `TextDim` | Primary, secondary text |
| Currency | `Gold`, `Gems` | Resource type colors |
| Combat | `Hp`, `Rage`, `Heal`, `Crit` | Battle-related indicators |
| Button | `ButtonPrimary`, `ButtonSecondary` | Action buttons |
| Nav | `NavBarBackground`, `NavBarActive`, `NavBarInactive` | Bottom navigation |
| Overlay | `PopupOverlay` | Popup dimming layer |
| Progress | `ProgressBarBackground`, `ProgressBarFill` | Progress bars |

### Popup Structure

PopupContainer is positioned with percentage-based anchors relative to SafeAreaContainer, leaving margin on all sides. Popups stretch to fill the container.

### Layout Usage

- Scrollable content: `LayoutElement.flexibleHeight = 1` on ScrollRect container
- Fixed elements (title, buttons): `LayoutElement.preferredHeight` from UISize
- Content sizing: `ContentSizeFitter` with `PreferredSize` on scroll content
