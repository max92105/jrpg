# CL6. The Skills tab: the class's skills in menu order with the level each is learned at

Editor, about 230 lines (about 120 of them tests, about 40 of XAML). Needs CL4, LV1. Design: section 3; decisions 949,
963, 967.

## Goal
A Skills tab: the class's skills in menu order (ADR 0020: the order the battle's Skills menu lists them), each row
"Fireball (fireball)" with a Level box (ADR 0062: the level the skill is learned at, 1 for known from the start). A box
offers every skill of the draft not already listed, by name; Add puts the chosen one after the selected row (last when
none) at level 1; Remove takes the selected row out; Up and Down move it (`Events/ListOrder`). A Level box that isn't a
whole number is the class's problem; Data's messages reach the line: a level below 1 or past the max level (LV1), a
skill using a resource the class lacks or needing more than its max (decision 949's default: the box doesn't hide it).

## New types
- `Catalogs/RecordChoice` (sealed record): one record a box offers, its id and name (decision 967).
  `RecordChoice(string Id, string Name)`; `Label`: "Fireball (fireball)"; `static RecordChoice Missing(string id)`:
  "(missing) fireball" for an id the draft lacks, so a row never throws. Its summary names the earlier one-catalog
  copies it doesn't replace (`Loot/DropItemChoice`, `Trade/PricedItemChoice`, `Skills/StatusChoice`,
  `Battle/TacticSkillChoice`, each with an extra field); a backlog line notes them for when next touched.
- `Party/ClassSkillRow` (`ObservableObject`): one skill of the class as typed: the skill and its level box.
  `RecordChoice Skill`; `[ObservableProperty] LevelText`; `Problem` ("Level of Fireball must be a whole number");
  `ClassSkill ToEntry()`.

## Changes
- `Party/ClassViewModel.Skills.cs` (new partial, so `ClassViewModel` stays near 200 lines):
  - `ObservableCollection<ClassSkillRow> Skills` (from the record's `Skills`, each named through the content's skills),
    `[ObservableProperty] ClassSkillRow? SelectedSkill`; `IReadOnlyList<RecordChoice> SkillsToAdd` (the draft's skills
    by name, the listed ones left out, raised whenever the list changes); `[ObservableProperty] RecordChoice?
    SkillToAdd`.
  - `[RelayCommand]` `AddSkill` (greyed without `SkillToAdd`), `RemoveSkill`, `MoveSkillUp`, `MoveSkillDown` (greyed
    at the ends and with nothing chosen).
  - `ToClass` writes `Skills = [.. Skills.Select(row => row.ToEntry())]` (no longer carried); `Problem` adds the first
    row's problem; a row's typed change and the list's change raise `Problem`.
  - `DuplicateOf` copies the rows as typed.
- `Party/ClassCatalogViewModel`: relays a class's `Skills` collection change to `Problem` (the item relay sees only
  property changes).
- `Party/ClassCatalogPage.xaml`: the Skills tab: the rows (name, a narrow Level box), Up, Down and Remove beside them,
  the box and Add under them; the Level header's tooltip: "The level a member learns it at; 1: from the start."
- `docs/map.md`: the Editor `Catalogs` row names `RecordChoice`; the `Party` row the Skills tab.

## Tests
- `Catalogs/RecordChoiceTests` (new): `Label_NameAndId_ShowsBoth`; `Missing_Id_SaysMissing`.
- `Party/ClassSkillsTests` (new; the draft's skills `with` over two repository skills by id and one built in the test
  that spends a resource `ClassFixtures.Scholar` lacks):
  - `Skills_FromClass_ListsThemInItsOrderWithLevels` (the record's order set opposite to id order, levels 1 and 4).
  - `SkillsToAdd_LeavesListedSkillsOut`; `SkillsToAdd_AfterAdd_RaisesChanged`.
  - `AddSkill_WithSelection_InsertsAfterItAtLevelOne`; `AddSkill_NothingSelected_AddsLast`.
  - `RemoveSkill_TakesTheSelectedOut`.
  - `MoveSkillUp_FirstRow_IsGreyed`; `MoveSkillDown_MovesItOneDown`.
  - `ToClass_TypedLevel_WritesIt` (typed "6": the entry's level 6).
  - `Problem_LevelNotANumber_NamesTheSkill`.
  - `Problem_LevelPastMax_ShowsDataMessage` (through `ClassCatalogViewModel.Problem`; LV1's check).
  - `Problem_SkillUsingResourceTheClassLacks_ShowsDataMessage`.
  - `Skills_IdTheDraftLacks_ShowsItAsMissing`.

## Verify by hand
1. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` line.
2. Database > Classes, `mage`, Skills tab: the mage's skills in their order, each at level 1. Select Meteor, type `2`
   in its Level, Apply, Save all: `classes.json`'s meteor entry has `"level": 2`. Run the game as Omer: LV2's and LV3's
   checks (no Meteor at level 1; "Omer learned Meteor." at level 2). Ctrl+Z, Save all to restore.
3. Type `99`: the line gives LV1's "past the max level 21" message and Apply greys. Revert.
4. Choose `fighter`, add Fireball: the line says Fireball uses mana, a resource the fighter doesn't have. Remove it: the
   line clears.
