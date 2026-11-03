# E3c. Remove a sheet whose image is missing, asked first

Builds on E3a, E3b, X2, decision 442 and design.md, "Key decisions" 6.

## Goal
When the shown sheet's image is missing (X2 lists it), "Remove sheet" works even while its tiles are used: after a question that counts what goes ("Sheet city's image is missing. Removing it erases 1,204 cells on maps start and interior_1, and removes objects house_1 to house_5 with their 9 placements, 2 of them with events. This can't be undone. Remove?"), it purges (E3b), writes, and the editor carries on from the written content. Refused, with the reason and no question, while the catalogs or the open map have unsaved changes ("Save first"), and while an event swaps to an object it would remove (named, so the developer edits that event or re-skins the object first, O1). A sheet whose image exists keeps E3a's rule.

## Changes
- `Editor/Catalogs/CatalogDraft.ReplaceAfterRewrite(ContentCatalog written)`: takes the written content as `Current`, starts a fresh history marked saved (the high-water mark keeps its value), and raises `Rewritten`; `CombinedHistory` forgets its steps on it.
- `Editor/Tiles/SheetRemoval`: its constructor's fourth parameter, the `ContentWriter`. The missing-image branch follows the editor's dialog pattern (`StartX` / `TryApplyX`): `StartPurge()` returns the question (`SheetPurgeQuestion`: its text and the `SheetPurgeResult`) or sets `Status` to the refusal and returns null; `TryApplyPurge(question)` writes through `ContentWriter.WriteMapCatalogsAndMaps` and calls `ReplaceAfterRewrite`, or sets `Status` to the writer's message. The Tiles panel's code-behind asks the question with the editor's yes/no box between the two, as the other dialogs do.
- `Editor/Shell/MainViewModel`: on `Rewritten`, reopens the open map from the new content and refreshes `Art` (about 4 lines, paid by X1's freed lines; the plan's net growth on the file is zero or less).

## New types
- `Editor/Tiles/SheetPurgeQuestion`: what the purge's question says and the purge it would apply.

## Tests (new `Tiles/SheetRemovalMissingImageTests`, `Shell/MainViewModelPurgeTests`)
- `SheetRemovalMissingImageTests: StartPurge_MissingImageUsedOnMaps_AsksWithTheCounts`.
- `SheetRemovalMissingImageTests: TryApplyPurge_Question_WritesAndRemovesEverything`.
- `SheetRemovalMissingImageTests: StartPurge_NotApplied_ChangesNothing` (the "No" answer: the question is dropped).
- `SheetRemovalMissingImageTests: StartPurge_UnsavedChanges_RefusesAskingToSave` (no question).
- `SheetRemovalMissingImageTests: StartPurge_AnEventSwapsToAnObject_RefusesNamingIt`.
- `SheetRemovalMissingImageTests: RemoveSheet_ImagePresentAndUsed_StillRefuses` (E3a's rule, the other side).
- `MainViewModelPurgeTests: RemoveSheet_Purged_ReopensTheOpenMapWithoutTheCells`.
- `MainViewModelPurgeTests: RemoveSheet_Purged_ClearsUndo` (nothing to undo after; the question said so).

Size: about 70 lines of code, 8 tests (about 175).

## Verify by hand
- In a test copy of the content, rename `City.png` aside and start the editor: Remove sheet on "city (image missing)" asks with the counts; No changes nothing; Yes erases the houses' cells and the houses, the open map reopens, the warning no longer names `City.png`; restart the editor and the game: both open.

- **Done, with deviations:** the question goes through a `PurgeQuestion` property the window watches (it asks with the editor's yes/no box, then calls `ApplyPurge` or `DropPurge`), so the one Remove sheet button and command serve both cases and the logic stays testable; the plan's `StartPurge`/`TryApplyPurge` pair became the command plus those two methods. The constructor holds four dependencies, the writer the fourth. `MainViewModel` grows by two lines (496): the reopen on `Rewriting`; the art warning refreshes by itself since the sheets change. Hand check (yours): `City.png` aside, Remove sheet, the question, No, then Yes, and restarting the editor and the game.
  After review: the plan's `Rewritten` became `Rewriting`, raised with the written content before `Current` changes, so the open map is replaced first; raised after, the window redrew the old map against the purged catalogs and crashed with the files already written (pinned by `RemoveSheet_Purged_OpenMapIsReplacedBeforeTheCatalogsChange`). The question carries its sheet id; a test covers an open map with unsaved changes.
