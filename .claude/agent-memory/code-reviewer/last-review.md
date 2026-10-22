---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-21-editor/slices/E9c.md, E9c (opening the shapes window from the map; uncommitted)
Round: 1 (verdict: Ready to commit; verify all pass, 1469 tests)

- open (Warning): MainWindow.xaml MouseDoubleClick on the whole ObjectBrowserView: a double-click on the preview image, a group header or the list's scrollbar opens the chosen object's shapes; hook ListBoxItem (ItemContainerStyle EventSetter) instead.
- open (Warning): Shift + double-click: the first Shift press starts an unsnapped move, so 1 px of jitter before release moves the object ~1/48 cell and leaves an undo step, contradicting the comment and the hand check "nothing is moved"; start the move only past SystemParameters.MinimumHorizontal/VerticalDragDistance.
- open (Suggestion): placed-object-by-uid lookup now in StartEditingShapes and MoveObject; a private PlacedObjectWithUid(uid) would share it.
