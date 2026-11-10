# MF1. A map's folder in content and the Map settings window

Core + Data + Editor, about 175 lines. Builds on AL1 (`MapSettingsViewModel.Problem`), AL2 and design.md, "Key decisions" 8. Decision 532 (Q12; no ADR: only the editor reads the field).

## Goal
A map's settings may hold `folder`, a path of folder names joined by "/" ("Act 1/Village"); none means the top of the list. Content and the editor agree on the path rule (one place, Core). The Map settings window's Folder box edits it as one undo step with the other settings: a second way, the tree's menus and drag (MF4-MF5) being the main one; the list shows it from MF3 on.

## New types
- `Core/Maps/MapFolderPath` (public static): the folder path rule: names joined by "/", their checks and nesting.
  - `public const char Separator = '/';`
  - `public static string? ProblemOf(string path)`: null when every name between separators has text, none starts or ends with a space, and none holds "\"; else "A folder path is names joined by '/', each with text and no space at either end."
  - `public static IReadOnlyList<string> NamesOf(string path)`, `public static bool IsWithin(string path, string folder)` (the folder itself or under it, by whole names: "Act 1" holds "Act 1/Village", not "Act 10").
  - Its tests in `Core.Tests/Maps/MapFolderPathTests`.

## Changes
- `Core/Maps/MapSettings`: `/// <summary>Where the editor lists the map (decision 532), a <see cref="MapFolderPath"/>; null for the top of the list. The game never reads it.</summary> public string? Folder { get; init; }`
- `Data/Maps/MapSettingsValidator`: "folder: " + `MapFolderPath.ProblemOf` when not null (an empty string is refused too: none is null).
- `Data/Content/AbsentWhenNullFilter`: `(typeof(MapSettings), "folder")`, decision 532.
- `Editor/Maps/Settings/MapSettingsViewModel`: `FolderText` (empty for none), `FolderProblem`, carried by `ToSettings` and compared by `AreSame` (the guiding rule); `Problem` (AL1's) becomes `SizeProblem ?? OddsProblem ?? AreaLevelProblem ?? FolderProblem`, so `TryApplySettings`' guard and the window's greying need no change; `MapSettingsWindow.xaml`: a Folder row.
- `docs/map.md`: the Core Maps row names `MapFolderPath`.

## Tests
- `Core.Tests/Maps/MapFolderPathTests`: `ProblemOf_TwoNames_IsNull`; `ProblemOf_EmptyName_NamesTheRule` ("Act 1//Village"); `ProblemOf_TrailingSeparator_NamesTheRule`; `ProblemOf_SpaceAtAnEnd_NamesTheRule`; `ProblemOf_Backslash_NamesTheRule`; `IsWithin_Subfolder_IsTrue`; `IsWithin_SameStartLongerName_IsFalse` ("Act 10" in "Act 1"); `IsWithin_Itself_IsTrue`.
- `Data.Tests/Maps/MapSettingsValidatorTests`: `InvalidSettings` rows for "", "Act 1//Village", " Act 1"; `Validate_NestedFolder_DoesNotThrow`.
- `Data.Tests/Content/ContentWriterFolderTests` (new): `WriteMap_NoFolder_LeavesTheFieldOut`, `WriteMap_Folder_RoundTrips`.
- `Editor.Tests/Maps/Settings/MapSettingsFolderTests` (new): `FolderText_MapInAFolder_ShowsIt`, `ToSettings_FolderTyped_CarriesIt`, `ToSettings_Empty_HasNoFolder`, `AreSettingsChanged_OnlyTheFolderChanged_IsTrue`, `FolderProblem_BadPath_NamesTheRule`, `Problem_OnlyTheFolderWrong_IsTheFolderProblem`, `ToSettings_LevelBannerOffAndFolder_AllKept` (applying the dialog keeps the three new fields: the copy-back rule).

## Verify by hand
1. Open "forest"; Map > Settings...: type `Act 1/Woods` in Folder; OK; Ctrl+S: `content/maps/forest.json` holds `"folder": "Act 1/Woods"`; other files unchanged.
2. Type `Act 1//Woods`: the problem line shows, OK greyed.
3. Open Map > Settings... and change only the music: the folder stays in the file after Ctrl+S.
4. F5 the game: nothing changes in play.
