# The forge
Status: draft

Split out of [the loot plan](../2026-10-27-loot/README.md) on 2026-10-27, when the developer approved it: materials, salvage and a bit of crafting (decisions 351-354), at a forge master NPC on the map (decision 353). Slices M1-M3, F1 and F2 come from the loot draft (M3 and M4 merged, F4 folded into F1, F3 merged into F2); E1, E2 and N1 are new. Runs after the loot plan.

- Numbers: decisions 339, 351-354 (from the loot plan) and 358; question 357. Take any new number from `docs/plans/README.md`'s "Next free numbers" line.
- Draft: three slices are over the 250-line budget after the merges the developer approved (M3 about 300, F1 about 320, F2 about 370); each slice file proposes its split for review before approval.

## Goal
1. **Materials**: Scrap and Essence, their own tier and color (decision 339), a Materials tab, dropped by goblins.
2. **Salvage**: an owned piece breaks into its tier's materials (decision 354).
3. **Upgrade and reroll** (decisions 351, 352): Common to Magic to Rare, one more affix each step; every affix rolled again at the same tier. Paid with materials.
4. **At a forge master** (decision 353): an `openForge` event step opens the Forge screen, with its editor support, and an NPC on the map runs it.

## Design notes
- `Crafting/CraftingRules` (salvage yields, upgrade costs by tier reached, the reroll cost; `rules.json` `crafting`) and `Crafting/Forge` (works owned pieces with materials: `TrySalvage`, `TryUpgrade`, `TryReroll`, with `YieldOf`, `UpgradeCostOf`, `RerollCostOf` for the screen), built by `GameSession` with the inventory, the items, the rules and the loot plan's `GearRoll`. Folder dependencies: `Crafting` → `Items`, `Loot`; `Content` and `Session` → `Crafting`.
- The Game's screen lives in `Game/Crafting/` (not `Forge/`, which would clash with Core's `Forge` type, CA1724).
- Unique pieces (decision 339) are neither salvage yields nor upgrade targets until unique items exist.
- Placeholders: salvage common 1 scrap, magic 2 scrap, rare 2 scrap and 1 essence; upgrade to magic 3 scrap, to rare 5 scrap and 1 essence; reroll 2 scrap and 1 essence; goblin Scrap 20 %, 1-2.

## Slices
- [ ] **M1. Materials: an item kind, their color, a Materials tab, two placeholder materials that drop** (Core + Data + content + Game, about 190 lines) → [slices/M1.md](slices/M1.md)
- [ ] **M2. Salvage in Core: `Crafting/Forge`, yields by rarity in content** (Core + Data + content, about 220 lines) → [slices/M2.md](slices/M2.md)
- [ ] **M3. Upgrade and reroll in Core** (Core + Data + content, about 300 lines; split proposed) → [slices/M3.md](slices/M3.md)
- [ ] **F1. The Forge screen lists owned pieces, in its final look** (Game + text, raw 200, about 320 lines at 1.6x; split proposed) → [slices/F1.md](slices/F1.md)
- [ ] **E1. The `openForge` event step: a map event opens the Forge screen** (Core + Data + Game, about 200 lines) → [slices/E1.md](slices/E1.md)
- [ ] **F2. Salvage, upgrade and reroll at the Forge** (Game + text, raw 230, about 370 lines at 1.6x; split proposed) → [slices/F2.md](slices/F2.md)
- [ ] **E2. The editor shows and adds the `openForge` step** (Editor, about 150 lines) → [slices/E2.md](slices/E2.md)
- [ ] **N1. A forge master stands on a map** (content + tests, about 60 lines) → [slices/N1.md](slices/N1.md)

## Decisions
From the loot plan (2026-10-27): 339 (Material is the materials' own tier and color), 351 (upgrade and reroll), 352 (every affix at once), 353 (a forge master NPC, an event step with editor support), 354 (Scrap and Essence; worn pieces can't be salvaged; salvage asks to confirm).
- **358. Salvage is refused, with nothing changed, when a material it would give has a full stack**: nothing is ever destroyed for less than it yields.

## Open design questions (developer decides)
- **357. An upgrade when the pool has no affix left for the piece's kind.** *Recommended:* refused and greyed, costing nothing (the piece would gain a tier name without a new line). Alternative: the tier rises with no new affix and the materials are spent (the loot draft's behavior).
- The Material tier's color is part of the loot plan's question 362.
