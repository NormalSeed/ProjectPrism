# Previous Work Log

---

## Session: Phase C Implementation

### Goal
Implement Phase C: ItemInventoryManager DI wiring, DungeonMap ↔ RunManager flow, and auth → MainMenu scene transition.

### Changes Made

**1. BoardLifetimeScope.cs**
- Added `builder.RegisterComponentInHierarchy<ItemInventoryManager>().As<IItemInventoryService>();`

**2. GameTest.unity**
- Added root GameObject "ItemInventoryManager" (fileID 2200000010) with Transform (2200000011) and MonoBehaviour (2200000012, script GUID `43e1037109d721b4b8c6ed97514b9f04`)
- Added fileID 2200000011 to SceneRoots.m_Roots

**3. GameManager.cs**
- `Start()`: subscribe `_dungeonMapService.OnRoomEntered += OnRoomEntered`; replaced lambda with method group for `OnBoardClear`
- Added `OnDestroy()`: unsubscribes both events
- Added `private void OnRoomEntered(RoomType _) => RefreshBoard()`
- `ReportMonsterDefeated()`: after `ClearCurrentRoom()`, checks `IsFloorComplete`; if true calls `_runService.AdvanceFloor()` then `_dungeonMapService.GenerateFloor(_runService.CurrentRun?.Floor ?? 1)`

**4. NewUserSetupPresenter.cs**
- Removed `IInventoryService` injection entirely (was causing MainMenu DI conflict due to `SpiritInventoryManager` requiring `IBoardService`)
- Replaced `_inventoryService.LoadOwnedSpirits(...)` with `PlayerPrefs.SetString(GameConsts.SelectedSpiritKey, _starterSpirit.name)`
- Replaced hardcoded `"IsNewUser"` string with `GameConsts.IsNewUserKey`

**5. MainMenuLifetimeScope.cs**
- Added `builder.RegisterComponentInHierarchy<NewUserSetupPresenter>();`

### Result
Unity console: 0 errors, 0 warnings after compile. All Phase C tasks complete.
