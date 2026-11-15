# Skills and statuses model: design

Read with the [README](README.md). Sections: 1 tunables, 2 design overview, 3 the content conversion, 4 ADR 0056's
options, 5 the formula worked through, 6 notes for row 7, 7 risks.

## 1. Tunables and placeholders

Every number is content; "placeholder" means the developer replaces it while balancing.

| Name | Where | Value | Meaning |
|---|---|---|---|
| Damage tiers | `rules.json` `damageTiers` | low 2, medium 4, high 9, massive 22, extreme 58 | Damage or healing per point of the user's Power (the developer's "Damage Test" sheet, decision 704). |
| Goblin Chief's power | `enemies.json` `power` | 5 (placeholder) | What its Cleave, Crushing Blow and Field Dressing multiply (decision 791). |
| Status numbers | `statuses.json` parts' `amount` | section 3.3 (today's numbers) | A tick's HP, Guard's points, Exposed's share (decision 705's tiered records). |
| Grant turns and chance | `skills.json` `statuses` | today's turns; Stun chances 60, 30, 40 | How long a grant lasts and how likely it lands (decision 798). |
| Burning's timing | `statuses.json` `burning_1.actsAt` | `startOfTurn` (placeholder) | Decision 805. |
| Weakened | `statuses.json` `weakened_1` | -25 % damage dealt (placeholder) | Decision 806. |
| Slowed | `statuses.json` `slowed_1` | -15 accuracy, -10 dodge points (placeholder) | Decision 807. |
| Cone of Cold, Enfeeble | `skills.json`, `classes.json` | ice at Low + Slowed 2 turns, 4 mana; Weakened 3 turns, +2 instability (placeholders) | Decision 808. |

No Core constant is added: a tier's multiplier, a status's amount and a grant's chance are all content.

## 2. Design overview

### 2.1 What exists (2026-11-15)
- **Skills:** `Skills/SkillDefinition` holds one `Effects/Effect` (kind, flat `Power`, targeting, one optional
  `StatusGrant`, range, hits, damage type). `Battle/EffectScaling.ScaledFor` raises a damage, heal or revive power by
  the user's `CombatStats.DamagePermille` (100 % plus Power's percent, decision 296) and the grant through
  `Statuses/GrantScaling`; `Battle/SkillEmpowerment` raises the result by a dominant force; `Battle/EffectLanding`
  lands it (hit roll, crit, block, `Effects/DamageFormula.Calculate` with resistance, Exposed and block; lifesteal;
  the grant on a standing target after a landed effect). Items land unscaled (`LandFromItem`).
