# Stats and gear slots: design

Part of [the stats and gear slots plan](README.md).

## Tunables and placeholders

Every value is a placeholder (decision 196's rule): the simulation guards the start area, the developer tunes the rest.

**`rules.json` `stats`** (`Stats/StatRules`). Every conversion is "per 100 points", so the numbers stay whole. Content gives whole percents; Core holds every derived percent in tenths (permille: crit chance 9 % is 90) and the screens show it rounded down to a whole percent (ADR 0026), so a +5 Agility ring (+1.25 % crit) moves the stored number and the shown one.

| Field | Placeholder | Meaning | Slice |
|---|---|---|---|
| `hpPercentPer100Constitution` | 100 | +1 % of the class's max HP per Constitution point | P2 |
| `baseAccuracy` | 90 | a member's accuracy before class and gear: hit chance against 0 dodge | P3a |
| `baseCritChance` | 5 | % | P3a |
| `baseDodge` | 0 | % | P3a |
| `baseCrowdControlResistance` | 0 | % (decision 278) | P3a |
| `damagePercentPer100Power` | 100 | +1 % to every raw damage and healing per Power point | P3a |
| `critDamagePercentPer100Power` | 50 | +0.5 % crit damage per Power point, over the base 150 % | P3a |
| `critChancePer100Agility` | 25 | +1 % crit chance per 4 Agility | P3a |
| `dodgePer100Agility` | 20 | +1 % dodge per 5 Agility | P3a |
| `statusPercentPer100Intelligence` | 100 | +1 % to a granted status's power per Intelligence point (Stun: its chance) | P3b |
| `intelligencePerExtraStatusTurn` | 25 | one more turn on every granted status but Stun per 25 Intelligence (the breakpoint; "status turn", not Ready's extra turn; decision 293) | P3b |
| `physicalResistancePer100Constitution` | 25 | +1 % Blunt, Piercing and Slashing resistance per 4 Constitution | P3b |
| `magicalResistancePer100Will` | 25 | +1 % Fire, Ice, Lightning, Light and Dark resistance per 4 Will | P3b |
| `unarmedDamageType` / `unarmedDamage` | `blunt` / 18 | a member's attack with no weapon (decision 306) | D1 / M1 |

**Limits in code** (ADR 0026: shared by members and enemies, read by the battle, which takes no rules).

| Constant | Where | Placeholder |
|---|---|---|
| `BaseCritDamagePercent` | `Stats/StatFormula` | 150 (decision 276) |
| `DodgeCap` | `Stats/StatFormula` | 50 |
| `Resistances.Cap` / `Floor` | `Stats/Resistances` | 75 / −100 (a weakness at most doubles a hit) |
| `HitFormula.FloorPercent` / `CeilingPercent` | `Effects/HitFormula` | 20 / 100 (a ceiling of 100 keeps rolls certain for test fixtures, ADR 0026) |
| `DamageFormula.ResistedThreshold` | `Effects/DamageFormula` | 25 % (decision 299: at or above it, the hit reads "Resisted") |
| `DamageFormula.MinimumDamage` | `Effects/DamageFormula` | 1 (exists) |

**Class primaries** (`classes.json` `base` / `growth`, P1). Max HP and speed stay as today. Every class has mid-to-high Power, 12-16 at level 1 and +2 a level (decision 294: Power is the universal damage stat); the other four set classes apart.

| Class | Power | Agility | Intelligence | Constitution | Will | Accuracy |
|---|---|---|---|---|---|---|
| fighter | 15 / 2 | 8 / 1 | 6 / 1 | 14 / 2 | 8 / 1 | 0 / 1 |
| mage | 12 / 2 | 8 / 1 | 16 / 2 | 6 / 1 | 14 / 2 | 0 / 1 |
| cleric | 12 / 2 | 6 / 1 | 12 / 2 | 10 / 1 | 14 / 2 | 0 / 1 |
| rogue | 13 / 2 | 16 / 2 | 10 / 1 | 8 / 1 | 6 / 1 | 5 / 1 |
| barbarian | 16 / 2 | 8 / 1 | 4 / 1 | 16 / 2 | 6 / 1 | 0 / 1 |
| paladin | 14 / 2 | 6 / 1 | 10 / 1 | 14 / 2 | 10 / 1 | 0 / 1 |
| monk | 14 / 2 | 14 / 2 | 8 / 1 | 10 / 1 | 8 / 1 | 5 / 1 |
| warlock | 12 / 2 | 8 / 1 | 14 / 2 | 6 / 1 | 14 / 2 | 0 / 1 |
| druid | 12 / 2 | 8 / 1 | 14 / 2 | 8 / 1 | 12 / 2 | 0 / 1 |

**Enemies** (`enemies.json`): goblin 30 HP, Piercing, accuracy 85, crit 5, dodge 5, crowd control resistance 10, Fire −25; its `attack` stays 16 until M2b, then `damage` 9, so the fighter takes about what it takes today (16 − 8 = 8; 9 less its 3.5 % physical resistance rounds down to 8; M2b). Training dummy 400 HP, Blunt, `attack` 20 until M2b then `damage` 13, accuracy 80, crit 0, dodge 0, crowd control resistance 50, Blunt and Piercing 25, Fire −50 (D1, D2, M2b, H1, H2, I1). Retuned per slice to keep the simulation green.

**Damage types** (D1): fire_blast, fireball, meteor, bomb, bomb_all Fire; power_strike Slashing; shield_bash, rampage, palm_strike, flurry Blunt; smite Light; eldritch_bolt, void_rift, chaos_lance Dark; equilibrium Piercing (placeholder: no nature type). Ice and Lightning have no skill yet. Weapons: bronze and iron sword Slashing (damage 24, 30), magic staff Blunt (20, Intelligence +3) (M1).

**Gear bonuses** (P4, G2): leather armor Constitution +2, the three physical resistances +5; chain mail Constitution +3, Slashing +15, Piercing +10, Blunt +5; wizard robe Will +3, Fire, Ice and Lightning +10. New (G2), every bonus at least 5 so a shown number moves: wooden shield (off-hand, physical +8), leather cap (head, Constitution +5), leather gloves (hands, accuracy +5), leather boots (feet, speed +5, dodge +5), amber amulet (Will +5, Fire +10), power ring (Power +5), sage ring (Intelligence +5), swift ring (Agility +5).

**Statuses** (I1, I2, M2b): Stun's power becomes its chance (shield_bash 60); Guarded's power becomes resistance points (aura 20, wide aura 12); Exposed 50, Poison 8, Regen 10 as today. New in I2 (decision 296): fire_blast grants Poison 5 for 2 turns (a burn), recovery grants Regen 6 for 2 turns, palm_strike grants Stun 30 for 1 turn, eldritch_bolt grants Exposed 20 for 2 turns.

**Will** (`willScaling` per resource, W1a and W1b; decision 295): `effect` is `raiseStart`, `raiseMax` or `lowerGain`, all with `willPerPoint` 10.

| Class | Resource | `effect` | Slice |
|---|---|---|---|
| fighter | stamina | `raiseStart` | W1a |
| mage | mana | `raiseStart` (Clarity: none) | W1a |
| cleric | faith (lasting) | `raiseMax` (a rest refills to the raised max, since Faith starts full) | W1b |
| rogue | readiness | `raiseMax` (one more charge) | W1a |
| barbarian | ferocity | `raiseStart` | W1a |
| paladin | conviction | `raiseStart` | W1a |
| monk | chakra | `raiseStart` | W1a |
| warlock | instability (harmful) | `lowerGain`: every gain to it is lower by the points, never below 1 | W1b |
| druid | growth, decay | `raiseStart` | W1a |

## Design overview

### What exists
- `Stats/CombatStats(Attack, Defense)` is all a combatant fights with. A basic attack is a damage effect whose power is the attacker's attack (`BattleState.Attack`); a skill's power is flat content, never scaled. `DamageFormula`: `max(1, (power − defense) × taken% ÷ 100)`; Exposed raises taken %, Guarded adds its power to defense (`StatusModifiers`).
- `Party/ClassStats(MaxHp, Attack, Defense, Speed)`, grown linearly by `ClassGrowth.StatsAt` (ADR 0017). `PartyMember` holds `BaseStats`, `TotalStats` (plus weapon and armor bonuses), `Speed`, and a weapon and an armor. Speed only sets the rogue's readiness charges (`ReadinessFormula`, `speedPerCharge` 10).
- `Items/EquipmentSlot { Weapon, Armor }` is both an item's kind and a member's slot; `EquipmentStats(Slot, AttackBonus, DefenseBonus)`.
- `EnemyDefinition` has `MaxHp`, `Attack`, `Defense`. The turn order is a seeded shuffle; nothing reads an enemy's speed.
- Saves (format 3) keep each member's `WeaponId` and `ArmorId`. Every random draw shares the session's `IRandom` (turn order, backfire, enemy tactics).

### The stat sheet

| Stat | Kind | Source | Base (placeholder) |
|---|---|---|---|
| Power, Agility, Intelligence, Constitution, Will | primary | class base + growth × (level − 1) + gear | per class (table above) |
| Max HP | derived | class HP at level × (100 + Con × rate) ÷ 100, + gear | class `maxHp` |
| Speed | derived | class base + growth, + gear (not Agility, decision 271) | class `speed` |
| Accuracy | derived | `baseAccuracy` + class + gear | 90 |
| Critical chance % | derived | `baseCritChance` + Agility × rate, + gear | 5 |
| Critical damage % | derived | 150 + Power × rate, + gear | 150 |
| Dodge % | derived | `baseDodge` + Agility × rate, + gear, capped | 0, cap 50 |
| Crowd control resistance % | derived | `baseCrowdControlResistance` + gear; an enemy's from content (decision 300) | 0 |
| Damage % (Power) | derived, shown as Power's caption | 100 + Power × rate | 100 |
| Status % and extra status turns (Intelligence) | derived, Intelligence's caption | 100 + Int × rate; Int ÷ breakpoint (never on Stun, decision 293) | 100, +0 |
| Blunt, Piercing, Slashing resistance % | resistance | Con × rate + gear, within −100..75 | 0 |
| Fire, Ice, Lightning, Light, Dark resistance % | resistance | Will × rate + gear, within −100..75 | 0 |

Gear adds to primaries first, so a Power ring also raises damage and crit damage; flat gear on a derived stat adds after the conversion. An enemy has max HP, damage and type, accuracy, crit chance, dodge, crowd control resistance and resistances from content; its damage % and status % are 100, its crit damage 150.

### Formulas (one place each)
- **Units and rounding** (ADR 0026, ADR 0027): content gives whole percents; Core holds every derived percent in tenths (permille), so `StatSheet` and `CombatStats` name them `…Permille` (`CritChancePermille` 90 is 9 %); screens show them rounded down to whole percents. A hit's damage is computed in a `long` from the permilles and rounded down once, at the end; no step rounds on its own. H2's test stacks power 999, a crit, Exposed and a −100 weakness to prove no overflow.
- **`Stats/StatFormula`** (P2-P3b): `MaxHp`, `CritChance`, `CritDamage`, `Dodge` (capped), `DamagePercent`, `StatusPercent`, `ExtraStatusTurns` (`intelligence / intelligencePerExtraStatusTurn`, rounded down), `PhysicalResistance`, `MagicalResistance`. Each is `base × 10 + points × ratePer100 × 10 / 100` in permille, rounded down once.
- **`Effects/HitFormula.Chance(accuracy, dodge)`** (H1): `clamp(accuracy − dodge, 20 %, 100 %)`, in permille. Accuracy already holds the base (decision 273's "base + accuracy").
- **`Effects/DamageFormula`** (D2, M2b, H2): `max(1, power × crit × (1000 − resistance) × (1000 + exposed) ÷ 1000³)`, resistance clamped to −1000..750 permille, crit 1000 on a normal hit, in a `long`, rounded down once. Power arrives already scaled by its user. `HitAffinity` (D3): Weak when the resistance is below 0, Resisted when at or above `ResistedThreshold`.
- **Healing** (M1): `power × damage permille ÷ 1000`, no crit, no resistance.
- **`Statuses/GrantScaling`** (I1): power × status permille ÷ 1000; turns + extra status turns, except Stun, whose turns never change (decision 293: `TurnFlow` skips a turn per Stun turn left, so extra turns would stun-lock) (a status counts its holder's turns, ADR 0021, so a "round" of decision 272 is one of the holder's turns). **Stun**: lands with chance `scaled power − target's crowd control resistance`, within 0..100 %. **Taunt** (I2): an enemy choosing its target while a member taunts ignores the taunt with its crowd control resistance as the chance.
- **`Resources/WillFormula.BonusPoints(will, willPerPoint)`** (W1a): `will / willPerPoint`. `raiseMax` raises the max, and the start too when the resource starts full (its start equals its max, its max is speed charges, or it is lasting and refills to its start, W1b); `raiseStart` raises the start, within the max; `lowerGain` (W1b) lowers every positive change to the resource (skill changes and triggers) by the points, never below 1.
- **Rolls** (`Battle/ChanceRolls`, H1): draw only when 0 < chance < 1000 permille.
- **Order on a skill**: Power and Intelligence (`Battle/EffectScaling`), then the dominant force's bonus (`SkillEmpowerment`), then per hit: hit roll, crit roll, mitigation.

### Percent conversions, so gear matters at every level
Each point converts at a fixed percent with no level divisor, and the percents apply to bases that grow with level:
- **Power** multiplies raw values: +5 Power is +5 % of every hit. A level 1 fighter (Power 15, ×1.15) gains 4.3 % from it (×1.20); at level 20 (Power 53, ×1.53) 3.3 % (×1.58). It shrinks slowly, never vanishes.
- **Constitution** is a percent of the class's HP, which grows: +5 Constitution is +5 % of 220 HP at level 1 (+11), of 638 at level 20 (+32). Gear Constitution grows with the member.
- **Resistances, crit, dodge and accuracy** are percents on gear too: +10 % Fire is worth exactly that at any level.
- So a fixed-stat item keeps meaning something for many levels, and the loot plan's item levels raise its numbers rather than its kind.

### Content shapes
- `classes.json`: `"base": { "maxHp": 220, "speed": 12, "accuracy": 0, "primaries": { "power": 15, "agility": 8, "intelligence": 6, "constitution": 14, "will": 8 } }`, the same for `growth`. A resource may add `"willScaling": { "willPerPoint": 10, "effect": "raiseStart" }` (`raiseStart`, `raiseMax`, `lowerGain`).
- `skills.json`: a damage effect adds `"damageType": "fire"`; any other kind has none. Stun's `power` is its chance.
- `items.json`: `"equipment": { "kind": "weapon", "damage": 24, "damageType": "slashing", "bonuses": { "intelligence": 3 } }`; any other kind has no damage or type: `"equipment": { "kind": "ring", "bonuses": { "power": 5, "fireResistance": 10 } }`. A bomb's effect has a damage type.
- `enemies.json`: `"maxHp": 30, "damage": 9, "damageType": "piercing", "accuracy": 85, "critChance": 5, "dodge": 5, "crowdControlResistance": 10, "resistances": { "fire": -25 }`; resistances absent are 0.
- `rules.json`: a `"stats": { … }` object with the fields above.
- Saves (format 4): each member's `"worn": { "weapon": "bronze_sword", "firstRing": "power_ring" }` replaces `weaponId` and `armorId`.

### Core types
New (each in the slice that first uses it):
- `Stats/PrimaryStats` (P1): the five primaries; adds another set.
- `Stats/StatRules` (P2): the conversion rates and bases `rules.json` gives.
- `Stats/StatFormula` (P2): the primaries-to-derived conversions, one pure place.
- `Stats/StatSheet` (P3a): a member's resulting numbers at their level with what they wear; built by `StatSheet.Of(class stats at level, worn bonuses, rules)`.
- `Stats/DamageType` (P3b): the closed list of eight; `Stats/DamageTypes` beside it says which are physical.
- `Stats/Resistances` (P3b): one percent per damage type, kept within floor and cap.
- `Stats/StatKind` (P4): the twenty stats a sheet shows and gear can add; `Stats/StatKinds.ResistanceTo(type)` maps a damage type to its kind; `StatSheet.ValueOf(kind)` reads any of them (P4), so screens never switch over twenty kinds.
- `Stats/StatBonuses` (P4): what an item adds, by stat kind.
- `Battle/AttackEffect` (D1): a combatant's basic attack as an effect: its raw attack and type.
- `Effects/Mitigation` (D2): what stands between a hit and the target's HP: resistance and extra damage taken (defense until M2b).
- `Battle/MemberBattleStats` (D1): a member's battle stat line from their sheet and worn weapon.
- `Battle/EffectScaling` (M1): an effect as its user lands it: raw values by Power, a grant by Intelligence (I1).
- `Effects/HitFormula` (H1): hit chance from accuracy and dodge.
- `Battle/ChanceRolls` (H1): the battle's percent rolls; draws only when the outcome is uncertain.
- `Effects/HitAffinity` (D3): whether a resistance made a hit Weak, Resisted or neither.
- `Battle/HitMissed` (H1), `Battle/StatusResisted` (I1): events.
- `Statuses/GrantScaling` (I1): a grant as Intelligence shapes it; Stun's chance against crowd control resistance.
- `Resources/WillScaling`, `Resources/WillEffect`, `Resources/WillFormula` (W1a): what Will does to a resource.
- `Items/EquipmentKind`, `Items/EquipmentFit` (G1a-1): what an item is worn as, and which slots a kind fits; `Party/WornEquipment` (G1a-2): a member's nine slots and what they add.

Changed: `CombatStats` becomes a sealed record (D1; a record struct's `default` would give accuracy 0) and the battle's stat line (init properties with neutral defaults: attack damage and type, damage %, accuracy, dodge, crit chance and damage, status % and extra status turns, crowd control resistance, resistances), filled from the sheet for a member and from content for an enemy. `ClassStats` gains `Primaries` and `Accuracy` (init, so the 18 `new ClassStats(` sites don't change in P1) and loses `Attack` (M1) and `Defense` (M2b). `EquipmentSlot` becomes the nine slots (G1a-2). `PartyMember.Recruit` takes `StatRules`; `Restore` becomes `Recruit` then `RestoreProgress` (P2), keeping both under four parameters; `Vitals.ResizeMax` (P2) sets a max at restore and on gear changes.

### Folder dependencies (Core)
`Stats` stays a leaf: it references no other Core folder. `Content` is the hub and nothing below it references it.

| Folder | References (new in bold) |
|---|---|
| Stats | none |
| Effects | Stats, Statuses |
| Statuses | Stats |
| Resources | none |
| Items | Effects, **Stats** (bonuses, damage type) |
| Skills | Effects, Resources |
| Party | Items, Progression, Resources, Skills, Stats |
| Battle | Effects, Items, Party, Progression, Randomness, Resources, Skills, Stats, Statuses |
| Content | Battle, Items, Party, Progression, Skills, **Stats** (`GameRules.Stats`), … |

No edge points back: `Stats` never sees `Items` or `Party`; `GrantScaling` (Statuses) and `WillFormula` (Resources) take numbers, not a combatant. Game: `Ui/StatNames` and `Ui/StatText` (L1) read Core's `StatKind` and are used by Menus and NewGame; Ui depends on no feature folder.

### The pages (Octopath style, `docs/ui-reference/menu-status.webp`, `menu-equipment.webp`)
- **Status page** (decision 292; 1366 × 848 beside the member column; S1 holds the arithmetic): a profile band with the name, class, LV and experience at the left, the **Attributes** column beside them (the five primaries, each with a caption of what it gives: "+15 % damage, +7 % crit damage"; "+2 % crit, +1 % dodge"; "+6 % effects, +0 turns"; "+14 % HP, +3 % physical"; "+2 % magical"), and a 160 px diamond portrait at the right. Below, **Combat** (HP with its bar, speed, accuracy, crit chance, crit damage, dodge, crowd control) and **Resistances** (the eight, a weakness in the danger color) side by side, 36 px rows. Then Resources (left) and Skills (right, 60 px rows). Twenty numbers plus level and experience. The worn list leaves (decision 303).
- **Equipment page**: the member's name, then nine slot rows (48 px: icon, slot name, item) and "Remove all" on the left, in the shared selection look. Under them a **Changes** section in two columns: with a choice open, every stat the highlighted item would change ("POWER 14 ▸ 17", green up, red down); with no choice open, what the highlighted slot's item gives (the same comparison against the slot emptied). The choice panel opens at the right as today.
- **Leader choice**: LV, HP, speed and the five primaries at level 1, in two columns (raw primaries are fine here: they compare classes).

### Dead-stat check (decisions 272, 296)
Agility and Constitution act for every class (crit, dodge, HP, physical resistances); Will always gives magical resistances. The skill dimensions after this plan (new in bold):

| Class | Power (raw values) | Intelligence (grants) | Will (resource) |
|---|---|---|---|
| fighter | power_strike, shield_bash | shield_bash (Stun chance) | stamina start |
| mage | fire_blast, fireball, meteor | **fire_blast (Poison as a burn), I2** | mana start |
| cleric | heal, recovery, heal_all | **recovery (Regen), I2** | **faith max, W1b** |
| rogue | attack only (accepted, decision 296) | expose | readiness max |
| barbarian | rampage | taunt (turns only) | ferocity start |
| paladin | smite | aura_of_protection, wide_aura | conviction start |
| monk | palm_strike, flurry | **palm_strike (Stun chance), I2** | chakra start |
| warlock | eldritch_bolt, void_rift, chaos_lance | **eldritch_bolt (Exposed), I2** | **instability gains lowered, W1b** |
| druid | equilibrium | rejuvenate, rot | growth, decay start |

### Replacement mapping

| Today | Becomes | Slice |
|---|---|---|
| `CombatStats(Attack, Defense)` | the battle's stat line (above); `Attack` is the raw attack, renamed `AttackDamage` when `Defense` leaves | D1, D2, M1, M2b, H1, H2, I1 |
| `ClassStats(MaxHp, Attack, Defense, Speed)` | `MaxHp`, `Speed`, `Accuracy`, `Primaries` | P1, M1, M2b |
| `PartyMember.BaseStats`, `TotalStats`, `Speed` | `PartyMember.Sheet`; `CombatStats` built from it | P3a, M2b |
| `PartyMember.StatsWith`, `StatsWithout` | `SheetWith(item, slot)`, `SheetWithout(slot)` | E1 |
| `EquipmentStats(Slot, AttackBonus, DefenseBonus)` | `EquipmentStats(Kind) { Damage, DamageType, Bonuses }` | P4, D1, M1, M2b, G1a-1 |
| `EquipmentSlot { Weapon, Armor }` | nine slots; `EquipmentKind`; armor is Chest | G1a-1, G1a-2 |
| `EnemyDefinition.Attack`, `Defense` | `Damage`, `DamageType`, `Accuracy`, `CritChance`, `Dodge`, `CrowdControlResistance`, `Resistances` | D1, D2, M2b, H1, H2, I1 |
| `DamageFormula.Calculate(power, defense, taken)` | `Calculate(power, Mitigation)`; defense out, crit in | D2, M2b, H2 |
| Guarded: + defense | + every resistance, in points | M2b |
| Exposed | unchanged; its power scales with Intelligence | I1 |
| `speedPerCharge` (rogue) | unchanged; speed is class + gear; Will may add a charge | P4, W1a |
| `Menus/StatPreview`, `StatPreviewText` | `Menus/StatComparison`, `StatComparisonText` | E1 |
| Save `weaponId`, `armorId` (format 3) | `worn` by slot (format 4) | G1b |

### Contradictions with the code (reported to the developer, and how each was settled)
- **Enemy speed** (decision 275) has no reader: turns are a seeded shuffle and decision 277 defers speed-based turns. The field lands with that plan (no field without a caller).
- **Crowd control resistance** (decision 278) had nothing to resist: settled by decision 300. Enemies get it; Stun (Shield Bash, and the monk's Palm Strike from I2) rolls against it, and an enemy rolls it to ignore a Taunt. A member's still waits for enemy skills (224).
- **Stun has no chance today** (its power is 0, "reads nothing"); decision 272's "stun chance" gives its power that meaning (I1), and Intelligence never adds Stun turns (decision 293).
- **Skills never scaled with attack**: today only basic attacks read a stat, so Power scaling skills is new behavior, not a re-mapping.
- **Will had no resource to raise for the cleric and the warlock**: settled by decision 295 (W1b).
- **`EquipmentSlot` is both kind and slot** today; decision 274's "one slot kind worn twice" needs the two apart (ADR 0028).

## Risks
- **Random draw order.** Hit, crit and Stun rolls add draws to the shared `IRandom`: seeded simulation numbers shift (expected, recorded), and seeded tests could break. Guard: draws only when uncertain; Core fixtures fight under `TestStatRules.Neutral` with certain hits and no crits; the Game's `BattleScenarios` (65 uses of repository battles) recruits under a copy of the neutral rules and uses a scenario goblin with dodge, crit and crowd control resistance 0 (H1, H2, I1).
- **Test churn.** `Recruit` (about 45 sites, P2), `new ClassStats(` (18, M1 and M2b), `TryEquip` (27, G1a-2), `EquipmentSlot.Armor` and `AsArmor` (18, G1a-1), defense in setups (about 40, folded into attack once in M2a). Each slice counts its sites.
- **Overflow.** Power, crit, Exposed and a weakness multiply; the `long` intermediate and H2's extreme test cover it.
- **HP and gear.** Max HP now moves with worn Constitution: restoring a save must wear items before setting HP, and `SaveGameRepair` must clamp HP against the max with gear, or a loaded member loses HP (P2's `ResizeMax`, P4).
- **Balance swings.** Defense leaving makes small hits bigger; misses and crits add variance. The goblin trio simulation must keep a 100 % win rate for every class after M1, M2b, H1 and H2; each records its table, B1 the final one.
- **Save format.** Format 4 refuses every older save (ADR 0022): the developer's test saves go at G1b.
- **Content coupling.** `rules.json` now feeds every member: a broken `stats` block stops the start (validated, named). The editor loads these files through Data; a content change it can't parse would stop it too (the Data validators cover it).
- **Determinism.** No wall clock, no unseeded random: rolls go through `ChanceRolls` over the battle's `IRandom`.
- **Performance.** The sheet is rebuilt on level-up and equipment changes only; screens format their text when the member or the choice changes, never per frame.
- **Folder cycles.** `Stats` must stay a leaf: `StatSheet.Of` takes class numbers and bonuses, never a `PartyMember` or an `ItemDefinition`.
