---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-26-stats-and-gear-slots/slices/M2b-2.md, slice M2b-2 (defense leaves members, items, pages;
Attack -> AttackDamage on CombatStats and StatSheet; MemberSheet.Of takes int? weaponDamage)
Round: 1 (verdict: Ready to commit; verify all pass, 2473 tests)

- S1 StatusSheet.cs:21-23 Combat param block not rewrapped after "Defense ..." removal (85-col first line); open (suggestion)
- S2 PartyMember.cs:39-42 Sheet summary lists primaries, max HP, speed, percents; misses attack damage; open (suggestion)
- S3 StatusParametersView.cs:37-38 vs Draw's resources Y: same offset written twice; a ResourcesTop const; open (suggestion)
- F1 StatusPageTests.ShownStats_Member_HasNoMp: name no longer says what it asserts (HP + weapon damage); follow-up
- F2 docs/decisions/README.md:23 ADR 0017 line still says attack and defense come from the class; follow-up
