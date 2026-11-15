# Skills and statuses model: design

Read with the [README](README.md). Sections: 1 tunables, 2 design overview, 3 the content conversion, 4 ADR 0056's
options, 5 the formula worked through, 6 notes for row 7, 7 risks.

## 1. Tunables and placeholders

Every number is content; "placeholder" means the developer replaces it while balancing.

| Name | Where | Value | Meaning |
|---|---|---|---|
| Tiered effects' `perPower` | `effects.json` | the sheet's tiers: suffix `_1` 2 (Low), `_2` 4 (Medium), `_3` 9 (High), `_4` 22 (Massive), `_5` 58 (Extreme) | Damage or healing per point of the user's Power (the developer's "Damage Test" sheet, decision 704). The suffix names the tier; the number is the record's, from 1 to 1000. |
| Goblin Chief's power | `enemies.json` `power` | 5 (placeholder) | What its Cleave, Crushing Blow and Field Dressing multiply (decision 791). |
| Status numbers | `statuses.json` parts' `amount` | section 3.3 (today's numbers) | A tick's HP, Guard's points, Exposed's share (decision 705's tiered records, read as decision 816 says). |
| Grant turns and chance | `skills.json` `statuses` | today's turns; Stun chances 60, 30, 40 | How long a grant lasts and how likely it lands (decision 798). |
| Burning's timing and type | `statuses.json` `burning_1` | fire, `startOfTurn` (placeholder) | Decision 805. |
| Weakened | `statuses.json` `weakened_1` | -25 % damage dealt (placeholder) | Decision 806. |
| Slowed | `statuses.json` `slowed_1` | -15 accuracy, -5 crit chance points; mark "L" (placeholders) | Decision 807. |
| Cone of Cold, Enfeeble | `skills.json`, `classes.json` | `ice_1` + Slowed 2 turns, 4 mana; Weakened 3 turns, +2 instability (placeholders) | Decision 808. |

One Core constant is added, a technical limit: `DamageFormula.MaxPower` (1 000 000), where a tiered amount is held so
`DamageFormula.Calculate`'s product stays within a `long` (decision 818).

## 2. Design overview

### 2.1 What exists (2026-11-15)
- **Skills:** `Skills/SkillDefinition` holds one `Effects/Effect` (kind, flat `Power`, targeting, one optional
  `StatusGrant`, range, hits, damage type) and, since row 2's SE1, an `EffectFile` (its own strip, checked by
  `OwnEffectStripValidator` in the skill and item validators). `Battle/EffectScaling.ScaledFor` raises a damage, heal
  or revive power by the user's `CombatStats.DamagePermille` (100 % plus Power's percent, decision 296) and the grant
  through `Statuses/GrantScaling`; `Battle/SkillEmpowerment` raises the result by a dominant force;
  `Battle/EffectLanding` lands it (hit roll, crit, block, `Effects/DamageFormula.Calculate` with resistance, Exposed
  and block; lifesteal; the grant on a standing target after a landed effect). Items land unscaled (`LandFromItem`).
