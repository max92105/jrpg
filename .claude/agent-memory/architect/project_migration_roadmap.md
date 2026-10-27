---
name: migration-roadmap
description: Plan status after the UI second pass was done (2026-10-26); next decision/ADR numbers; open questions; later plans
metadata:
  type: project
---

Migration, code-health, new-game flow and class-mechanics plans are done (`docs/plans/done/`); leftovers wait in `docs/plans/backlog.md`.

UI second pass (`docs/plans/done/2026-10-25-ui-second-pass/`, done 2026-10-26): capture key S0, first-menu-open bug BG1, `docs/ui-reference/` rename, sizing pass, one selection look everywhere (decision 252), title, framed windows, menu motion + page time step, status content, member column, tab glyphs, glass-shatter encounter. Decisions 231-235 and 249-264; open 265; next free 266. ADR 0025 = font roles (Cormorant Garamond); next ADR 0026.

Direction (decision 234): an investor demo (clean code, UI/UX, some animation, music, playable Act 1, no bugs). Later plans: music/SFX, Act 1 content, map banner, autosave, enemy skills (224), gold/inn prices (222), other classes' lasting costs (220), speed-based turn order, skill costs on the Status page (SkillCostText needs battle meters).

Still-open older questions: 92, 112, 147, 151-155, 179, 180, 182, 183, 208-220, 222-228.

**Why:** the developer asked for a UI/clean-up pass after the class mechanics; game design decisions stay theirs.

**How to apply:** continue numbering from 267/0026 (check the UI plan's README first); reuse `docs/map.md` vocabulary. See [[content-quirks]], [[plan-lessons]].
