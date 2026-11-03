# E1a. A swap step naming an object counts as a use

Builds on design.md, "Key decisions" 4.

## Goal
`CatalogUsage.UsersOfObject` names a map when it places the object or when an event on it (an object's or a cell's) swaps to it. The undo of a new object that an event already swaps to is refused, as `MapEventValidator` would refuse the save; E1b's removal reads the same method.

## Changes
- `Editor/Catalogs/CatalogUsage.UsersOfObject`: also matches `SwapObjectStep.ObjectId` over `layout.AllEvents`; its summary says both uses.
- `Editor/Shell/CombinedHistory.RefusalOfCatalogUndo`: "is placed on" becomes "is used on".

## New types
None.

## Tests (a sibling `Catalogs/CatalogUsageObjectTests` if `CatalogUsageTests` is near 300)
- `CatalogUsageObjectTests: UsersOfObject_SwapStepNamesIt_NamesTheMap`.
- `CatalogUsageObjectTests: UsersOfObject_CellEventSwapsToIt_NamesTheMap`.
- `CatalogUsageObjectTests: UsersOfObject_AnotherObjectSwappedTo_IsEmpty` (the other side).
- `CatalogUsageObjectTests: UsersOfObject_SwapOnlyInTheOpenMapsUnsavedChanges_NamesTheMap`.
- `CombinedHistoryObjectTests: Undo_NewObjectAnEventSwapsTo_IsRefusedNamingTheMap`.
- Re-pinned: the existing undo refusal tests asserting "is placed on".

## Verify by hand
- Make a new object, add a swap step to it on a scratch map's chest event, Ctrl+Z past the event to the object's creation: refused, "is used on map ...".

- **Done, with deviations:** no cell-event test: the validator refuses a swap on a cell event ("swapObject only works in an object's event"), so that use can't exist in saved content; the usage tests put the swap on a placed object's event instead, with a saved-map case in place of the open-map-only one (the open map case is the first test). The tests went into `CatalogUsageTests` (95 lines) rather than a sibling.
