# EC2b. One encounter's Background and Music boxes

Editor (`Encounters/`); EC2's split: EC2 landed `Encounters/EncounterViewModel` (the id, the enemies in slot order, its
`Problem`, `Label`, `Changed` and `ToEncounter`, the record's own background and music carried over), `EnemySlotRow`,
`EnemyChoice`, `EncounterChoices(Enemies)` and the tests' `Encounters/EncounterFixtures`. About 120 lines with tests
(about 40 of code, 7 tests). Decision 906. Design: "New types".

## Goal
An encounter's Background box with "(the map's)" first and its Music box with "(battle theme)" first, a file no longer
in its folder kept listed, so opening the page never changes an encounter; `ToEncounter` writes the two boxes.

## Changes
- `Encounters/EncounterChoices`: gains `IReadOnlyList<string> Backgrounds` (the backgrounds folder's images) and
  `IReadOnlyList<string> Tracks` (the music folder's tracks); `EncounterFixtures.Choices` names "Cave.png",
  "Forest.png" and "boss/loop.wav".
- `Encounters/EncounterViewModel`:
  - `const string MapsBackground = "(the map's)"`, `const string BattleTheme = "(battle theme)"`;
  - `BackgroundChoices` and `[ObservableProperty] SelectedBackground`; `MusicChoices` and `[ObservableProperty]
    SelectedMusic` (each list: the first entry, the encounter's file when the folder no longer has it, then the
    folder's: Map settings' `ChoicesFor(current, none, files)` rule, BG2, its second copy; a third is extracted);
  - `SelectedBackground` and `SelectedMusic` raise `Changed`;
  - `ToEncounter`: the boxes over the record, "(the map's)" and "(battle theme)" written as null;
  - the class summary names the two boxes.
- `docs/map.md`: the `Encounters` row names the boxes.

## Tests
`EncounterViewModelBoxTests` (Editor, new; EC2's two test files are near 180 and 140 lines):
- `From_EncounterWithoutItsOwnBackground_ChoosesTheMaps`
- `From_EncounterWithItsOwnBackground_ChoosesIt`
- `From_BackgroundNoLongerInTheFolder_StaysListed`
- `From_MusicNoLongerInTheFolder_StaysListed`
- `From_NoMusic_ChoosesTheBattleTheme`
- `ToEncounter_MapsBackgroundAndBattleTheme_WritesNulls`
- `Changed_BoxPicked_IsRaisedOnce` (a theory over the two boxes)
- EC2's `ToEncounter_OwnBackgroundAndMusic_Kept` becomes `ToEncounter_OwnBackgroundAndMusic_WritesTheFiles`, both
  files in the folders.

## Verify by hand
None: no page yet (EC5). The suite runs green.
- **Done, with deviations:** about 220 lines with tests (code +74 -16, tests +138 -4, the map's two rows). Map settings'
  `ChoicesFor` was extracted at its second copy, not left private, as cheap and clean: `Catalogs/FileBoxChoices.Of`
  (18 lines, 3 tests in `Catalogs/FileBoxChoicesTests`), called by both view models. With every field now typed,
  `ToEncounter` builds a new record and the source field went. Two `Changed` facts instead of one theory, and
  `ToEncounter_Untouched_EqualsTheRecordButForItsEnemyList` added (EC3's no-step check). `EncounterViewModel` is 155
  lines, `EncounterViewModelBoxTests` 104; 7337 tests.
