---
name: stale-derived-ui-text
description: Editor notes/labels/list rows computed at an action aren't refreshed on every path that changes their inputs (undo, redo)
metadata:
  type: feedback
---

A note, label or list row computed once at an action keeps describing state that has since changed:
ObjectPicking.Note after the picked object moved (UO1) or the lock/layer changed (UO2).

A false alarm to learn from (MF4): the Critical "the map list is rebuilt only when IsDirty flips, so undo of a move
leaves the row in the old folder" was wrong. MapViewModel.Refresh raises PropertyChanged(IsDirty) on every step, undo
and redo, whatever the value, so OpenMaps.DirtyChanged (now documented so) rebuilds the rows every time; the undo test
passed with no change. The developer reverted the settings event built on the claim.

**Why:** flagged twice in the map-fixes plan; the stale text tells the developer something false. The false alarm cost
a revert.

**How to apply:** for each new text/row a view model derives, list the inputs it reads and check every path that
changes each one (the action, its undo and redo, another window's edit) refreshes it. Before calling a refresh
missing, trace the event from where it is raised, not from its summary: "raised when X changed" often means "raised
on every step". Confirm with a run or a test (`dotnet test` with a filter) before marking it Critical.
