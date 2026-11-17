# EC3. Every encounter, and Apply as one undo step

Editor; about 250 lines with tests (about 120 of code, 13 tests). Needs EC1, EC2 and the mode bar plan's stage 1
(`Workspaces/IEditedCatalog`, `ICatalogEditing<T>`). Decisions 905, 907 (its undo half in EC4). Design: "New types".

## Goal
The Encounters page's state and its apply, headless: every encounter in id order, then the ones added since the page
last applied; Add, and Remove for an added one only (decision 581's rule, as the mode bar plan's decision 918 keeps
it); the problem line (the chosen encounter's, then another's by id or position, then Data's for the whole list)
stopping Apply; Apply as one `CatalogDraft.SetEncounters` undo step, none when nothing changed.

## New types
- `Encounters/EncounterCatalogViewModel` (internal sealed partial, `ObservableObject`, `Workspaces/IEditedCatalog`):
  the Encounters page's state.
  - `EncounterCatalogViewModel(ContentCatalog current, EncounterChoices choices)`;
  - `ObservableCollection<EncounterViewModel> Encounters`, `[ObservableProperty] SelectedEncounter` (the first on
    starting);
  - `[RelayCommand] Add()` (a new encounter last, chosen); `[RelayCommand(CanExecute = nameof(CanRemove))] Remove()`
    (only `IsNew`; the next row chosen);
  - `string? Problem` (raises its change, as the page recipe needs): `RecordListProblem.FirstOf(Encounters, e => e.Id,
    e => e.Problem, "Encounter")`, then `CatalogProblems.OfEncounters(records, current)`;
  - each encounter's typed changes (`Id`, `SelectedBackground`, `SelectedMusic`, and its `Enemies` collection's
    changes) refresh the problem line; its own `Label` and `Problem` notices are skipped (the editor rule on relayed
    `PropertyChanged`); `Catalogs/ListItemWatch` keeps the row watch right across a move, as the other lists;
  - `IReadOnlyList<EncounterDefinition> ToEncounters()`: throws on a `Problem`;
  - `IEditedCatalog`: `ChosenKey` (the chosen encounter's id), `Choose(key)` (an unknown id keeps the choice).
- `Encounters/EncounterEditingViewModel` (internal sealed, `Workspaces/ICatalogEditing<EncounterCatalogViewModel>`):
  `Start`/`Apply` of the Encounters page, as the shops'. `EncounterEditingViewModel(CatalogDraft draft,
  Func<EncounterChoices> readChoices)`; `Start()` builds the list from `draft.Current` and choices read now (so an image
  or track dropped in since the editor started is offered); `Apply(list)` returns false on a problem, else calls
  `draft.SetEncounters(list.ToEncounters())`.

## Changes
- `Catalogs/CatalogDraft.Database` (about +20): `SetEncounters(IReadOnlyList<EncounterDefinition> encounters)`: one
  step, none when the same by content (`IsSameEncounter`: the record `with` the enemy list, then the lists by
  `SequenceEqual`, as `IsSameEnemy`). `SetEnemies`' summary loses "the editor doesn't edit them": encounters name
  enemies, and catalog steps undo in order, so an encounter's pick of an added enemy is undone first (no refusal). The
  class summary lists the encounters.
- `docs/map.md`: the `Encounters` row names both types; `Catalogs` names `SetEncounters`.

## Tests
- `EncounterCatalogViewModelTests` (Editor, new; a catalog built in the test with encounters "b_pair" and "a_trio",
  listed out of order):
  - `Constructor_Encounters_InIdOrder`
  - `Add_NewEncounter_LastAndChosen`
  - `Remove_SavedEncounter_CannotExecute` and `Remove_AddedEncounter_RemovesItAndChoosesTheNext`
  - `Problem_TwoEncountersSameId_NamesTheLaterOne`
  - `Problem_EnemyAddedToAnotherEncounter_Refreshes` (a slot added to an unchosen encounter fixes its problem; the line
    clears and `Problem`'s change is raised)
  - `ToEncounters_AddedEncounter_Included`
  - `Choose_KnownId_ChoosesIt`, `Choose_UnknownId_KeepsTheChoice`, `ChosenKey_EncounterChosen_IsItsId` (the page
    recipe's three)
- `EncounterEditingViewModelTests` (Editor, new):
  - `Apply_BackgroundChanged_RecordsOneStep`
  - `Apply_NothingChanged_RecordsNoStep`
  - `Apply_Undo_RestoresTheEncounter`

## Verify by hand
None: no page yet (EC5). The suite runs green.
- **Note from EC2:** `EncounterViewModel` raises `Changed` for the typed id, a slot added, removed or moved (and from
  EC2b each box's pick), never for its `Label` or `Problem` notices or a slot's choice, as the affixes and items do: the
  list subscribes to `Changed` instead of each encounter's `PropertyChanged` and `Enemies` collection, so no
  `ListItemWatch` is needed. EC2 split: the Background and Music boxes are EC2b's, which lands before EC3.
- **Done, with deviations:** split at start (estimate about 440 lines with tests, from the affixes' list and editing
  view models): `CatalogDraft.SetEncounters` and `EncounterEditingViewModel` went to [EC3b](EC3b.md), and EC4 needs
  both (its note). The commands are `AddEncounter` and `RemoveEncounter`, as the shops' and affixes', not `Add` and
  `Remove`. The list listens to each encounter's `Changed` (EC2's note), so no `ListItemWatch`. Added beyond the plan:
  `Problem_Constructed_IsNull`, `CanExecuteChanged_AddedEncounterChosen_LightsRemove`,
  `Problem_AnotherChosen_RaisedAndNamesTheNewOneByPosition`, `Problem_EnemyTheContentLacks_IsDatasMessage`,
  `ToEncounters_WithAProblem_Throws` and `ChosenKey_NewEncounterWithoutId_IsNull`. About 345 lines with tests:
  `EncounterCatalogViewModel` 97, `EncounterCatalogViewModelTests` 167, `EncounterCatalogViewModelChoiceTests` 51,
  `EncounterFixtures` +30 (`Catalog`: the test world with the goblin and an orc, b_pair and a_trio); 7353 tests.
