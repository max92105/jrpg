# OM4. Replace the map cells with the new object

Builds on design.md, "Key decisions" 12 and Risks (the replacement changes how the cells draw); README guiding rule on undo; decision 515 (Q11's answer and C4). Needs OM3.

## Goal
When the composer was opened from the map, it shows a **Replace the cells on the map with the object** check box, ticked. Save then makes the object (one catalog step) and, with the box ticked, erases on the base layer and each taken layer over it (levels 0 and up; the layers under the base are never erased, so the ground never leaves a hole) the rectangle's cells that the saved object still holds: a map cell goes only where the object has the same sheet cell at that place on that layer's level (C4: a cell erased or repainted in the composer stays on the map). It then places the object at the rectangle's top-left on the base layer (`baseZ`, the layer chosen when the composer opened), as one map step, and selects it in Objects mode. Ctrl+Z undoes the replacement first (the cells come back, the placement goes), then the object. Unticked, the map is left as it was (OM3's behavior). The composer from New object or Edit object never shows the box.

## New types
- `Editor/Maps/Editing/ReplaceCellsWithObjectEdit` (`internal sealed record ReplaceCellsWithObjectEdit(IReadOnlyList<SetTilesEdit> Erased, PlacedObject Placed) : IMapEdit`): erases cells on some layers and places an object, as one step. Apply: each erase, then the placement; revert in reverse (`SetTilesEdit.Revert`, then removing the last object, as `PlaceObjectEdit`).

## Changes
- `Objects/Composing/ObjectComposerViewModel` (about 103 after OM3, ends about 112): `public bool CanReplaceMapCells { get; init; }` (false for New object and Edit object) and `[ObservableProperty] public partial bool IsReplacingMapCells { get; set; }` (true at the start when it can).
- `Objects/Composing/ObjectComposing.StartNewFromMap` (OM3) sets both on the composer it returns.
- `Shell/MainViewModel.ObjectFromMap.cs`:
  - `StartObjectFromMap` keeps what the apply needs in a private record `MapSelectionSource(MapCellRectangle Cells, int BaseZ, IReadOnlyList<(int Z, int Level)> TakenLayers)` (OM2's `TakenLayersOf`, read once).
  - `public bool TryApplyObjectFromMap(ObjectComposerViewModel composer)`: `Composing.TryApply(composer)`; false stops; then, when `IsReplacingMapCells`, one `ReplaceCellsWithObjectEdit` on the shown map: per taken layer at level 0 or higher, a `SetTilesEdit` erasing each rectangle cell whose map tile's sheet cell equals the saved object's tile at that place on the layer's level (`ObjectCanvas.Cells` of the saved canvas, keyed by `CellOnLevel`); and a `PlacedObject` with the next uid of the new object (`PlacedObjectUids.Next`), its position the rectangle's top-left, `Z = BaseZ`. Then `Placing.Select([uid])`, `Mode = EditingMode.Objects`, the map selection cleared.
- `Shell/MainWindow.ObjectEditing.cs`: `Compose` takes the apply function as a second parameter (`Func<ObjectComposerViewModel, bool>`), New and Edit passing `Composing.TryApply`, the map entry `TryApplyObjectFromMap`.
- `Objects/Composing/ObjectComposerWindow.xaml` (about 111 after OM3): `<CheckBox Content="Replace the cells on the map with the object" IsChecked="{Binding IsReplacingMapCells}" Visibility="{Binding CanReplaceMapCells, Converter=...}" />` (+3).
- `Shell/ControlsHelpWindow.xaml`, Objects: the New object from map selection line adds "Replace the cells (ticked): Save also erases the cells the object kept and places it there on the chosen layer; Ctrl+Z undoes the replacement, then the object."

## Tests
New `Maps/Editing/ReplaceCellsWithObjectEditTests`:
- `Apply_TwoLayers_ErasesTheirCellsAndPlacesTheObject`.
- `Revert_PutsTheCellsBackAndRemovesTheObject` (the draft equal to before).

In `Shell/MainViewModelObjectFromMapTests` (OM3):
- `TryApplyObjectFromMap_Replacing_ErasesTheBaseAndOverLayersCells`.
- `TryApplyObjectFromMap_Replacing_LeavesTheLayersUnderTheBase` (the ground on level -1 keeps its map tiles).
- `TryApplyObjectFromMap_CellErasedInTheComposer_StaysOnTheMap` (C4: one cell erased on the canvas before Save keeps its map tile).
- `TryApplyObjectFromMap_CellRepaintedInTheComposer_StaysOnTheMap` (another sheet cell stamped there: the map's tile isn't the object's, so it stays).
- `TryApplyObjectFromMap_Replacing_PlacesTheObjectAtTheSelectionOnTheBaseLayer` (position = the top-left cell, `Z` = `baseZ`, selected, Objects mode).
- `TryApplyObjectFromMap_ThenUndoTwice_UndoesTheReplacementThenTheObject` (the first undo leaves the object in the catalog, the second removes it; neither refused).
- `TryApplyObjectFromMap_NotReplacing_LeavesTheMap` (no map step).
- `TryApplyObjectFromMap_ComposerWithAProblem_ChangesNothing`.
- `ObjectComposingLevelTests: StartNew_FromTheTilesPanel_CannotReplaceMapCells`; `StartNewFromMap_CanReplaceAndIsTicked`.

Size: about 75 lines of code (the edit 15, the partial 40, the composer 8, the window 8, XAML 4); 11 tests (about 135): about 210.

## Map
`docs/map.md`, Editor `Maps / Maps/Editing` row: `ReplaceCellsWithObjectEdit`; `Composing/ObjectComposerWindow`: the "Replace the cells" box from the map.

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5). As in OM3's check: Decoration layer chosen, Ctrl + drag around a painted tree, Objects > New object from map selection.
2. The composer shows **Replace the cells on the map with the object**, ticked. Leave level -1 (the grass) as it is: the replacement never erases the layers under the base. Name it, **Save**, draw its outline in the Shapes window, OK.
3. On the map the tree looks the same and the grass under it is still the map's; the Objects tab is open and the tree is selected (cyan). Turn on Collision (Ctrl+H): the tree's outline is green; the cells under it no longer show the old blocked marks of its tiles.
4. Ctrl+Z: the tree's painted cells are back and the placement gone. Ctrl+Z again: the object leaves the list.
5. Ctrl+Y twice: the object and the replacement come back.
6. Ctrl+S, start the game, walk up to the tree: the player is blocked by its outline and drawn in front of or behind it by depth, as other objects are (an above-player zone drawn in the Shapes window puts its top over the player).
7. Objects > New object (not from the map): no Replace box.

- **Done, with deviations:** the replacement erases only the base layer and the layers over it (decision 515 as amended at approval: the ground under the base stays), so the first test is `..._ErasesTheBaseLayersCells` and `..._LeavesTheLayersUnderTheBase` its pair. What the composer opened from is a private `MapSelectionSource` record; the erasing is `ErasedCells` and a per-layer `ErasedOn`. A layer the replacement empties is dropped from the saved map, as any empty layer (the edit test says so). No capture (Mana Seed art). Review: a test with the ground chosen erases the layer over it too (level 1), and one with a base z that has no layer on the map erases nothing; only the map's own layers are read for the erasing.
