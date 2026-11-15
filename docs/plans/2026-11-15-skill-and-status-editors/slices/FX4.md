# FX4. A status as typed: its fields and parts

Editor, about 220 lines (about 110 of them tests). Decision 984 (row 6's 795-797, 804, 807; design.md section 2.3 of
row 6 for the part kinds). Design: design.md, section 2.4. Builds on SK2 (`SkillFixtures`).

## Goal
One status as the Statuses page will edit it: id while new, name, description, harmful, family (empty for its own
id), acts at (start or end of turn), mark letter (empty for the name's first), and parts as rows: a kind from the
closed list, an amount labelled by its kind (hidden for a skip or a taunt), a damage type for a damage tick only, a
stat for a stat change only; the "Used by" line naming the skills granting it. Data's ranges and combinations stay
Data's (FX5's problem line).

## New types
- `Editor/Statuses/StatusPartRow` (internal sealed partial, `ObservableObject`): one part as typed.
  - `Kind` (from `Kinds`, `Enum.GetValues<StatusPartKind>()`), `AmountText`, `DamageType` (`DamageType?`), `Stat`
    (`StatKind?`, from `Stats`: `StatusModifiers.ChangeableStats`).
  - `AmountLabel` through a switch over every kind (its default arm throws): `damageEachTurn` "HP lost each turn",
    `healEachTurn` "HP regained each turn", `resistanceChange` "points on every resistance", `damageTakenChange` "% more
    taken from each hit", `damageDealtChange` "% on the damage dealt", `statChange` "points on the stat", the two
    others empty; `HasAmount`, `CanTypeDamageType`, `CanTypeStat` read it.
  - `Problem` (an amount not a whole number; a stat change with no stat); `ToPart()`: amount 0 without one, the type
    only on a damage tick, the stat only on a stat change; `static From(StatusPart)`, `New()` (`damageEachTurn`, 1);
    `Changed` for typed fields only.
- `Editor/Statuses/StatusViewModel` (internal sealed partial, `ObservableObject`): one status as typed.
  - `IdText` (editable while `IsNew`), `Name`, `Description`, `IsHarmful`, `FamilyText` (empty writes null),
    `ActsAt` (from `Timings`, `Enum.GetValues<StatusTiming>()`), `MarkLetterText` (empty writes null).
  - `Parts` (`ObservableCollection<StatusPartRow>`), `SelectedPart`, `AddPartCommand` (after the chosen one),
    `RemovePartCommand`, `MovePartUpCommand`, `MovePartDownCommand` (`Events/ListOrder`).
  - `UsedByText` ("Used by: rot", or "Used by no skill"), from the skills given at construction.
  - `Problem` (the typed id through `Objects/ObjectIds.ProblemOf`, then "Parts row 2: ..."); `ToStatus()`; `static
    From`, `New`, `DuplicateOf`; `Changed` for typed fields and part list changes (`Catalogs/ListItemWatch.Follow`).

## Changes
- `Core/Statuses/StatusModifiers.ChangeableStats`: public if row 6's SL left it internal (the editor offers it; one
  word, no behavior).

## Tests
- `Editor.Tests/Statuses/StatusPartRowTests` (new; parts built in the test):
  - `AmountLabel_EveryKind_HasOne` (a theory over `StatusPartKind`: no throw; empty only for `skipTurn` and
    `drawAttacks`).
  - `ToPart_SkipTurn_WritesNoAmount`.
  - `ToPart_TypeChosenThenKindChanged_WritesNoType`.
  - `ToPart_StatChange_WritesTheStat`.
  - `Problem_StatChangeWithoutStat_SaysSo`.
- `Editor.Tests/Statuses/StatusViewModelTests` (new; `SkillFixtures`):
  - `From_Status_FillsEveryBoxAndPart`.
  - `ToStatus_EmptyFamilyAndLetter_WritesNulls`.
  - `ToStatus_StartOfTurn_Kept`.
  - `AddPart_ChosenFirst_InsertsSecond`.
  - `MovePartDown_FirstPart_ToStatusReadsTheNewOrder`.
  - `UsedByText_SkillGrantsIt_NamesIt`.
  - `Problem_NewWithATakenId_NamesIt`.
  - `Changed_AmountTyped_Raised`.

## Verify by hand
None: the page comes in FX6.
