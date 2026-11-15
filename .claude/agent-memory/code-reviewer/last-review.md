---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-event-window-cleanup, slice EW0 (Debug binding trace in App.xaml.cs, open-editor-windows.ps1
-EventWindows and -Map). 2026-11-15. Round 1. Verdict: fix the Criticals. SE3's follow-ups (docs/map.md:71, :225)
rechecked in the tree: fixed. SE3's suggestions dropped (SE3 committed).

- Critical (open): App.xaml.cs TraceBindingErrors lacks PresentationTraceSources.Refresh(); probed (scratch WinExe,
  same two lines, no debugger, stderr redirected): no "System.Windows.Data Error" line without Refresh, the line
  appears with Refresh first. The Done note's "no binding-error line" baseline is a false negative; re-run after fix.
- Critical (open): open-editor-windows.ps1 has mixed line endings (lines 1-91 LF, 92-164 CRLF); staged blob ff33fa83
  keeps the CRs (ls-files i/-text), diff is a whole-file rewrite (214+/122-, 97+/5- ignoring CR). Fix with
  sed 's/\r*$/\r/' (hash then equals the LF-normalized 7595bec3).
- Suggestion (open): Open-EventWindows' left edge is window.Left + 280, but the Done note says it is taken from the
  map list's right; measure the tree's BoundingRectangle.Right or fix the note.
- Suggestion (open): editor.md line: the new clause sits between the PreviewWidth rule and its "(the cinematics
  window's PreviewWidth ...)" source, which now reads as the source of the binding-error clause.
