# E4a. Blocked tiles: Mark blocked

Part of [the editor plan](../README.md). Design: [design.md](../design.md), "What exists" (Core's walkability rule); decisions 114, 119; ADR 0014.

- **E4a. Blocked tiles: Mark blocked** (Editor, about 180 lines)
  - **Goal:**
    - A **Mark blocked** toggle in the Tiles panel (and Tiles > Mark blocked in the menu; decision 119, the only way to mark walkability): while it is on, a click on a sheet cell flips it between walkable and blocked instead of choosing it. A cell that isn't a tile yet becomes a blocked tile.
    - Blocked tiles carry a red cross on the sheet.
    - Catalog changes count as unsaved: the status bar says "Unsaved changes"; closing asks; switching maps doesn't ask, since the change belongs to no map and stays.
    - Ctrl+S writes the catalogs even when the map is unchanged.
    - The game blocks cells holding a blocked tile (Core's existing rule); the map shows it from E8 on.
    - **Known gap until E4b:** Ctrl+Z doesn't undo a mark; clicking the cell again does.
  - **New types:**
    - `Tiles/WalkabilityMarking` (`ObservableObject`): the Mark blocked toggle, and flipping a sheet cell's walkability through the catalog draft.
      - `WalkabilityMarking(CatalogDraft catalog, TileBrowserViewModel browser)`.
      - `IsMarkingBlocked` (observable).
      - `FlipCellAt(double x, double y)`: the cell under the pixel (`browser.CellAt`); its tile's walkability flipped, or a blocked tile added.
  - **Changes:**
    - `Catalogs/CatalogDraft.SetWalkable(int tileId, bool isWalkable)`: replaces the tile record; nothing when unchanged. A blocked tile on a new cell is `AddTile` then `SetWalkable(id, false)`; `AddTile` keeps decision 114's default.
    - `Tiles/TilesheetMarks.Draw`: blocked crosses.
    - `Tiles/TileBrowserView`: a `Marking` property set by the window (like `Images`); a sheet click goes to `Marking.FlipCellAt` while `IsMarkingBlocked`, else to `SelectCellAt`; redraws on catalog changes.
    - `Shell/MainViewModel`:
      - owns `Marking` (built with the draft and `Painting.Browser`);
      - `HasUnsavedChanges` (map or catalog dirty) feeds the status bar instead of `CurrentMap.IsDirty`;
      - `ConfirmClose` asks when either is dirty;
      - `TryOpenMap` and `TryCreateMap` still ask only about the map.
  - **Tests:**
    - `CatalogDraftTests`: `SetWalkable_ExistingTile_ReplacesItAndMarksDirty`, `SetWalkable_SameValue_StaysClean`.
    - `WalkabilityMarkingTests`:
      - `FlipCellAt_CellNotATile_AddsBlockedTile`;
      - `FlipCellAt_BlockedTile_MakesItWalkable`;
      - `FlipCellAt_OutsideWholeCells_ChangesNothing`.
    - `MainViewModelTests`:
      - `ConfirmClose_UnsavedCatalogChange_AsksTheUser`;
      - `TryOpenMap_UnsavedCatalogChangeOnly_OpensWithoutAsking`;
      - `Save_CatalogChangedMapUnchanged_WritesTilesAndClearsUnsavedChanges`.
  - **Verify by hand:**
    - [ ] On `city`, turn on Mark blocked, click wall cells: red crosses; click one again: it clears. With the toggle off, clicks choose cells again.
    - [ ] "Unsaved changes" shows; switch maps: no question, crosses still there; close: asked.
    - [ ] Ctrl+S, F5: the player can't walk onto a cell painted with a blocked tile.
