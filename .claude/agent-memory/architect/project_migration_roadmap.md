---
name: migration-roadmap
description: Plan status after the merge of the two machines (2026-10-27); next decision/ADR numbers; open questions; later plans
metadata:
  type: project
---

Migration, code-health, new-game flow, class-mechanics, UI second pass and escape menu third pass plans are done (`docs/plans/done/`); leftovers wait in `docs/plans/backlog.md`.

Escape menu third pass (`docs/plans/done/2026-10-26-escape-menu-third-pass/`, done 2026-10-27): decisions 266-272, 282-290 and 333-336 (F1, the developer's first look: Info button, section bands, Status redone, every pointer glides; first numbered 291-294 by mistake, renumbered at the retro); no ADR.

Stats and gear slots (`docs/plans/2026-10-26-stats-and-gear-slots/`, approved 2026-10-26): decisions 291-302 and 316-332 (questions 303-315 all answered), ADRs 0026 (stat model), 0027 (damage types), 0028 (equipment slots); drafted on 267-308 on the other machine and moved up 24 at the merge. Next free decision 337, next ADR 0029 (both kept in `docs/plans/README.md`'s "Next free numbers" line). Stale stubs to delete: slices/W1.md, slices/G1a.md.

Direction (decision 234): an investor demo. Developer wants ARPG-style loot: the next plan after stats is rarity, affix rolls, drop tables, salvage, materials, crafting (decision 294). Later: speed-based turn count (301, with enemy speed), music/SFX, Act 1 content, map banner, autosave, enemy skills (224), gold/inn prices (222), other classes' lasting costs (220), skills for dead stat dimensions (310).

Still-open older questions: 92, 112, 147, 151-155, 179, 180, 182, 183, 208-220, 222-228.

**Why:** game design decisions stay the developer's; numbering must continue without gaps.

**How to apply:** take numbers from `docs/plans/README.md`'s "Next free numbers" line and bump it in the same change; reuse `docs/map.md` vocabulary. See [[content-quirks]], [[plan-lessons]].
