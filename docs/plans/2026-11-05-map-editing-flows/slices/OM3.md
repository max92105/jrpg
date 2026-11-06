# OM3. Objects > New object from map selection

Builds on design.md, "Key decisions" 11 and Risks (the replacement changes how the cells draw); OM1 (the selection) and OM2 (`MapSelectionCanvas`); decisions 514 and 516 (C5), and 459 and 462 (the composer's naming, the Shapes window after Save).

## Goal
Objects > **New object from map selection** (also a button in the Objects tool bar) opens the composer on the canvas the map selection makes around the base layer (the layer chosen in the tool bar, OM2): the id and category typed in the composer as for New object, every level editable, Save making the object as one undo step from the map's own tiles (no tile added), then the Shapes window. An amber line in the composer warns when the object will draw differently from the cells (decision 516): a taken layer's z at or above `MapLayers.AbovePlayerZ` ("Layer z 5 drew over the player; in an object it draws by depth.") or the base layer's z under `MapLayers.PlayerZ` ("The base layer z 0 is under the player: the object will always draw under the player."). With no selection, or a selection OM2 refuses, the status bar says why and nothing opens.

## New types
None (the partial file below is new).

## Changes
- `Objects/Composing/MapSelectionCanvas` (OM2): `public static string? DrawingWarningOf(IReadOnlyList<(int Z, int Level)> takenLayers, int baseZ)`: the warnings above, joined; null when neither applies.
- `Objects/Composing/ObjectComposerViewModel` (100): `public string? MapLayerWarning { get; init; }` (null for New object and Edit object).
- `Objects/Composing/ObjectComposerWindow.xaml` (109): an amber line bound to `MapLayerWarning`, collapsed when null (+2).
- `Objects/Composing/ObjectComposing` (93): `public ObjectComposerViewModel StartNewFromMap(ObjectCanvas canvas, string? mapLayerWarning)`: the composer for a new object on the canvas a map selection made; it shares `StartNew`'s construction through a private `StartNewOn(ObjectCanvas)` (no behavior change for New object). OM4 marks this composer as able to replace the cells.
- New partial `Shell/MainViewModel.ObjectFromMap.cs` (summary: "Objects > New object from map selection: the composer on the map's cells, and (OM4) the cells replaced by the object."):
  - `public ObjectComposerViewModel? StartObjectFromMap()`: null with the status bar's `CursorStatus` set to the reason (no selection: "Ctrl + drag a rectangle of cells on the map first (Tiles tab)."; else `MapSelectionCanvas.ProblemOf`), as `ShowPaintHint` does; otherwise, with `baseZ = Layer.Z`, `Composing.StartNewFromMap(MapSelectionCanvas.Of(layout, cells, baseZ, Catalog.Tiles), MapSelectionCanvas.DrawingWarningOf(MapSelectionCanvas.TakenLayersOf(layout, cells, baseZ), baseZ))`.
  - `public bool CanStartObjectFromMap => Composing is not null;` (the menu's `IsEnabled`; the reasons go to the status bar, so the entry needs no refresh on every selection change).
- `Shell/MainWindow.ObjectEditing.cs`: `OnNewObjectFromMapClicked` → `Compose(_viewModel.StartObjectFromMap())`; `Compose` unchanged.
- `Shell/MainWindow.xaml`: Objects menu `<MenuItem Header="New object from map selection..." Click="OnNewObjectFromMapClicked" IsEnabled="{Binding CanStartObjectFromMap}" ToolTip="Make an object of the map cells selected with Ctrl + drag in the Tiles tab: the chosen layer is level 0, the layers under and over it the levels under and over." />`; the same as a button in the Objects tool bar (+2, ends about 293).
- `Shell/ControlsHelpWindow.xaml`, Objects: "Objects > New object from map selection: the composer on the cells selected with Ctrl + drag (Tiles tab); the chosen layer becomes level 0, each layer under or over it with a tile there the next level down or up."

## Tests
In `Objects/Composing/MapSelectionCanvasTests` (OM2), one per side of each rule:
- `DrawingWarningOf_LayerAtAbovePlayerZ_Warns`; `DrawingWarningOf_LayerJustUnderAbovePlayerZ_IsNull`.
- `DrawingWarningOf_BaseUnderPlayerZ_Warns`; `DrawingWarningOf_BaseAtPlayerZ_IsNull`.

New `Shell/MainViewModelObjectFromMapTests` (the test world's forest):
- `StartObjectFromMap_NoSelection_SaysToSelectAndReturnsNull` (the `CursorStatus` text).
- `StartObjectFromMap_SelectionOfEmptyCells_SaysWhyAndReturnsNull`.
- `StartObjectFromMap_GroundSelectedAsBase_StartsTheCanvasWithTheMapsCells` (two by two on the ground layer).
- `StartObjectFromMap_BaseUnderThePlayer_CarriesTheWarning`.
- `StartObjectFromMap_Saved_MakesAnObjectOfTheMapsTilesInOneStep` (the object's tile ids are the map's; the tile count unchanged; one undo removes it).
- `ObjectComposingTests` is 261 lines: `StartNewFromMap_Canvas_StartsANewObjectOnIt` (its cells on their levels, no id yet, the warning kept) goes in `ObjectComposingLevelTests` (87).

Size: about 50 lines of code, 5 of XAML and F1; 10 tests (about 110): about 165.

## Map
`docs/map.md`, Editor `Shell` row: `MainViewModel.ObjectFromMap` (Objects > New object from map selection).

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5). On a map where a tree is painted with tiles on two layers (Decoration over Ground), click the **Tiles** tab and choose the Decoration layer in the tool bar's Layer box.
2. Ctrl + drag a rectangle around the tree. Objects > **New object from map selection...**: the composer opens with the tree on level 0 and the grass under it on level -1 (its Level box marks both); erase the grass level if you don't want it in the object (the map keeps its ground either way, decision 515).
3. Type an id (`test_tree`) and a category, press **Save**: the Shapes window opens; draw the trunk's outline, OK.
4. Objects tab: `test_tree` is in the list; place it elsewhere: it looks like the painted one.
5. Choose the Ground layer (under the player), select the same rectangle, New object from map selection: the amber line says the object will always draw under the player. Cancel.
6. Without a selection, the menu entry only writes in the status bar: "Ctrl + drag a rectangle of cells on the map first (Tiles tab)."
7. Ctrl+Z removes `test_tree` (after removing its placement first, or the undo is refused, naming the map).
