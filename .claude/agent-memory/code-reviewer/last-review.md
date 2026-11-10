---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice MF6 (Rename folder, written at once), 2026-11-10. Round 1.
Verdict: ready to commit (no Critical). Plan's last slice; status retro pending.

- Open (Warning): MapNavigation.Follow removes/adds one at a time, so the result depends on set order: expanded
  {"A","A/A"}, rename "A" to "A/A" gives {"A/A/A"} not {"A/A","A/A/A"} (renamed folder shows collapsed). Fix:
  ExceptWith(within) then UnionWith(moved).
- Open (Suggestion): MapFolderChoiceViewModel.Problem nested ternary on one ~170-col line; MapsMoved a noun name.
- Open (Suggestion): docs/map.md MapListPanel entry still lists "Rename map, Move to folder and New folder".
- Open (Suggestion): TryRenameFolder's refused-write branch untested (SaveError set, catalog and folders unchanged).
- Follow-up (carried from MF5, not in backlog): MF4's ~150-170-column lines in MainViewModel.MapFolders.
- Fixed since MF5: drag payload private format (MapRowFormat); row menus via ItemContainerStyle and MenuFolder set in
  ContextMenuOpening; New folder at the top greys OK (NeedsAName); MainWindow.MapFolders partial exists.
- start.json residue: the developer's hand test, excluded by the caller.
