---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-encounters-and-backgrounds, slice BG7 (first part at its split point: Catalogs/ArtImageRows,
ArtImageRow, the Effect strips and Character sprites lists over it; BG7b holds the two Battle backgrounds view models).
2026-11-17. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (fixed in the BG7 commit): EffectStripCatalogViewModel sorts `images` once (`inNameOrder`).
- Follow-up (closed in BG6): FrameGridTests.WritePng_EveryFrame_IsAtItsRecordsFrameOrigin reads each frame back at
  FrameOrigin.
- Follow-up (recorded, drop): the BattleHeight rename is a note in slices/BG9.md.
