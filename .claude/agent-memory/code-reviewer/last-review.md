---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-editor-push, slice NET1 (unexpected-error net, RenderTick added at hand check). 2026-11-15. Round 1.
Verdict: fix the Criticals, then commit.

- Critical (open): docs/map.md Hosting row: path reads `%LocalAppData%Jrpgditorrrors` (backslashes eaten by the
  shell); should be `%LocalAppData%\Jrpg\Editor\Errors`.
- Critical (open): docs/plans/backlog.md: removing the DispatcherUnhandledException line also removed the heading
  "## From the drop table editor plan (2026-11-13, at approval)" and its blank line; restore them.
- Warning (open): decision 690's key (type, TargetSite) merges distinct errors thrown from a shared framework throw
  site (XamlParseException from WpfXamlLoader, ThrowHelper's ArgumentOutOfRange): the second broken window in one
  run gets no box, no report, and the script prints "no window opened". Developer's call (decision 690).
- Suggestion (open): MainViewModel.UnsavedChanges skips a stroke still being dragged (ConfirmClose ends it first);
  a throw mid-drag on a clean map makes the box say "Nothing is unsaved."
