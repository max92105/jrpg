---
name: migration-roadmap
description: Plan status after the class-mechanics plan (done 2026-10-25); next decision/ADR numbers; open questions; later plans
metadata:
  type: project
---

Migration, code-health, new-game flow and class-mechanics plans are done (moved to `docs/plans/done/`); leftovers wait in `docs/plans/backlog.md`.

Class mechanics (`docs/plans/done/2026-10-24-class-mechanics/`, done 2026-10-25): MP gone (ADR 0022; until the first release a save format change bumps the version and refuses older saves), per-class resources (0019), class skills (0020), statuses (0021), battle formation (0023), lasting resources + rest through an inn step and a Camp Kit (0024). Fighter replaces warrior (id `fighter`). Save format is 3. Decisions 188-207, 229, 230; open questions 208-220 and 222-228; next free decision 231, next ADR 0025.

Still-open older questions: 147, 150-155, 92, 112, 179, 180, 182, 183. Later plans: other classes' lasting costs (220), enemy skills (224), gold/inn prices (222), speed-based turn order, more cutscene steps, map banner, autosave, event battles, animated battlers, NPC movement, music/SFX, loading screen, a map sprite per leader (58), the story, icon art (Move's icon, the extra turn's hourglass).

**Why:** the developer asked for real features after the cleanup; game design decisions stay theirs.

**How to apply:** continue numbering from 231/0025 (check the class-mechanics README first for answers that used numbers); reuse the vocabulary in `docs/map.md`. See [[content-quirks]], [[plan-lessons]].
