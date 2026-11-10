---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice MF3 (map list panel as a TreeView, MapTree.PathTo,
EditorTreeTemplates.xaml), 2026-11-10. Round 1. Verdict: fix the Criticals.

- Open (Critical): MapListPanel.xaml the Rename map... item binds `PlacementTarget.Tag.*` but the Tag is on the Grid
  and the menu on the TextBlock: PlacementTarget.Tag is null (probed), so never greyed, no tip. Tag onto the TextBlock.
- Open (Critical): MapListPanel.xaml.cs NoteExpanded: a generated container with IsExpanded=true raises Expanded
  (probed), so a filter notes every shown folder open; Esc leaves them open (hand check 5). Ignore while Filter set + test.
- Open (Warning): SelectMap returns early for null or a map not in the shown tree, leaving a stale row selected (filter
  "for", open forest, Alt+Left: forest still highlighted, a click on it doesn't reopen); doc says "null selects none".
- Open (Suggestion): name the template parts PART_Header / ItemsHost as WPF's TreeViewItem expects.
- Follow-up: a session folder stays after a map moves in and out again (decision 534) - for MF4.
- MF2 items: IsWithin test (fixed, Of_SiblingFolderSharingAPrefix), filter predicate twice (fixed, Matches),
  MapTreeNode factories (fixed), nested session folder test (fixed).
- Checked fine: TryOpenMap re-entrance (same id returns early, no rebuild), TreeExpander IsChecked two-way to
  TemplatedParent, BasedOn on the ItemContainerStyle, PathTo.
- start.json residue: the developer's hand test, excluded by the caller.
