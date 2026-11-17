---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-classes-and-progression, slice PM3 (SetPartyMembers, UsersOfPartyMember, RefusalOfPartyMemberUndo,
PartyMemberEditingViewModel). 2026-11-17. Round 1. Verdict: Ready to commit.

- PM2 Warning (ToMember art names untrimmed): fixed (PartyMemberViewModel.cs:102-104 trim).
- Follow-up, open: CombinedHistory.cs at 291 lines; only PM3's Done note says the next refusal needs a partial; add a
  backlog line.
- Follow-up, open: PM4's Remove-while-new on a member applied in an earlier session is a step, not an undo, so no
  refusal checks a map recruiting it; PM4 should refuse or note it.
