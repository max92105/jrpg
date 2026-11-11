---
name: migration-roadmap
description: Where to find plan status and numbers; the game's direction; still-open questions
metadata:
  type: project
---

Plan status lives in `docs/plans/README.md`, each plan's decisions and ADRs in its own README, and leftovers in `docs/plans/backlog.md`. Quests (save format 7), the enemy skills and bosses plan and the music and SFX plan were all done on 2026-11-01; settings kept between runs (decision 415) is the next known plan. The sound pass (`2026-11-02-sound-pass`) was done on 2026-11-02; the Mana Seed reset (`done/2026-11-03-mana-seed-reset`) was done on 2026-11-03: the maps were wiped for the Mana Seed art (decision 453) and the developer paints them as content work; the object composer and animated tiles plan (`2026-11-03-object-composer-and-animated-tiles`) is next; the developer wants every future placeholder sound from freesound.org (`.claude/rules/content.md`).

2026-11-03: the developer considers every system in; the next work is content and art in the Mana Seed family (16 px, drawn 3x at the 48 px cell), packs kept outside the repository; plan `2026-11-03-mana-seed-reset` approved the same day (decisions 438-450, ADRs 0037 test world, 0038 art sources; test world, art manifest, missing-art tolerance, removal, re-skin, rebuild). Characters, monsters, autotiles and lighting are later plans. `2026-11-03-object-composer-and-animated-tiles` approved the same day and done the same day (decisions 455-471, ADR 0039): animation = `frameCount` + `frameMilliseconds` rightward from the tile's cell (all 13 Mana Seed animations), imported from the pack's Tiled `.tsx`; the editor's map stays still (backlog). Pack animations are overlays on their own layers, not object parts. 2026-11-05: `2026-11-05-object-layers-and-sheet-zoom` approved and done the same day (decisions 472-487, ADR 0040: object tile `level`; `CellZoom` for the sheet and canvas). `2026-11-05-map-editing-flows` approved the same day (decisions 497-516, no ADR, 24 slices): the developer refused a save prompt on map switch; maps stay open in memory with their undo (OpenMaps), Save all + a three-way close question.

Rule seen at the 2026-11-05 approval: a review that adds features (pickers, previews, saved stocks) roughly doubles a plan; renumber slice ids to reuse file names, stub the orphans.

2026-11-10: `2026-11-10-map-fixes-banner-and-folders` approved the same day (decisions 518-535, ADRs 0041 draw order, 0042 map id rename): save deadlock, canopy layer order (no depth offset: the developer's houses and trees sit on z -3 to -5, the layer alone is the model), reaching covered objects, area level + per-map `showsBanner` + entry banner, map id rename (reverses question 70), map folders as a settings field managed from a tree. A field only the editor reads gets a decision, not an ADR.

2026-11-11: `2026-11-11-code-health-cleanup` (editor + Data) approved (decisions 536-549; V1 is the developer's verify.ps1 edit for 130-column code lines); `2026-11-11-game-code-health-cleanup` approved the same day (decisions 550-558; next free 559), run after the editor/Data one.

Direction (decision 234): an investor demo. Later: speed-based turn count (301, with enemy speed), Act 1 content, map banner, autosave, a battle event step (decision 399), gold/inn prices (222), other classes' lasting costs (220), skills for dead stat dimensions (310).

Still-open older questions: 92, 112, 147, 151-155, 179, 180, 182, 183, 208-220, 222, 223, 225-228 (224 answered by the enemy skills plan).

**Why:** game design decisions stay the developer's; numbering must continue without gaps.

**How to apply:** take numbers from `docs/plans/README.md`'s "Next free numbers" line and bump it in the same change; reuse `docs/map.md` vocabulary. See [[content-quirks]], [[plan-lessons]].
