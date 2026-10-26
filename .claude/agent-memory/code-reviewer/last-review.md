---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-25-ui-second-pass/slices/M3a.md, slice M3a (Status page: speed, resources, skills)
Round: 1 (verdict: Ready to commit)

- W1 StatusSheet.AmountText speed-scaled max (rogue Readiness) has no test; add Open_Rogue_ShowsReadinessChargesOfSpeed with a golden value; open
- S1 StatusLabels: six empty <param> tags, and Messages' doc says "the format" for a whole UiMessages; open
- S2 StatusPage.ShownSpeed lacks the summary its sibling accessors have; open
- F1 Readiness max rule now lives in Core ResourceStarts.StartOf and Game StatusSheet.AmountText; a Core "max for member" read would join them; follow-up
- (C4's S1, move sheet tests to StatusSheetTests: the M3a slice put its tests in StatusPageTests; not re-raised)
