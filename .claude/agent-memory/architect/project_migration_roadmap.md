---
name: migration-roadmap
description: Plan status after the stats-and-gear-slots draft (2026-10-26); next decision/ADR numbers; open questions; later plans
metadata:
  type: project
---

Migration, code-health, new-game flow, class-mechanics and UI second pass plans are done (`docs/plans/done/`); leftovers wait in `docs/plans/backlog.md`.

Stats and gear slots (`docs/plans/2026-10-26-stats-and-gear-slots/`, approved 2026-10-26): decisions 267-278 and 292-308 (questions 279-291 all answered), ADRs 0026 (stat model), 0027 (damage types), 0028 (equipment slots). Next free decision 309, next ADR 0029. Stale stubs to delete: slices/W1.md, slices/G1a.md.

Direction (decision 234): an investor demo. Developer wants ARPG-style loot: the next plan after stats is rarity, affix rolls, drop tables, salvage, materials, crafting (decision 270). Later: speed-based turn count (277, with enemy speed), music/SFX, Act 1 content, map banner, autosave, enemy skills (224), gold/inn prices (222), other classes' lasting costs (220), skills for dead stat dimensions (286).

Still-open older questions: 92, 112, 147, 151-155, 179, 180, 182, 183, 208-220, 222-228.

**Why:** game design decisions stay the developer's; numbering must continue without gaps.

**How to apply:** continue numbering from 309/0029 (check the stats plan's README first in case review renumbered); reuse `docs/map.md` vocabulary. See [[content-quirks]], [[plan-lessons]].
