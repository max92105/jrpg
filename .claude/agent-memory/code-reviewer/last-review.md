---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice AN2 (GifImport/ComposedGif, GifImportOptions.FrameMilliseconds,
GifConversion PaceNoteOf, PreviewFrames.OfPixels, GifImportOptionsViewModel factories, GifImportWindow.AskFor +
FramePreview, EnemyCatalogPage.GifOptionsFor). 2026-11-16. Round 1. Verdict: Ready to commit.

- Warning (open, unprobed): a GIF whose screen descriptor reads 0 wide or high (GifReader takes the stored ushort)
  composes to empty frames; GifImportWindow.ShowGif's BitmapSource.Create(0, ...) (or BackgroundRemoval frames[0][0])
  throws in the window's constructor, outside AskFor's catch. Fix: ComposedGif.Read throws InvalidDataException on an
  empty canvas.
- Suggestion (open): test ComposedGif.Read on a one-frame GIF (its doc and the slice state it is read).
- Suggestion (open): GifImportWindow.xaml comment "the box stays the only one" unclear.
- Suggestion (open): EnemyCatalogPage.GifOptionsFor's switch repeats ArtImageImport.TakesGifs' folder list.
- Checked, no finding: catch covers composing (GifFrameComposer/GifFrameTiming can't throw past GifReader's checks);
  other-folder branch equals the old `!TakesGifs || no gif` -> Default.
- AN1b carried: OfStrip_NoFrames_IsEmpty fixed; FramePreview Rendering comment fixed; EffectStripViewModel re-parsing
  texts still open (Follow-up, committed slice).
