# EC3b. Apply the Encounters page as one undo step

Editor; EC3's split: EC3 landed `Encounters/EncounterCatalogViewModel` (every encounter in id order, Add and Remove,
the problem line, `IEditedCatalog`, `ToEncounters`) and the tests' `EncounterFixtures.Catalog` (the test world with
the goblin and an orc, encounters b_pair and a_trio). About 120 lines with tests (about 45 of code, 4 tests). Needs
EC3. Decisions 905, 907 (its undo half in EC4). Design: "New types".

## Goal
The Encounters page's apply, headless: the whole session one `CatalogDraft.SetEncounters` undo step, none when nothing
changed by content.

## New types
- `Encounters/EncounterEditingViewModel` (internal sealed): `Start`/`Apply` of the Encounters page, as the affixes'
  and the cinematics'. `EncounterEditingViewModel(CatalogDraft catalog, Func<EncounterChoices> readChoices)`; `Start()`
  builds the list from `catalog.Current` and choices read now (so an image or track dropped in since the editor
  started is offered); `Apply(edited)` calls `catalog.SetEncounters(edited.ToEncounters())`. It returns nothing, as
  every page's apply does (`MainWindow.Database.OpenInDialog` takes an `Action<TEdited>`): the dialog greys OK on the
  page's `Problem`. No `ICatalogEditing<T>`: the mode bar's WS2 hasn't landed.

## Changes
- `Catalogs/CatalogDraft.Database` (187 lines, about +20): `SetEncounters(IReadOnlyList<EncounterDefinition>
  encounters)`: one step, none when the same by content (`IsSameEncounter`: the record `with` the enemy list, then
  the lists by `SequenceEqual`, as `IsSameEnemy`). `SetEnemies`' summary loses "the editor doesn't edit them":
  encounters name enemies, and catalog steps undo in order, so an encounter's pick of an added enemy is undone first
  (no refusal). The class summary lists the encounters.
- `docs/map.md`: the `Encounters` row names `EncounterEditingViewModel`; `Catalogs` names `SetEncounters`.

## Tests
`EncounterEditingViewModelTests` (Editor, new; a `CatalogDraft` over `EncounterFixtures.Catalog`):
- `Apply_BackgroundChanged_RecordsOneStep`
- `Apply_NothingChanged_RecordsNoStep` (the enemy lists are new list objects: only the content comparison sees them
  equal)
- `Apply_SlotsReordered_RecordsOneStep` (the same enemies in another order: the list comparison is by order)
- `Apply_Undo_RestoresTheEncounter`

## Verify by hand
None: no page yet (EC5). The suite runs green.
- **Done, with deviations:** about 140 lines with tests (code +48 -4, tests 89, the map's two rows): one test added,
  `Start_ImageAddedSinceTheEditorStarted_IsOffered` (the boxes read as the page opens, not when the editor starts), 5
  tests. `EncounterEditingViewModel` 22 lines, `EncounterEditingViewModelTests` 89, `CatalogDraft.Database.cs` 209;
  7358 tests.
