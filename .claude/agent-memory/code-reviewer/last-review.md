---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice NG2b first part (move greying + row watch extraction, leader list;
starting items split to NG2c). 2026-11-17.
Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Suggestion, open: the "Problem raised once per list change" rule is asserted as "raised", not counted.
- Suggestion, open: Leaders rows are value-equal RecordChoice records; IndexOf-based Remove/Move/greying would act on
  the first of two equal rows (only reachable from a repeated leader the loader refuses, so not a Warning).
- Follow-up, open: the seven other ListItemWatch.Follow callers (Statuses, SkillCosts, SkillGrants, ItemEquipment,
  UniqueAffixes, TacticList, EnemyTactics, Cinematic) keep their own initial loop + subscription; WatchRowsOf fits.
- NG2 items: TypedDecimal exponent suggestion fixed (AllowExponent); culture-test suggestion not carried;
  CinematicStillRow follow-up dropped (backlog.md lists it).
