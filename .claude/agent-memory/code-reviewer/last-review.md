---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice SV1 (save deadlock, decision 518), 2026-11-10. Round 1.
Verdict: fix the Criticals, then commit.

- Open (Critical): MainViewModel.Saving.cs:84 TryWriteTogether returns void; code-quality.md line 18 says Try...
  returns bool. Rename (SaveTogether) or return bool.
- Open (Suggestion): ContentWriterMapsTests WriteMaps_TwoMapsValidOnlyTogether comment says "each map refused against
  the other's saved file", but a forest only grown by rows passes alone; narrow it too, or reword.
- Open (Suggestion): MainViewModelSaveTogetherTests.SaveAll_OneMapRefused_WritesNoMapAndKeepsEveryMark repeats the
  re-pinned MainViewModelSavingTests.SaveAll_SecondMapRefused_WritesNeitherAndShowsTheReason set-up and asserts.
- Open (Suggestion): docs/map.md Shell line still says MainViewModel.Saving "decisions 499-501" (518 missing).
- Follow-up: docs/plans/backlog.md:44 names ContentWriter.WriteMap as "the editor's map save"; it is WriteMaps now.
- Previous review (Duplicate object, no plan) items not carried: different change.
