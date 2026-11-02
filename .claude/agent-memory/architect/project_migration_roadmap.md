---
name: migration-roadmap
description: Where to find plan status and numbers; the game's direction; still-open questions
metadata:
  type: project
---

Plan status lives in `docs/plans/README.md`, each plan's decisions and ADRs in its own README, and leftovers in `docs/plans/backlog.md`. Quests (save format 7), the enemy skills and bosses plan and the music and SFX plan were all done on 2026-11-01; settings kept between runs (decision 415) is the next known plan. The sound pass (freesound placeholders, `2026-11-02-sound-pass`) was done on 2026-11-02 (the developer heard every sound and kept them all); the developer wants every future placeholder sound from freesound.org (`.claude/rules/content.md`).

Rule seen at that approval: a review that adds features (pickers, previews, saved stocks) roughly doubles a plan; renumber slice ids to reuse file names, stub the orphans.

Direction (decision 234): an investor demo. Later: speed-based turn count (301, with enemy speed), Act 1 content, map banner, autosave, a battle event step (decision 399), gold/inn prices (222), other classes' lasting costs (220), skills for dead stat dimensions (310).

Still-open older questions: 92, 112, 147, 151-155, 179, 180, 182, 183, 208-220, 222, 223, 225-228 (224 answered by the enemy skills plan).

**Why:** game design decisions stay the developer's; numbering must continue without gaps.

**How to apply:** take numbers from `docs/plans/README.md`'s "Next free numbers" line and bump it in the same change; reuse `docs/map.md` vocabulary. See [[content-quirks]], [[plan-lessons]].
