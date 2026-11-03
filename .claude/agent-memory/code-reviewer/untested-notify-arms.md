---
name: untested-notify-arms
description: PropertyChanged relay/hook arms (OnXChanged, OnPartChanged filters) ship with only one arm tested
metadata:
  type: project
---

A view model that relays a part's PropertyChanged (a `nameof(A) or nameof(B)` filter, or an `OnXChanged` hook) tends to
ship with a test for one arm only; deleting the others passes every test. Seen in CO2 (RowsText hook) and CO3
(ObjectComposerViewModel.OnPartChanged: only IdProblem pinned).

**Why:** the window binds the relayed property (Save's enabled state, the problem line); a missing arm shows a stale
problem the developer notices at once.

**How to apply:** for each arm whose stale value blocks a user action, check a test changes that part and asserts the
relayed name is raised. Warning when the arm gates Save; Suggestion otherwise. Related: [[tautological-tests]].
