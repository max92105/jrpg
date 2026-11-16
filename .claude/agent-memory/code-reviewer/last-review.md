---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice AN1b (Editor Animation/FramePreview + PreviewFrames.OfStrip,
PreviewFramesTests; EffectStripCatalogPage on the shared control; EffectStripViewModel FrameMilliseconds/HitFrame).
2026-11-16. Round 1. Verdict: Ready to commit.

- Warning (open): PreviewFrames.OfStrip's `frameCount <= 0` guard untested since the deleted
  PreviewLoop_ImageTallerThanWide_IsNone; a taller-than-wide effects image gives FrameCount 0 and without the guard
  `% 0` throws. Add OfStrip_NoFrames_IsEmpty.
- Suggestion (open): FramePreview.OnRendering comment says "once per frame"; WPF can raise Rendering twice with the
  same RenderingTime, which is why `now > last`; say so.
- Suggestion (open): EffectStripViewModel LengthText/IsListed/ToStrip/HoldsTheLoadedRecord/HitFrameProblem still
  parse the texts again; read the new FrameMilliseconds/HitFrame.
- Checked, no finding: partial-frame strip (width not a multiple of height) shows nothing in the preview, but Data's
  ArtReferenceCheck.AddPartialFrameStrips refuses such a strip, so not raised.
- AN1a Suggestions: _elapsed comment and FrameCount summary fixed in code; mid-frame pause test not carried
  (committed slice, Suggestion only).
