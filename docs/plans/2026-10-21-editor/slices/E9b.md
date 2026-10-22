# E9b. The object shapes window

Part of [the editor plan](../README.md). Design: [design.md](../design.md), "Key decisions" 7, "Tunables" (shapes window). Builds on [E9a](E9a.md)'s rules. Split from E9 in three (developer, 2026-10-21).

- **E9b. The object shapes window** (Editor, about 220 lines)
  - **Goal:**
    - Objects > **Edit shapes...** opens the **object shapes window** for the object chosen in the Objects panel (the menu entry is disabled with none chosen). Opening it from the map is E9c.
    - The window shows the object's picture, fitted into a 560 px box, with its collision outline drawn over it in the overlay's green (E8).
    - A click on the picture adds a point (E9a's rules). **Backspace** or **Ctrl+Z** removes the last point; **Clear shape** empties the outline.
    - With 1 or 2 points **OK** is disabled and the window says why. **Cancel** (or Esc) leaves the object as it was.
    - OK applies the outline (E9a): "Unsaved changes"; the map, its canopies and the collision overlay redraw at once.
    - Dragging, inserting and removing a chosen point come in E11a; snapping and zoom in E11b.
  - **New types:**
    - `Objects/ObjectShapesWindow` (XAML + code-behind, no rules): a `Viewbox` holding a grid of the object's size in map pixels (`Width * MapDrawing.CellPixels`), with the picture (`MapDrawing.DrawObject`) and a `Canvas` for the outline; turns a click into a 0..1 point (`position / (cells * CellPixels)`) and keys into the view model's calls; OK sets `DialogResult` when `Problem` is null.
  - **Changes:**
    - `Objects/ObjectPlacingViewModel`: `StartEditingShapes()`: an `ObjectShapesViewModel` for the chosen object, else null; `ApplyShapes(ObjectShapesViewModel edited)`: `SetObject(edited.ToDefinition())`.
    - `Maps/CollisionDrawing`: its outline stroke and fill brushes become `internal static` so the shapes window draws with the same colors (one place per tunable).
    - `Shell/MainWindow`: the menu entry, opening the window and calling `ApplyShapes` on OK. Controls help (F1): a "Shapes window" section.
  - **Tests:**
    - `ObjectPlacingViewModelTests`: `StartEditingShapes_NothingChosenOrSelected_ReturnsNull`, `ApplyShapes_ChangedOutline_PutsTheObjectInTheCatalog`.
    - `MainViewModelTests`: `Save_ObjectOutlineChanged_LoadsBackWithTheNewOutline` (saves into a temporary content copy, loads it with `ContentCatalogLoader`).
  - **Verify by hand:**
    - [ ] Choose `house_1`, Objects > Edit shapes: the picture and its green outline show; it fills the box.
    - [ ] Clear, click four corners of the walls: the outline follows; Backspace removes the last; with 2 points OK is greyed with the reason.
    - [ ] OK: "Unsaved changes"; with Show collision on (E8), every placed `house_1` shows the new outline; Ctrl+Z in the main window: the old outline is back; Ctrl+Y: the new one.
    - [ ] Cancel after edits: nothing changed.
    - [ ] Ctrl+S; `git diff` shows only `house_1`'s `collisionPolygon` in `objects.json`; F5: the player is blocked by the new outline.
