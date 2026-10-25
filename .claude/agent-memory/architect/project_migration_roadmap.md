---
name: migration-roadmap
description: Plan status after the new-game flow; the approved class-mechanics plan; next decision/ADR numbers; open questions; next plans
metadata:
  type: project
---

Migration, code-health and new-game flow plans are done (moved to `docs/plans/done/`); leftovers wait in `docs/plans/backlog.md`.

Class mechanics plan approved 2026-10-24 (`docs/plans/2026-10-24-class-mechanics/`): MP removed from the game (ADR 0022, first save migration), per-class resources (0019), class skills (0020), statuses (0021), battle formation for paladin aura ranges (0023), lasting resources + rest via inn step and rest item (0024). Fighter replaces warrior (id `fighter`). Decisions 188-207, open questions 208-227; next free decision 228, next ADR 0025.

Still-open older questions: 147, 150-155, 92, 112, 179, 180, 182, 183. Later plans: other classes' lasting costs (220), enemy skills (224), gold/inn prices (222), speed-based turn order, more cutscene steps, map banner, autosave, event battles, animated battlers, NPC movement, music/SFX, loading screen, a map sprite per leader (58), the story.

**Why:** the developer asked for real features after the cleanup; game design decisions stay theirs.

**How to apply:** continue numbering from 228/0025 (check the class-mechanics README first for answers that used numbers); reuse the vocabulary in `docs/map.md`. See [[content-quirks]], [[plan-lessons]].
