---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-mode-bar, slice WD6 (Battle/EffectStripCatalogPage in CatalogDialog via OpenInDialog; RenderTick on
Loaded/Unloaded; EffectStripCatalogViewModel implements IEditedCatalog, CanApply removed; HasEffectStripEditing
removed). 2026-11-16. Round 1. Verdict: Ready to commit. No Critical, no Warning.

- Suggestion, open: the Loaded/Unloaded RenderTick wiring and its comment are now identical in EnemyCatalogPage.xaml.cs
  and EffectStripCatalogPage.xaml.cs; WD7 (Cinematics) would make a third copy: give RenderTick a method tying it to
  an element's Loaded/Unloaded before then.
- Fixed (from WD5): EnemyCatalogPage.xaml.cs comment on Start after Unloaded reworded.
- Fixed (from WD5): art-src/README.md now says "the Enemies page's Import...".
- Checked: no EffectStripCatalogWindow / HasEffectStripEditing / CanApply on the effect strips left in src, tests, map,
  rules, smoke script; CatalogDialog greys OK on Problem and shows the problem line; Choose falls back to SelectedStrip.
