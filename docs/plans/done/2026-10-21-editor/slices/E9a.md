# E9a. Collision outline editing: the rules and the catalog step

Part of [the editor plan](../README.md). Design: [design.md](../design.md), "Key decisions" 7, "Tunables" (shapes window), "Legacy mapping" (`ObjectEditorWindow`); ADR 0014. Split from E9 in three (developer, 2026-10-21): the window is [E9b](E9b.md), opening it from the map is [E9c](E9c.md). Nothing is visible until E9b.

- **E9a. Collision outline editing: the rules and the catalog step** (Editor, about 260 lines)
  - **Goal:**
    - The rules of the shapes window, headless: points are added at the end of the outline, clamped into the object (a click past the edge lands on it) and rounded to the object's pixel grid (one map pixel: 1 / (cells x 48) of its size), so outlines stay on whole pixels as the game's canopy mask reads them. The last point can be removed (legacy Backspace, Ctrl+Z), or the outline cleared (an empty outline never blocks, as in Core).
    - An outline with 1 or 2 points can't be applied and says why: "The collision outline needs at least 3 points (has 2), or none."
    - Applying puts the object in the catalog draft as one catalog step (undone by the main window's Ctrl+Z, E4b; saved with Ctrl+S). The Objects panel lists the new record and keeps it chosen, so its preview and the ghost show it.
    - Zones are kept untouched (E10 edits them).
  - **New types:**
    - `Objects/ObjectShapesViewModel` (`ObservableObject`): the collision outline being edited for one object definition, and the problem that stops it being saved.
      - `ObjectShapesViewModel(ObjectDefinition definition)`.
      - `CollisionOutline` (`IReadOnlyList<Point2D>`, observable, replaced on each change), `ObjectId`, `Width`, `Height`.
      - `AddPoint(Point2D point)`: `point` in 0..1 over the object; clamped into 0..1 (the validator refuses anything outside), then rounded to the pixel grid (`MapDrawing.CellPixels` per cell).
      - `RemoveLastPoint()`, `ClearShapeCommand`.
      - `Problem` (null when the outline is empty or has 3+ points).
      - `ToDefinition()`: the definition with the edited outline; throws `InvalidOperationException` with `Problem` when there is one (as `NewMapViewModel.ToMap` does).
  - **Changes:**
    - `Catalogs/CatalogDraft.SetObject(ObjectDefinition definition)`: replaces the object with its id in `Current` (adds it if the id is new, E12b's first use); one undoable step (E4b); nothing when the record is equal.
    - `Objects/ObjectBrowserViewModel`: rebuilds `Objects` when `Current` changes and re-selects the chosen object by id.
  - **Tests:**
    - `ObjectShapesViewModelTests`:
      - `Constructor_HouseWithOutline_StartsWithItsPoints`;
      - `AddPoint_InsideObject_AppendsToCollisionOutline`;
      - `AddPoint_OutsideTheObject_ClampsToTheEdge` ((1.3, -0.2) -> (1, 0));
      - `AddPoint_BetweenPixels_RoundsToTheObjectsPixelGrid` (0.5004 on a 1-cell object -> 24 / 48);
      - `RemoveLastPoint_ThreePoints_RemovesTheNewest`;
      - `RemoveLastPoint_NoPoints_ChangesNothing`;
      - `ClearShapeCommand_OutlineWithPoints_EmptiesIt`;
      - `Problem_TwoPoints_SaysAtLeastThree`;
      - `Problem_NoPoints_IsNull`;
      - `ToDefinition_OutlineEdited_KeepsIdTilesAndZones`.
    - `CatalogDraftTests`: `SetObject_ExistingId_ReplacesItAndMarksDirty`, `SetObject_EqualRecord_StaysClean`, `Undo_AfterSetObject_RestoresTheOldOutline`.
    - `ObjectBrowserViewModelTests`: `Objects_AfterSetObject_ListsTheNewRecordAndKeepsTheSelection`.
  - **Verify by hand:** nothing to see until E9b.
- **Done, with deviations:** split from E9 in three (developer). `SetObject` counts a record whose lists hold the same points as unchanged (records compare lists by reference), so a no-op OK adds no undo step. `ObjectBrowserViewModel.Objects` became observable, rebuilt only when `Current.Objects` is another dictionary. Review: tests for the chosen object undone away and a tile change keeping the list; the comparison comment says what the with-copy compares.
