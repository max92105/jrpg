# PO2. The placed object panel's rules: swapping the object

Builds on design.md, "Key decisions" 10 and Risks (swapping a placed object's object); decision 513. Needs PO1. Kept apart from PO1: together they pass 300 lines with their tests (about 205 + 155).

## Goal
Headless: the panel also offers the object a placement uses, from every object in the catalog; choosing another and applying swaps it, keeping the uid, position, layer and events (one undo step with the other fields). A warning names each event that waits for an object id (`OnlyWhenObjectId`) the placement will never be: neither the new object nor one of its events' swap targets. The list follows the catalog: a new object joins it, and an undo that removes the chosen object leaves the field with a problem.

## New types
None.

## Changes
- `Objects/PlacedObjectEditing` (about 70 after PO1, ends about 115):
  - `IReadOnlyList<string> ObjectIds` (the catalog's object ids, sorted; re-read on every `CatalogDraft.Current` change, the constructor taking the `CatalogDraft` as a second parameter).
  - `[ObservableProperty] string ObjectId` (raising `Warning`, `Problem`, `ApplyCommand`).
  - `string? Problem` gains "Choose an object." (empty, or no longer in the catalog).
  - `string? Warning`: "Event chest_1_0 runs only while the object is chest_closed; this one never becomes it." for the first such event (the reachable ids: the chosen object and every `SwapObjectStep` target in the placement's events); null otherwise. A warning, not a problem: Apply still works (a later swap step or edit may fix it).
  - `Apply` includes the object id in the edited record.
- `Objects/ObjectPlacingViewModel`: `Editing` built with the catalog (one argument more, same line).

## Tests (in `Objects/PlacedObjectEditingTests`, about 200 after PO1; these go in a new `Objects/PlacedObjectSwapTests` to keep it under 250)
- `PlacedObjectSwapTests: ObjectIds_ListEveryCatalogObjectSorted`.
- `PlacedObjectSwapTests: Apply_ObjectSwapped_KeepsUidPositionLayerAndEvents`.
- `PlacedObjectSwapTests: Apply_ObjectSwapped_UndoBringsTheOldObjectBack`.
- `PlacedObjectSwapTests: Warning_EventWaitsForAnObjectTheSwapNeverReaches_NamesTheEvent` (a placement built in code with one event waiting for `door_closed` and no swap step, swapped to another object: the warning names that event).
- `PlacedObjectSwapTests: Warning_EventWaitsForTheNewObject_IsNull`.
- `PlacedObjectSwapTests: Warning_EventWaitsForASwapTarget_IsNull`.
- `PlacedObjectSwapTests: ObjectIds_ChosenObjectUndoneAway_LeavesTheListAndTheChoiceGetsAProblem` (compose an object; with the panel showing another placement, choose the new object in its box without applying; undo the composing: the list loses it and the problem says "Choose an object.").
- `PlacedObjectSwapTests: ObjectIds_NewObjectComposed_JoinsTheList` (a catalog change raises `ObjectIds`).

Size: about 45 lines of code; 8 tests (about 110): about 155.

## Map
`docs/map.md`, Editor `Tiles, Objects` row: `PlacedObjectEditing` (the selected placed object's object, position and layer as typed; a warning when a gated event can't run).

## Hand check (yours)
Nothing to see yet (PO3).

- **Done, with deviations:** the constructor already had the catalog (PO1). `ShowSelected` sets the four fields from one tuple to stay under the complexity limit. `Problem` checks the chosen object first ("Choose an object."), and the center rule now reads the chosen object's size. Review: a test pins that size (a wider object on the edge), the warning test checks Apply stays on, and `ObjectIds` is a toolkit property.
