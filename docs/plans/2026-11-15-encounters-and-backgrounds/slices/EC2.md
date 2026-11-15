# EC2. One encounter as edited

Editor (`Encounters/`, a new folder); about 210 lines with tests (about 100 of code, 12 tests). Needs EC1 (none of its
code, but the page it serves writes through it). Decisions 905, 906. Design: "New types".

## Goal
One encounter as the Encounters page edits it, headless: its id typed while new, its enemies in slot order (an
enemy's place on screen) added after the chosen slot from the enemies by name, removed and moved, the same enemy any
number of times; its Background box with "(the map's)" first and its Music box with "(battle theme)" first, a file no
longer in its folder kept listed; its own problem; the record it becomes.

## New types
- `Encounters/EnemySlotRow` (internal sealed **class**, not a record): one place in an encounter's line of enemies.
  `EnemySlotRow(string enemyId, string name)`, `EnemyId`, `Name`. A class so equality is by reference: two goblins
  are two rows that `ListOrder` (which finds a row by `IndexOf`) and `SelectedSlot` tell apart; a record's value
  equality would move or remove the first goblin whichever was chosen.
- `Encounters/EnemyChoice` (internal sealed record): one enemy the add box offers. `(string Id, string Name)`, shown as
  "Goblin (goblin)".
- `Encounters/EncounterChoices` (internal sealed record): what the page's boxes offer, read once when it starts.
  `(IReadOnlyList<EnemyChoice> Enemies, IReadOnlyList<string> Backgrounds, IReadOnlyList<string> Tracks)`.
- `Encounters/EncounterViewModel` (internal sealed partial, `ObservableObject`): one encounter as edited.
  - `const string MapsBackground = "(the map's)"`, `const string BattleTheme = "(battle theme)"`;
  - `static EncounterViewModel From(EncounterDefinition encounter, EncounterChoices choices)`,
    `static EncounterViewModel New(EncounterChoices choices)` (`IsNew`, no enemy yet);
  - `[ObservableProperty] Id` (editable while new), `ObservableCollection<EnemySlotRow> Enemies`,
    `[ObservableProperty] SelectedSlot`, `[ObservableProperty] EnemyToAdd`;
  - `BackgroundChoices` and `[ObservableProperty] SelectedBackground`; `MusicChoices` and `[ObservableProperty]
    SelectedMusic` (each list: the first entry, the encounter's file when the folder no longer has it, then the
    folder's);
  - commands: `AddEnemy` (after the chosen slot, or last; the new row chosen; greyed without `EnemyToAdd`),
    `RemoveEnemy`, `MoveEnemyUp`, `MoveEnemyDown` (greyed without a chosen slot) through `Events/ListOrder`;
  - `string Label`: "goblin_trio: Goblin, Goblin, Goblin", or "(new encounter)" while the id is empty;
  - `string? Problem`: the typed id while new (`Objects/ObjectIds.ProblemOf`), then "An encounter needs at least one
    enemy."; the list's Data check comes in EC3;
  - `EncounterDefinition ToEncounter()`: throws `InvalidOperationException` on a `Problem`; "(the map's)" and
    "(battle theme)" written as null.

## Changes
- `docs/map.md`: a new Editor row, `Encounters`, naming these types (EC3-EC5 extend it).

## Tests
`EncounterViewModelTests` (Editor, new; encounters and choices built in the test: enemies "goblin" (Goblin) and "orc"
(Orc), backgrounds "Cave.png" and "Forest.png", tracks "boss/loop.wav"):
- `From_EncounterWithoutItsOwnBackground_ChoosesTheMaps`
- `From_EncounterWithItsOwnBackground_ChoosesIt`
- `From_BackgroundNoLongerInTheFolder_StaysListed`
- `From_NoMusic_ChoosesTheBattleTheme`
- `AddEnemy_SlotChosen_InsertsAfterItAndChoosesIt` (an orc after the first of two goblins)
- `AddEnemy_SameEnemyTwice_AddsTwoRows`
- `AddEnemy_NothingToAdd_CannotExecute`
- `MoveEnemyDown_FirstOfTwoGoblins_MovesThatRow` (the chosen row object moves, not the other goblin)
- `RemoveEnemy_LastSlot_ProblemNamesTheMissingEnemy`
- `Problem_NewWithBadId_NamesTheId` and `Problem_SavedEncounter_NoIdProblem`
- `ToEncounter_MapsBackgroundAndBattleTheme_WritesNulls`
- `ToEncounter_EnemiesInSlotOrder_KeepsTheOrder` (rows built in the reverse of the record's order)

## Verify by hand
None: no page yet (EC5). The suite runs green.
