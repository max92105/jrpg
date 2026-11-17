# CL3b. The classes' undo step: the session applied as one `SetClasses` step

Editor, about 160 lines (about 100 of them tests). Needs CL3 (`Party/ClassCatalogViewModel`). Split from CL3 on
2026-11-17 (CL3's estimate passed 320 lines with tests). Design: section 3; decision 945; the mode bar's WS12 recipe,
steps 2 and 9.

## Goal
The Classes page's apply, headless: the editing view model is an `ICatalogEditing<ClassCatalogViewModel>`, starting
the page from the draft; Apply records one `CatalogDraft.SetClasses` step, and an Apply with no change records none.
No undo refusal (decision 945): only members name a class, and catalog steps undo in order.

## New types
- `Party/ClassEditingViewModel(CatalogDraft catalog)` (`ICatalogEditing<ClassCatalogViewModel>`): starts the Classes
  page from the draft and applies its session as one step. `Start()` from `catalog.Current`; `Apply(edited)` calls
  `catalog.SetClasses(edited.ToClasses())`.

## Changes
- `Catalogs/CatalogDraft.Party.cs` (new partial): `public void SetClasses(IReadOnlyList<ClassDefinition> classes)`, one
  step, nothing when the same by content (`HasSameRecords` with `IsSameClass`: the record `with` the lists, then
  `SkillIds` and `Resources` by sequence, as `IsSameEnemy` compares tactics). The summary says why there is no undo
  refusal (decision 945).
- `Catalogs/CatalogDraft.Skills.cs` (row 7's SK2): `SetSkills`' summary loses "classes and enemies name skills but the
  editor writes neither": the editor now writes classes, and a skill a class still lists, once removed, is Data's
  message at the next Apply or save, not an undo refusal (catalog steps undo in order).

## Tests
- `Catalogs/CatalogDraftClassTests` (new; a `CatalogDraft` over the test world's content `with` `Classes` set to
  `ClassFixtures.Scholar` and a second class built in the test): `SetClasses_ChangedGrowth_RecordsOneStep`;
  `SetClasses_SameClasses_RecordsNothing` (lists rebuilt, same content); `SetClasses_Undo_RestoresTheClasses`.
- `Party/ClassEditingViewModelTests` (new): `Apply_EditedName_ChangesTheDraft`; `Start_AfterUndo_ReadsTheDraftAgain`.

## Verify by hand
None: the page comes in CL4.
- **Note from CL1:** `CatalogDraft.Database.cs` is at 214 lines (118 when planned), so `SetClasses` goes in the new
  `Catalogs/CatalogDraft.Party.cs` partial. CL1 already rewrote `CatalogDraft.SetSkills`' summary ("no page edits what
  they name"); reword it again here, now that the Classes page edits classes.
