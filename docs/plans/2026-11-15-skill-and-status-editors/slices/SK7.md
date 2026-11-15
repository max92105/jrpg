# SK7. The statuses a skill grants, with turns and chance

Editor, about 180 lines (about 90 of them tests). Decision 981 (row 6's 798, 809). Design: design.md, section 2.4.
Builds on SK6.

## Goal
The Skills page's Statuses section: the effect's grants as rows in order, each a status (the draft's statuses,
"poison_1: Poison"), turns and chance in percent; Add puts a row after the chosen one (the first status by id, 1 turn,
100 %), Remove, Up and Down move it. Data's rules (turns at least 1, chance 1 to 100, a status once, `grantStatus`
needing one) show as its message. From this slice the page edits every field of a skill.

## New types
- `Editor/Skills/StatusGrantRow` (internal sealed partial, `ObservableObject`): one grant as typed: `StatusId`,
  `TurnsText`, `ChanceText`; `static New(string firstStatusId)` (1 turn, 100 %); `static From(StatusGrant grant)`;
  `Problem` (a box not a whole number, no status picked); `ToGrant(IReadOnlyDictionary<string, StatusDefinition>
  statuses)`: the record resolved by id.
- `Editor/Skills/StatusChoice` (internal sealed record): one status the grant box offers: `Id`, `Line` ("poison_1:
  Poison"); `static IReadOnlyList<StatusChoice> AllIn(ContentCatalog content)` by id.
- `Editor/Skills/SkillGrants` (internal sealed partial, `ObservableObject`): a skill's grant rows.
  - `Rows`, `SelectedRow`; `AddRowCommand` (greyed when the draft has no status), `RemoveRowCommand`,
    `MoveUpCommand`, `MoveDownCommand` (`Events/ListOrder.MoveBy`).
  - `Problem` ("Statuses row 2: turns must be a whole number."); `ToGrants(statuses)`; `Changed` for typed fields and
    list changes (`Catalogs/ListItemWatch.Follow`).

## Changes
- `Editor/Skills/SkillChoices`: `IReadOnlyList<StatusChoice> Statuses` and `IReadOnlyDictionary<string,
  StatusDefinition> StatusRecords` (the draft's when the page opened; one session at a time).
- `Editor/Skills/SkillViewModel`: `Grants` (from the record's `Effect.Statuses`); `Problem` reads it after the effect;
  `ToSkill` sets `Effect.ToEffect(...) with { Statuses = Grants.ToGrants(choices.StatusRecords) }` (SK6's kept grants
  go); `Changed` relays it.
- `Editor/Skills/SkillCatalogPage.xaml`: a "Statuses" section band after Effect: rows (status combo by
  `SelectedValue`, `DisplayMemberPath="Line"`; turns; chance with a "%" label), Add, Remove, Up, Down; a row chosen by
  its `PreviewGotKeyboardFocus` `EventSetter`. About +35 lines.

## Tests
- `Editor.Tests/Skills/SkillGrantsTests` (new; `SkillFixtures`):
  - `From_SkillWithTwoGrants_RowsInOrder`.
  - `AddRow_ChosenFirst_InsertsSecondWithOneTurnAndFullChance`.
  - `AddRow_NoStatusInTheDraft_Greyed`.
  - `MoveDown_FirstRow_SwapsTheOrder` (and `ToGrants` reads the new order).
  - `Problem_ChanceNotANumber_NamesTheRow`.
  - `ToGrants_RowNamingPoison_ResolvesTheDraftRecord` (the instance in `StatusRecords`).
  - `Changed_TurnsTyped_Raised`, `Changed_RowMoved_Raised`.
- `Editor.Tests/Skills/SkillCatalogViewModelTests`: `Problem_ChanceZero_IsDataMessage`, `Problem_StatusTwice_IsDataMessage`.
- `Editor.Tests/Skills/SkillViewModelTests`: `ToSkill_GrantsEdited_WritesTheRows` (replaces SK6's "keeps the loaded
  grants" test).

## Verify by hand
1. Database > Skills..., `shield_bash`: one row, `stun`, 1 turn, 60 %.
2. Add a row: `burning_1`, 2 turns, 100 %; OK, Save all: `git diff content/skills.json` shows the second grant only;
   run the game and use Shield Bash: the goblin burns. Ctrl+Z, Save all.
3. Type 0 in a chance: Data's "chance must be within 1..100" line, OK greys.
4. Run `.claude/scripts/open-editor-windows.ps1`: no `ERROR` or binding-error line.
