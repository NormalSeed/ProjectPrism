# Previous Work Log

---

## Session: 2026-04-29 — Spirit Selection Gating & Auto-Start

### Task
Move spirit selection to MainMenu scene, make GameTest auto-start when a spirit is available, and add a debug fallback for direct play-mode entry from the editor. Keep `SpiritInventoryUI` as an in-game inventory viewer (B안).

### Problem
1. `BoardManager._generateBoardButton` was `null` after the button GameObject was deleted in a prior cleanup pass — caused NullReferenceException at `onClick.AddListener` on init.
2. `CreateNewStageAsync()` was never called automatically — the game would not start without the now-deleted button.
3. `SpiritInventoryManager` and `SpiritInventoryUI` were not registered in `BoardLifetimeScope`, so their `[Inject]` dependencies were never resolved.

### Changes (`Assets/Scripts/Puzzles/Board/BoardManager.cs`)
- Removed `[SerializeField] private Button _generateBoardButton` field and all three usages (`onClick.AddListener`, two `interactable` assignments including the early-return path).
- Added `[Header("Debug")] [SerializeField] private SpiritData _debugDefaultSpirit` — assignable in the Inspector for editor-only testing without MainMenu flow.
- Changed spirit-loading `else { InitInventory(); }` branch to `else if (_debugDefaultSpirit != null) { SetSpirit(_debugDefaultSpirit); }`.
- Added `CreateNewStageAsync().Forget();` at the end of `InitializeAsync()` — game auto-starts after spirit is loaded (from PlayerPrefs or debug fallback).

### Changes (`Assets/Scripts/Core/LifetimeScope/BoardLifetimeScope.cs`)
- Added `builder.RegisterComponentInHierarchy<SpiritInventoryManager>().As<IInventoryService>();`
- Added `builder.RegisterComponentInHierarchy<SpiritInventoryUI>();`
- Both components confirmed present in GameTest scene (instanceID 53160) before registering.

### Result
- No compilation errors (Unity console clean).
- GameTest scene auto-starts via PlayerPrefs spirit key written by `MainMenuPresenter.OnSpiritChosen()`.
- Editor testing: assign a `SpiritData` to `_debugDefaultSpirit` on BoardManager inspector to bypass MainMenu.
- `SpiritInventoryUI` remains in-game as an inventory viewer; `_startButton.onClick` still only logs (no game-start side effect needed since auto-start handles it).
