# CL3. The classes as a catalog: Add, Duplicate, Remove while new (the undo step is CL3b)

Editor, about 230 lines (about 120 of them tests). Needs CL2. Design: section 3; decisions 945, 946; the mode bar's
WS12 recipe, steps 1 and 7.

## Goal
The Classes page's state, headless: every class of the draft in id order, Add (the blank, last, chosen), Duplicate (a
copy of the chosen class's boxes after it, its skills and resources included, its id left to type), Remove (only a
class added this session), the problem line (the chosen class's own problem, then any class's by id, then Data's
message for the whole list through `CatalogProblems.OfClasses`). The catalog view model is an `IEditedCatalog` (its
chosen class's id). The editing view model and its `CatalogDraft.SetClasses` step are CL3b's goal.

## New types
- `Party/ClassCatalogViewModel` (`ObservableObject`, `IEditedCatalog`): every class on the page, Add, Duplicate, Remove
  while new, and the problem line.
  - `ClassCatalogViewModel(ContentCatalog content)`; `ObservableCollection<ClassViewModel> Classes`;
    `[ObservableProperty] ClassViewModel? SelectedClass` (notifies the Duplicate and Remove commands).
  - `IEditedCatalog`: `ChosenKey` = `SelectedClass?.Id`; `Choose(key)` selects the class with that id, an unknown key
    keeping the choice.
  - `string? Problem` (raises its change): `RecordListProblem.FirstOf(Classes, c => c.Id, c => c.Problem, "Class") ??
    CatalogProblems.OfClasses([.. Classes.Select(c => c.ToClass())], _content)`; `bool CanApply`.
  - `[RelayCommand]` `AddClass`, `DuplicateClass` (greyed with nothing chosen), `RemoveClass` (greyed unless the chosen
    class `IsNew`).
  - `IReadOnlyList<ClassDefinition> ToClasses()` (throws on a problem).
  - Relays its classes' typed changes to `Problem`, skipping their `Label` and `Problem` notices
    (`.claude/rules/editor.md`), as `Battle/EnemyCatalogViewModel` does.

## Changes
- `Party/ClassViewModel`: `static ClassViewModel DuplicateOf(ClassViewModel chosen)`, copying the boxes as typed
  (`ClassStatsBoxes.CopyFrom`), not the record, so a half-typed class duplicates (decision 585's rule).

## Tests
- `Party/ClassCatalogViewModelTests` (new; the test world's content `with` `Classes` set to `ClassFixtures.Scholar`
  and a second class built in the test):
  - `Constructor_Classes_ListsThemInIdOrder` (the two fixtures added in the opposite order).
  - `AddClass_AddsBlankLastAndChoosesIt`.
  - `DuplicateClass_TypedBoxes_CopiesThemWithAnEmptyId`; `DuplicateClass_ClassWithResources_CopiesThem`.
  - `RemoveClass_SavedClass_IsGreyed`; `RemoveClass_AddedClass_RemovesIt`.
  - `Problem_TwoClassesWithOneId_NamesTheId`.
  - `Problem_EmptyDescription_ShowsDataMessage`.
  - `Problem_TypedChange_RaisesProblemChanged`; `CanApply_WithProblem_IsFalse`.
  - The recipe's three: `Choose_KnownId_ChoosesIt`, `Choose_UnknownId_KeepsTheChoice`,
    `ChosenKey_NothingChosen_IsNull`.

## Verify by hand
None: the page comes in CL4.
- **Note from CL1:** `CatalogDraft.Database.cs` is at 214 lines (118 when planned), so `SetClasses` goes in the new
  `Catalogs/CatalogDraft.Party.cs` partial. CL1 already rewrote `CatalogDraft.SetSkills`' summary ("no page edits what
  they name"); CL3 rewords it again once the Classes page edits a class's skills (CL6 at the latest).
- **Note from CL2:** needs CL2b, which holds `ClassViewModel` and its `Changed` event (the id, name, description or a
  box typed); relay that event to the list's problem, as `Encounters/EncounterCatalogViewModel` does.
- **Done, with deviations:** split on 2026-11-17, the whole slice estimated at about 440 lines with tests: CL3 built
  `Party/ClassCatalogViewModel` (99 lines) and `ClassViewModel.DuplicateOf` (+17, the file at 125) with
  `Party/ClassCatalogViewModelTests` (234, 23 tests), the second class being `ClassFixtures.Archer` (42 lines with it;
  one skill, `expose`, so CL3b reuses it); `ClassEditingViewModel`, `CatalogDraft.SetClasses` (the new
  `CatalogDraft.Party` partial), `SetSkills`' summary and their tests moved to CL3b. No `CanApply`: the Database mode's
  Apply greys on the page's `Problem` (`DatabaseWorkspaceViewModel.CanApply`), so `CanApply_WithProblem_IsFalse`
  became `Problem_Constructed_IsNull`. `Problem` starts with the chosen class's own, as the goal and decision 946 say
  (the type list left it out), and `ChosenKey` is null while a new class has no id, as the affixes' and encounters'
  are. Tests beyond the list: Duplicate's skills (`DuplicateClass_ClassWithSkills_CopiesThem`, beside the resources'
  test, one class having each), Duplicate greyed with nothing chosen, the commands' `CanExecuteChanged`, another class's
  problem by id, the chosen class's first, `Problem` raised on a choice, `ToClasses` (its throw and its records), and
  `ChosenKey` for a new class and a saved one.
