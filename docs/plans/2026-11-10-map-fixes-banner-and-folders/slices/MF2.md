# MF2. `MapTree`: the map list as folders and maps

Editor, about 200 lines. Builds on MF1 and design.md, "Key decisions" 8. Decisions 534, 535 (Q14, Q15).

## Goal
The map list's rows arranged into a tree: folders first (by name, any case), then maps (by display name, then id, as today), each folder holding its subfolders and maps; the session's empty folders included; while the filter holds text, only the matching maps inside their folders, every folder open. Rules only: the view comes in MF3.

## New types
- `Maps/MapTreeNode` (observable class): one row of the map tree as the tree shows it: a folder with its rows, or a map.
  - `Text` (a folder's last name, or the map entry's text), `FolderPath` (a folder's full path; null for a map), `Map` (`MapEntry?`), `Rows` (`IReadOnlyList<MapTreeNode>`), `[ObservableProperty] IsExpanded`, `[ObservableProperty] IsSelected`.
  - `IsFolder => FolderPath is not null`.
- `Maps/MapTree` (static): the map rows arranged into folders, narrowed by a filter.
  - `public static IReadOnlyList<MapTreeNode> Of(IReadOnlyList<MapEntry> entries, IReadOnlyCollection<string> sessionFolders, string filter, IReadOnlySet<string> expandedFolders)`: the top rows. A filtered tree opens every folder shown and leaves the empty session folders out; an unfiltered one opens the folders in `expandedFolders`.
  - `public static IEnumerable<MapEntry> MapsInOrder(IReadOnlyList<MapTreeNode> rows)`: depth first, as shown; Enter in the filter opens its first.

## Changes
- `Maps/MapEntry`: gains `string? Folder` (from the map as it is now, so an unsaved move shows at once); `MapEntry.Of` fills it; the text is unchanged.
- `Shell/MapNavigation` (67 lines): `ShownEntries` stays (Back and its tests read it); new `[ObservableProperty] ShownTree` rebuilt with it; `TryOpenFirstShown` opens the first of `MapTree.MapsInOrder(ShownTree)`; it holds the session folders (`AddSessionFolder(string path)`, decision 534) and the expanded paths (`NoteExpanded(string path, bool isExpanded)`, so a rebuild after a save keeps folders open).
  - Its summary: "the map list as filtered and as a tree, the session's empty folders, and the map shown before".

## Tests
`Editor.Tests/Maps/MapTreeTests` (new; entries built in the test, given in the opposite of the expected order):
- `Of_MapsWithoutFolders_AtTheTop`.
- `Of_NestedFolder_HoldsItsMaps` ("Act 1/Village" under "Act 1").
- `Of_FoldersBeforeMaps_EachSortedByName`.
- `Of_FolderNamesDifferingByCase_SortedIgnoringCase`.
- `Of_SessionFolderEmpty_Shown`.
- `Of_SessionFolderEmpty_HiddenWhileFiltering`.
- `Of_Filter_KeepsOnlyMatchingMapsAndTheirFolders`.
- `Of_Filter_OpensEveryFolderShown`.
- `Of_NoFilter_OpensOnlyTheExpandedFolders`.
- `MapsInOrder_Nested_DepthFirst`.
`Editor.Tests/Shell/MapNavigationTreeTests` (new; `MapNavigationTests` is at 154 and stays):
- `TryOpenFirstShown_MapInAFolderFirstInTheTree_OpensIt`.
- `ShowEntries_FolderWasExpanded_StaysExpanded`.
- `AddSessionFolder_ShowsItInTheTree`.
`Editor.Tests/Maps/MapEntryTests` (or the file that tests `MapEntry.Of`): `Of_OpenMapMovedUnsaved_TakesTheNewFolder`.

## Verify by hand
Rules only; MF3 shows the tree.
