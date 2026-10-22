# E9c. Opening the shapes window from the map

Part of [the editor plan](../README.md). Builds on [E9b](E9b.md)'s window. Split from E9 in three (developer, 2026-10-21).

- **E9c. Opening the shapes window from the map** (Editor, about 120 lines)
  - **Goal:**
    - Objects > Edit shapes opens the window for the one placed object selected on the map if there is exactly one (Ctrl + drag, E2), else for the object chosen in the Objects panel.
    - A double-click on an object in the panel, or Shift + double-click on a placed object on the map in Objects mode, opens it too (developer, 2026-10-21: a plain click places the chosen object, so a plain double-click would drop a copy first).
  - **Changes:**
    - `Objects/ObjectPlacingViewModel`: `StartEditingShapes()` prefers the single selected placed object's definition; `StartEditingShapesAt(Point2D point)`: for the placed object under a map point (the double-click), or null.
    - `Shell/MainWindow`: the two double-clicks.
  - **Tests:**
    - `ObjectPlacingViewModelTests`: `StartEditingShapes_OnePlacedObjectSelected_EditsItsDefinition`, `StartEditingShapesAt_PlacedObjectUnderPoint_EditsItsDefinition`, `StartEditingShapesAt_EmptyCell_ReturnsNull`.
  - **Verify by hand:**
    - [ ] Shift + double-click a placed tree on the map: its shapes open; nothing is placed or moved. Cancel: nothing changed.
    - [ ] Select one placed house (Ctrl + drag), Objects > Edit shapes: the house's shapes open, whatever the panel has chosen.
    - [ ] Double-click `chest` in the Objects panel: its shapes open.
- **Done, with deviations:** the map gesture is Shift + double-click (developer). `StartEditingChosenShapes()` added for the panel double-click, so it edits the chosen object even while one placed object is selected; `HandleObjectButton` takes the click count. `CanEditShapes` also follows the map selection. Review: the panel double-click opens only on a listed object (not the preview, a header or the scroll bar); a Shift press and release within Windows' drag distance moves nothing (`IsPastDragDistance`), so the double-click never nudges the object; `PlacedObjectWithUid` shared.
