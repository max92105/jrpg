---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice AN4 (EffectStripCatalogViewModel.TakeImport/ImportLineOf, Strips and
SwingChoices ObservableCollections, EffectStripCatalogPage Import..., GifImportOptionsViewModel.ForEffectStrips,
GifConversion.ConvertToEffect loop + PacedEffectNoteOf, Catalogs/ArtImageFileDialog). 2026-11-16. Round 1.
Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open): TakeImportedImage's insert index counts rows with FrameCount not null; a short why comment
  (missing-image rows sit after every folder row) would help.
- Suggestion (open): PacedEffectNoteOf says "OK writes its record" even at a typed 80 ms, the unlisted pace, which
  OK writes no record for.
- Follow-up: replacing an effect image with fewer frames, then Cancel, leaves the draft's record with a hit frame past
  the strip (ArtReferenceCheck.AddHitFramesPastTheirStrip refuses at the game's start); by decision 988, also via the
  Enemies page's effects import.
- Checked, no finding: decision 988 holds structurally (the VM holds the immutable ContentCatalog; ImportArtImages
  touches no record, only Art.Refresh); size lookup re-reads the PNG; dialog titles/filters unchanged for Enemies and
  Character sprites (TakesGifs(FieldSprites) false).
- AN2 carried: ComposedGif 0-wide Warning fixed (ComposedGif.Read throws); EffectStripViewModel re-parsing dropped
  (backlog lists it); AN2 suggestions dropped (committed slice).
