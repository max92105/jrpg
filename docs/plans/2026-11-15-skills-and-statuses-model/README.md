# Skills and statuses model (editor push, roadmap row 6)
Status: approved

Drafted on 2026-11-15 from roadmap row 6 of [the editor push](../2026-11-15-editor-push/README.md), its design.md
sections 3.8 and 5, and the developer's decisions 675-677, 695-697 and 702-705 there. Approved on 2026-11-15 by
delegation from the developer, with the devil's advocate's review applied: an effects catalog of named records
(decision 782, his "that is why there is fire_1 fire_2 or poison_1 etc."), statuses kept by family (800), the chance
split out of the swap (CH, decision 819), Slowed given a part that bites (807), bounds against overflow (818), and the
loud defaults below. Every game-design choice taken while he was away is listed in "Defaults to confirm", his to
overturn.

**Before this plan starts:** row 2's [skill effect strips](../2026-11-15-skill-effect-strips/README.md) SE1 is
committed (`EffectFile` on skills and items, `OwnEffectStripValidator` chained into both validators); SE2 and SE3 will
be committed before T1. T2 and T4 add their rules to those chains, and T4's content edits keep every `effectFile`
line.

## Goal
1. **Damage tiers (decisions 703, 704):** `effects.json` holds named records (`fire_1`, `heal_2`, ...), each a damage
   or heal whose amount is its user's Power times its `perPower` (the developer's "Damage Test" sheet: the suffix
   `_1` to `_5` is Low 2, Medium 4, High 9, Massive 22, Extreme 58), then times 1 plus the sum of its modifiers. Every
   skill in `skills.json` names its record instead of a flat power; enemies get a Power for theirs.
2. **Statuses as records (decisions 676, 695, 705):** `statuses.json` holds tiered status records (`poison_1`,
   `regen_2`, ...) built from a closed Core list of parts and grouped in families; an effect grants any number of
   them, each with turns and a landing chance; each status acts at the start or the end of its holder's turn; Burning
   hurts as fire; Weakened and Slowed are new parts. Today's `StatusKind` is gone.
3. **Every later battle row builds on this schema** (status, effect and skill editors, enemy behavior, items): the plan
   settles the file shapes those editors will write.

## Order, and why
1. **Damage tiers first** (ADR 0056, T1-T4, EP): the content reference converter they need (a record named by id,
   resolved at load) is the one the status swap reuses, and Weakened (WK) adds to the tiered formula. Core first (T1),
   then the catalog and its loading (T2), enemies' Power (T3, EP), and the content conversion last (T4), with the
   simulations rerun.
2. **The status catalog before the swap** (ADR 0055, SR1): `statuses.json` loads and is checked while the battle still
   runs on `StatusKind`, so the swap only rewires.
3. **Tests re-pinned before the swap** (R1, R2, tests only): the Core and Game test sites naming `StatusKind` or
   `new StatusGrant(` go through two `TestStatuses` fixture files, so the swap changes those two files' bodies rather
   than every test file.
4. **Several statuses per effect (MS), then every grant's chance (CH), before the swap,** so `skills.json`'s
   `statuses` list and its chances land once and the swap only rewrites each grant's status.
5. **The swap (SW):** grants name records; Core, Data, Game and content change together, the one slice that can't be
   split further (its file says why).
6. **New behaviors after, one per slice, each with its content:** typed ticks (TT), start of turn (ST), Weakened (WK),
   Slowed (SL).

## Guiding rules
- **Every slice ends green and changes behavior only where its file says so.** The swap (SW) keeps today's numbers:
  every grant becomes a record with the same numbers (design.md, section 3.2); the behavior changes are each named in
  the slice that makes them (CH: every grant rolls its chance; SW: Burning and Poison run together).
- **Numbers stay content:** `perPower` in `effects.json`, status numbers in `statuses.json`, chances and turns on the
  grants. Placeholder numbers are listed in design.md, section 1; the one Core constant is a technical limit
  (`DamageFormula.MaxPower`, decision 818).
- **The battle never reads a catalog:** a content id resolves to its record at load (decision 789).
- **Tests on repository content never pin today's values** (`.claude/rules/tests.md`): a repository test that pinned
  a skill's power or a grant's numbers is rewritten to assert ids and shape, never the new value.
- **Sizes:** Core and Data slices about 300 lines with tests at about 13 lines each; the Editor slice about 200. Raw
  JSON in `effects.json` and `statuses.json` is counted apart.
