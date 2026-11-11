# UO2. Dim and lock other layers

Editor, about 150 lines. Builds on UO1 (`PlacedObjectHits.AllAt`) and DO2 (a canopy belongs to its object's layer). Decisions 522, 525 (Q6).

## Goal
While **Dim and lock other layers** is ticked, the objects on other layers are faint and ignore every click: Ctrl + click and Ctrl + drag selection, Shift + drag, right click, Delete under the mouse, Shift + double-click, and Events mode's click. An object under another layer's object is then reached with a plain Ctrl + click once its layer is chosen.

## Changes
- `Maps/LayerChoiceViewModel`: `AreOtherLayersDimmed` keeps its name (its bindings and the drawing read it) and its summary says "drawn faint and locked: clicks reach only the chosen layer's objects (decision 525)"; a query `public bool Accepts(int z) => !AreOtherLayersDimmed || z == Z;`.
- `Objects/ObjectPlacingViewModel` (ends about 281): one private `HitsAt(point)` = `PlacedObjectHits.AllAt(...)` filtered by `_layer.Accepts(placed.Z)`; `ObjectAt`, `SelectObjectAt` (UO1's cycle over the filtered list), `GrabbedObjectAt` read it; `SelectObjectsIn` filters the rectangle's objects the same way.
- `Events/EventEditingViewModel`: the constructor takes the `LayerChoiceViewModel` (3 parameters); `StartEditingAt(point)` takes the first of `AllAt` the layer accepts.
- `Shell/MainViewModel.cs`: the `new EventEditingViewModel(Content, _openMaps, Layer)` line (no line added).
- `Shell/MainWindow.xaml`: the tool bar's box and the View menu entry read "Dim and lock other layers", with the tooltip "Other layers are drawn faint and ignore clicks; choose the layer of what you want to reach." (same lines, the file stays at its count).
- `Shell/MainWindow.EventEditing.cs`: the Events tab's text (in `MainWindow.xaml`, same line) and `ControlsHelpWindow.xaml` name the lock.
- A click that hits only locked objects: `ObjectPlacingViewModel.PickNote` (UO1) says "Locked: only layer z {Z} takes clicks" so a forgotten tick is visible.

## Tests
`Editor.Tests/Objects/ObjectPlacingLockTests` (new):
- `ObjectAt_LockedAndCoveredByAnotherLayer_GivesTheChosenLayersObject` (the covering object listed last).
- `ObjectAt_NotLocked_GivesTheTop` (the dim box off: today's behavior).
- `SelectObjectsIn_Locked_LeavesOtherLayersOut`.
- `GrabbedObjectAt_Locked_SkipsOtherLayers`.
- `PickNote_LockedAndOnlyOtherLayersHere_SaysLocked`.
- `ObjectAt_LockTurnedOff_ReachesEveryLayerAgain` (ticked, then unticked).
`Editor.Tests/Events/EventEditingViewModelLockTests` (new; `EventEditingViewModelTests` stays as it is):
- `StartEditingAt_LockedOnTheCoveredObjectsLayer_OpensItsEvents`.
- `StartEditingAt_LockedOnALayerWithNoObjectThere_GivesNull` (the click then opens the cell, as today's window code does).
`LayerChoiceViewModelTests` (or the file testing it): `Accepts_NotDimmed_TakesEveryLayer`, `Accepts_Dimmed_TakesOnlyTheChosen`.

- **Done, with deviations:** the filtered list is `ObjectPlacingViewModel.ObjectsAt` (UO1's public list, now the reached objects only) over a private `AllObjectsAt`; `IsLockHidingObjectsAt(point)` gives the "Locked: only layer z 1 takes clicks" note when the lock is what left the click with nothing, and `ObjectPicking` shows it. `EventEditingViewModel.StartEditingAt` takes the first of `AllAt` the layer accepts (the window's cell fallback then opens the cell, as today). The placing view model ends at 296 lines. The lock tests found the dialog's `Title` is what names the placed object (`EventListViewModel` has no uid property). From the review: the pick's note ends when the layer choice or the lock changes (`Picking.Forget` on the layer's property changes, tested); `PlacedObjectHits.TopAt` is deleted, `AllAt` having taken its last caller, and its three tests read `AllAt`. Left as a design choice for the developer: an object on another layer selected before the lock is ticked stays selected, so Delete, the arrows and the panel still act on it; decision 525 covers clicks only.

## Verify by hand
1. Place a rug on layer -3 and a table on layer 1 over it (Layer box before each placing).
2. Layer box: "Decoration (below player) (-3)"; tick **Dim and lock other layers** (tool bar): the table is faint.
3. Ctrl + click on the table's spot: the rug is selected; Shift + drag moves the rug; right click removes the rug; Ctrl+Z.
4. Events tab: a click on the spot opens the rug's events.
5. Choose layer 1: the table takes the clicks again. Untick the box: the table is on top again, as before this slice.
6. With the lock on and a layer chosen where nothing is, Ctrl + click on the table: nothing selected, the panel's note says the lock is on.
