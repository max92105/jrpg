---
name: migration-roadmap
description: Where to find plan status and numbers; the game's direction; still-open questions
metadata:
  type: project
---

Plan status lives in `docs/plans/README.md`, each plan's decisions and ADRs in its own README, and leftovers in `docs/plans/backlog.md`. Quests (save format 7), the enemy skills and bosses plan and the music and SFX plan were all done on 2026-11-01; settings kept between runs (decision 415) is the next known plan. The sound pass (`2026-11-02-sound-pass`) was done on 2026-11-02; the Mana Seed reset (`done/2026-11-03-mana-seed-reset`) was done on 2026-11-03: the maps were wiped for the Mana Seed art (decision 453) and the developer paints them as content work; the object composer and animated tiles plan (`2026-11-03-object-composer-and-animated-tiles`) is next; the developer wants every future placeholder sound from freesound.org (`.claude/rules/content.md`).

2026-11-03: the developer considers every system in; the next work is content and art in the Mana Seed family (16 px, drawn 3x at the 48 px cell), packs kept outside the repository; plan `2026-11-03-mana-seed-reset` approved the same day (decisions 438-450, ADRs 0037 test world, 0038 art sources; test world, art manifest, missing-art tolerance, removal, re-skin, rebuild). Characters, monsters, autotiles and lighting are later plans. `2026-11-03-object-composer-and-animated-tiles` approved the same day (decisions 455-471, ADR 0039): animation = `frameCount` + `frameMilliseconds` rightward from the tile's cell (all 13 Mana Seed animations), imported from the pack's Tiled `.tsx`; the editor's map stays still (backlog). Pack animations are overlays on their own layers, not object parts.

Rule seen at that approval: a review that adds features (pickers, previews, saved stocks) roughly doubles a plan; renumber slice ids to reuse file names, stub the orphans.

Direction (decision 234): an investor demo. Later: speed-based turn count (301, with enemy speed), Act 1 content, map banner, autosave, a battle event step (decision 399), gold/inn prices (222), other classes' lasting costs (220), skills for dead stat dimensions (310).

Still-open older questions: 92, 112, 147, 151-155, 179, 180, 182, 183, 208-220, 222, 223, 225-228 (224 answered by the enemy skills plan).

**Why:** game design decisions stay the developer's; numbering must continue without gaps.

**How to apply:** take numbers from `docs/plans/README.md`'s "Next free numbers" line and bump it in the same change; reuse `docs/map.md` vocabulary. See [[content-quirks]], [[plan-lessons]].
