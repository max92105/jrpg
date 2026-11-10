---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice AL1 (map area level, decision 527), 2026-11-10. Round 1.
Verdict: ready to commit. No Critical.

- Open (Warning): CanApply greys OK but only AreaLevelText's raise of Problem/CanApply is tested; the Done note says
  "tested through the level and the size", yet Problem_SizeWrong_IsTheSizeProblem asserts the value, not the event.
  Add a PropertyChanged test on WidthText (tests.md line 32) or correct the Done note.
- Open (Suggestion): MainViewModel.TryApplySettings summary (~line 228) lists "the name, the track and the random
  battles", not the area level; MapSettingsWindow.xaml.cs summary names only the size as a refusal reason.
- Open (Suggestion): MapSettingsViewModel AreaLevelProblem/ToSettings call .Trim() before TypedCounts.Parse, which
  trims itself.
- UO3 Suggestions (EventEditingCellTests chest coverage, ControlsHelpWindow line order) not carried: new slice.
- start.json residue: the developer's hand test, excluded by the caller.
