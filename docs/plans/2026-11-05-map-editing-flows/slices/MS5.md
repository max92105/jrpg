# MS5. The map list's filter box and Back to previous map

Builds on design.md, "Key decisions" 5; decisions 504 and 505; B2. Needs MS0 (the window's MapSwitching partial and the styles' room) and MS1 (maps stay open: Back only switches).

## Goal
A filter box sits over the map list: typing narrows the list to the maps whose display name or id holds the text (any case); Enter opens the first map shown; Esc clears the filter and gives the focus back to the map; Ctrl+M (also Map > Go to map...) focuses the box with its text selected. Map > Back to previous map (Alt+Left) shows the map shown before the current one; pressed again, it returns. Neither asks anything (maps stay open). Back is greyed when there is no previous map or it is no longer listed. Whatever the filter, the shown map ends up selected in the list whenever it is listed.

## New types
- `Editor/Shell/MapNavigation` (`internal sealed partial class : ObservableObject`): the map list as filtered, and the map shown before, for Back.
  - `MapNavigation(Func<string, bool> tryOpenMap)`.
  - `[ObservableProperty] string Filter` (raises `ShownEntries`); `IReadOnlyList<MapEntry> ShownEntries` (the rows whose `Text` holds the filter, ignoring case, in the list's order; all with an empty filter); `string? PreviousMapId`.
  - `void ShowEntries(IReadOnlyList<MapEntry> entries)` (also re-raises `GoBackCommand`'s `CanExecute`); `void NoteShown(string? mapId)` (the shown map becomes the previous one when another is shown; the same map again changes nothing).
  - `bool TryOpenFirstShown()` (Enter); `[RelayCommand(CanExecute = nameof(CanGoBack))] void GoBack()`; `CanGoBack`: a previous id that one of the entries still has (B2).

## Changes
- `MainViewModel.MapSwitching.cs`: `public MapNavigation Navigation => field ??= new(TryOpenMap);`; `ShowMapList` hands it the rows; `OnCurrentMapChanged` calls `Navigation.NoteShown(newValue?.Map.Id)`. `MainViewModel.cs` is not touched.
- `Shell/MainWindow.xaml`: over `MapList`, in a `DockPanel`: `<TextBox x:Name="MapFilterBox" DockPanel.Dock="Top" Text="{Binding Navigation.Filter, UpdateSourceTrigger=PropertyChanged}" ToolTip="Type part of a map's name or id; Enter opens the first one (Ctrl+M)" PreviewKeyDown="OnMapFilterKeyDown" />`; `MapList.ItemsSource="{Binding Navigation.ShownEntries}"`; Map menu: `Go to map...` (`InputGestureText="Ctrl+M"`) and `Back to previous map` (`Command="{Binding Navigation.GoBackCommand}" InputGestureText="Alt+Left"`); key bindings Ctrl+M (a `RoutedCommand` the partial handles: focus and select all) and Alt+Left. About +12 lines, ends about 277.
- `Shell/MainWindow.MapSwitching.cs`: `OnMapFilterKeyDown` (Enter: `TryOpenFirstShown`; Esc: clear the filter, focus the map; Down: focus the list); the Ctrl+M handler; `OnMapListUpdated`, which calls `ReselectShownMap` whenever the list's rows change: the list's binding is `ItemsSource="{Binding Navigation.ShownEntries, NotifyOnTargetUpdated=True}"` with `TargetUpdated="OnMapListUpdated"`, so it runs after the rows are in (a filter typed, a mark added, a save). The `MapEntries` case of `OnViewModelChanged` in `MainWindow.xaml.cs` leaves (that file shrinks by 4 lines).
- `Shell/ControlsHelpWindow.xaml`, View: "Ctrl+M: filter the map list by name or id; Enter opens the first map shown, Esc clears it." and "Alt+Left: back to the previous map; again to return."

## Tests (new `Shell/MapNavigationTests`, on `MapEntry` rows built in code and a recording `tryOpenMap`)
- `ShownEntries_EmptyFilter_ShowsEveryRowInOrder`.
- `ShownEntries_FilterInTheName_ShowsOnlyMatchingRows` (`"mill"` keeps "Old Mill (old_mill)", drops "Forest (forest)").
- `ShownEntries_FilterInTheIdOnly_ShowsTheRow` (`"interior"` keeps "Inn (interior_1)").
- `ShownEntries_FilterInAnotherCase_ShowsTheRow`.
- `Filter_Changed_RaisesShownEntries`.
- `TryOpenFirstShown_TwoMatches_OpensTheFirst`; `TryOpenFirstShown_NoMatch_OpensNothingAndReturnsFalse`.
- `GoBackCommand_NoMapShownBefore_CannotExecute`.
- `GoBack_AfterShowingAnother_ShowsThePrevious`; `GoBack_Twice_ReturnsToTheSecondMap`.
- `NoteShown_SameMapAgain_KeepsThePrevious`.
- `GoBackCommand_PreviousNoLongerListed_CannotExecute` (B2: entries shown again without it; the command's `CanExecuteChanged` raised).
- `MainViewModelOpenMapsTests: GoBack_MapLeftUnsaved_ShowsItWithItsChanges` (through `MainViewModel.Navigation`; nothing asked).

Size: about 55 lines of code, XAML 12, handlers 35; 13 tests (about 115): about 215. The handlers are view-only keys.

## Map
`docs/map.md`, Editor `Shell` row: `MapNavigation` (the map list's filter and Back to previous map).

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5).
2. Press Ctrl+M: the box over the map list has the focus. Type part of a map's name (`for`): only matching maps show. Type part of an id instead: it matches too. Press Enter: the first shown map opens, and it is the one selected in the list.
3. Press Esc in the box: the filter clears, the whole list shows, the map just opened still selected.
4. Click map A, then map B in the list. Press Alt+Left: A shows (with any unsaved change you left). Alt+Left again: B. Map > Back to previous map does the same and shows "Alt+Left".
5. Make a new map (File > New map...): it is listed and selected. Alt+Left goes back to the map before; Alt+Left again shows the new map.
6. Type in the tool bar's layer box and press Alt+Left: the caret stays in the box.

- **Done, with deviations:** Ctrl+M runs WPF's `Find` command (so Ctrl+F also focuses the filter box) in place of a new `RoutedCommand`, which a XAML `x:Static` on the internal window would need. The list's `TargetUpdated` only sets the selection (the old `UpdateTarget` there would raise it again); `MapNavigation` keeps the previous map private. One test beyond the plan: rows replaced while filtered keep the filter. The look is unchecked by capture (hand check). Review: the editor-level Back test asserts the command can run (the rows reach `MapNavigation`).
