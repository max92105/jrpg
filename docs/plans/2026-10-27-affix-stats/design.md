# Affix stats: design

Part of [the affix stats plan](README.md).

## Tunables and placeholders

Every value is a placeholder (decision 196's rule) until the developer tunes it; the simulations print what they give.

**`affixes.json`** (`Loot/AffixDefinition`). Whole percents in content, permille on the sheet (ADR 0026). Weight 10 like the rest of the pool.

| Affix id | Stat | Range | Rolls on | Meaning | Slice |
|---|---|---|---|---|---|
| `life_steal` | lifeSteal | 8-15 % (decision 365) | weapon, ring | a share of the HP each landed hit of an attack or a damaging skill took, healed on the holder, rounded down per hit | A1 |
| `armor_penetration` | armorPenetration | 3-8 points (decision 364) | weapon | percentage points off a positive resistance after the 75 % cap, never below 0 | A2 |
| `block` | blockChance | 3-8 % | offHand | the chance a landed hit on the holder is blocked | A3 |
| `drop_luck` | dropLuck | 5-15 % | amulet, ring | added over the active members; raises drop chances and the tiers above Common by that percent | A4b |

**Core constants.**

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| `BlockedDamagePercent` | `Effects/DamageFormula` | 50 | what a blocked hit deals of what it would, inside the one rounding (A3) |
| drop luck's cap | none | no cap (decision 367) | five members in full luck gear reach about +225 % |

**`enemies.json`: `stone_beetle`** (decision 363; A2). Resistances and art are decided; the rest is the planner's pick near the goblin's (marked *pick*).

| Field | Value | Note |
|---|---|---|
| `name` | "Stone Beetle" | may be renamed in content; the id stays |
| `resistances` | slashing 25, piercing 25, blunt −25 | decision 363 |
| `maxHp` | 30 | the goblin's |
| `damage` | 9 | the goblin's |
| `damageType` | blunt | *pick*: a beetle rams |
| `accuracy`, `dodge`, `critChance`, `crowdControlResistance` | 75, 0, 5, 10 | *pick*: the goblin's |
| `experienceReward` | 40 | *pick*: the goblin's |
| `dropTableId` | `goblin` | *pick*: a forest fight still feeds loot |
| `portraitFile`, `battleSpriteFile`, `attackEffectFile` | `Goblin.png`, `Goblin.png`, `AttackEnemy.png` | decision 363: the goblin's art as placeholder |

**`encounters.json` and the forest** (A2). `stone_beetle`: two `stone_beetle`s (*pick*: a longer fight than one beetle shows penetration on more hits, and two stay easier than the trio), `Forest.png`. `maps/forest.json` lists `goblin_trio` and `stone_beetle`, each as likely as the other (placeholder; a weight per encounter is out of scope).

**`drop-tables.json`** (A3, decision 366): `goblin` gains `wooden_shield` at 4 %, like the cap and the boots, so the off-hand `block` affix can roll in play.

**Status page text** (ST1b). The section title `status_affixes` ("Affixes", decision 368) and the short label `status_penetration` ("Penetration", *pick*: "ARMOR PENETRATION" in spaced caps doesn't fit a 231 px cell beside its value; `status_crowd_control` is the precedent).

## Design overview

### What exists
- `Stats/StatKind` lists twenty kinds; `StatSheet` holds them (positional, plus `ValueOf(kind)`: resistances through `StatKinds.TypeResistedBy`, the rest through `DerivedOrPrimary`, 8 arms, whose default is `PrimaryStats.ValueOf`, which throws on an unknown kind). `CombatStats` is a sealed record with init properties; `Battle/MemberBattleStats.Of` copies the sheet into it.
- `Party/MemberSheet.Of` builds the sheet from the class and `StatBonuses worn` (gear's percents to permille through `PermilleOf`).
- `Effects/DamageFormula.Calculate(power, crit, Mitigation)` clamps the resistance within floor and cap and rounds once in a `long`; `Mitigation(ResistancePermille, ExtraTakenPermille)` is a sealed record.
- `Battle/EffectLanding` (112 lines): `Land` loops targets and hits; each damaging hit rolls to land, then to crit, then `EffectResolver.Apply` and the resource triggers. `BattleState.Attack`, `UseSkill` and `UseItem` all call `Land`.
- `Loot/GearRoll.Roll(item)` picks a rarity with `WeightedPick` over the rows in rarity order; `Loot/DropRoll.Roll(tables)` rolls each entry's chance through `ChanceRolls`. `Session/GameSession.SpoilsOf` calls `_dropRoll.Roll(tables)`; `GameSession.DropRollFor` builds the roll (internal, shared with `LootSimulation`).
- Game: `Menus/StatComparison.Between` calls `StatSheet.ValueOf` for every `StatKind`; `Ui/StatNames` is a table of every kind's name id (`StatNamesTests` checks each kind by theory); `Ui/StatText` writes a permille kind as a whole percent (`StatKinds.IsHeldInPermille` is true for any kind it doesn't list, so the new kinds read as percents with no change). `Battle/ActionPlayback` (241 lines) turns events into beats; an unknown event throws.
- The Status page (`MenuLayout.SingleColumn`, 1100 px wide, 1044 of content, 848 tall): profile band 232, middle band 312 (`StatusParametersView`: Combat left, Resistances then Resources right), 16 px, Skills 256: `StatusSheetView.ContentHeight` 836.

### Core changes
- **Stat kinds** (A1, A2, A3, A4b): `LifeSteal`, `ArmorPenetration`, `BlockChance`, `DropLuck`, appended after `DarkResistance` in that order, so the twenty keep their order (and their saved and content names). `StatKinds.AffixOnly` (A1): the kinds only affixes give, in that order, the order the Status page lists them; each slice appends its kind.
- **The sheet** (same slices): `StatSheet.LifeStealPermille`, `ArmorPenetrationPermille`, `BlockChancePermille`, `DropLuckPermille`, init properties defaulting to 0, so no `new StatSheet(...)` call site changes. `ValueOf` reads them through a private `AffixStatOf(kind)` switch (A1) called from `DerivedOrPrimary`'s default; its own default is `Primaries.ValueOf`. `DerivedOrPrimary` stays at 8 arms (the limit is 10). `MemberSheet.Of` sets each from the worn bonus (`PermilleOf`).
- **Battle stats** (A1-A3): `CombatStats.LifeStealPermille`, `ArmorPenetrationPermille`, `BlockChancePermille` (init, 0; enemies keep 0); `MemberBattleStats.Of` copies them. Drop luck isn't on `CombatStats`: battles don't read it.
- **Formulas, one place each:** `Effects/DamageFormula.LifeStolen(hpTaken, permille)` (A1), `PenetratedResistance(resistancePermille, penetrationPermille)` (A2), `BlockedDamagePercent` inside `Calculate` (A3); `Loot/LootFormula.ChancePermille` and `RarityWeight` (A4a). Each with its reasoning in a comment.
- **Landing** (A1-A3): `EffectLanding` gains a private `HitSource` record (who lands the hits and what their hits carry: A1 `Actor`, `LifeStealPermille`; A2 `PenetrationPermille`), built by `Land` from the actor's `CurrentStats` and by `LandFromItem` with 0s (decision 321), so the hit loop keeps four parameters and has no flag argument. `Mitigation` gains `IsBlocked` (A3).
- **Events**: `Battle/LifeStolen(Combatant Actor, int Amount)` (A1), `Battle/HitBlocked(Combatant Target)` (A3). Each lands with its `ActionPlayback` case, since content produces it in the same slice.
- **Luck** (A4a, A4b): `GearRoll.Roll(item, luckPermille)` and `DropRoll.Roll(tables, luckPermille)` (A4a, with 0 from every caller); `GameSession.SpoilsOf` sums the active members' `Sheet.DropLuckPermille`, the fallen included (A4b, decision 356).

### Folder dependencies (Core and Data)
No new edge. `Stats` stays a leaf; `Loot` takes luck as a number, so it never sees `Party`; `Battle` never sees `Loot`.

| Folder | References | What this plan adds there |
|---|---|---|
| Stats | none | the four kinds, `StatKinds.AffixOnly`, sheet and combat-stat fields |
| Effects | Stats | `DamageFormula.LifeStolen`, `PenetratedResistance`, `BlockedDamagePercent`; `Mitigation.IsBlocked` |
| Battle | Effects, Party, Randomness, Resources, Stats, Statuses, … (unchanged) | `LifeStolen`, `HitBlocked`, `EffectLanding`'s `HitSource` and `LandFromItem`, `MemberBattleStats` copies |
| Party | Items, Progression, Resources, Skills, Stats (unchanged) | `MemberSheet` sets the four |
| Loot | Items, Randomness, Stats (unchanged) | `LootFormula`; the luck parameters |
| Session | Battle, Content, Exploration, Items, Loot, Party, Randomness, … (unchanged) | the luck sum in `SpoilsOf` |
| Data/Items | Core Items, Stats, Effects (unchanged) | `ItemCatalogValidator` refuses an affix-only kind in an item's bonuses |

**Game:** `Battle/ActionPlayback` reads the two new events (Core Battle) and `Ui/MessageIds`. `Menus/StatusParametersView`, the new `Menus/StatusResourcesView` and `Menus/StatusSheetView` use `Ui/SheetRowLook` and `Ui/SectionTitle`; `Ui` depends on no feature folder. `Ui/StatNames` gains four rows.

### Key decisions
- Penetration counts in points after the cap (decision 364): `PenetratedResistance` first holds the resistance at the cap, then takes the points off a positive one, never below 0. The "Resisted" cue reads the result, so the beetle's 25 % with 3-8 points no longer reads "Resisted" (threshold 25 %, decision 323).
- Block is rolled after the crit, through `ChanceRolls`, only on a landed damaging hit; it draws only when 0 < chance < 100 % (ADR 0026), and no enemy has block, so no seeded fixture gains a draw.
- Lifesteal heals the actor by what each landed damaging hit took (the `int` `EffectResolver.Apply` returns), within max HP, only while the actor stands; an item's effect lands through `LandFromItem` and steals nothing (decision 321). Reported only when above 0.
- Drop luck raises each entry's chance (capped at 100 %) and every rarity weight above Common by the same percent (decision 349); Common's weight is unchanged, so luck grows the total and never removes Common.
- A base item can't carry an affix-only kind (`ItemCatalogValidator`, A1): ADR 0030 says only affixes give them.

### Replacement mapping
| Existing | Becomes | Slice |
|---|---|---|
| `EffectLanding.Land` for items (`BattleState.UseItem`) | `EffectLanding.LandFromItem` | A1 |
| `MitigationOf(target, type)` | `MitigationOf(target, type, source)`, the penetration from `HitSource` | A2 |
| `GearRoll.Roll(item)`, `DropRoll.Roll(tables)` | `Roll(item, luckPermille)`, `Roll(tables, luckPermille)` | A4a |
| The Status page's Resources, under Resistances | `Menus/StatusResourcesView`, beside Skills | ST1a |
| Resources' slot in the middle band | the Affixes section, two columns of two rows | ST1b |
| `Battle/GoblinTrioSimulation` | `Battle/ForestFightsSimulation` (the trio and the beetles) | A2 |

## Risks
- **Skills beside Resources (ST1a).** Skills narrow from 1032 to 684 px of text, with 222 px under their band. A skill is 71 px with a one-line description, 93 px with two; the warlock's three descriptions (with today's "Placeholder" sentences) may already take two lines each, which hides the third skill. ST1a's hand check counts the skills shown per class before and after; it stops and asks if moving Resources hides a skill that showed before.
- **Label width (ST1b).** An affix cell is 231 px wide; "PENETRATION" plus "8 %" fits by estimate, "ARMOR PENETRATION" doesn't. The hand check measures.
- **Determinism.** Lifesteal and penetration draw nothing. Block draws only for a member with 0 < block < 100 %. Luck changes no draw count except where it lifts a chance to 100 % (a certain entry draws nothing). The forest's list of three still draws once per encounter (`NextBelow(count)`), so the exploration draws don't change; tests that start a forest battle build their own table (`StartMapScenarios`).
- **Small lifesteal numbers.** 8 % of a 10 HP hit rounds to 0 (decision 365 accepts the rounding); the hand check uses a high roll or a crit.
- **Overflow.** `Calculate`'s product gains a fifth factor (A3): at most 1000 power × 3000 crit × 2000 × 2000 × 1000 ≈ 1.2 × 10¹⁶, within a `long`.
- **File sizes.** `ActionPlaybackTests.cs` is 287: the new playback tests go in a new `ActionPlaybackAffixTests.cs`. `ItemCatalogValidatorTests.cs` 233 (+15 in A1, +1 line each after). `EffectLanding.cs` 112 → about 150 after A3. `CombatantTests.cs` 198 → about 240 after A3. `DamageFormulaTests.cs` 122 → about 210 after A3. `StatusSheetView.cs` 226 → about 240 after ST1a.
- **Save format.** None: saves keep affix ids and values and resolve the stat from content (ADR 0030); the new kinds only appear through new affixes.
