---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice AL2 (map shows the entry banner, decision 528), 2026-11-10. Round 1.
Verdict: ready to commit. No Critical.

- Open (Warning): AbsentWhenTrueFilter.SkipTrueOptionalFields is the third copy of the Null/Zero filters' loop (only
  the predicate differs); share one loop taking the table and the absent test, keep the three tables.
- Open (Suggestion): MainViewModel.TryApplySettings summary (~line 229-230) lists the fields applied without the banner.
- Open (Suggestion): docs/map.md Content line (47) names the null and zero filters, not AbsentWhenTrueFilter.
- AL1 items fixed: PropertyChanged tests on size/odds exist, TryApplySettings names the area level, window summary
  lists the level, the extra Trim before TypedCounts.Parse is gone.
- start.json residue: the developer's hand test, excluded by the caller.
