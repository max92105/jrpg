# CL2b. One class as typed: its id, name, description and the two sets of boxes

Editor, about 230 lines (about 120 of them tests). Needs CL2 (`Party/ClassStatsBoxes`, `Party/ClassFixtures`). Split
from CL2 on 2026-11-17 (CL2's estimate passed 320 lines with tests). Design: section 3; decisions 945-947.

## Goal
The view model of one class as the Classes page will type it: its id (typed while new), name, description, and its
base stats and growth as CL2's boxes. Its problem is the typed id while new, then the first box that isn't a whole
number, base before growth ("Growth will must be a whole number (was 'x')."). Its record is the original `with` the
edited fields, so the skills and resources no tab edits yet are carried silently (decision 945), and an untouched class
gives back a record equal to its source.

## New types
- `Party/ClassViewModel` (`ObservableObject`): one class as the Classes page types it, carrying from its record what no
  tab edits yet.
  - `static ClassViewModel From(ClassDefinition)`, `static ClassViewModel New()` (from `_blank`, design section 1:
    name and description empty, base max HP 1, every other number 0, no skills, no resources), `bool IsNew`.
  - `[ObservableProperty]` `Id` (notifies `Label`, `Problem`), `Name` (notifies `Label`), `Description`.
  - `ClassStatsBoxes Base` ("Base"), `ClassStatsBoxes Growth` ("Growth"); a change in either raises `Problem`.
  - `event EventHandler? Changed`: the id, name, description or a box typed, never the label's or the problem's own
    notices, so CL3's list runs Data's check once per change (as `Encounters/EncounterViewModel.Changed`).
  - `string Label`: "mage: Mage", or "(new class)" while it has no id.
  - `string? Problem`: `(IsNew ? ObjectIds.ProblemOf(Id, []) : null) ?? Base.Problem ?? Growth.Problem`.
  - `ClassDefinition ToClass()`: `_original with { Id, Name = Name.Trim(), Description = Description.Trim(), Base =
    Base.Typed, Growth = Growth.Typed }`; throws `InvalidOperationException` on a problem.

## Changes
- `docs/map.md`, Editor `Party` row (CL2's): `ClassViewModel`.

## Tests
- `Party/ClassViewModelTests` (new; over `ClassFixtures.Scholar`, `with` for skills):
  - `From_Class_ShowsEveryBaseAndGrowthNumber` (the sixteen boxes against the fixture's distinct numbers).
  - `ToClass_Untouched_EqualsTheSource`.
  - `ToClass_TypedGrowth_SetsGrowth` (typed "7" in Growth power: 7, not the fixture's number).
  - `ToClass_UntouchedClass_KeepsItsSkillsAndResources` (carried by content).
  - `ToClass_TypedNameWithSpaces_TrimsIt`; `ToClass_WithProblem_Throws`.
  - `Problem_BaseAndGrowthBoxesNotNumbers_NamesTheBase` (base before growth).
  - `Problem_NewClassWithBadId_NamesTheId`; `Problem_SavedClass_IgnoresIdRule`.
  - `Label_NewClassWithoutId_SaysNewClass`; `Label_SavedClass_NamesIdAndName`.
  - `Problem_ChangedInBase_RaisesProblemChanged` (the relayed `PropertyChanged` arm), and one for Growth.
  - `Changed_BoxTyped_IsRaisedOnce`; `Changed_NameTyped_IsRaised`.
- `New()`: `New_Constructed_IsTheBlank` (base max HP 1, everything else 0, `IsNew`).

## Verify by hand
None: no page yet (CL4).
- **Done, with deviations:** `Party/ClassViewModel` (108 lines) with `Party/ClassViewModelTests` (237, 23 tests);
  `ClassFixtures` (32) gains `TextsOf`, the eight boxes' texts in the page's order, which `ClassStatsBoxesTests` (106)
  now uses too: a third copy of that list would have come with the sixteen-box test. Tests beyond the list: an
  untouched class's problem, a new class's legal id, the growth's box alone, and `Changed` for the id and the
  description (one test per raising branch), and the review's two: the id's notices `[Id, Label, Problem]` and the
  name's `[Name, Label]`, which CL3's list label and CL4's problem line read.
  `ToClass_UntouchedClass_KeepsItsSkillsAndResources` became `ToClass_NameTyped_KeepsItsSkillsAndResources`, since
  `ToClass_Untouched_EqualsTheSource` already covers an untouched class; `New_Constructed_IsTheBlank` became
  `New_IdTyped_IsTheBlankClass` (a blank id refuses `ToClass`). About 345 lines with tests in all, the review's two
  tests included.
