---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-20-battle/slices/B5d.md, B5d (effect strips PNGs, ArtFolders.Effects + partial-frame check, EffectStrip, PlaceholderCue, ActionPlayback Strips/Cues, ImpactEffectsView)
Round: 1 (verdict: Ready to commit; verify all pass, 1243 tests)

- open (Suggestion): PlaceholderCueTests names start `At_` but the method is `OpacityAt`; rename to `OpacityAt_...`.
- open (Suggestion): fade-in/out ramps of PlaceholderCue.OpacityAt untested (only plateau 1 and past-end 0); pin 0.5 at 0.075 s and 0.5 at 0.525 s.
- open (Suggestion): AddLanding's `BannerText ?? string.Empty` would show an empty cue for a non-damage effect from a bannerless Attack; unreachable today.
- follow-up: enemy attack strip drawn unmirrored over party members (no flip rule decided).