- **Files near 300 (checked 2026-11-15):** `Core/Battle/BattleState.cs` 283 (no slice adds to it);
  `Game/Battle/ActionPlayback.cs` 274 (SW changes two lines in place); test files past 240:
  `ContentCatalogLoaderTests` 272, `ItemCatalogValidatorTests` 248, `BattleStatusTests` 246, `CombatantTests` 244,
  `ActionPlaybackTests` 243, `BattleTurnSequenceTests` 240: new tests go to the siblings each slice names.
- **Every switch over a part kind** (Core's `StatusModifiers` and `TurnUpkeep`, Data's `StatusCatalogValidator`, the
  Game's tick tone and sound) gets its arm in the slice that adds the kind; each slice names the helper that keeps its
  method under CA1502.

## Out of scope
- Status icons: nothing draws one yet; the field comes with the slice that draws it (decision 795).
- A speed-based turn order, so Slowed can slow turns (decision 807; backlog).
- The status, effect and skill editors (roadmap row 7), enemy tactics forms (row 8), gear's "+X % damage" stat (row 9).
- The basic attack's formula (weapon damage times Power's percent): unchanged but for Weakened's modifier (decisions
  784, 806).
- A ticking part scaling from the granter's stat (decision 816's extension, about one slice after SW).
- A skill's own effect strip (row 2, its own plan; decision 814).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when
a slice is done and write its deviations in the slice file. Tunables, design and risks: [design.md](design.md).

### Phase 1: damage tiers
- [ ] ADR 0056, damage tiers as an effects catalog. Write it from decisions 782-794, 817 and 818, with design.md,
  section 4's options and costs (the catalog chosen; tiers in `rules.json` and today's flat power refused) and the
  resolved-references rebuild row 7 needs (decision 789); the developer delegated approval, so no question is asked.
- [ ] **T1. A tiered skill deals its user's Power times its record's `perPower`** (Core, about 160 lines) →
  [slices/T1.md](slices/T1.md)
- [ ] **T2. `effects.json` holds the tiered records and an effect names one by id** (Data + content, about 270 lines
  plus about 60 of JSON) → [slices/T2.md](slices/T2.md)
- [ ] **T3. Enemies have a Power for their tiered skills** (Core + Data, about 170 lines) →
  [slices/T3.md](slices/T3.md)
- [ ] **EP. The Enemies window edits an enemy's Power** (Editor, about 100 lines) → [slices/EP.md](slices/EP.md)
- [ ] **T4. Every skill names its tiered effect** (content + Data + Game, about 220 lines plus `skills.json`) →
  [slices/T4.md](slices/T4.md)

### Phase 2: statuses as records
- [ ] ADR 0055, statuses as content records (amends ADR 0021). Write it from decisions 795-812, 816 and 819 (the
  options: records over a closed part list, today's closed kinds with per-skill numbers, instant effects re-run each
  turn (refused, decision 677)); no question is asked.
- [ ] **SR1. `statuses.json`: the status records and their families, loaded and checked** (Core + Data + content, about
  290 lines plus about 180 of JSON) → [slices/SR1.md](slices/SR1.md)
- [ ] **R1. Core tests build statuses through `TestStatuses`** (tests only, about 250 lines, mechanical) →
  [slices/R1.md](slices/R1.md)
- [ ] **R2. Game tests build status events through `TestStatuses`** (tests only, about 90 lines) →
  [slices/R2.md](slices/R2.md)
- [ ] **MS. An effect grants several statuses** (Core + Data + content, about 200 lines) → [slices/MS.md](slices/MS.md)
- [ ] **CH. Every grant has a landing chance** (Core + Data + content, about 110 lines) → [slices/CH.md](slices/CH.md)
- [ ] **SW. A grant names a status record; `StatusKind` goes** (Core + Data + Game + content, about 260 lines) →
  [slices/SW.md](slices/SW.md)

### Phase 3: the new status behaviors
- [ ] **TT. A tick may have a damage type: Burning meets fire resistance** (Core + Data + content, about 150 lines) →
  [slices/TT.md](slices/TT.md)
- [ ] **ST. A status acts at the start or the end of its holder's turn** (Core + Data + content, about 240 lines) →
  [slices/ST.md](slices/ST.md)
- [ ] **WK. Weakened: a status lowers the damage its holder deals** (Core + Data + content, about 220 lines) →
  [slices/WK.md](slices/WK.md)
- [ ] **SL. Slowed: a status changes a stat by points, with its own mark letter** (Core + Data + Game + content, about
  260 lines) → [slices/SL.md](slices/SL.md)

## Decisions

Numbers 781-819. "(default ...)" marks a game-design choice taken while the developer was away; his to overturn.