- **Statuses (ADR 0021):** `Statuses/StatusKind` (Stun, Taunt, Guarded, Exposed, Poison, Regen);
  `StatusGrant(Kind, Power, Turns)`; `ActiveStatus(Kind, Power, TurnsLeft, IsFresh)`; `StatusSet` (one per kind,
  refresh keeps the stronger power); `StatusModifiers` (Guarded on resistances, Exposed on damage taken);
  `GrantScaling` (Intelligence on power and turns, never Stun's turns; Stun's chance against crowd control
  resistance); `Battle/TurnUpkeep.TickStatuses` (Poison, Regen at the end of the holder's turn); `Battle/TurnFlow`
  skips a stunned actor; `Battle/EnemyTargeting` reads Taunt. Events: `StatusApplied`, `StatusEnded`, `StatusTicked`,
  `TurnSkipped`, `StatusResisted`, each carrying a `StatusKind`.
- **Game:** `Battle/StatusNames` (kind to `ui.json` id), `StatusMarksView.IsHarmful(kind)`, `StatusMark` keyed by
  kind, `BattleSounds.TickFileOf(kind)`, `ActionPlayback`'s tick tone by kind.
- **Data:** `skills.json` and `items.json` load straight into the records; `Effects/EffectValidator` with
  `Statuses/StatusGrantValidator` (Stun's power 1..100); no reference between content files is resolved at load (ids
  are looked up at use: `EnemyTactics` takes the skills, `PartyMember.Recruit` the skill catalog).
- **Not used by the battle:** speed (`Battle/TurnOrder` is a shuffled queue), so a status slowing turns has nothing to
  act on (decision 807).

### 2.2 Core changes, by slice
| Slice | Change |
|---|---|
| T1 | `Effects/DamageTier`; `Effect.Tier`; `CombatStats.Power` (members from the sheet); `DamageFormula.TieredAmount`; `EffectScaling` uses it for a tiered effect. |
| T2 | `Content/GameRules.DamageTiers`. |
| T3 | `Battle/EnemyDefinition.Power`, copied by `Combatant.FromEnemy`. |
| SC1 | `Statuses/StatusDefinition`, `StatusPart`, `StatusPartKind` (six kinds); `ContentCatalog.Statuses`. |
| MS | `Effect.Statuses` replaces `Effect.Status`; `Effect` compares them in order; landing, scaling and empowerment go over the list. |
| SW | `StatusGrant(Status, Turns) { Chance }`; `ActiveStatus(Status, TurnsLeft, IsFresh)`; `StatusSet` by id with `FirstWith(kind)`; `StatusModifiers`, `TurnUpkeep`, `TurnFlow`, `EnemyTargeting`, `EffectLanding` read parts; `GrantScaling.ChancePermille`; the five events carry `StatusDefinition`; `StatusKind` deleted. |
| TT | `StatusPart.DamageType`; a typed tick meets resistance. |
| ST | `Statuses/StatusTiming`, `StatusDefinition.ActsAt`; start-of-turn ticks in `TurnFlow`. |
| WK | `StatusPartKind.DamageDealtChange`; `CombatStats.DamageModifierPermille`; `TieredAmount` takes the modifiers. |
| SL | `StatusPartKind.StatChange`, `StatusPart.Stat`; `StatusModifiers` changes four stats. |

### 2.3 The part kinds (decisions 796, 797)

| Kind | `amount` means | Allowed amount | Read by | Since |
|---|---|---|---|---|
| `damageEachTurn` | HP lost per tick | 1 or more | `TurnUpkeep` | SC1 (ticks from SW) |
| `healEachTurn` | HP regained per tick | 1 or more | `TurnUpkeep` | SC1 |
| `skipTurn` | nothing | 0 (left out) | `TurnFlow`, `GrantScaling` (no extra turns, crowd control) | SC1 |
| `drawAttacks` | nothing | 0 (left out) | `EnemyTargeting` | SC1 |
| `resistanceChange` | points on every resistance | not 0 | `StatusModifiers.Apply` | SC1 |
| `damageTakenChange` | percent of each hit taken | above -100, not 0 | `StatusModifiers.ExtraDamageTakenPercent` | SC1 |
| `damageDealtChange` | percent into the damage modifiers' sum | above -100, not 0 | `StatusModifiers.Apply` | WK |
| `statChange` | points on its `stat` (accuracy, dodge, crit chance, crowd control resistance) | not 0 | `StatusModifiers.Apply` | SL |

`damageType` is allowed on `damageEachTurn` only (TT); `stat` on `statChange` only, and required there (SL). At most
one of `damageEachTurn` and `healEachTurn` per status (`StatusDefinition.TickingPart` is that one); every kind at most
once, except `statChange` once per stat.

### 2.4 Structure and folder dependencies

New types and their folders:
- Core `Effects/DamageTier` (T1): a named step of damage or healing per point of its user's Power.
- Core `Statuses/StatusDefinition`, `StatusPart`, `StatusPartKind` (SC1); `StatusTiming` (ST).
- Data `Content/CatalogReferenceConverter<T>` (T2): reads a content id as the record it names, writes the record's id.
- Data `Statuses/StatusCatalogValidator` (SC1): the rules of `statuses.json`.
- Tests: `Jrpg.Core.Tests/Builders/TestStatuses` (R1), `Jrpg.Game.Tests/Battle/TestStatuses` (R2).

| From | To | Why | New? |
|---|---|---|---|
| Core `Statuses` | `Stats` | `StatFormula.Scaled` (strengthening), `DamageType` (TT), `StatKind` (SL) | `DamageType` and `StatKind` new uses |
| Core `Effects` | `Statuses`, `Stats` | `Effect.Statuses`, `DamageType` | existing |
| Core `Content` | `Effects` | `GameRules.DamageTiers` | new edge; `Effects` never reads `Content` |
| Core `Battle` | `Effects`, `Statuses`, `Stats` | landing, upkeep, scaling | existing |
| Data `Content` | Data `Statuses`, Core `Statuses`, `Effects` | loader, converter, filters | existing folders |
| Data `Statuses` | Core `Statuses`, `Stats` | validators | existing |
| Game `Battle` | Core `Statuses` | names, marks, ticks | existing |

`Statuses` stays below `Effects` and `Battle`: it reads neither (ADR 0021's rule kept). No cycle.

### 2.5 Key decisions (README numbers)
- **Where tiers live (782):** `rules.json`, named by the skill's effect; section 4 compares.
- **The tiered amount (783, 784, 786):** Power x `perPower` x (1 + modifiers), then today's hit chain; Power's percent
  stays on the basic attack only. Section 5 works it through.
- **References resolve at load (789):** `CatalogReferenceConverter<T>` is given the loaded tiers (T2) and statuses
  (SW) and registered on the options that read `skills.json` and `items.json` (`ContentJsonOptions.StrictWith`); the
  loader reads `rules.json` and `statuses.json` before them. An unknown id is a `JsonException` the reader turns into
  "skills.json at $[3].effect.tier: no damage tier 'lw'.".
- **The landed status is the record with raised amounts (799):** `StatusDefinition.Strengthened(permille)` scales every
  part's amount once (Intelligence) and again for dominance, rounding down each time as today's two steps do, so the
  swap keeps every number.
- **Kept by id, the stronger part by part (800):** two records of one id differ only by a positive factor, so "farther
  from zero" part by part is ADR 0021's "stronger" for every kind, a negative one (Weakened) included.
- **Chance (801):** `GrantScaling.ChancePermille(scaled, crowdControlResistancePermille)` = the scaled chance in
  permille, less the resistance when the status has `skipTurn`, within 0..1000; `ChanceRolls.Succeeds` draws only
  between.

### 2.6 Replacement mapping
| Today | Becomes | Slice |
|---|---|---|
| A skill effect's flat `power` x `DamagePermille` | the tier: Power x `perPower` (`DamageFormula.TieredAmount`) | T1, T4 |
| `Effect.Status` (one grant) | `Effect.Statuses` (a list) | MS |
| `StatusKind` (six members) | `StatusDefinition` records over `StatusPartKind` | SC1, SW |
| `StatusGrant(Kind, Power, Turns)` | `StatusGrant(Status, Turns) { Chance }` | SW |
| `ActiveStatus(Kind, Power, ...)` | `ActiveStatus(Status, ...)`, amounts inside the strengthened record | SW |
| `StatusSet.Has(kind)` | `Has(id)` (the game's marks), `FirstWith(part kind)` (the rules) | SW |
| `GrantScaling.StunChancePermille` | `GrantScaling.ChancePermille` | SW |
| `StatusModifiers` by kind, `TurnUpkeep`'s Poison/Regen switch | sums over parts, `StatusDefinition.TickingPart` | SW |
| `Data/Statuses/StatusGrantValidator`'s Stun power rule | the chance rule (1..100) | SW |
| `Game/Battle/StatusNames`, `ui.json` `status_*` | `StatusDefinition.Name` | SW |
| `StatusMarksView.IsHarmful(kind)` | `StatusDefinition.IsHarmful` | SW |
| `BattleSounds.TickFileOf(kind)`, `ActionPlayback`'s tick tone | the tick part's kind | SW |

### 2.7 Extension points
- **An `effects.json` catalog** (section 4, option B): the skill's effect would name a record instead of a tier; the
  converter already resolves ids.
- **A `family` field** on a status, if tiers of one status should replace each other (decision 800's overturn).
- **More `statChange` stats:** one arm in `StatusModifiers` and one allowed stat in the validator each.
- **Gear's damage modifier (row 9):** adds to `CombatStats.DamageModifierPermille` beside the statuses' (WK).
- **Slowed slowing turns:** needs a speed-based `TurnOrder` first (backlog).
- **Two instant effects per skill (decision 675):** unchanged; `SkillDefinition.Effect` stays one.

## 3. The content conversion

### 3.1 Skills to tiers (T4; decision 790)
Today's amount at level 1 is the flat power x (100 + the class's base Power) / 100, rounded down; an enemy's is its
power (its `DamagePermille` is 100 %). The tier chosen is the nearest on a ratio scale. Class base Power: mage, cleric,
warlock, druid 12; rogue 13; paladin, monk 14; fighter 15; barbarian 16; Goblin Chief 5 (decision 791).

| Skill | User | Type | Today's power | Today at level 1 | Tier | Tier at level 1 |
|---|---|---|---|---|---|---|
| fire_blast | mage | fire | 15 | 16 | low | 24 |
| fireball | mage | fire | 20 | 22 | low | 24 |
| meteor | mage | fire | 60 | 67 | medium | 48 |
| heal | cleric | (heal) | 15 | 16 | low | 24 |
| recovery | cleric | (heal) | 30 | 33 | low | 24 |
| heal_all | cleric | (heal) | 50 | 56 | medium | 48 |
| power_strike | fighter | slashing | 35 | 40 | low | 30 |
| shield_bash | fighter | blunt | 15 | 17 | low | 30 |
| rampage | barbarian | blunt | 70 | 81 | medium | 64 |
| smite | paladin | light | 40 | 45 | medium | 56 |
| eldritch_bolt | warlock | dark | 20 | 22 | low | 24 |
| void_rift | warlock | dark | 25 | 28 | low | 24 |
| chaos_lance | warlock | dark | 70 | 78 | high | 108 |
| equilibrium | druid | piercing | 45 | 50 | medium | 48 |
| palm_strike (2 hits) | monk | blunt | 10 | 11 a hit | low | 28 a hit |
| flurry (4 hits) | monk | blunt | 8 | 9 a hit | low | 28 a hit |
| cleave | Goblin Chief | slashing | 10 | 10 | low | 10 |
| crushing_blow | Goblin Chief | blunt | 24 | 24 | medium | 20 |
| field_dressing | Goblin Chief | (heal) | 25 | 25 | medium | 20 |

Taunt, Aura of Protection, Wide Aura, Ready, Expose, Rejuvenate, Rot and War Cry change no HP and name no tier. Every
tiered effect keeps `"power": 0` (decision 792). Descriptions lose "15 HP", "30 HP", "50 HP" (decision 793).

### 3.2 Grants to records (SC1 writes the records, MS the list, SW the ids)
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

At SW every number is today's; what changes is named: Fire Blast's burn and Rot's poison are two statuses now, and
Aura of Protection with Wide Aura on one ally add up (decision 800). TT and ST then change Burning on purpose
(decision 805).

### 3.3 `statuses.json` (in the writer's layout, sorted by id; names from today's `ui.json`)
| Id | Name | Harmful | Parts | Added |
|---|---|---|---|---|
| `burning_1` | Burning | yes | `damageEachTurn` 5 (`fire` from TT; `startOfTurn` from ST) | SC1 |
| `exposed_1` | Exposed | yes | `damageTakenChange` 20 | SC1 |
| `exposed_2` | Exposed | yes | `damageTakenChange` 50 | SC1 |
| `guard_1` | Guard | no | `resistanceChange` 12 | SC1 |
| `guard_2` | Guard | no | `resistanceChange` 20 | SC1 |
| `poison_1` | Poison | yes | `damageEachTurn` 8 | SC1 |
| `regen_1` | Regen | no | `healEachTurn` 6 | SC1 |
| `regen_2` | Regen | no | `healEachTurn` 10 | SC1 |
| `slowed_1` | Slowed | yes | `statChange` accuracy -15, `statChange` dodge -10 | SL |
| `stun` | Stun | yes | `skipTurn` | SC1 |
| `taunt` | Taunt | no | `drawAttacks` | SC1 |
| `weakened_1` | Weakened | yes | `damageDealtChange` -25 | WK |

Stun and Taunt have no number, so no tier: their strength is the grant's chance (Stun) or nothing.

## 4. ADR 0056's options, honestly

**A. Tiers in `rules.json`, named by the skill's effect (chosen, decision 782).** `"effect": { "kind": "damage",
"damageType": "fire", "power": 0, "tier": "low", ... }`.
- Pros: no new catalog or window; the skill form (row 7) gets two drop-downs it needs anyway; retuning "High" is one
  number; the effect keeps its kind and damage type, so nothing is written twice.
- Cons: no record called `fire_1`; a per-type tier (a fire High unlike an ice High) is impossible without a change.

**B. An `effects.json` catalog of named records (the developer's own words in decision 704).** `{ "id": "fire_1",
"kind": "damage", "damageType": "fire", "tier": "low" }`, the skill naming `"effect": "fire_1"`.
- Pros: his vocabulary; per-record tuning; items could share records (decision 703).
- Cons: about 40 records for eight types and five tiers, plus heals; the skill's effect either loses its kind and
  damage type (read through the record: every reader of `Effect.Kind`, 33 source uses, changes) or repeats them (a
  validator keeps the copies equal); one more Database window in row 7; about two more slices here.
- Overturn path: T4's content becomes records, `Effect.Tier` becomes a reference to an `EffectRecord` resolved by the
  same converter; T1-T3 stand.

**C. Keep a flat power raised by Power's percent (today).** Refused: decision 704 asked for tiers.

## 5. The formula worked through
- Developer's sheet: Low at Power 1 with gear 33 % and other 440 %: 1 x 2 x (1 + 4.73) = 11.46, rounded down to 11
  (`DamageFormula.TieredAmount(1, 2, 4730)`, WK's test); Extreme at Power 500: 500 x 58 x 5.73 = 166 170.
- A level 1 fighter (Power 15) with Power Strike (slashing, low): 15 x 2 = 30. A crit multiplies by 157 % (150 % plus
  Power's 7.5 %): 47. Against the goblin (no slashing resistance): 30, or 47. Weakened (WK): 30 x 0.75 = 22.
- The same fighter's basic attack: unchanged, weapon damage x 115 %.
- A druid with Growth dominant using Rejuvenate (Regen 10, Intelligence 14 at 100 % per 100 points): 10 x 1.14 = 11,
  then x 1.5 = 16 per tick, exactly today's two roundings.

## 6. Notes for row 7 (the editors)
- The draft holds resolved records: a skill's `Effect.Tier` is the `DamageTier`, a grant's `Status` the
  `StatusDefinition`. Editing a status record in a Statuses window must rebuild every skill grant naming it (or the
  skills compare by content and look changed); the Skills window resolves its picks from the draft at Apply.
- The writer must register both converters to write ids; `skills.json` and `statuses.json` are relayed through it
  once (load-write-load test); `statuses.json` is already in its layout (SC1's test).
- `"power": 0` beside a tier (decision 792) is the relay's to drop if `Effect`'s power becomes optional.

## 7. Risks
- **Balance:** tiered damage grows linearly with Power, not as a flat power times a percent. At level 20 (Power about
  53) Low deals 106 and Extreme 3 074; enemy HP are placeholders. Palm Strike and Flurry triple at level 1 (no tier
  below Low). The forest simulations are unaffected (attack only, no enemy skills); the Chief's numbers barely move
  (section 3.1). Mitigation: placeholders, decision 790's rule written down so the developer reruns it.
- **Behavior changes at the swap** (decision 800): two Guards add up; Burning and Poison on one target both tick. Named
  in SW's hand check.
- **Test churn:** 130 test sites name `StatusKind` or `new StatusGrant(` (26 files: 92 in Core.Tests, 32 in
  Game.Tests, 6 in Data.Tests). R1 and R2 move them behind fixtures first; SW still touches about 340 lines, most of
  them mechanical or deleted (its file explains why it can't split).
- **Repository tests pinning skills:** `ContentCatalogLoaderTests` pins Shield Bash's grant and Recovery's whole effect
  (rewritten to ids and shape in T4, MS and SW); Game tests run repository skills through `BattleScenarios`
  (`StatusMarksViewTests` bashes a 30 HP goblin with Sarxian, Power 12: Low deals 24, it survives; T4 checks).
- **Determinism:** every grant now rolls its chance, but `ChanceRolls` draws only when uncertain and every repository
  grant but the stuns is 100 %, so seeded tests keep their draws; tests of a chance below 100 use
  `FixedSequenceRandom`.
- **Equality of records holding lists:** `StatusDefinition` (parts) and `Effect` (statuses) override equality to
  compare in order (SC1, MS), as `DropTable` does (decision 604); a test that compared two effects by reference would
  otherwise pass or fail by accident.
- **Playback of start ticks** (ST): ticks of the next actor arrive after the actor's events in one `Execute`, as a
  stunned actor's ticks already do; ST adds a playback test.
- **Hot files:** `BattleState.cs` 283 (untouched); `ActionPlayback.cs` 274 (SW edits two lines in place);
  `TurnFlow.AdvanceToNextActor` gains the start tick through a named helper (ST); `StatusModifiers.Apply` reaches four
  stat arms in SL through a per-stat helper.
- **Save format:** none (decision 812).
- **Load order:** `rules.json` (T2) and `statuses.json` (SW) are read before `skills.json` and `items.json`; a missing
  `statuses.json` stops the start naming it, as every catalog does.
