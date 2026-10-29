# Affix stats
Status: draft

Split out of [the loot plan](../done/2026-10-27-loot/README.md) on 2026-10-27, when the developer approved it: the four stats decision 294 named as affixes, and the Status page showing them (decision 350). Slices A1-A4 come from the loot draft; ST1 is new. Runs after the loot plan (it needs `affixes.json`, `GearRoll` and the drops).

- Numbers: decisions 294, 346-350 (from the loot plan) and 356. No ADR (ADR 0030 records these as stat kinds that only affixes give). Take any new number from `docs/plans/README.md`'s "Next free numbers" line.
- Draft: sizes and the notes below get their review before approval.

## Goal
1. **Lifesteal**: the holder heals by a share of the HP their attacks and damaging skills take (decision 346).
2. **Armor penetration**: the holder's hits ignore a share of a positive resistance (decision 347).
3. **Block**: a chance to take half of a landed hit, with a "Blocked" cue (decision 348).
4. **Drop luck**: the active members' luck raises drop chances and the weights of the tiers above Common (decisions 349, 356).
5. **The Status page shows all twenty-four stats** (decision 350).

Each is a `StatKind` appended after the twenty (so the existing order holds), on `StatSheet` in permille as an init property defaulting to 0, and on `CombatStats` for the three the battle reads; gear gets it only through its affix in `affixes.json`. The Equipment comparison lists them with no code.

## Guiding rules
The loot plan's, plus: each slice tests the sheet-to-`CombatStats` copy with a fixture above 0 (tests rule); rolls draw only when uncertain (no enemy has block, so no seeded fixture gains a draw); formulas in `Effects/DamageFormula` (lifesteal, penetration, block) and `Loot/LootFormula` (luck), each with its reasoning.

## Notes carried from the loot plan's review (settle before approval)
- **Armor penetration needs an enemy with a positive resistance reachable in play.** The goblin has none (Fire −25 only); the training dummy (Blunt and Piercing 25) is in `encounters.json` but on no map's encounter list. A2 must make one reachable, for the hand check and for the effect to exist at all: put the training dummy on the forest's encounter list, or give the goblin a physical resistance, or add a practice enemy (the plan lessons: plan a practice enemy when a mechanic isn't observable). The developer picks.
- **Drop luck must be visible.** Nothing in a battle shows it, and drops are random: ST1 puts it on the Status page, the Equipment comparison lists its change, and A4 extends the loot simulation with a table under +50 % luck.
- **ST1's layout.** Four more numbers on a page proven for twenty (decision 316): settle which section gives room (a fourth band, tighter Combat rows, or Resources moving beside Skills) at review, not mid-slice (the plan lessons: question 265).

## Slices
- [ ] **A1. Lifesteal** (Core + content + Game, about 230 lines) → [slices/A1.md](slices/A1.md)
- [ ] **A2. Armor penetration** (Core + content + text, about 170 lines, plus the reachable enemy) → [slices/A2.md](slices/A2.md)
- [ ] **A3. Block, with its "Blocked" cue** (Core + content + Game, about 230 lines) → [slices/A3.md](slices/A3.md)
- [ ] **A4. Drop luck on chances and tiers; the simulation's luck table** (Core + content + text + Simulations, about 220 lines) → [slices/A4.md](slices/A4.md)
- [ ] **ST1. The Status page shows twenty-four stats** (Game + text, raw 160, about 250 lines at 1.6x) → [slices/ST1.md](slices/ST1.md)

## Decisions
From the loot plan (2026-10-27): 346 (lifesteal as recommended: attacks and damaging skills, not items, within max HP, not while fallen), 347 (penetration on positive resistances only), 348 (block: half a landed hit, any type, members only, rolled after the crit), 349 (drop luck raises drop chances and the tiers above Common by the same percent), 350 (the Status page shows everything).
- **356. Drop luck counts every active member, fallen ones included**: luck is gear, and a fallen member still wears it.

## Open design questions
None numbered yet; the three notes above become questions at review if the developer wants them asked.
