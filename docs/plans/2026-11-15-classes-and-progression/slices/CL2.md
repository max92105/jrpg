# CL2. A class's base or growth as eight typed boxes (the class itself is CL2b)

Editor, about 210 lines (about 100 of them tests). Needs CL1. Design: section 3; decisions 945-947.

## Goal
The view model of one class as the Classes page will type it: its id (typed while new), name, description, and its
base stats and growth as the same eight boxes each (max HP, speed, accuracy, power, agility, intelligence,
constitution, will). Its problem names the first box that isn't a whole number, in the form's order ("Growth will must
be a whole number (was 'x')."). Its record is the original `with` the edited fields, so the skills and resources no tab
edits yet are carried silently (decision 945).

## New types
- `Party/ClassStatsBoxes` (`ObservableObject`): one `ClassStats` as eight typed boxes, with the first box that isn't a
  whole number.
  - `ClassStatsBoxes(ClassStats stats, string label)`; `label` is "Base" or "Growth", the problem's first word.
  - `[ObservableProperty]` `MaxHpText`, `SpeedText`, `AccuracyText`, `PowerText`, `AgilityText`, `IntelligenceText`,
    `ConstitutionText`, `WillText` (each `[NotifyPropertyChangedFor(nameof(Problem), nameof(Typed))]`).
  - `string? Problem`: `TypedNumber.ProblemOf($"{label} max HP", MaxHpText) ?? ...`, in the form's order.
  - `ClassStats? Typed`: the stats as typed, null while there is a problem (CL5's tab reads it without a throw,
    `.claude/rules/editor.md`).
  - `void CopyFrom(ClassStatsBoxes other)`: the eight texts, for Duplicate (CL3).
- `Party/ClassViewModel` (`ObservableObject`): one class as the Classes page types it, carrying from its record what no
  tab edits yet.
  - `static ClassViewModel From(ClassDefinition)`, `static ClassViewModel New()` (from `_blank`, design section 1),
    `bool IsNew`.
  - `[ObservableProperty]` `Id` (notifies `Label`, `Problem`), `Name` (notifies `Label`), `Description`.
  - `ClassStatsBoxes Base`, `ClassStatsBoxes Growth`; a change in either raises `Problem`.
  - `string Label`: "mage: Mage", or "(new class)" while it has no id.
  - `string? Problem`: `(IsNew ? ObjectIds.ProblemOf(Id, []) : null) ?? Base.Problem ?? Growth.Problem`.
  - `ClassDefinition ToClass()`: `_original with { Id, Name = Name.Trim(), Description = Description.Trim(), Base,
    Growth }`; throws `InvalidOperationException` on a problem.

## Changes
- `docs/map.md`, Editor table: a `Party` row ("The Classes page's rules: one class as typed, its base and growth
  boxes"; `ClassViewModel`, `ClassStatsBoxes`), grown by the later slices.

## Tests
- `Party/ClassFixtures` (new, shared by this plan's Party test files): `Scholar`, a class built in the file (base and
  growth with every number different, one non-lasting resource `focus`, no skills); `WithResource`, `WithSkills`
  helpers over it.
- `Party/ClassViewModelTests` (new):
  - `From_Class_ShowsEveryBaseAndGrowthNumber` (the sixteen boxes against the fixture's distinct numbers).
  - `ToClass_TypedGrowth_SetsGrowth` (typed "7" in Growth power: 7, not the fixture's number).
  - `ToClass_UntouchedClass_KeepsItsSkillsAndResources` (carried by content).
  - `ToClass_TypedNameWithSpaces_TrimsIt`.
  - `Problem_BaseMaxHpNotANumber_NamesBaseMaxHp`.
  - `Problem_GrowthWillNotANumber_NamesGrowthWill` (the last box: the order holds to the end).
  - `Problem_NewClassWithBadId_NamesTheId`; `Problem_SavedClass_IgnoresIdRule`.
  - `Typed_BoxNotANumber_IsNull`; `Typed_AllBoxesRead_IsTheStats`.
  - `Label_NewClassWithoutId_SaysNewClass`.
  - `Problem_ChangedInBase_RaisesProblemChanged` (the relayed `PropertyChanged` arm).

## Verify by hand
None: no page yet (CL4).
- **Done, with deviations:** split on 2026-11-17, the whole slice estimated at about 370 lines with tests. CL2 built
  `Party/ClassStatsBoxes` (111 lines) with `Party/ClassFixtures` (23, `Scholar` only: `WithResource` and `WithSkills`
  wait for a test that needs them, `with` doing the rest) and `Party/ClassStatsBoxesTests` (116, 10 tests: the boxes'
  numbers, `Typed` untouched, typed and unreadable, the problem's label, its last box and its order, `CopyFrom` with a
  half-typed box, the notices). `Party/ClassViewModel` and its tests moved to CL2b, with a `Changed` event added there
  for CL3's list (the coordinator's model, `Encounters/EncounterViewModel`).
  The map's new Editor `Party` row names `ClassStatsBoxes`.
