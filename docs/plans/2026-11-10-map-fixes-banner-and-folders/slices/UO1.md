# UO1. Ctrl + click again selects the next object under the point

Core + Editor, about 215 lines (Core: one key method). Builds on DO1, DO2 (`MapDrawOrder`, `DrawOrderKey.ForCanopy`) and design.md, "Key decisions" 5. Decisions 523, 524 (Q5).

## Goal
An object covered by another, on any layer, can be selected, moved and edited: Ctrl + click on the same spot again selects the next object under it, in the order things are drawn (topmost first, wrapping), where a roof's above-player zone counts as the roof drawn over everything under the canopy layer; the Placed object panel's **Next under** button does the same; Shift + drag grabs a selected object under the mouse before the one drawn over it. With the object selected, Delete, the panel (position, layer, Edit events...) and Objects > Edit shapes already work on it.

## Changes
- `Core/Maps/MapDrawOrder`: `public static DrawOrderKey CanopyKeyOf(PlacedObject placed, ObjectDefinition definition, int order)` (the key `Fill` gives the object's canopy part; `Fill` calls it, no copy).
- `Maps/Editing/PlacedObjectHits` (56 lines, ends about 95):
  - `public static IReadOnlyList<PlacedObject> AllAt(MapLayout map, Point2D point, IReadOnlyDictionary<string, ObjectDefinition> definitions)`: the objects covering the point (today's `Covers`), topmost first. An object whose above-player zones hold the point (`Polygon.Contains` on each zone scaled to the object's cells, the scaling `CollisionShapes.OutlineOf` already does for outlines: reuse it or the same helper, no third copy) ranks by `MapDrawOrder.CanopyKeyOf`, else by `SpriteKeyOf`.
  - `TopAt` becomes `AllAt(...).FirstOrDefault()`.
  - `public static PlacedObject? NextUnder(MapLayout map, Point2D point, string? selectedUid, IReadOnlyDictionary<string, ObjectDefinition> definitions)`: the object after `selectedUid` in `AllAt`, the first when `selectedUid` isn't there or is the last (wrap).
- `Objects/ObjectPlacingViewModel` (260, ends about 275):
  - `SelectObjectAt(point)`: with exactly one object selected and covering the point, selects `NextUnder`; otherwise the top one, as today. Remembers the point (`_lastPickPoint`).
  - `[ObservableProperty] PickNote` ("bench_2 selected: 2 of 3 objects here"; empty when one or none), set by `SelectObjectAt` and cleared when the selection changes otherwise.
  - `[RelayCommand(CanExecute)] SelectNextUnder()`: `SelectObjectAt(_lastPickPoint)`; enabled while the last pick found more than one object.
  - `public PlacedObject? GrabbedObjectAt(Point2D point)`: the selected object covering the point first (topmost of those), else `ObjectAt(point)`.
- `Objects/PlacedObjectPanel.xaml` (47 lines): a **Next under** button and, under the panel's title, a `TextBlock` showing `PickNote` (not the status bar: `CursorStatus` changes on every mouse move, and `MainWindow.xaml` has no room). The panel's DataContext is `Placing.Editing`, so both bind through the Objects tab's DataContext (`{Binding DataContext.Placing.SelectNextUnderCommand, RelativeSource={RelativeSource AncestorType=TabItem}}`), never through a forwarding member on `PlacedObjectEditing`. Tooltip: "Selects the next object under the last Ctrl + click (or Ctrl + click there again)."
- `Shell/MainWindow.ObjectEditing.cs`: the Shift + drag branch calls `Placing.GrabbedObjectAt(point)` in place of `Placing.ObjectAt(point)` (no new branch: `HandleObjectButton` keeps its complexity).
- The Objects tab's tooltip (`MainWindow.xaml`, same line): "...Ctrl + click selects (again: the next one under)...". `ControlsHelpWindow.xaml`: one line.

## Tests
`Editor.Tests/Maps/Editing/PlacedObjectHitsTests` (add to the existing file if under 250 lines, else `PlacedObjectHitsStackTests`):
- `AllAt_ThreeOverlapping_TopmostFirst` (listed bottom first in the layout).
- `AllAt_HigherLayerListedFirst_ComesFirst` (layer beats base).
- `AllAt_VillagerOnLayer1UnderAMinus4HousesRoof_HouseFirst` (the point inside the roof's above-player zone: the house's canopy is drawn over the villager, so the house comes first; listed villager last).
- `AllAt_VillagerOnLayer1BesideTheRoofZone_VillagerFirst` (the same objects, a point on the house's wall outside its zone: the villager, on the higher layer, first).
- `NextUnder_SelectedTop_GivesTheSecond`.
- `NextUnder_SelectedLast_WrapsToTheTop`.
- `NextUnder_SelectedNotThere_GivesTheTop`.
`Editor.Tests/Objects/ObjectPlacingSelectionTests` (new; `ObjectPlacingViewModelTests` is at 198 and this adds about 90):
- `SelectObjectAt_AgainOnTheSameSpot_SelectsTheNextUnder`.
- `SelectObjectAt_AfterTheLast_WrapsToTheTop`.
- `SelectObjectAt_SelectedObjectNotUnderThePoint_SelectsTheTop`.
- `PickNote_ThreeObjectsHere_SaysTheirCountAndPlace` ("2 of 3").
- `PickNote_OneObjectHere_IsEmpty`.
- `SelectNextUnderCommand_OneObjectAtTheLastPick_CannotExecute` and `SelectNextUnderCommand_TwoObjectsAtTheLastPick_CanExecute` (the event raised, `.claude/rules/tests.md`).
- `GrabbedObjectAt_SelectedObjectCovered_GivesTheSelected`.
- `GrabbedObjectAt_NothingSelectedThere_GivesTheTop`.

## Verify by hand
1. Objects tab; place a bench, then a tree over it on the same layer, its base lower on screen (the tree covering the bench).
2. Ctrl + click where both overlap: the tree is selected (cyan frame), the panel says "1 of 2 objects here".
3. Ctrl + click again on the same spot: the bench is selected, "2 of 2". Click **Next under** in the panel: back to the tree.
4. With the bench selected, Shift + drag on the overlap: the bench moves, not the tree; Ctrl+Z puts it back.
5. With the bench selected, the panel's Edit events... opens the bench's events; Delete removes the bench; Ctrl+Z.
6. Place an NPC (layer 1) under the roof of a house on -4: Ctrl + click on the roof over the NPC selects the house first, then the NPC.
7. F1: the line for Ctrl + click again is there.
