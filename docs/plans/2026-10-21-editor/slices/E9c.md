# E9c. Opening the shapes window from the map

Part of [the editor plan](../README.md). Builds on [E9b](E9b.md)'s window. Split from E9 in three (developer, 2026-10-21).

- **E9c. Opening the shapes window from the map** (Editor, about 120 lines)
  - **Goal:**
    - Objects > Edit shapes opens the window for the one placed object selected on the map if there is exactly one (Ctrl + drag, E2), else for the object chosen in the Objects panel.
    - A double-click on an object in the panel, or on a placed object on the map in Objects mode, opens it too.
  - **Changes:**
    - `Objects/ObjectPlacingViewModel`: `StartEditingShapes()` prefers the single selected placed object's definition; `StartEditingShapesAt(Point2D point)`: for the placed object under a map point (the double-click), or null.
    - `Shell/MainWindow`: the two double-clicks.
  - **Tests:**
    - `ObjectPlacingViewModelTests`: `StartEditingShapes_OnePlacedObjectSelected_EditsItsDefinition`, `StartEditingShapesAt_PlacedObjectUnderPoint_EditsItsDefinition`, `StartEditingShapesAt_EmptyCell_ReturnsNull`.
  - **Verify by hand:**
    - [ ] Double-click a placed tree on the map: its shapes open. Cancel: nothing changed.
    - [ ] Select one placed house (Ctrl + drag), Objects > Edit shapes: the house's shapes open, whatever the panel has chosen.
    - [ ] Double-click `chest` in the Objects panel: its shapes open.
