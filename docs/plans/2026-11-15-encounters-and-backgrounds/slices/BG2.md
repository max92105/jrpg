# BG2. Map settings' Battle background box

Editor; about 150 lines with tests. Needs BG1. Decisions 892, 909. Design: "Which background a battle uses".

## Goal
Map > Settings... has a **Battle background** box beside the random battles: "(none)" first, then every image in the
backgrounds folder. Choosing one and OK sets the map's `battleBackgroundFile` as one undo step with the other fields.
With random battles on and "(none)" chosen, the problem line says so and OK is greyed.

## New types
None.

## Changes
- `Maps/Settings/MapSettingsViewModel` (173 lines, about +30):
  - `public const string NoBattleBackground = "(none)";`
  - the constructor takes a fourth parameter, `IReadOnlyList<string> backgroundFiles` ("the images in the backgrounds
    folder, so only a file that exists can be chosen"); `BattleBackgroundChoices` built as `MusicChoicesFor` builds the
    tracks (a file no longer in the folder stays listed, decision 909): extract the shared rule as
    `ChoicesFor(string? current, string none, IReadOnlyList<string> files)` and call it for both;
  - `[ObservableProperty] SelectedBattleBackground` notifying `BattleBackgroundProblem`, `Problem`, `CanApply`;
    `HasRandomEncounters` also notifies `BattleBackgroundProblem`;
  - `BattleBackgroundProblem`: "Random battles need a battle background." while `HasRandomEncounters` and "(none)";
  - `Problem` chains it after `OddsProblem`; its summary lists it;
  - `ToSettings`: `BattleBackgroundFile = SelectedBattleBackground == NoBattleBackground ? null :
    SelectedBattleBackground` (replaces BG1's copy); `AreSame` compares it; the class summary names the background.
- `Maps/Settings/MapSettingsWindow.xaml` (74 lines, about +6): a "Battle background" label and combo box bound to
  `BattleBackgroundChoices`/`SelectedBattleBackground`, under the random battles' encounter list; its tooltip: "The
  picture this map's battles are fought on, unless an encounter names its own (the Encounters page). The backgrounds
  folder also holds the title's and the menu's pictures."
- `Shell/MainViewModel.MapSettings` (58 lines): passes `ArtFolders.ImagesIn(ArtRoot, ArtFolders.Backgrounds)`.
- Test constructor sites (three, `MapSettingsFolderTests`, `MapSettingsBannerTests`, `MapSettingsAreaLevelTests`, plus
  `MapSettingsViewModelTests`' helper): pass `[]`.

## Tests
`MapSettingsBattleBackgroundTests` (Editor, created by BG1; maps built in the test with `with` over the test world's
forest):
- `Constructor_MapNamingABackground_SelectsIt`
- `Constructor_MapWithoutABackground_SelectsNone`
- `Constructor_BackgroundNoLongerInTheFolder_StaysListed`
- `ToSettings_NoneChosen_WritesNull`
- `ToSettings_ImageChosen_WritesIt` (replaces BG1's `ToSettings_MapWithABackground_KeepsIt`, whose fixture differs
  from the choice)
- `Problem_RandomBattlesAndNone_NamesTheBackground`
- `Problem_RandomBattlesOffAndNone_IsNull`
- `Problem_RandomBattlesAndAnImage_IsNull`
- `HasRandomEncounters_TurnedOn_RaisesProblemChanged` (with "(none)" chosen)
- `AreSettingsChanged_OnlyTheBackgroundChanged_IsTrue`

## Verify by hand
1. `.claude/scripts/open-editor-windows.ps1` (its default list already opens Map > Settings...): no `ERROR` line, no
   binding-error line in the error output.
2. Run the editor, open `milestone_outside`, **Map > Settings...**: the **Battle background** box shows `Forest.png`.
   Choose **(none)**: the problem line reads "Random battles need a battle background." and OK is grey. Untick
   **Random battles**: the line goes, OK is lit. Cancel.
3. Open `milestone` (a town, no random battles), **Map > Settings...**, choose `Forest.png`, **OK**, **Ctrl+S**: its
   file gains `"battleBackgroundFile": "Forest.png"` before `"randomEncounters"`. **Ctrl+Z**, **Ctrl+S**: `git status`
   shows it unchanged.