- **Statuses (ADR 0021):** `Statuses/StatusKind` (Stun, Taunt, Guarded, Exposed, Poison, Regen);
  `StatusGrant(Kind, Power, Turns)`; `ActiveStatus(Kind, Power, TurnsLeft, IsFresh)`; `StatusSet` (one per kind,
  refresh keeps the stronger power); `StatusModifiers` (Guarded on resistances, Exposed on damage taken);
  `GrantScaling` (Intelligence on power and turns, never Stun's turns; Stun's chance against crowd control
  resistance); `Battle/TurnUpkeep.TickStatuses` (Poison, Regen at the end of the holder's turn); `Battle/TurnFlow`
  skips a stunned actor; `Battle/EnemyTargeting` reads Taunt. Events: `StatusApplied`, `StatusEnded`, `StatusTicked`,
  `TurnSkipped`, `StatusResisted`, each carrying a `StatusKind`.
- **Game:** `Battle/StatusNames` (kind to `ui.json` id), `StatusMarksView.IsHarmful(kind)`, `StatusMark` keyed by
  kind, `BattleSounds.TickFileOf(kind)`, `ActionPlayback`'s tick tone by kind; the Status page's Power caption says
  "% damage" (`Menus/AttributeCaptions`).
- **Data:** `skills.json` and `items.json` load straight into the records; `Effects/EffectValidator` with
  `Statuses/StatusGrantValidator` (Stun's power 1..100); no reference between content files is resolved at load (ids
  are looked up at use: `EnemyTactics` takes the skills, `PartyMember.Recruit` the skill catalog).
- **Not in the game:** a gear stat for "+X % damage" (the sheet's "gear 33 %"), a weapon giving Power, and speed in
  the battle (`Battle/TurnOrder` is a shuffled queue), so a status slowing turns has nothing to act on.

### 2.2 Core changes, by slice
| Slice | Change |
|---|---|
| T1 | `Effects/TieredEffect`; `Effect.Tier`; `CombatStats.Power` (members from the sheet); `DamageFormula.TieredAmount(power, perPower, modifierPermille)` and `MaxPower`; `EffectScaling` uses it for a tiered effect. |
| T2 | `Content/ContentCatalog.TieredEffects`. |
| T3 | `Battle/EnemyDefinition.Power`, copied by `Combatant.FromEnemy`. |
| SR1 | `Statuses/StatusDefinition` (with `Family`), `StatusPart`, `StatusPartKind` (six kinds); `ContentCatalog.Statuses`. |
| MS | `Effect.Statuses` replaces `Effect.Status`; `Effect` compares them in order; landing, scaling and empowerment go over the list. |
| CH | `StatusGrant.Chance`; `GrantScaling.ChancePermille` replaces `StunChancePermille`; every grant rolls. |
| SW | `StatusGrant(Status, Turns) { Chance }`; `ActiveStatus(Status, TurnsLeft, IsFresh)`; `StatusSet` by family with `FirstWith(kind)`; `StatusModifiers`, `TurnUpkeep`, `TurnFlow`, `EnemyTargeting`, `EffectLanding` read parts; the five events carry `StatusDefinition`; `StatusKind` deleted. |
| TT | `StatusPart.DamageType`; a typed tick meets resistance. |
| ST | `Statuses/StatusTiming`, `StatusDefinition.ActsAt`; start-of-turn ticks in `TurnFlow`. |
| WK | `StatusPartKind.DamageDealtChange`; `CombatStats.DamageModifierPermille`, which tiered damage and the basic attack read. |
| SL | `StatusPartKind.StatChange`, `StatusPart.Stat`; `StatusModifiers` changes four stats; `StatusDefinition.MarkLetter`. |

### 2.3 The part kinds (decisions 796, 797)

| Kind | `amount` means | Allowed amount | Read by | Since |
|---|---|---|---|---|
| `damageEachTurn` | HP lost per tick | 1 or more | `TurnUpkeep` | SR1 (ticks from SW) |
| `healEachTurn` | HP regained per tick | 1 or more | `TurnUpkeep` | SR1 |
| `skipTurn` | nothing | 0 (left out) | `TurnFlow`, `GrantScaling` (no extra turns, crowd control) | SR1 |
| `drawAttacks` | nothing | 0 (left out) | `EnemyTargeting` | SR1 |
| `resistanceChange` | points on every resistance | not 0 | `StatusModifiers.Apply` | SR1 |
| `damageTakenChange` | percent of each hit taken | above -100, not 0 | `StatusModifiers.ExtraDamageTakenPercent` | SR1 |
| `damageDealtChange` | percent into the damage modifiers' sum | above -100, not 0 | `StatusModifiers.Apply` | WK |
| `statChange` | points on its `stat` (accuracy, dodge, crit chance, crowd control resistance) | not 0 | `StatusModifiers.Apply` | SL |

`damageType` is allowed on `damageEachTurn` only (TT); `stat` on `statChange` only, and required there (SL). At most
one of `damageEachTurn` and `healEachTurn` per status (`StatusDefinition.TickingPart` is that one); every kind at most
once, except `statChange` once per stat. Records of one family have the same parts in the same order (kinds, types and
stats), so a refresh can compare them part by part (decision 800).

### 2.4 Structure and folder dependencies

New types and their folders:
- Core `Effects/TieredEffect` (T1): a named damage or heal whose amount is its user's Power times its `perPower`.
- Core `Statuses/StatusDefinition`, `StatusPart`, `StatusPartKind` (SR1); `StatusTiming` (ST).
- Data `Content/CatalogReferenceConverter<T>` (T2): reads a content id as the record it names, writes the record's id.
- Data `Effects/EffectCatalogValidator` (T2): the rules of `effects.json`.
- Data `Statuses/StatusCatalogValidator` (SR1): the rules of `statuses.json`.
- Tests: `Jrpg.Core.Tests/Builders/TestStatuses` (R1), `Jrpg.Game.Tests/Battle/TestStatuses` (R2).

| From | To | Why | New? |
|---|---|---|---|
| Core `Statuses` | `Stats` | `StatFormula.Scaled` (strengthening), `DamageType` (TT), `StatKind` (SL) | `DamageType` and `StatKind` new uses |
| Core `Effects` | `Statuses`, `Stats` | `Effect.Statuses`, `DamageType` | existing |
| Core `Content` | `Effects`, `Statuses` | `ContentCatalog.TieredEffects`, `.Statuses` | new edges; neither reads `Content` |
| Core `Battle` | `Effects`, `Statuses`, `Stats` | landing, upkeep, scaling | existing |
| Data `Content` | Data `Effects`, `Statuses`; Core `Effects`, `Statuses` | loader, converter, filters | existing folders |
| Data `Effects`, `Statuses` | Core `Effects`, `Statuses`, `Stats` | validators | existing |
| Game `Battle` | Core `Statuses` | names, marks, ticks | existing |

`Statuses` stays below `Effects` and `Battle`: it reads neither (ADR 0021's rule kept). No cycle.

### 2.5 Key decisions (README numbers)
- **An effects catalog (782):** `effects.json` holds `fire_1`, `heal_2` and the rest; a skill's effect names one as
  `"tier": "fire_1"` and still writes its own `kind` and `damageType`, which Data checks against the record. Section 4
  compares.
- **The tiered amount (783, 784, 786, 817):** Power x `perPower` x (1 + modifiers), then today's hit chain; a dominant
  force multiplies after; Power's percent stays on the basic attack only. Section 5 works it through.
- **References resolve at load (789):** `CatalogReferenceConverter<T>` is given the loaded tiered effects (T2) and
  statuses (SW) and registered on the options that read `skills.json` and `items.json`
  (`ContentJsonOptions.StrictWith`); the loader reads `effects.json` and `statuses.json` before them. An unknown id is
  a `JsonException` the reader turns into "skills.json at $[3].effect.tier: no effect 'fire_9'.".
- **The landed status is the record with raised amounts (799):** `StatusDefinition.Strengthened(permille)` scales every
  part's amount once (Intelligence) and again for dominance, rounding down each time as today's two steps do, so the
  swap keeps every number.
- **Kept by family, the stronger part by part (800):** a status's `family` (its id when absent) is what a holder keeps
  one of; records of one family have the same parts, so "farther from zero" part by part is ADR 0021's "stronger" for
  every kind, a negative one (Weakened) included.
- **Chance (801, CH):** `GrantScaling.ChancePermille(scaled, crowdControlResistancePermille)` = the scaled chance in
  permille, less the resistance for a skipping status (Stun until SW, `skipTurn` after), within 0..1000;
  `ChanceRolls.Succeeds` draws only between.

### 2.6 Replacement mapping
| Today | Becomes | Slice |
|---|---|---|
| A skill effect's flat `power` x `DamagePermille` | its tiered effect: Power x `perPower` (`DamageFormula.TieredAmount`) | T1, T4 |
| `Effect.Status` (one grant) | `Effect.Statuses` (a list) | MS |
| Stun's power as its chance; `GrantScaling.StunChancePermille` | `StatusGrant.Chance`; `GrantScaling.ChancePermille` | CH |
| `StatusKind` (six members) | `StatusDefinition` records over `StatusPartKind` | SR1, SW |
| `StatusGrant(Kind, Power, Turns)` | `StatusGrant(Status, Turns) { Chance }` | SW |
| `ActiveStatus(Kind, Power, ...)` | `ActiveStatus(Status, ...)`, amounts inside the strengthened record | SW |
| `StatusSet.Has(kind)`, one per kind | `Has(familyKey)` (the game's marks), `FirstWith(part kind)` (the rules), one per family | SW |
| `StatusModifiers` by kind, `TurnUpkeep`'s Poison/Regen switch | sums over parts, `StatusDefinition.TickingPart` | SW |
| `Data/Statuses/StatusGrantValidator`'s Stun power rule | the chance rule (1..100) | CH |
| `Game/Battle/StatusNames`, `ui.json` `status_*` | `StatusDefinition.Name` | SW |
| `StatusMarksView.IsHarmful(kind)`, the name's first letter | `StatusDefinition.IsHarmful`; `MarkLetter`, else the name's first letter | SW, SL |
| `BattleSounds.TickFileOf(kind)`, `ActionPlayback`'s tick tone | the tick part's kind | SW |

### 2.7 Extension points
- **A ticking part scaling from the granter's stat** (decision 816's overturn): a `damageEachTurn` or `healEachTurn`
  part may give a `perPower` against the granter's Power or Intelligence, captured when granted, like a tiered effect;
  about one slice after SW.
- **A sixth, smaller tier** (decision 790's alternative): records with `perPower` 1 (`blunt_0`); content only.
- **More `statChange` stats:** one arm in `StatusModifiers` and one entry in `StatusModifiers.ChangeableStats` each.
- **Gear's damage modifier (row 9):** adds to `CombatStats.DamageModifierPermille` beside the statuses' (WK).
- **Slowed slowing turns:** needs a speed-based `TurnOrder` first (backlog).
- **Two instant effects per skill (decision 675):** unchanged; `SkillDefinition.Effect` stays one.

## 3. The content conversion

### 3.1 Skills to tiered effects (T4; decision 790)
Today's amount at level 1 is the flat power x (100 + the class's base Power) / 100, rounded down; an enemy's is its
power (its `DamagePermille` is 100 %). The tier chosen is the nearest on a ratio scale. Class base Power: mage, cleric,
warlock, druid 12; rogue 13; paladin, monk 14; fighter 15; barbarian 16; Goblin Chief 5 (decision 791).

| Skill | User | Today's power | Today at level 1 | Tiered effect | At level 1 |
|---|---|---|---|---|---|
| fire_blast | mage | 15 | 16 | `fire_1` | 24 |
| fireball | mage | 20 | 22 | `fire_1` | 24 |
| meteor | mage | 60 | 67 | `fire_2` | 48 |
| heal | cleric | 15 | 16 | `heal_1` | 24 |
| recovery | cleric | 30 | 33 | `heal_1` | 24 |
| heal_all | cleric | 50 | 56 | `heal_2` | 48 |
| power_strike | fighter | 35 | 40 | `slashing_1` | 30 |
| shield_bash | fighter | 15 | 17 | `blunt_1` | 30 |
| rampage | barbarian | 70 | 81 | `blunt_2` | 64 |
| smite | paladin | 40 | 45 | `light_2` | 56 |
| eldritch_bolt | warlock | 20 | 22 | `dark_1` | 24 |
| void_rift | warlock | 25 | 28 | `dark_1` | 24 |
| chaos_lance | warlock | 70 | 78 | `dark_3` | 108 |
| equilibrium | druid | 45 | 50 | `piercing_2` | 48 |
| palm_strike (2 hits) | monk | 10 | 11 a hit | `blunt_1` | 28 a hit |
| flurry (4 hits) | monk | 8 | 9 a hit | `blunt_1` | 28 a hit |
| cleave | Goblin Chief | 10 | 10 | `slashing_1` | 10 |
| crushing_blow | Goblin Chief | 24 | 24 | `blunt_2` | 20 |
| field_dressing | Goblin Chief | 25 | 25 | `heal_2` | 20 |
| cone_of_cold (SL) | mage | (new) | | `ice_1` | 24 |

`effects.json` therefore holds 11 records at T4 (`blunt_1`, `blunt_2`, `dark_1`, `dark_3`, `fire_1`, `fire_2`,
`heal_1`, `heal_2`, `light_2`, `piercing_2`, `slashing_1`) and 12 after SL (`ice_1`). Each damage record's
`damageType` is its stem; the heals have none. Status grants stay on the skill's effect, never on the record.

What the conversion does, in numbers (decision 790, a default): Flurry becomes 4 x 28 = 112 at level 1, the strongest
single-target skill (ahead of Rampage 64 and Meteor 48), usable every turn (its 3 Chakra come back from its own four
hits); Palm Strike deals 56 with its 30 % stun at no cost; Heal (15) and Recovery (30) both become 24; Eldritch Bolt
and Void Rift both become Low. The alternative not taken: a sixth tier of `perPower` 1 (`blunt_0`: Flurry 56, Palm
Strike 28).

Taunt, Aura of Protection, Wide Aura, Ready, Expose, Rejuvenate, Rot, War Cry and Enfeeble change no HP and name no
tiered effect. Every tiered effect keeps `"power": 0` (decision 792). Descriptions lose "15 HP", "30 HP", "50 HP", and
the Status page's Power caption says "attack damage" (decision 793).

### 3.2 Grants to records (SR1 writes the records, MS the list, CH the chances, SW the ids)
| Skill | Today's grant | Becomes |
|---|---|---|
| fire_blast | poison 5, 2 turns | `burning_1`, 2 turns |
| recovery | regen 6, 2 turns | `regen_1`, 2 turns |
| shield_bash | stun 60, 1 turn | `stun`, 1 turn, chance 60 |
| taunt | taunt 0, 2 turns | `taunt`, 2 turns |
| aura_of_protection | guarded 20, 3 turns | `guard_2`, 3 turns |
| wide_aura | guarded 12, 3 turns | `guard_1`, 3 turns |
| expose | exposed 50, 3 turns | `exposed_2`, 3 turns |
| eldritch_bolt | exposed 20, 2 turns | `exposed_1`, 2 turns |
| rejuvenate | regen 10, 3 turns | `regen_2`, 3 turns |
| rot | poison 8, 3 turns | `poison_1`, 3 turns |
| palm_strike | stun 30, 1 turn | `stun`, 1 turn, chance 30 |
| crushing_blow | stun 40, 1 turn | `stun`, 1 turn, chance 40 |
| war_cry | guarded 20, 2 turns | `guard_2`, 2 turns |
| cone_of_cold (SL) | (new) | `slowed_1`, 2 turns |
| enfeeble (WK) | (new) | `weakened_1`, 3 turns |

At SW every number is today's; one behavior changes and is named: Fire Blast's Burning and Rot's Poison are two
families, so both tick on one target (today one Poison refreshed the other). Two Guards still keep the stronger
(family `guard`). TT and ST then change Burning on purpose (decision 805).

### 3.3 `statuses.json` (in the writer's layout, sorted by id; names from today's `ui.json`)
| Id | Name | Family | Harmful | Parts | Added |
|---|---|---|---|---|---|
| `burning_1` | Burning | `burning` | yes | `damageEachTurn` 5 (`fire` from TT; `startOfTurn` from ST) | SR1 |
| `exposed_1` | Exposed | `exposed` | yes | `damageTakenChange` 20 | SR1 |
| `exposed_2` | Exposed | `exposed` | yes | `damageTakenChange` 50 | SR1 |
| `guard_1` | Guard | `guard` | no | `resistanceChange` 12 | SR1 |
| `guard_2` | Guard | `guard` | no | `resistanceChange` 20 | SR1 |
| `poison_1` | Poison | `poison` | yes | `damageEachTurn` 8 | SR1 |
| `regen_1` | Regen | `regen` | no | `healEachTurn` 6 | SR1 |
| `regen_2` | Regen | `regen` | no | `healEachTurn` 10 | SR1 |
| `slowed_1` | Slowed | `slowed` | yes | `statChange` accuracy -15, `statChange` critChance -5; `markLetter` "L" | SL |
| `stun` | Stun | (its id) | yes | `skipTurn` | SR1 |
| `taunt` | Taunt | (its id) | no | `drawAttacks` | SR1 |
| `weakened_1` | Weakened | `weakened` | yes | `damageDealtChange` -25 | WK |

Stun and Taunt have no number, so no tier and no family: their strength is the grant's chance (Stun) or nothing.

## 4. ADR 0056's options, honestly

**B. An `effects.json` catalog of named records (chosen, decision 782; the developer's own words, decisions 703 and
704: "that is why there is fire_1 fire_2 or poison_1 etc.").** `{ "id": "fire_1", "kind": "damage", "perPower": 2,
"damageType": "fire" }`, the skill writing `"effect": { "kind": "damage", "damageType": "fire", "power": 0, "tier":
"fire_1", ... }`.
- Pros: his vocabulary; one record per named effect, tuned on its own (a fire High unlike an ice High); everything
  naming `fire_1` reads the same numbers (decision 703); items could name records later (row 9).
- Costs: 11 records for today's content, 12 after SL, not one per type and tier; the skill keeps writing its kind and
  damage type, so `Effect.Kind`'s 33 source readers don't change and Data refuses a mismatch with the record
  ("effect.damageType is ice but fire_1 is fire."); one more Database window in row 7 (id, kind, type, `perPower`); and the
  resolved references row 7 must rebuild when a record is edited (decision 789, section 6).

**A. Tiers in `rules.json`, named by the skill's effect.** `"tier": "low"` against a five-entry `damageTiers` table.
- Pros: one number per tier; no catalog or window.
- Cons: no `fire_1` record, against the developer's two statements; no per-record tuning. Refused.

**C. Keep a flat power raised by Power's percent (today).** Refused: decision 704 asked for tiers.

## 5. The formula worked through
- The developer's sheet: Low at Power 1 with gear 33 % and other 440 %: 1 x 2 x (1 + 4.73) = 11.46, rounded down to 11
  (`DamageFormula.TieredAmount(1, 2, 4730)`, T1's test); Extreme at Power 500: 500 x 58 x 5.73 = 166 170.
- A level 1 fighter (Power 15) with Power Strike (`slashing_1`): 15 x 2 = 30. A crit multiplies by 157 % (150 % plus
  Power's 7.5 %, kept, decision 817): 47. Against the goblin (no slashing resistance): 30, or 47. Weakened (WK): 30 x
  0.75 = 22.
- Dominance multiplies after the sum (decision 817): a Weakened druid with Growth dominant deals x 0.75 x 1.5 = x 1.125,
  where the sheet's one sum would give 1 - 0.25 + 0.5 = x 1.25.
- The same fighter's basic attack: weapon damage x 115 %, Weakened's sum applied (WK).
- A druid with Growth dominant using Rejuvenate (`regen_2` 10, Intelligence 14 at 100 % per 100 points): 10 x 1.14 = 11,
  then x 1.5 = 16 per tick, exactly today's two roundings.
- From level 1 to 20 (decision 816): a mage's Power goes from 12 to about 50, so a tiered skill grows about 4.4x with
  gear; its Intelligence from 16 to about 54, so a tick grows from x 1.16 to x 1.54, about 1.3x.

## 6. Notes for row 7 (the editors)
- **An Effects window** (id, kind, damage type, `perPower`); the Skills form picks `fire_1` and fills the effect's kind
  and damage type from it.
- The draft holds resolved records: a skill's `Effect.Tier` is the `TieredEffect`, a grant's `Status` the
  `StatusDefinition`. Editing a tiered effect or a status must rebuild every skill naming it (or the skills compare by
  content and look changed); the Skills window resolves its picks from the draft at Apply.
- The writer must register both converters to write ids; `skills.json` is relayed through it once (load-write-load
  test); `effects.json` and `statuses.json` are already in its layout (T2's and SR1's tests).
- `"power": 0` beside a tiered effect (decision 792) is the relay's to drop if `Effect`'s power becomes optional.

## 7. Risks
- **Balance:** tiered damage grows linearly with Power, about 4.4x from level 1 to 20, while ticks grow about 1.3x
  (decision 816); Flurry and Palm Strike jump (decision 790). Enemy HP are placeholders. The forest simulations are
  unaffected (attack only, no enemy skills); the Chief's numbers barely move (section 3.1).
- **Behavior changes:** every grant now rolls its chance and Intelligence raises every chance (CH; only the stuns are
  below 100 today); Burning and Poison on one target both tick (SW). Each is named in its slice.
- **Test churn:** 130 test sites name `StatusKind` or `new StatusGrant(` (92 in Core.Tests, 32 in Game.Tests, 6 in
  Data.Tests). R1 and R2 move the Core and Game ones behind two fixture files first; SW still touches about 260 lines,
  most of them mechanical or deleted.
- **Repository tests pinning skills:** `ContentCatalogLoaderTests` pins Shield Bash's grant and Recovery's whole effect
  (rewritten to ids and shape in T4, MS, CH and SW); Game tests run repository skills through `BattleScenarios`
  (`StatusMarksViewTests` bashes a 30 HP goblin with Sarxian, Power 12: `blunt_1` deals 24, it survives; T4 checks);
  `AttributeCaptionsTests` pins the Power caption's words (T4 rewords both).
- **Determinism:** `ChanceRolls` draws only when uncertain and every repository grant but the stuns is 100 %, so seeded
  tests keep their draws; tests of a chance below 100 use `FixedSequenceRandom`.
- **Overflow:** `DamageFormula.Calculate` multiplies the power by up to about 1.8 x 10^12 in a `long`; a tiered amount
  is held at `MaxPower` (10^6), and Data bounds `perPower` (1000) and an enemy's power (10 000) (decision 818). A
  member's Power has no bound but grows by a few points a level.
- **Equality of records holding lists:** `StatusDefinition` (parts) and `Effect` (statuses) override equality to
  compare in order (SR1, MS), as `DropTable` does (decision 604).
- **Playback of start ticks** (ST): ticks of the next actor arrive after the actor's events in one `Execute`, as a
  stunned actor's ticks already do; ST adds a playback test.
- **Hot files:** `BattleState.cs` 283 (untouched); `ActionPlayback.cs` 274 (SW edits two lines in place);
  `TurnFlow.AdvanceToNextActor` gains the start tick through a named helper (ST); `StatusModifiers.Apply` reaches four
  stat arms in SL through a per-stat helper.
- **Row 2's validators:** SE1 chained `OwnEffectStripValidator` into the skill and item validators, and SE2 and SE3
  land before this plan; T2's item rule and T4's skill rule join those chains (each slice names its place) and T4's
  content edits keep every `effectFile` line.
- **Save format:** none (decision 812).
- **Load order:** `effects.json` (T2) and `statuses.json` (SW) are read before `skills.json` and `items.json`; a missing
  file stops the start naming it, as every catalog does.
