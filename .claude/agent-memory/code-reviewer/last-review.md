---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-10-map-fixes-banner-and-folders, slice MB3 (the banner's look: serif name, hairline, LV label + number,
gradient band, slide; decisions 527-529), 2026-11-10. Round 1. Verdict: fix the Critical, then commit.

- Open (Critical): src/Jrpg.Game/Exploration/MapBannerView.cs:98-104, the hairline is drawn only after the no-level
  return, and _bandHeight (line 79) leaves its room out; the slice's goal and hand check 2 want name + hairline on a
  map without a level.
- Open (Warning): MapBannerView.cs:57,78,92,95, `BandWidth` is text + 2 * padding but the drawn band is text + padding
  + MapBannerRight (to the screen edge); Draw adds the padding then subtracts it back. Store the text width instead.
- Open (Warning): design.md tunables row 14 still names `map_banner_level` (removed); no rows for padding, label gap,
  band gradient that the slice asked for.
- Open (Suggestion): fractional slide moves the text by sub-pixels while the band/hairline truncate (1 px shimmer);
  round the slide.
- Open (Suggestion): the hairline's 1 px height is a bare literal in Update and twice in Draw.
- MB2 items fixed: StartMapScenarios SceneOf doc comment clean; ExplorationSceneBannerTests read BannerView.NameLine.
- start.json residue: the developer's hand test, excluded by the caller.
