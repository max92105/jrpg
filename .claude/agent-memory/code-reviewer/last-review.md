---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice RN3 (MapRenameWindow, Map > Rename map..., map list context
menu), 2026-11-10. Round 1. Verdict: ready to commit.

- Open (Warning): MapListPanel.xaml.cs:73-80 OnMapListRightButtonDown (+ its XAML hook and comment, MapListPanel.xaml:11,
  18) repeats what ListBoxItem already does: a right button down selects the row (probed: RaiseEvent of
  MouseRightButtonDown on item 1 with no handler -> SelectedIndex 1). Remove; MF3's tree needs its own (TreeViewItem
  doesn't select on right click).
- Open (Suggestion): a refused write leaves the dialog open with nothing in it saying why (SaveError is on the main
  window's "Not saved:" line behind it).
- Checked fine: ContextMenu DataContext reaches the main view model on open (probed); ContainerFromElement on empty
  space returns null, no throw; greyed entries raise no Click and StartRenamingMap returns null as backstop.
- RN2 items fixed: own-id case comparison (OrdinalIgnoreCase), CatalogDraft Rewriting/ReplaceAfterRewrite name the
  rename, TryRenameMap_IdWithAProblem_WritesNothing exists.
- Follow-up (carried, not in backlog): ContentWriter.RenameMap: an IOException on the old files' File.Delete leaves the
  new files and rewritten warps on disk while TryWrite reports failure and the editor keeps the old content.
- start.json residue: the developer's hand test, excluded by the caller.
