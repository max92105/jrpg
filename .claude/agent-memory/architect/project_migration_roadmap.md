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

Enemy skills and bosses (`docs/plans/2026-10-29-enemy-skills-and-bosses/`, draft 2026-10-29, drafted beside three other plans: local Q1-Q12, unnumbered proposed decisions, caller adds the README row): T1, T2, D1, C1, B1-B4, S1; answers question 224 once approved.

Quests (`docs/plans/2026-10-29-quests/`, draft 2026-10-29, same parallel-draft rules: local Q1-Q13): QD, QL, QS, QC, QV, SV, J1, G1, E1-E3, N1, G2-G4, R1+G5 (notices, only if Q2 keeps them); two ADR candidates (condition placement; quest log vs story flags).

Gold and shops (`docs/plans/2026-10-29-gold-and-shops/`, approved 2026-10-29, runs BEFORE quests): decisions 374-385, ADR 0031 (gold and trade, a step before G3); 27 slices G1-G4, S1-S6, L1, H1, E1, E2, N1, B1-B3, L2a, L2b, T1-T3, I1, I2a, I2b, I3; save format 6 (gold + rolled shop stocks), quests takes 7. Core `Trade/` (`Shop` buys, `ItemSales` sells, `Pricing`, `ShopStock` rolled once and rerolled after a leader level), `Party/Purse`; Game `Trade` may read `Menus`. Next free after it: decision 386, ADR 0032.

Music and SFX (`docs/plans/2026-10-29-music-and-sfx/`, draft 2026-10-29, same parallel-draft rules: local Q1-Q10): V1, C1, C2, P1-P3, E1, S1-S4, B1, B2, D1, F1, V2, V3 (only if Q1 saves settings); ADR candidates: audio names (`content/audio.json` + Data `SoundEffectIds`), how the game asks (music from flow state, scenes report effect ids via `IScene.Sounds`), music playback (looped `SoundEffectInstance` WAV vs `Song` OGG), settings file.

Rule seen at that approval: a review that adds features (pickers, previews, saved stocks) roughly doubles a plan; renumber slice ids to reuse file names, stub the orphans.

Direction (decision 234): an investor demo. Later: speed-based turn count (301, with enemy speed), Act 1 content, map banner, autosave, enemy skills (224), gold/inn prices (222), other classes' lasting costs (220), skills for dead stat dimensions (310).

Still-open older questions: 92, 112, 147, 151-155, 179, 180, 182, 183, 208-220, 222-228.

**Why:** game design decisions stay the developer's; numbering must continue without gaps.

**How to apply:** take numbers from `docs/plans/README.md`'s "Next free numbers" line and bump it in the same change; reuse `docs/map.md` vocabulary. See [[content-quirks]], [[plan-lessons]].
