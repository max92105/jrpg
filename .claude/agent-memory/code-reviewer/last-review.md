---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-mode-bar, slice WS5 (the Database view hosts every page: CatalogGroup, DatabasePageViews,
ShowDatabasePage, menu entries as pages; decision 1010). 2026-11-17. Round 1. Verdict: Fix the Criticals.

- Critical: MainWindow.Database.cs:82 RegisterAnimatedPages misleading (Effect strips, Battle backgrounds animate but
  sit in RegisterPicturePages; the split is "takes the shared animation clock"); rename e.g.
  RegisterPagesOnTheAnimationClock, comment both pages, WS6.md:47 cites the name — open
- Suggestion: ShowDatabasePage from World runs Open() (builds last page + view) then ShowPage replaces it — open
- Suggestion: test ShowDatabasePageCommand_KnownTitle_IsLit -> _IsEnabled ("lit" = shown button) — open
- Suggestion: open-editor-windows.ps1:141 page not found reports "no window opened" -> "no page shown" — open
- WS4 Warning History.Refusal/SaveError stale after fix/revert — fixed (MainViewModel.Saving OnDatabaseWorkspaceChanged)
- WS4 Suggestion UnappliedPage name — fixed (UnappliedPageLine)
