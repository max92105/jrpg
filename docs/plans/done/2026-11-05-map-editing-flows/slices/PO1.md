# PO1. The placed object panel's rules: position and layer

Builds on design.md, "What exists" (editing a placed object today) and "Key decisions" 10; decision 513. Needs MO1 (Ctrl + click selects one object).

## Goal
Headless: while exactly one placed object is selected, its uid, X, Y and layer z are shown as typed text; X and Y are numbers with a dot (free positions, as placed), z a whole number (any, as the layer box takes); Apply changes what was typed as one undo step, nothing when unchanged; the problem names a bad field or an object whose center would leave the map (placing's own rule). The fields follow the selection and every map change: an undo shows the restored values, an undo that removes the object empties the panel. No view yet (PO3).

## New types
- `Editor/Objects/PlacedObjectEditing` (`internal sealed partial class : ObservableObject`): the one selected placed object's fields as typed, applied as one step.
  - `PlacedObjectEditing(ObjectPlacingViewModel placing)` (the selection, the open map and the object sizes through it).
  - `bool HasObject`; `string Uid`; `[ObservableProperty] string XText`, `YText`, `ZText` (each raising `Problem` and `ApplyCommand`'s `CanExecute`).
  - `string? Problem`: "X and Y must be numbers, with a dot for decimals." / "The layer must be a whole number." / "The object's center must stay on the map."; null when it can apply.
  - `[RelayCommand(CanExecute = nameof(CanApply))] void Apply()`: the edited record (`with` position and z) through `ReplacePlacedObjectEdit`; no step when it equals the placed one.
  - Refresh: on `placing.SelectedObjectUids` (exactly one: load its record; else `HasObject` false) and on the open map's `Map` change (re-read the record by uid, the typed fields reset to it; gone: `HasObject` false). Unsaved typing is reset by a map change, the panel never showing a stale record (`.claude/rules/editor.md`).
- `Editor/Maps/Editing/ReplacePlacedObjectEdit` (`internal sealed record ReplacePlacedObjectEdit(PlacedObject Before, PlacedObject After) : IMapEdit`): replaces one placed object's record, keeping its place in the order; both have the same uid.

## Changes
- `Maps/Editing/MapDraft` (100): `public void ReplaceObject(PlacedObject placed)` by uid (+5); `MoveObject` and `SetObjectEvents` stay.
- `Objects/ObjectPlacingViewModel` (about 248 after CP2): `public PlacedObjectEditing Editing { get; }` built in the constructor (+2, ends about 250).

## Tests (new `Objects/PlacedObjectEditingTests`, the test world's forest, one object Ctrl + click selected through `SelectObjectAt`)
- `PlacedObjectEditingTests: HasObject_OneSelected_ShowsItsUidPositionAndLayer` (a free position (2.25, 3.5) shown "2.25", "3.5").
- `PlacedObjectEditingTests: HasObject_TwoSelected_IsFalse`; `HasObject_NoneSelected_IsFalse`.
- `PlacedObjectEditingTests: Apply_PositionTyped_MovesItInOneStep` (one undo restores it).
- `PlacedObjectEditingTests: Apply_LayerTyped_ChangesItsZOnly` (uid, object id, events and order unchanged).
- `PlacedObjectEditingTests: Apply_NothingChanged_RecordsNoStep`.
- `PlacedObjectEditingTests: Problem_InvalidFields_NamesTheField` (theory rows: X `"2,5"`, Y `""`, Z `"1.5"`, Z `"x"`).
- `PlacedObjectEditingTests: Problem_CenterOffTheMap_SaysSo`; `Problem_CenterOnTheLastCell_IsNull` (the accepted bound) and `Problem_NegativeLayer_IsNull` (z -1 is a layer).
- `PlacedObjectEditingTests: ApplyCommand_ProblemTyped_CannotExecuteAndIsRaised`.
- `PlacedObjectEditingTests: MapUndo_WithThePanelShowingIt_ShowsTheRestoredPosition` (moved by Shift + drag's `MoveObject`, then undone).
- `PlacedObjectEditingTests: MapUndo_RemovingTheObject_EmptiesThePanel` (its placement undone).
- `ObjectEditingTests: ReplacePlacedObjectEdit_ApplyThenRevert_KeepsItsPlaceInTheOrder` (the middle of three).

Size: about 85 lines of code (the view model 70, the edit 8, the draft 5, the property 2); 13 tests (about 120 with a theory and a shared arrange): about 205.

## Map
`docs/map.md`, Editor `Tiles, Objects` row: `PlacedObjectEditing` (the selected placed object's position and layer as typed); `Maps/Editing` row: `ReplacePlacedObjectEdit`.

## Hand check (yours)
Nothing to see yet (PO3).

- **Done, with deviations:** `PlacedObjectEditing` takes the catalog draft beside the placing view model, for the objects' sizes (the placing view model keeps its catalog private). X and Y accept a minus sign: an object near the left or top edge has a negative top-left. The undo check is its own test. Review: X and Y refuse "NaN" and "Infinity", which `double.TryParse` reads and the save can't write (a crash on save); the still step's seconds got the same guard (`EventStepViewModel.Seconds`, one theory row); the panel checks again on every catalog change, an object's size moving its center, with a test.
