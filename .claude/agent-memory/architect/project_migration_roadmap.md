---
name: migration-roadmap
description: Plan status after the merge of the two machines (2026-10-27); next decision/ADR numbers; open questions; later plans
metadata:
  type: project
---

Migration, code-health, new-game flow, class-mechanics, UI second pass, escape menu third pass and stats and gear slots plans are done (`docs/plans/done/`); leftovers wait in `docs/plans/backlog.md`.

Escape menu third pass (`docs/plans/done/2026-10-26-escape-menu-third-pass/`, done 2026-10-27): decisions 266-272, 282-290 and 333-336 (F1, the developer's first look: Info button, section bands, Status redone, every pointer glides; first numbered 291-294 by mistake, renumbered at the retro); no ADR.

Stats and gear slots (`docs/plans/done/2026-10-26-stats-and-gear-slots/`, done 2026-10-27): decisions 291-302, 316-332 and 337, ADRs 0026 (stat model), 0027 (damage types), 0028 (equipment slots). Numbers are kept in `docs/plans/README.md`'s "Next free numbers" line.

Loot (`docs/plans/done/2026-10-27-loot/`, done 2026-10-29): decisions 338-355, 359-361 and 362 (tier colors: ARPG convention), ADRs 0029/0030 option A. Split off: `2026-10-27-affix-stats/` (approved 2026-10-29 after review: A1-A3, A4a/A4b, ST1a/ST1b; decisions 356, 363-368; stone_beetle practice enemy in the forest) and `2026-10-27-forge/` (approved 2026-10-29, runs after affix stats: 15 slices M1-M4, F1a, E1, E2, N1, F1b-F1d, F2a-F2c; decisions 357, 358, 369-372; Core `Forge` returns `ForgeQuote`s; forge master on the start map). Tiers: Common/Magic/Rare rolled, Unique reserved, Material a color only (teal).

Direction (decision 234): an investor demo. Later: speed-based turn count (301, with enemy speed), music/SFX, Act 1 content, map banner, autosave, enemy skills (224), gold/inn prices (222), other classes' lasting costs (220), skills for dead stat dimensions (310).

Still-open older questions: 92, 112, 147, 151-155, 179, 180, 182, 183, 208-220, 222-228.

**Why:** game design decisions stay the developer's; numbering must continue without gaps.

**How to apply:** take numbers from `docs/plans/README.md`'s "Next free numbers" line and bump it in the same change; reuse `docs/map.md` vocabulary. See [[content-quirks]], [[plan-lessons]].
