# E4b. Ctrl+Z undoes catalog changes

Part of [the editor plan](../README.md). Design: [design.md](../design.md), "Key decisions" 8 (one undo order); decision 117; ADR 0014. First catalog change to undo: E4a's blocked marks.

- **E4b. Ctrl+Z undoes catalog changes** (Editor, about 300 lines)
  - **Goal:**
    - Ctrl+Z (and Edit > Undo) undoes the newest change, whether it went to the open map or to the catalogs; Ctrl+Y redoes in the same order. A blocked mark is undone like a paint stroke.
    - Catalog steps survive switching maps (the catalogs belong to no map); the closed map's steps don't (as today: a map's history is dropped when another opens).
    - A tile made by painting (E3d) is not a step of its own: undoing the paint keeps it. A blocked tile made by a mark on a new cell is part of that mark's step: undoing the mark removes it.
    - An undo that would remove a tile still used, by the open map (unsaved content included), a saved map or an object, is refused; the status line names the user: "Can't undo: tile 284 is used on map forest."
    - The unsaved state follows undo: undoing back to the saved catalog clears "Unsaved changes" for the catalogs.
    - Every later catalog change (cuts, shapes, new objects, NPCs) is a step through the same path with no extra code in those slices beyond a test each.
  - **New types:**
    - `Catalogs/CatalogEdit` (record): one catalog change that can be undone and redone: two functions from catalog to catalog, and the text naming it ("Mark tile 57 blocked").
    - `Catalogs/CatalogHistory`: the undo and redo stacks of catalog edits.
    - `Catalogs/CatalogUsage` (static, pure): which maps and objects use a tile (and, from E6, a sheet's tiles), in a catalog plus the open map's unsaved layout.
    - `Shell/UndoJournal`: the order in which steps went to the open map or to the catalogs, so Undo and Redo go to the newest.
  - **Changes:**
    - `Catalogs/CatalogDraft`: `SetWalkable` and the mark's add-then-block go through a private `Do(CatalogEdit)`; `Undo()`, `Redo()`, `CanUndo`, `CanRedo`; a `StepRecorded` event; dirty becomes "differs from the saved version" (a version number per step, remembered by `MarkSaved`).
    - `Maps/MapViewModel`: a `StepRecorded` event when `Do` or a finished stroke records a step.
    - `Shell/MainViewModel`: `Undo` and `Redo` commands go through the journal; before a catalog undo, `CatalogUsage` checks what it would remove; the journal drops map entries when another map opens.
  - **Tests:**
    - `CatalogHistoryTests`: `Undo_AfterOneEdit_ReturnsIt`, `Redo_AfterUndo_ReturnsTheSameEdit`, `Push_AfterUndo_ClearsRedo`.
    - `CatalogDraftTests`: `Undo_AfterSetWalkable_RestoresTheTile`, `Undo_BackToSavedVersion_IsClean`, `Undo_NothingDone_ChangesNothing`.
    - `UndoJournalTests`: `Undo_MapStepThenCatalogStep_UndoesTheCatalogFirst`, `Undo_AfterAnotherMapOpened_SkipsTheClosedMapsSteps`, `Redo_AfterANewStep_HasNothing`.
    - `CatalogUsageTests`: `UsersOfTile_TileOnSavedMapAndOpenMap_ListsBoth`, `UsersOfTile_UnusedTile_IsEmpty`.
    - `MainViewModelTests`: `Undo_BlockedTileMarkThenPaint_UndoesThePaintFirst`, `Undo_NewBlockedTileUsedOnAnotherMap_RefusesAndNamesTheMap`.
  - **Verify by hand:**
    - [ ] Mark a tile blocked, paint a stroke, Ctrl+Z twice: the stroke goes, then the cross; Ctrl+Y twice brings both back.
    - [ ] Mark a tile blocked, switch maps, Ctrl+Z: the cross goes (the step survived the switch).
    - [ ] Mark a new cell blocked, paint it, save, open another map, Ctrl+Z: refused, naming the first map.
    - [ ] Mark then undo: "Unsaved changes" clears when nothing else is unsaved.
  - **If it grows past about 300 lines:** split the refusal (`CatalogUsage` and its check) into a slice right after; until then an undo that removes a used tile is caught by the save's validation (E3c), which names the map.
