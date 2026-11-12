---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-12-character-sprites-and-npcs, slice FS1 (field-sprites.json, FieldSpriteSheet/FacingRows/FacingCycle,
FieldSpriteCatalogValidator, FieldSpriteReferenceCheck, ContentFileReader missing-file message).
2026-11-12. Round 1. Verdict: ready to commit. No Critical.

- Warning (open): ContentCatalogLoaderFieldSpriteTests.cs:23-24 pins live content's player numbers (48,48,1,0 / 8,100);
  ADR 0037 says repository tests pin ids, never cells; move the numbers to the frozen test world or assert ids only.
- Earlier Suggestions from decision 592 fix (PlacedObjectHits "topmost first" wording, MainViewModel `?? []`): other work, not carried.