Plan:
- **781.** One plan, three phases: damage tiers (T1-T4, EP), statuses as records (SR1, R1, R2, MS, CH, SW), the new
  status behaviors (TT, ST, WK, SL); ADR 0056 before T1, ADR 0055 before SR1.

Damage tiers (ADR 0056):
- **782.** Damage tiers are an effects catalog, `content/effects.json` (the developer's decisions 703 and 704; the
  review overturned the draft's `rules.json` table). A record is `{ id, kind, perPower, damageType }`: kind `damage`
  (with a type) or `restoreHp` (without); ids are `<type>_<n>` and `heal_<n>`, the suffix naming the sheet's tier (1
  Low 2, 2 Medium 4, 3 High 9, 4 Massive 22, 5 Extreme 58), the number the record's own. Records exist only as content
  needs them (11 at T4, 12 after SL). A skill's effect names one as `"tier": "fire_1"` and still writes its own `kind`
  and `damageType`, which Data refuses when they differ from the record's; status grants stay on the skill's effect.
- **783.** A tiered amount is the user's Power times the record's `perPower` times (1 + the sum of the damage
  modifiers), one `long` product rounded down once and held at `DamageFormula.MaxPower`
  (`DamageFormula.TieredAmount`); then crit, resistance after penetration, Exposed's extra share and block, exactly as
  today (`DamageFormula.Calculate` unchanged); a dominant force raises the result after (`SkillEmpowerment` unchanged,
  decision 817).
- **784.** Power's damage percent (`CombatStats.DamagePermille`) doesn't raise a tiered effect: the record already
  multiplies Power, and the percent would count it twice. It still raises the basic attack (weapon damage times
  Power's percent, decision 696) and any flat effect. Crit damage keeps Power's share (decision 300; 817).
- **785.** (default taken while the developer was away; his to overturn) Items keep a flat power and name no tiered
  effect: an item isn't raised by its user (decision 321's spirit), so a potion heals its number. A revive skill keeps
  a flat power too (none exists).
- **786.** The modifiers' sum starts with one source, Weakened's damage dealt change (WK). Gear has no "+X % damage"
  stat today, so the sheet's "gear 33 %" waits for the items plan (row 9), which adds to the same sum; and no weapon
  gives Power yet. The sum applies to damage only (tiered skills and the basic attack), never to heals or items.
- **787.** Enemies get `power` (a whole number from 0 to 10 000, 0 and left out of the file by default), what their
  tiered skills multiply; an enemy whose tactics or phases name a tiered skill needs at least 1 (Data's message names
  the enemy and the skill).
- **788.** `CombatStats.Power` is a member's sheet Power, gear included, or an enemy's content power.
- **789.** A content id resolves to its record at load (`Data/Content/CatalogReferenceConverter<T>`): a skill's tiered
  effect from T2, a grant's status from SW. The battle reads records and never a catalog; the converter writes the id
  back for row 7's writer, and row 7 rebuilds the skills naming a record it edits (design.md, section 6). ADR 0002's
  "straight into Core records" holds, with this one mechanism for references.
- **790.** (default taken while the developer was away; his to overturn) Each skill's flat power becomes the record
  whose amount at its class's level 1 base Power (an enemy's: its power) is nearest today's, on a ratio scale
  (design.md, section 3.1), with the numbers that follow (see "Defaults to confirm", 1).
- **791.** (default taken while the developer was away; his to overturn) The Goblin Chief gets power 5, so Cleave at
  `slashing_1` keeps today's 10.
- **792.** A tiered effect keeps `"power": 0` in the file: `Effect`'s power stays a required positional, as
  `grantStatus` effects already write it; Data refuses any other power beside a tiered effect. Row 7's writer may make
  it optional when it takes `skills.json` over.
- **793.** Skill descriptions drop their HP numbers (Heal, Recovery, Heal All): the amount now depends on the user. The
  Status page's Power caption says "% attack damage" (`status_power_gives`), since Power's percent now raises only the
  basic attack.
- **794.** The Enemies window's Stats tab gets a Power box (EP), so an enemy's skill damage is set without JSON.

