# AL1. A map's area level in content and the Map settings window

Core + Data + Editor, about 185 lines. Builds on design.md, "What exists" (map settings). Decision 527 (Q8).

## Goal
A map's settings may hold `areaLevel`, the party level the area is made for, one whole number from 1; a town has none. The Map settings window edits it (empty for none), as one undo step with the other settings. Nothing in the game reads it until MB1.

## Changes
- `Core/Maps/MapSettings`: `/// <summary>The party level the area is made for, shown beside its name on entering (decision 527); null for none, such as a town. Display only.</summary> public int? AreaLevel { get; init; }`
- `Data/Maps/MapSettingsValidator`: "areaLevel must be at least 1 (was 0)." through `RangeProblems.AtLeast` when not null.
- `Data/Content/AbsentWhenNullFilter`: `(typeof(MapSettings), "areaLevel")` with decision 527 as its reason.
- `Editor/Maps/Settings/MapSettingsViewModel` (128 lines, ends about 155):
  - `AreaLevelText` (observable, empty for none), `AreaLevelProblem` ("The area level is empty or a whole number of at least 1."), through `TypedCounts.Parse(text, int.MaxValue)`.
  - `ToSettings` carries it; `AreSame` compares it (the guiding rule: a field copied in is copied back).
  - `public string? Problem => SizeProblem ?? OddsProblem ?? AreaLevelProblem;` (notified with each of them): what stops OK, one property AL2 needs nothing from and MF1 adds `FolderProblem` to.
  - Its summary keeps "the id can't change" until RN2 rewrites it.
- `Editor/Maps/Settings/MapSettingsWindow.xaml` (64 lines): an "Area level" row with the box; the problem line and OK's greying read `Problem`.
- `Shell/MainViewModel.TryApplySettings` (in `MainViewModel.cs`, one line changed, no line added): the guard becomes `CurrentMap is not { } open || edited.Problem is not null || edited.NewSize is not { } size`.

## Tests
- `Data.Tests/Maps/MapSettingsValidatorTests`: rows in `InvalidSettings` for `AreaLevel = 0` and `-1`; `Validate_AreaLevelOne_DoesNotThrow`; no level is the existing `Validate_ValidSettings_DoesNotThrow` (say so, no new test).
- `Data.Tests/Content/ContentWriterAreaLevelTests` (new, as `ContentWriterMusicTests`): `WriteMap_NoAreaLevel_LeavesTheFieldOut` (the test world's settings byte for byte), `WriteMap_AreaLevel_RoundTrips` (7).
- `Editor.Tests/Maps/Settings/MapSettingsAreaLevelTests` (new):
  - `AreaLevelText_MapWithLevel_ShowsIt` (a map built with 7).
  - `ToSettings_LevelTyped_CarriesIt` (12, not a default).
  - `ToSettings_Empty_HasNoLevel`.
  - `AreSettingsChanged_OnlyTheLevelChanged_IsTrue`.
  - `AreaLevelProblem_Zero_NamesTheRule`; `AreaLevelProblem_One_IsNull`; `AreaLevelProblem_NotANumber_NamesTheRule`.
  - `Problem_OnlyTheLevelWrong_IsTheLevelProblem`; `Problem_PropertyChangedOnLevelTyped_IsRaised`.
- `Editor.Tests/Shell/MainViewModelMapSettingsTests` (new; `MainViewModelTests` is at 292): `TryApplySettings_AreaLevelTyped_SetsItAsOneStep`, `TryApplySettings_AreaLevelTyped_Undo_RemovesIt`, `TryApplySettings_LevelProblem_AppliesNothing`.

- **Done, with deviations:** OK's greying binds a `CanApply` bool (`Problem is null`), since a string can't enable a button without a converter; the area level's own problem line sits under its box, as the size's and the odds' do, and `Problem` is what `TryApplySettings` checks. Every property feeding `Problem` notifies it and `CanApply`, each with a raised-event test (the level, the two size boxes, the battles box and its two odds boxes; the review asked for the last five). The test world's start settings file writes back byte for byte without the field.

## Verify by hand
1. Open "forest"; Map > Settings...: the new Area level box is empty; type 3; OK; the map row gets " *"; Ctrl+S.
2. `content/maps/forest.json` holds `"areaLevel": 3`, laid out as the writer lays it out; other maps' files are unchanged.
3. Map > Settings... again: 3 shows; type 0: the problem line shows and OK is greyed; clear the box, OK: the field leaves the file on Ctrl+S.
4. Ctrl+Z after setting it: the box shows the old value when reopened.
