# Previous Work Log

---

## Session: 2026-04-28 — BottomUIPanel Overflow Prevention (BoardBG + GamePlayHUD)

### Task
Prevent all elements inside BoardBG (BoardManager) and GamePlayHUD from overflowing BottomUIPanel boundaries across any aspect ratio or screen size.

### Problem
`UpdateBoardLayOut()` and `UpdateHUDLayout()` only computed cell size from the rect **width**. On shorter screens (wide aspect ratios) the grid cells grew taller than the available height, causing tiles and HUD icons to overflow vertically out of BottomUIPanel.

### Changes (`Assets/Scripts/Puzzles/Board/BoardManager.cs`)
- Modified `UpdateBoardLayOut()`: now computes both `cellSizeByWidth` and `cellSizeByHeight` and uses `Mathf.Min` of the two, ensuring cells never overflow in either axis.
- Height formula: `(boardHeight - totalVPadding - totalVSpacing) / _gridSize` (same symmetry as width).
- Added `finalCellSize <= 0` guard to skip degenerate layouts.

### Changes (`Assets/Scripts/Puzzles/Board/GamePlayHUD.cs`)
- Added `_lastHUDRectSize` (Vector2) field to track last known rect size.
- Added `Update()`: calls `UpdateHUDLayout()` only when `_gameHUDRect.rect.size` changes — mirrors BoardManager pattern.
- Added `Canvas.ForceUpdateCanvases()` in `InitializeAsync` before the first `UpdateHUDLayout()` call.
- Modified `UpdateHUDLayout()`: now computes `cellSizeByWidth` and `cellSizeByHeight` and uses `Mathf.Min`; height formula for single-row HUD: `hudHeight - totalVPadding` (no row-spacing term needed).
- Added `hudWidth <= 0` and `finalCellSize <= 0` guards.

---

## Session: 2026-04-28 — Tile Auto-Sizing Refactor for New CanvasScaler Environment

### Task
Refactored `BoardManager.UpdateBoardLayOut()` (tile auto-sizing on `UICanvas > BottomUIPanel > BoardLayout > BoardBG`) to work correctly with the new global `UIScaleInitializer` + `CanvasScaler (ScaleWithScreenSize / Expand)` environment.

### Problem
`UpdateBoardLayOut()` was called only once after a single `NextFrame` wait — too early for `UIScaleInitializer` to have applied `CanvasScaler` settings, and with no mechanism to re-run on screen or layout changes.

### Changes (`Assets/Scripts/Puzzles/Board/BoardManager.cs`)
- Added `_lastBoardRectSize` (Vector2) field to track the board's last known rect size.
- Added `Update()`: compares `_boardRect.rect.size` against `_lastBoardRectSize` each frame; calls `UpdateBoardLayOut()` only when the size changes — covers safe-area, orientation, and VerticalLayoutGroup-driven shifts.
- Added `Canvas.ForceUpdateCanvases()` before the initial `UpdateBoardLayOut()` in `InitializeAsync` — ensures CanvasScaler and layout groups have committed values before first tile-size calculation.
- Added `boardSize <= 0` guard in `UpdateBoardLayOut()` and updates `_lastBoardRectSize` inside it to prevent re-entrance.
- `IBoardService` and all callers are unaffected; `UpdateBoardLayOut()` signature unchanged.

---

## Session: 2026-04-28 — Global UI Auto-Scaling System

### Task
Implemented a global, resolution-aware UI auto-scaling system that applies to all scenes automatically without requiring per-scene setup.

### Files Created
- `Assets/Scripts/UI/UIScaleSettings.cs` — new: ScriptableObject holding `_referenceResolution` (Vector2, default 1080×1920) and `_matchWidthOrHeight` (float 0~1, default 1.0). Create asset via: Right-click → Create → Project Prism → UI Scale Settings.
- `Assets/Scripts/UI/UIScaleInitializer.cs` — new: static class with `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`; subscribes to `SceneManager.sceneLoaded` and configures every screen-space `CanvasScaler` in each loaded scene. WorldSpace canvases are skipped. Falls back to (1080×1920, matchHeight=1) if no asset is found in Resources.

### How It Works
1. `UIScaleInitializer.Initialize()` is called automatically by Unity before any scene loads.
2. On every `SceneManager.sceneLoaded` event, `OnSceneLoaded` finds all `Canvas` objects in the scene.
3. WorldSpace canvases are skipped; all screen-space canvases have their `CanvasScaler` set to `ScaleWithScreenSize` with the configured reference resolution and match value.
4. Settings are driven by `Assets/Resources/UIScaleSettings.asset`; if the asset is missing, hardcoded defaults apply.

### Pending (Manual Unity Editor steps)
1. Right-click in Project window → **Create → Project Prism → UI Scale Settings**
2. Name the asset exactly **`UIScaleSettings`**
3. Move the asset to **`Assets/Resources/`**
4. Set `Reference Resolution` and `Match Width Or Height` as needed
5. Enter Play Mode to verify all scenes scale correctly
