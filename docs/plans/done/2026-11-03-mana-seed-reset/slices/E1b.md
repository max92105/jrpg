# E1b. Remove an object definition nothing uses

Builds on E1a and design.md, "Key decisions" 3.

## Goal
In the Objects panel, a Remove button takes the chosen object definition out of the catalog as one undo step when no map places it and no event swaps to it (the open map read as it is on screen in place of its saved file); otherwise nothing changes and the panel says why, naming the first user. Ctrl+Z puts it back; Ctrl+S saves it with the catalogs.

## Changes
- `Editor/Catalogs/CatalogDraft.RemoveObject(string objectId)`: one `CatalogEdit` (without the object, then with it back); throws `InvalidOperationException` for an unknown id.
- `Editor/Objects/ObjectBrowserViewModel`: takes the open map's layout (`Func<MapLayout?> openLayout`, as `TileBrowserViewModel` does; `ObjectPlacingViewModel` passes it); `[RelayCommand(CanExecute = HasSelection)] RemoveSelected()`; `RemovalStatus` ("Removed object tree." or "Can't remove object tree: it is used on map forest."), cleared when the selection changes.
- `Editor/Objects/ObjectBrowserView.xaml`: a Remove button under the list and the status line (themed, `BasedOn`).

## New types
None.

## Tests (new `Objects/ObjectBrowserRemovalTests`, `Catalogs/CatalogDraftRemovalTests`)
- `CatalogDraftRemovalTests: RemoveObject_KnownObject_LeavesTheCatalogAsOneUndoStep`.
- `CatalogDraftRemovalTests: Undo_AfterRemoveObject_BringsItBack`.
- `ObjectBrowserRemovalTests: RemoveSelected_UnusedObject_RemovesItAndSaysSo`.
- `ObjectBrowserRemovalTests: RemoveSelected_PlacedOnASavedMap_RefusesNamingTheMap`.
- `ObjectBrowserRemovalTests: RemoveSelected_PlacedOnlyInTheOpenMapsUnsavedChanges_Refuses`.
- `ObjectBrowserRemovalTests: RemoveSelected_PlacementRemovedFromTheOpenMapButStillSaved_Removes` (the open map replaces its saved file, as for undo).
- `ObjectBrowserRemovalTests: RemoveSelected_SwappedToByAnEvent_RefusesNamingTheMap`.
- `ObjectBrowserRemovalTests: RemoveSelected_Refused_LeavesTheCatalogUnchanged` (same `Current`, no undo step).

Each test builds its catalog and maps (test-world copies or in-memory layouts), never a repository object that happens to be unused.

## Verify by hand
- Objects panel: choose `direction_3` (placed only on the forest): Remove says it is used on map forest. Make an object, don't place it, Remove: it leaves the list; Ctrl+Z brings it back. The button is visible under the list.

- **Done, with deviations:** two more tests than listed: `RemoveObject_UnknownId_Throws` and an undo after a remove-then-set of the same id (the removed record comes back), and the browser's "nothing chosen" and "status cleared on a new choice" cases. The button sits under the list, above the preview. Hand check (yours): the look of the button and the line.
