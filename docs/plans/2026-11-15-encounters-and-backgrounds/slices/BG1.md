# BG1. A map names its battle background; an encounter's own wins

Core, Data, Game, Editor (one line), content; about 260 lines with tests. Decisions 892-895. Design: "Which background a battle uses",
"Core changes".

## Goal
A map's settings name the background its battles are fought on (`battleBackgroundFile`); a map with random battles must
name one. An encounter's `backgroundFile` becomes optional and wins when present. The battle screen asks Core which one
to draw. The repository moves `Forest.png` from the encounters to the maps, so every battle looks as before.

## New types
- Core `Exploration/EncounterBackground` (static): which background a battle on a map is fought on.
  - `public static string? On(EncounterDefinition encounter, MapSettings map)`: the encounter's own, else the map's;
    null when neither names one.

## Changes
- Core `Maps/MapSettings`: `public string? BattleBackgroundFile { get; init; }`, declared just before
  `RandomEncounters`, its summary: the background, in the backgrounds folder, that the map's battles are fought on
  unless their encounter names its own (decisions 700, 892); required while the map has random battles.
- Core `Battle/EncounterDefinition.BackgroundFile`: `string?`, no longer `required`; summary: the encounter's own
  background, a boss's arena, winning over the map's; null for the map's.
- Core `Exploration/ExplorationState`: `public string? PendingBattleBackground => PendingEncounter is { } encounter ?
  EncounterBackground.On(encounter, CurrentMap.Settings) : null;` with a summary (276 lines, about +4).
- Data `Content/AbsentWhenNullFilter`: `(typeof(MapSettings), "battleBackgroundFile")`, so a map without one is
  written as it is today. (The encounter's filter comes with the writer, EC1.)
- Data `Maps/MapSettingsValidator`: after the folder check, `FindBackgroundProblem(settings)`:
  - a blank `battleBackgroundFile`: "battleBackgroundFile must not be blank; leave it out for none.";
  - `randomEncounters` set and no `battleBackgroundFile`: "randomEncounters needs battleBackgroundFile, the background
    its battles are fought on (decision 892).".
- Data `Battle/EncounterCatalogValidator`: a blank `backgroundFile` is refused: "backgroundFile must not be blank; leave
  it out for the map's."
- Data `Art/ArtReferenceCheck`: the three encounter-background lines move into a new partial
  `Art/ArtReferenceCheck.Backgrounds.cs`, `AddBackgroundProblems(problems, catalog, artRoot)`: every encounter's own
  background (named by "encounter '<id>'") and every map's battle background (named by "map '<id>'"), by exact name,
  maps in id order. `FindProblems`' summary says "every encounter's own background and every map's battle background".
- Game `Battle/BattleScene`: `_backgroundFile = session.Exploration.PendingBattleBackground ?? throw new
  InvalidOperationException("A battle screen needs the background its encounter is fought on.");` and an internal read
  accessor `BackgroundFile` for the tests (a test seam, as `ExplorationScene.TileAnimationTime`).
- Editor `Maps/Settings/MapSettingsViewModel.ToSettings`: `BattleBackgroundFile = _currentSettings.BattleBackgroundFile`,
  so OK in Map settings keeps the map's background (the dialog rebuilds its settings with `new()`; the map fixes plan's
  lesson). BG2 replaces the copy with its box.
- Content:
  - `content/encounters.json`: the four `backgroundFile` lines removed; the file rewritten in the writer's layout (one
    enemy id per line, ids in order, CRLF, trailing newline), so EC1's identical-bytes test passes when the writer
    takes it over.
  - `content/maps/milestone_outside.json`, `content/maps/primeval_growth_crossroad.json`:
    `"battleBackgroundFile": "Forest.png",` before `"randomEncounters"`.
  - `tests/TestWorld/maps/forest.json`, `tests/TestWorld/maps/to_aslas_1.json`: the same line.
