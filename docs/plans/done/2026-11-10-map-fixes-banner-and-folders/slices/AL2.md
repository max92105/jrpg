# AL2. Whether a map shows the entry banner

Core + Data + Editor, about 150 lines. A sibling of AL1, which would pass about 250 lines with it. Builds on AL1 (`MapSettingsViewModel.Problem`, the settings window's new rows). Decision 528 (Q9).

## Goal
A map's settings say whether entering it shows the banner: `showsBanner`, true by default and left out of the file while true, so every existing map shows it and only a map the developer unticks (an intro stage, a cutscene room) carries `"showsBanner": false`. The Map settings window's **Show banner on entry** box edits it as one undo step with the other settings. MB1 reads it.

## New types
- `Data/Content/AbsentWhenTrueFilter` (static): leaves a true-by-default field added later out of a file while it is true, so every file that predates it writes back byte for byte. One entry: `(typeof(MapSettings), "showsBanner")`, decision 528. Registered where `AbsentWhenNullFilter` and `AbsentWhenZeroFilter` are (the content JSON options' modifiers).

## Changes
- `Core/Maps/MapSettings`: `/// <summary>Whether entering the map shows its name and level at the top right (decision 528); true unless the developer turned it off.</summary> public bool ShowsBanner { get; init; } = true;`
- `Editor/Maps/Settings/MapSettingsViewModel`: `[ObservableProperty] ShowsBanner` from the map; `ToSettings` carries it, `AreSame` compares it.
- `Editor/Maps/Settings/MapSettingsWindow.xaml`: a "Show banner on entry" check box under Area level, tooltip "Entering this map shows its name and area level at the top right for a few seconds."
- No validator change (a bool has no bad value).

## Tests
- `Data.Tests/Content/ContentWriterBannerTests` (new):
  - `WriteMap_ShowsBanner_LeavesTheFieldOut` (the test world's settings byte for byte).
  - `WriteMap_BannerOff_WritesFalse` (the text holds `"showsBanner": false`).
  - `Load_SettingsWithoutTheField_ShowsTheBanner` (an old file loads as true: the initializer kept by the serializer).
  - `WriteMap_BannerOff_RoundTrips`.
- `Editor.Tests/Maps/Settings/MapSettingsBannerTests` (new):
  - `ShowsBanner_MapWithBannerOff_IsFalse` (the fixture differs from the default true).
  - `ToSettings_BannerUnticked_CarriesFalse`.
  - `AreSettingsChanged_OnlyTheBannerChanged_IsTrue`.
  - `ToSettings_LevelAndBannerOff_BothKept` (the copy-back rule over both new fields).
- `Editor.Tests/Shell/MainViewModelMapSettingsTests` (AL1's file): `TryApplySettings_BannerUnticked_Undo_TicksItAgain`.

- **Done, with deviations:** `AbsentWhenTrueFilter` is registered after the null and zero filters in the content JSON options; the test world's start settings file writes back byte for byte and loads as showing the banner. From the review: the three filters' loop was a third copy, so `OptionalFieldFilter.SkipWhen` holds it and each filter keeps its table and its reasons.

## Verify by hand
1. Open the intro map ("intro"); Map > Settings...: Show banner on entry is ticked; untick it; OK; Ctrl+S: `content/maps/intro.json` holds `"showsBanner": false`; no other settings file changed.
2. Tick it again, OK, Ctrl+S: the field leaves the file.
3. Whether the intro map keeps the banner is the developer's content choice; MB2's hand check shows the effect in the game.