Statuses (ADR 0055, amending ADR 0021):
- **795.** A status is a record in `statuses.json`: `id`, `name`, `description` (may be empty), `isHarmful`, an
  optional `family` (its id when absent), `parts`; `actsAt` from ST, an optional `markLetter` from SL. No icon yet:
  nothing draws one, and an optional field costs nothing to add with the slice that draws it (decision 676's icon,
  deferred). A record compares its parts in order (as decision 604's tables).
- **796.** The closed part list, one `amount` each whose meaning the kind gives: `damageEachTurn` (HP; optional
  `damageType` from TT), `healEachTurn` (HP), `skipTurn` (none), `drawAttacks` (none), `resistanceChange` (points on
  every resistance), `damageTakenChange` (percent of each hit), `damageDealtChange` (percent, WK), `statChange`
  (points on one `stat`, SL).
- **797.** A status holds at most one per-turn HP part (a tick reports one amount and one tone), and each kind at most
  once, except `statChange` once per stat; each kind's amount range is in design.md, section 2.3. The records of one
  family have the same parts in the same order (kinds, types, stats).
- **798.** A grant is `{ "status": id, "turns", "chance" }`: turns at least 1, chance a percent from 1 to 100, 100 when
  left out. A grant carries no strength (decision 702: one strength per status); one effect can't grant the same status
  twice.
- **799.** Intelligence's status percent raises every part's amount and the chance, and Intelligence's extra turns
  lengthen every status but one with `skipTurn` (decision 317); a dominant force raises the parts of a status-only
  skill's statuses, as it raises their power today. The landed status is the record with its amounts raised
  (`StatusDefinition.Strengthened`); the set and the events carry it.
- **800.** A holder keeps one status per family: `poison_1` and `poison_2` don't run together, Exposed 50 and 20 don't
  add to 70, two Guards keep the stronger, and the game shows one mark per family. A grant of a held family refreshes
  it: the granted record with the new grant's turns, each part's amount the one farther from zero (ADR 0021's "the
  stronger"). Burning and Poison are different families, so both tick on one target (today one refreshed the other).
- **801.** Every grant rolls its chance, drawing only when uncertain (ADR 0026); Intelligence raises every chance, not
  only Stun's (see "Defaults to confirm"); crowd control resistance cuts the chance only of a status that skips a turn
  (decision 324's Stun rule); `drawAttacks` keeps its per-attack shrug-off in `EnemyTargeting`, since a self-granted
  taunt's own holder would otherwise resist it.
- **802.** A grant that fails its roll reports `StatusResisted`, whatever the status.
- **803.** A `damageEachTurn` part without a type ignores resistances (today's Poison); with a type (TT) it meets the
  holder's resistance to it, Guard's raise included, through `DamageFormula.Calculate` (at least 1), and never Exposed,
  crit, block, penetration or a hit trigger: it isn't a hit.
- **804.** `actsAt` (ST) is `startOfTurn` or `endOfTurn` (the default) and moves only the HP parts; the countdown stays
  at the end of the holder's turn (ADR 0021). A start tick runs when the holder's turn starts, before a stun skip, never
  on a readied extra turn; a holder it fells loses the turn, and the outcome is checked at once.
- **805.** (default taken while the developer was away; his to overturn) Burning (`burning_1`, Fire Blast's) is fire and
  acts at the start of its holder's turn; Poison and Regen act at the end, as today.
- **806.** (default taken while the developer was away; his to overturn) Weakened (`weakened_1`) adds -25 % to the
  modifiers' sum of every damage its holder deals, tiered skills and the basic attack, never heals; a
  `damageDealtChange` is above -100 and not 0.
- **807.** (default taken while the developer was away; his to overturn) Slowed (`slowed_1`) doesn't slow: turn order
  ignores speed (`TurnOrder` is a shuffled queue). Until it can, it lowers accuracy by 15 and crit chance by 5 points
  (every enemy has dodge 0, so a dodge change would do nothing), and its mark shows "L" (`markLetter`), not Stun's "S"
  on the same red rim. `statChange` accepts accuracy, dodge, crit chance and crowd control resistance, in percentage
  points, each result kept at 0 or above and dodge within its cap.
- **808.** (default taken while the developer was away; his to overturn) Two placeholder skills show the new parts:
  Cone of Cold for the mage (`ice_1`, Slowed for 2 turns, 4 mana) and Enfeeble for the warlock (Weakened for 3 turns,
  +2 instability).
- **809.** An effect's `statuses` list replaces its `status` (decision 676): each lands in list order on every target
  still standing after the effect landed; an `Effect` compares its statuses in order.
- **810.** The game reads the record: the status's name (the six `status_*` ids leave `ui.json`), the mark's letter
  (`markLetter`, else the name's first letter), marks by family, the rim from `isHarmful`, a tick's tone and sound
  from its part (damage: Poison's tone and tick sound; heal: Regen's).
- **811.** The test re-pin: `TestStatuses` fixtures in `Jrpg.Core.Tests/Builders/` and `Jrpg.Game.Tests/Battle/`,
  filled in tests-only slices (R1, R2) before the swap; CH and the swap change their bodies.
- **812.** No save format change: statuses end with the battle (ADR 0021), skills come from classes (ADR 0020), and
  neither effects, rules, enemies nor statuses are saved; `formatVersion` stays.

Saves, simulations and other rows:
- **813.** The simulations run after T4 and SW; their bounds hold by design (the forest's parties attack only and its
  enemies have no skills; the Chief's damage barely moves), and a failing bound is re-pinned only with its reason in
  the slice's Done note.
- **814.** Row 2 (a skill's own effect strip): no change; the strip stays on the skill and the item (SE1's
  `EffectFile`), not on the tiered record.
- **815.** Row 7 (the editors) gets an Effects window (id, kind, damage type, `perPower`); its Skills form picks
  `fire_1` and fills the effect's kind and damage type; it takes `skills.json` over with the writer, registers both
  converters for writing, and rebuilds the skills naming a tiered effect or status it edits (design.md, section 6).
- **816.** (default taken while the developer was away; his to overturn) Decision 705's "poison will scale from stats"
  is read the safe way: a status's numbers stay flat on its record, scaled by its granter's Intelligence as today. A
  tiered skill grows about 4.4x from level 1 to 20 while a tick grows about 1.3x. The extension, about one slice
  after SW: a ticking part may give a `perPower` against the granter's stat, captured when granted (design.md, 2.7).
- **817.** (default taken while the developer was away; his to overturn) A dominant force multiplies after the
  modifiers' sum, not inside it: Weakened with Growth dominant gives 0.75 x 1.5 = 1.125 where the sheet's one sum would
  give 1.25; and crit damage keeps Power's share.
- **818.** Bounds against overflow: a tiered amount is held at `DamageFormula.MaxPower` (1 000 000, a technical limit
  so `Calculate`'s product stays within a `long`); Data bounds a record's `perPower` at 1000 and an enemy's power at
  10 000.
- **819.** The chance comes before the swap (CH): `StatusGrant` gains `Chance`, the stun grants move their power there,
  and every grant rolls, so the swap only changes what a grant names.

## Defaults to confirm
Taken while the developer was away, each his to overturn; the loud ones first:
1. **790. The conversion's real effect.** Flurry becomes 4 x 28 = 112 at level 1, the strongest single-target skill
   (ahead of Rampage 64 and Meteor 48), usable every turn; Palm Strike deals 56 with its 30 % stun at no cost; Heal (15)
   and Recovery (30) both become 24; Eldritch Bolt and Void Rift both become `dark_1`. Not taken: a sixth tier of
   `perPower` 1 (`blunt_0`: Flurry 56, Palm Strike 28).
2. **816. Statuses scale from Intelligence, flat on the record.** Damage grows about 4.4x from level 1 to 20, a tick
   about 1.3x; the stat-scaled tick is a named extension.
3. **807. Slowed doesn't slow:** turn order ignores speed. It lowers accuracy (-15) and crit chance (-5) instead, marked
   "L".
4. **801. Intelligence now scales every grant's chance,** not only Stun's (only the stuns are below 100 today).
5. **817. Dominance multiplies after the modifiers' sum** (x 1.125, not the sheet's x 1.25, for Weakened with Growth),
   and crit damage keeps Power's share.
6. **785.** Items and revives keep a flat power.
7. **805.** Burning is fire and acts at the start of the turn.
8. **806.** Weakened weakens skills and attacks, not heals, by 25 %.
9. **791, 808.** The Goblin Chief's power 5; the placeholder skills Cone of Cold and Enfeeble.

## Open design questions (developer decides)
- None blocking: the defaults above are the questions. For later: a speed-based turn order (Slowed's real meaning);
  status icons on the marks; a ticking part scaling from a stat (decision 816).

## Notes for other rows
- **Row 2 (skill effect strips):** SE1 committed, SE2 and SE3 before this plan; the strip stays a skill and item field
  (decision 814).
- **Row 7 (editors):** decision 815; the Statuses window offers the part kinds with each kind's amount label and the
  family (design.md, sections 2.3 and 6).
- **Row 8 (enemy behavior):** the Power box exists (EP); an enemy using a tiered skill needs Power 1 to 10 000.
- **Row 9 (items):** gear has no "+% damage" stat today and no weapon gives Power yet; a gear damage modifier joins the
  same sum (decision 786); an item names no tiered effect (decision 785) unless row 9 decides otherwise.

## ADRs needed
- **0056, damage tiers as an effects catalog** (before T1): decisions 782-794, 817, 818; options and costs in
  design.md, section 4.
- **0055, statuses as content records** (before SR1), amending ADR 0021: decisions 795-812, 816, 819.
