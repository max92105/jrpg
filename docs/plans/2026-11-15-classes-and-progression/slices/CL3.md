# CL3. The classes as a catalog: Add, Duplicate, Remove while new, one undo step

Editor, about 230 lines (about 120 of them tests). Needs CL2. Design: section 3; decisions 945, 946; the mode bar's
WS12 recipe, steps 1, 2 and 9.

## Goal
The Classes page's state and its apply, headless: every class of the draft in id order, Add (the blank, last, chosen),
Duplicate (a copy of the chosen class's boxes after it, its skills and resources included, its id left to type),
Remove (only a class added this session), the problem line (the chosen class's own problem, then any class's by id,
then Data's message for the whole list through `CatalogProblems.OfClasses`). The catalog view model is an
`IEditedCatalog` (its chosen class's id), the editing view model an `ICatalogEditing<ClassCatalogViewModel>`: Apply
records one `CatalogDraft.SetClasses` step; an Apply with no change records none.

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
- `Party/ClassEditingViewModel(CatalogDraft catalog)` (`ICatalogEditing<ClassCatalogViewModel>`): starts the Classes
  page from the draft and applies its session as one step. `Start()` from `catalog.Current`; `Apply(edited)` calls
  `catalog.SetClasses(edited.ToClasses())`.

## Changes
- `Party/ClassViewModel`: `static ClassViewModel DuplicateOf(ClassViewModel chosen)`, copying the boxes as typed
  (`ClassStatsBoxes.CopyFrom`), not the record, so a half-typed class duplicates (decision 585's rule).
- `Catalogs/CatalogDraft.Party.cs` (new partial): `public void SetClasses(IReadOnlyList<ClassDefinition> classes)`, one
  step, nothing when the same by content (`HasSameRecords` with `IsSameClass`: the record `with` the lists, then
  `SkillIds` and `Resources` by sequence, as `IsSameEnemy` compares tactics). The summary says why there is no undo
  refusal (decision 945).
- `Catalogs/CatalogDraft.Skills.cs` (row 7's SK2): `SetSkills`' summary loses "classes and enemies name skills but the
  editor writes neither": the editor now writes classes, and a skill a class still lists, once removed, is Data's
  message at the next Apply or save, not an undo refusal (catalog steps undo in order).

## Tests
- `Party/ClassCatalogViewModelTests` (new; a `CatalogDraft` over the test world's content `with` `Classes` set to
  `ClassFixtures.Scholar` and a second class built in the test):
  - `Constructor_Classes_ListsThemInIdOrder` (the two fixtures added in the opposite order).
  - `AddClass_AddsBlankLastAndChoosesIt`.
  - `DuplicateClass_TypedBoxes_CopiesThemWithAnEmptyId`; `DuplicateClass_ClassWithResources_CopiesThem`.
  - `RemoveClass_SavedClass_IsGreyed`; `RemoveClass_AddedClass_RemovesIt`.
  - `Problem_TwoClassesWithOneId_NamesTheId`.
  - `Problem_EmptyDescription_ShowsDataMessage`.
  - `Problem_TypedChange_RaisesProblemChanged`; `CanApply_WithProblem_IsFalse`.
  - The recipe's three: `Choose_KnownId_ChoosesIt`, `Choose_UnknownId_KeepsTheChoice`,
    `ChosenKey_NothingChosen_IsNull`.
- `Catalogs/CatalogDraftClassTests` (new): `SetClasses_ChangedGrowth_RecordsOneStep`;
  `SetClasses_SameClasses_RecordsNothing` (lists rebuilt, same content); `SetClasses_Undo_RestoresTheClasses`.
- `Party/ClassEditingViewModelTests` (new): `Apply_EditedName_ChangesTheDraft`; `Start_AfterUndo_ReadsTheDraftAgain`.

## Verify by hand
None: the page comes in CL4.
- **Note from CL1:** `CatalogDraft.Database.cs` is at 214 lines (118 when planned), so `SetClasses` goes in the new
  `Catalogs/CatalogDraft.Party.cs` partial. CL1 already rewrote `CatalogDraft.SetSkills`' summary ("no page edits what
  they name"); CL3 rewords it again once the Classes page edits a class's skills (CL6 at the latest).
- **Note from CL2:** needs CL2b, which holds `ClassViewModel` and its `Changed` event (the id, name, description or a
  box typed); relay that event to the list's problem, as `Encounters/EncounterCatalogViewModel` does.
