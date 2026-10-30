---
name: migration-roadmap
description: Plan status (forge done 2026-10-29); next decision/ADR numbers; open questions; later plans
metadata:
  type: project
---

Migration, code-health, new-game flow, class-mechanics, UI second pass, escape menu third pass, stats and gear slots, loot, affix stats and forge plans are done (`docs/plans/done/`); leftovers wait in `docs/plans/backlog.md`.

Escape menu third pass (`docs/plans/done/2026-10-26-escape-menu-third-pass/`, done 2026-10-27): decisions 266-272, 282-290 and 333-336 (F1, the developer's first look: Info button, section bands, Status redone, every pointer glides; first numbered 291-294 by mistake, renumbered at the retro); no ADR.

Stats and gear slots (`docs/plans/done/2026-10-26-stats-and-gear-slots/`, done 2026-10-27): decisions 291-302, 316-332 and 337, ADRs 0026 (stat model), 0027 (damage types), 0028 (equipment slots). Numbers are kept in `docs/plans/README.md`'s "Next free numbers" line.

Loot (`docs/plans/done/2026-10-27-loot/`, done 2026-10-29): decisions 338-355, 359-361 and 362 (tier colors: ARPG convention), ADRs 0029/0030 option A. Split off: `done/2026-10-27-affix-stats/` (done 2026-10-29: A1-A3, A4a/A4b, ST1a/ST1b; decisions 356, 363-368 and 373 (tighter Status skill rows); stone_beetle practice enemy in the forest) and `done/2026-10-27-forge/` (done 2026-10-29: M1-M4, F1a, E1, E2, N1, F1b-F1d, F2a-F2c; decisions 357, 358, 369-372; Core `Forge` returns `ForgeQuote`s, the Game formats them; forge master on the start map). Next free: decision 374, ADR 0031. Tiers: Common/Magic/Rare rolled, Unique reserved, Material a color only (teal).

Enemy skills and bosses (`docs/plans/2026-10-29-enemy-skills-and-bosses/`, approved 2026-10-29): decisions 399-414, ADR 0034 (tactics option A); slices T0, T1, D1, C1, T2, T3, S1, B1, B4, B2, B3; answers question 224. Gold-and-shops and quests approved the same day (numbers before 399); music-and-sfx draft. Next free: decision 415, ADR 0035.

Quests (`docs/plans/2026-10-29-quests/`, approved 2026-10-29, runs AFTER gold): decisions 386-398, ADRs 0032 (quest log; non-quest story state = main-quest stage, decision 398) and 0033 (conditions gate whole events, five kinds incl. reachedStage); 23 slices QD, QL, QS, R1 (ExplorationState split, unconditional), QC, QV (+ no-shadowing rule), E1-E3, N1, SV (format 7), J1, G1-G3, L1-L3, G4 (notices), QE1-QE4 (Quests editor window). Quests tab first in the menu; Active/Completed via Left/Right. Next free after it: decision 399, ADR 0034.

Gold and shops (`docs/plans/2026-10-29-gold-and-shops/`, approved 2026-10-29, runs BEFORE quests): decisions 374-385, ADR 0031 (gold and trade, a step before G3); 27 slices G1-G4, S1-S6, L1, H1, E1, E2, N1, B1-B3, L2a, L2b, T1-T3, I1, I2a, I2b, I3; save format 6 (gold + rolled shop stocks), quests takes 7. Core `Trade/` (`Shop` buys, `ItemSales` sells, `Pricing`, `ShopStock` rolled once and rerolled after a leader level), `Party/Purse`; Game `Trade` may read `Menus`. Next free after it: decision 386, ADR 0032.

Music and SFX (`docs/plans/2026-10-29-music-and-sfx/`, approved 2026-10-29): decisions 415-425, ADR 0035 (extends 0018); 18 slices V1, C1, P1-P3, E1, C2 (after the enemy plan's C1), S1-S4, D1, B1-B5, V2. Settings saved between runs = a separate small plan later (decision 415). Next free after it: decision 426, ADR 0036.

Rule seen at that approval: a review that adds features (pickers, previews, saved stocks) roughly doubles a plan; renumber slice ids to reuse file names, stub the orphans.

Direction (decision 234): an investor demo. Later: speed-based turn count (301, with enemy speed), Act 1 content, map banner, autosave, a battle event step (decision 399), gold/inn prices (222), other classes' lasting costs (220), skills for dead stat dimensions (310).

Still-open older questions: 92, 112, 147, 151-155, 179, 180, 182, 183, 208-220, 222, 223, 225-228 (224 answered by the enemy skills plan).

**Why:** game design decisions stay the developer's; numbering must continue without gaps.

**How to apply:** take numbers from `docs/plans/README.md`'s "Next free numbers" line and bump it in the same change; reuse `docs/map.md` vocabulary. See [[content-quirks]], [[plan-lessons]].
