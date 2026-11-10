---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice MF5 (MapFolderWindow, MapListPanel folder menus and drag),
2026-11-10. Round 1. Verdict: ready to commit (no Critical).

- Open (Warning): drag payload is a plain string with Move effect: text dragged from the filter box or another app is
  accepted and cut from its source. Fix: private DataObject format.
- Open (Warning): row menus sit on the template's TextBlock/StackPanel, so a right click right of a row's name and
  Shift+F10 get the tree's one-item menu, and MenuFolder (set on right button down) is stale for the keyboard. Fix:
  menus on the TreeViewItem via ItemContainerStyle, MenuFolder set in ContextMenuOpening.
- Open (Warning): New folder on the empty space opens on "(top of the list)" with OK lit; OK does nothing, no reason.
- Open (Suggestion): folder dialog handlers in a MainWindow.MapFolders partial instead of MapSwitching.
- Follow-up: MF4's ~150-170-column lines (MainViewModel.MapFolders TryMoveMap/KnownFolders, Folders initializer).
- Accepted by the Done note: Move to folder acts on the shown map; Choose returns nothing.
- start.json residue: the developer's hand test, excluded by the caller.
