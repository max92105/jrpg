---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice PM2 (PartyMemberViewModel, PartyMemberCatalogViewModel, headless).
2026-11-17. Round 1. Verdict: Ready to commit.

- Warning, open: PartyMemberViewModel.cs:103-105 ToMember copies the three art names untrimmed; EnemyArtViewModel.cs:114-116
  trims the same three fields (typed "Omer.png " is written with the space).
- Not raised (slice note records it): Changed via one OnPropertyChanged deny-list override instead of per-field hooks.
- Follow-ups dropped (already in backlog.md): RecordListProblem's two blank ids "listed twice"; catalog list copies.
