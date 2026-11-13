---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Post-plan editor bug fixes after 2026-11-12-character-sprites-and-npcs (decisions 594, 595): import counts as
edited (FieldSpriteCatalogViewModel.RefreshAfterImport/EditedSetups), FieldSpriteWindow Grid + splitter + WorkArea
cap, NPCs menu. 2026-11-13. Round 1. Verdict: ready to commit.

- EN7 items: battler IsReadable guard, SelectedEnemy!, ImportArtImages shared, all fixed; ArtStatus follow-up is in
  backlog.md (dropped).
- Warning (open): an import whose guess has a problem greys OK for the rest of the session; nothing un-counts it, so
  earlier edits need a junk permanent record (decision 569) or Cancel. Developer question.
- Warning (open): FieldSpriteWindow WorkArea cap keeps CenterOwner; a non-maximized or second-monitor editor still
  places the window off-screen (WPF doesn't clamp CenterOwner; WorkArea is the primary monitor's).
- Suggestion (open): docs/map.md Sprites row "Problem naming another opened setup's" -> edited; mention imports count.
- Suggestion (open): ListBox item ToolTip repeats a label the wrap already shows whole.
