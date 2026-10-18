---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-17-world-exploration-core.md, 5a (Session snapshot and play time)
Round: 1 (verdict: Ready to commit)

Accepted (don't re-raise): PlayTime set by object initializer in Restore; worn items put straight on the member (ADR 0003, save assumed repaired by Data 5c); Capture throws during event/pending battle (ADR 0007); WorldState ordinal ordering; restore resets grace steps/touch arming like a warp (plan: "enters through WarpArrival"); SaveGame record holds lists (5b compares element by element).

- open (Warning): restored ObjectSwaps never asserted on the restored side (no WorldState restore-ctor test, session test only checks save.ObjectSwaps); dropping the SwapObject loop keeps all tests green
- open (Suggestion): WorldState CompletedEvents/ObjectSwaps ordinal order untested (single element in every test)
- open (Suggestion): PartyMember.Progress() is a noun method; consider CaptureProgress() to pair with GameSession.Capture (plan named it Progress, developer's call)
- follow-up: ADR 0007 envelope key "save" vs plan 5b "game"; ADR says FileSaveStore reads against a ContentCatalog, plan 5b Read(slotName) has none