- Existing tests to fix (budgeted, about 10 lines): `Data.Tests/Maps/MapSettingsValidatorTests` (115 lines) fixtures with
  a random table gain a background; grep `RandomEncounters =` and `"randomEncounters": {` in `tests/` once more. Core
  and Game tests build encounters with `BackgroundFile` set, which still wins: none should break; grep `new
  BattleScene(` in `Game.Tests` to confirm their catalogs reach a background.

## Tests
- `EncounterBackgroundTests` (Core, new; records built in the test):
  - `On_EncounterNamesItsOwn_ReturnsTheEncounters` (both name one, different files)
  - `On_EncounterNamesNone_ReturnsTheMaps`
  - `On_NeitherNamesOne_ReturnsNull`
- `ExplorationEncounterTests` (Core, 171 lines, about +25):
  - `PendingBattleBackground_AfterARollOnAMapWithABackground_IsTheMaps` (an encounter with none, a map naming
    "Map.png", built with `with` over the test world's forest)
  - `PendingBattleBackground_NoBattlePending_IsNull`
- `MapSettingsValidatorTests` (Data), rows of its invalid-settings theory:
  - random battles without `battleBackgroundFile`; a blank `battleBackgroundFile`.
  and facts:
  - `Validate_RandomBattlesWithABackground_Accepts`
  - `Validate_NoRandomBattlesAndNoBackground_Accepts`
- `EncounterCatalogValidatorTests` (Data, 47 lines):
  - `Validate_EncounterWithoutBackground_Accepts`
  - `Validate_BlankBackground_IsRefused`
- `ArtReferenceCheckTests` (Data, 222 lines: a sibling) → new `ArtReferenceCheckBackgroundTests`:
  - `FindProblems_MapBackgroundMissing_NamesTheMap`
  - `FindProblems_MapBackgroundWrongCase_IsAProblem`
  - `FindProblems_EncounterWithoutOwnBackground_NoProblem`
  - (the existing encounter-background test moves here unchanged, if it lives in `ArtReferenceCheckTests`)
- `BattleSceneTests` (Game, 196 lines, about +15): `Constructor_EncounterWithoutItsOwnBackground_UsesTheMaps` (the
  test world's forest settings `with` a background "Arena.png" and an encounter built with none, so the file asserted
  is one only the map names).
- `MapSettingsBattleBackgroundTests` (Editor, new; globbed: no such file): `ToSettings_MapWithABackground_KeepsIt`
  (a map built in the test naming "Arena.png").
- `RepositoryEncounterTests` (Data): `Load_RepositoryContent_RandomBattleMapsNameABackground` (asserts
  `milestone_outside` and `primeval_growth_crossroad` each name one, by id, not the file).

## Verify by hand
1. `dotnet run --project src/Jrpg.Game`, new game, walk `milestone_outside` until a battle: the forest background shows,
   as before. Same on `primeval_growth_crossroad`.
2. Edit `content/maps/milestone_outside.json` locally to remove its `battleBackgroundFile` line, start the game: the
   start stops naming `maps/milestone_outside.json` and "randomEncounters needs battleBackgroundFile". Revert.
3. `dotnet run --project src/Jrpg.Editor`, open `milestone_outside`, **Map > Settings...**, change the display name,
   **OK**, **File > Save**: `content/maps/milestone_outside.json` still has its `battleBackgroundFile` line. Undo,
   Save, and check `git status` shows the file unchanged.

- **Done, with deviations:** code +101 -15, tests +299 -30 (26 of them the two encounter-background tests moved
  unchanged), content +25 -12: about 330 lines net against the 260 planned, the excess in tests. The Core test
  builder gained `WithBattleBackground(mapId, file)`, since Core tests have no test world to build "with" over; the
  field map in `ExplorationEncounterTests` is fought on "Map.png" and its encounter names none. One test beyond the
  list: `ArtReferenceCheckBackgroundTests.FindProblems_TwoMapBackgroundsMissing_ListsThemInIdOrder` (maps put in the
  catalog in reverse order, for the maps-in-id-order sort). `ExplorationState.cs` is 281 lines.
