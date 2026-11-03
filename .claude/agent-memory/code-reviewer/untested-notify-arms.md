---
name: untested-notify-arms
description: PropertyChanged relay/hook arms and [NotifyCanExecuteChangedFor] ship untested or with one arm tested
metadata:
  type: project
---

A view model that relays a part's PropertyChanged (a `nameof(A) or nameof(B)` filter, or an `OnXChanged` hook) tends to
ship with a test for one arm only; deleting the others passes every test. Seen in CO2 (RowsText hook), CO3
(ObjectComposerViewModel.OnPartChanged: only IdProblem pinned), AE2 (TileAnimationViewModel's
NotifyCanExecuteChangedFor: tests query CanExecute directly, never the CanExecuteChanged event; a command whose
CanExecute reads the catalog isn't re-raised on catalog change at all).

**Why:** the window binds the relayed property (Save's enabled state, the problem line); a missing arm shows a stale
problem the developer notices at once. Toolkit RelayCommand doesn't hook CommandManager, so a CanExecute reading state
nobody notifies stays stale in WPF.

**How to apply:** for each arm whose stale value blocks a user action, check a test changes that part and asserts the
relayed name (or CanExecuteChanged) is raised. Warning when the arm gates Save or a stale state is reachable by
Ctrl+Z/Ctrl+Y; Suggestion otherwise. Related: [[tautological-tests]].
