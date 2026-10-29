# Affix stats
Status: approved

Split out of [the loot plan](../done/2026-10-27-loot/README.md) on 2026-10-27, when the developer approved it: the four stats decision 294 named as affixes, and the Status page showing them (decision 350). Runs right after the loot plan, which is done (it needs `affixes.json`, `GearRoll` and the drops); the [forge plan](../2026-10-27-forge/README.md) runs after this one. Reviewed by the devil's advocate and approved by the developer on 2026-10-29 (decisions 363-368).

- Numbers: decisions 294, 346-350 and 356 (from the loot plan), 363-368 (this plan's review). No ADR: ADR 0030 records these as stat kinds that only affixes give.
- Tunables, the design overview, the folder dependencies and the risks: [design.md](design.md).

## Goal
1. **Lifesteal**: the holder heals by a share of the HP their attacks and damaging skills take (decisions 346, 365).
2. **Armor penetration**: the holder's hits take flat percentage points off a positive resistance, after the cap (decision 364), against an enemy the forest now rolls (decision 363).
3. **Block**: a chance to take half of a landed hit, with a "Blocked" cue (decision 348), on a shield the goblins now drop (decision 366).
4. **Drop luck**: the active members' luck raises drop chances and the weights of the tiers above Common (decisions 349, 356, 367).
5. **The Status page shows all twenty-four stats** (decisions 350, 368).

Each is a `StatKind` appended after the twenty (so their order holds), listed in `StatKinds.AffixOnly`, on `StatSheet` in permille as an init property defaulting to 0 and read by `StatSheet.ValueOf` through one `AffixStatOf` arm, and on `CombatStats` for the three the battle reads. Gear gets it only through its affix in `affixes.json`; a base item listing it is refused at load. The Equipment comparison lists a change in it once `ValueOf` reads it (`StatComparison.Between` asks `ValueOf` for every kind).

## Order and why
A1 first: it lays the shared pieces its siblings reuse (`StatKinds.AffixOnly`, `AffixStatOf`, `EffectLanding`'s `HitSource`, `ActionPlaybackAffixTests`). A2 and A3 extend `HitSource` and the landing in turn. A4a moves the loot rolls to take luck before A4b gives luck a source, so A4b stays small. ST1a moves Resources beside Skills before ST1b fills the slot with the four stats, which then all exist.

## Guiding rules
The loot plan's, plus:
- Each slice that adds a stat tests the sheet-to-`CombatStats` copy with a fixture above 0 (tests rule), adds its `StatSheetTests` theory case (`ValueOf` throws on a kind it doesn't read), and its `ItemCatalogValidatorTests` case.
- Rolls draw only when uncertain (ADR 0026): no enemy has block, so no seeded fixture gains a draw.
- Formulas in one place each, with their reasoning: `Effects/DamageFormula` (lifesteal, penetration, block), `Loot/LootFormula` (luck).
- A method that takes `ContentCatalog` lives in Content or Session (the loot retro): the luck sum is `GameSession`'s, the rolls take numbers.
- Only tests that can fail: a pinned draw on each side of a boundary, fixtures that differ from the default.
- The new playback tests go in `ActionPlaybackAffixTests.cs` (`ActionPlaybackTests.cs` is 287 lines).

## Out of scope
- The forge (`AddAffix`, `Reroll`, scrap): the forge plan.
- A cap on drop luck (decision 367), enemies with affix stats, items that steal life or penetrate (decision 321).
- The beetle's own art, a block animation beyond the cue, tuning any value in design.md's tunables.
- A weight per encounter on a map: the forest lists the trio and the beetles once each, so the two fights are as likely as each other (placeholder).

## Slices
### Battle stats
- [x] **A1. Lifesteal** (Core + Data + content + Game, about 250 lines) → [slices/A1.md](slices/A1.md)
- [x] **A2. Armor penetration, and the stone beetles in the forest** (Core + content + Data tests + Simulations, about 200 lines) → [slices/A2.md](slices/A2.md)
- [x] **A3. Block, with its "Blocked" cue; the shield in the goblin's drops** (Core + content + Game, about 180 lines) → [slices/A3.md](slices/A3.md)

### Drop luck
- [x] **A4a. Loot rolls take a luck: `LootFormula`** (Core + Simulations, about 140 lines) → [slices/A4a.md](slices/A4a.md)
- [ ] **A4b. Drop luck from gear; the simulation's luck table** (Core + content + Game text + Simulations, about 110 lines) → [slices/A4b.md](slices/A4b.md)

### Status page
- [ ] **ST1a. Resources move beside Skills** (Game, raw about 100, about 160 at 1.6x) → [slices/ST1a.md](slices/ST1a.md)
- [ ] **ST1b. The Affixes section: twenty-four stats** (Game + text, raw about 80, about 130 at 1.6x) → [slices/ST1b.md](slices/ST1b.md)

## Decisions
From the loot plan (2026-10-27): 346 (lifesteal: attacks and damaging skills, not items, within max HP, not while fallen), 347 (penetration on positive resistances only; its "share" is revised by 364), 348 (block: half a landed hit, any type, members only, rolled after the crit), 349 (drop luck raises drop chances and the tiers above Common by the same percent), 350 (the Status page shows everything).
- **356. Drop luck counts every active member, fallen ones included**: luck is gear, and a fallen member still wears it.

From this plan's review (developer, 2026-10-29):
- **363. A placeholder monster for armor penetration:** `stone_beetle` "Stone Beetle", resistances Slashing 25, Piercing 25, Blunt −25, HP and damage near the goblin's, the goblin's battler file as placeholder art, in its own forest encounter `stone_beetle` listed beside `goblin_trio`. A2 adds a simulation row proving a lone level 1 leader attacking only wins it. The display name may be renamed in content later; the id stays.
- **364. Penetration counts in flat percentage points** taken off a positive resistance after the cap, never below 0 (25 % becomes 17-22 % with 3-8 points); a weakness is untouched. Revises decision 347's "share". The ARPG convention (Path of Exile).
- **365. Lifesteal's placeholder range is 8-15 %**, rounded down per hit: 1-3 % of a goblin's 30 HP rounds to 0.
- **366. The wooden shield joins the goblin's drop table at 4 %**, like the cap and the boots, so the off-hand `block` affix can roll in play.
- **367. No cap on drop luck** for now (five members in full luck gear reach about +225 %).
- **368. ST1's layout:** the four affix stats as two columns of two rows in the slot Resources holds today (under the Resistances: 198 + 46 + 2 × 34 = 312, the band's height unchanged), and Resources moves beside Skills. The page is `MenuLayout.SingleColumn` (1100 px wide, 1044 of content), `StatusSheetView.ContentHeight` is 20 + 232 + 312 + 16 + 256 = 836 of 848, so no layout that grows the band fits; `ContentHeight_ProfileBandAndThreeSkills_FitsThePage` is the guard. The new section's title is a text key (placeholder "Affixes").
- Already decided, noted: items never steal life or penetrate (decision 321): `HitSource` gains `PenetrationPermille` in A2, set to 0 by `LandFromItem`; block works against any landed hit (348).

Planner's picks (placeholders, unnumbered; the developer may change any): the beetle's other numbers (blunt attacks, the goblin's accuracy, crit, crowd control resistance, experience and drop table), two beetles per encounter, the Resources column 300 px wide beside Skills, the short label "Penetration". All in design.md's tunables.

## Open design questions (developer decides)
None: the review's questions became decisions 363-368.

## ADRs needed
None (ADR 0030 covers stat kinds only affixes give; decision 364 revises a decision, not an ADR).
