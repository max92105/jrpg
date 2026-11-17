# CL6b. The Skills tab on the page: Add, Remove, Up and Down over the rows

Editor, about 230 lines (about 110 of them tests, about 45 of XAML). Split from CL6 on 2026-11-17; needs CL6's
`Catalogs/RecordChoice`, `Party/ClassSkillRow` and `ClassViewModel.Skills`. Design: section 3; decisions 949, 967,
1014.

## Goal
The Classes page gains its Skills tab: CL6's rows in menu order, each "Fireball (fireball)" with a narrow Level box. A
box offers every skill of the draft not already listed, by name; Add puts the chosen one after the selected row (last
when none) at level 1; Remove takes the selected row out; Up and Down move it (`Events/ListOrder`). The list's changes
reach the problem line through `ClassViewModel.Changed` (decision 1014).

## Changes
- `Party/ClassViewModel.Skills.cs`:
  - `[ObservableProperty] ClassSkillRow? SelectedSkill`; `IReadOnlyList<RecordChoice> SkillsToAdd` (the draft's skills
    by name, the listed ones left out, raised whenever the list changes); `[ObservableProperty] RecordChoice?
    SkillToAdd`.
  - `[RelayCommand]` `AddSkill` (greyed without `SkillToAdd`; the new row at level 1, chosen), `RemoveSkill`,
    `MoveSkillUp`, `MoveSkillDown` (greyed at the ends and with nothing chosen).
  - `Skills.CollectionChanged` follows the rows (`Catalogs/ListItemWatch.Follow`, so an added row's typed level is
    heard and a removed one's no longer), and raises `Problem`, `SkillsToAdd` and `Changed`.
- `Party/ClassCatalogPage.xaml`: the Skills tab after Levels: the rows (name, a narrow Level box), Up, Down and Remove
  beside them, the box and Add under them; the Level header's tooltip: "The level a member learns it at; 1: from the
  start." A click into a row's Level box chooses the row (`Catalogs/RowFocus.ChoosesRow`, as the Skills page's rows).
- `.claude/scripts/open-editor-windows.ps1`: nothing new (the page is already in its list).
- `docs/map.md`: the `Party` row's `ClassCatalogPage` names the Skills tab.

## Tests
- `Party/ClassSkillsTests` (CL6's file, over its draft of expose, taunt and the test's spark):
  - `SkillsToAdd_LeavesListedSkillsOut`; `SkillsToAdd_AfterAdd_RaisesChanged`.
  - `AddSkill_WithSelection_InsertsAfterItAtLevelOne`; `AddSkill_NothingSelected_AddsLast`;
    `AddSkill_NothingToAdd_IsGreyed`.
  - `RemoveSkill_TakesTheSelectedOut`; `RemoveSkill_RemovedRowTyped_RaisesNothing`.
  - `MoveSkillUp_FirstRow_IsGreyed`; `MoveSkillDown_LastRow_IsGreyed`; `MoveSkillDown_MovesItOneDown`.
  - `Changed_SkillAdded_IsRaisedOnce`; `Problem_AddedSkillUsingResourceTheClassLacks_ShowsDataMessage` (through
    `ClassCatalogViewModel.Problem`, the scholar adding spark; Remove clears it).
  - `ToClass_AddedSkill_WritesItAtLevelOne`.

## Verify by hand
1. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` line.
2. Database > Classes, `mage`, Skills tab: the mage's skills in their order, each at level 1. Select Meteor, type `2`
   in its Level, Apply, Save all: `classes.json`'s meteor entry has `"level": 2`. Run the game as Omer: LV2's and LV3's
   checks (no Meteor at level 1; "Omer learned Meteor." at level 2). Ctrl+Z, Save all to restore.
3. Type `99`: the line gives LV1's "past the max level 21" message and Apply greys. Revert.
4. Choose `fighter`, add Fireball: the line says Fireball uses mana, a resource the fighter doesn't have. Remove it: the
   line clears.
