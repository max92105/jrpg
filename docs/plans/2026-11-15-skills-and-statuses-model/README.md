# Skills and statuses model (editor push, roadmap row 6)
Status: draft

Drafted on 2026-11-15 from roadmap row 6 of [the editor push](../2026-11-15-editor-push/README.md), its design.md
sections 3.8 and 5, and the developer's decisions 675-677, 695-697 and 702-705 there. The developer was away: every
game-design choice below is a default he confirms or overturns at approval ("Defaults to confirm").

## Goal
1. **Damage tiers (decision 704):** a skill's damage or heal is its user's Power times its tier (the developer's
   "Damage Test" sheet: Low 2, Medium 4, High 9, Massive 22, Extreme 58 per point of Power), then times 1 plus the sum
   of its modifiers. Every skill in `skills.json` names its tier instead of a flat power; enemies get a Power for
   theirs.
2. **Statuses as records (decisions 676, 695, 705):** `statuses.json` holds tiered status records (`poison_1`,
   `regen_2`, ...) built from a closed Core list of parts; an effect grants any number of them, each with turns and a
   landing chance; each status acts at the start or the end of its holder's turn; Burning hurts as fire; Weakened and
   Slowed are new parts. Today's `StatusKind` is gone.
3. **Every later battle row builds on this schema** (status and skill editors, enemy behavior, items): the plan settles
   the file shapes those editors will write.

## Order, and why
1. **Damage tiers first** (ADR 0056, T1-T4, EP): the content reference converter they need (a tier named by id
   resolved at load) is the one the status swap reuses, and Weakened (WK) adds to the tier formula. Core first (T1),
   then its loading (T2), enemies' Power (T3, EP), and the content conversion last (T4), with the simulations rerun.
2. **The status catalog before the swap** (ADR 0055, SC1): `statuses.json` loads and is checked while the battle still
   runs on `StatusKind`, so the swap only rewires.
3. **Tests re-pinned before the swap** (R1, R2, tests only): the ~130 test sites naming `StatusKind` or
   `new StatusGrant(` go through `TestStatuses` fixtures, so the swap changes the fixtures' bodies, not 26 files.
4. **Several statuses per effect (MS) before the swap,** so `skills.json`'s `statuses` list lands once and the swap
   only rewrites each grant.
5. **The swap (SW):** grants name records; Core, Data, Game and content change together, the one slice that can't be
   split further (its file says why).
6. **New behaviors after, one per slice, each with its content:** typed ticks (TT), start of turn (ST), Weakened (WK),
   Slowed (SL).

## Guiding rules
- **Every slice ends green and changes behavior only where its file says so.** The swap (SW) keeps today's numbers:
  every grant becomes a record with the same numbers (design.md, section 3.2); the two behavior changes it brings
  (statuses by id, decision 800; every grant rolls its chance, decision 801) are named.
- **Numbers stay content:** tier multipliers in `rules.json`, status numbers in `statuses.json`, chances and turns on
  the grants. Placeholder numbers are listed in design.md, section 1.
- **The battle never reads a catalog:** a content id resolves to its record at load (decision 789).
- **Tests on repository content never pin today's values** (`.claude/rules/tests.md`): a repository test that pinned
  a skill's power or a grant's numbers is rewritten to assert ids and shape, never the new value.
- **Sizes:** Core and Data slices about 300 lines with tests at about 13 lines each; the Editor slice about 200. Raw
  JSON in `statuses.json` is counted apart.
- **Files near 300 (checked 2026-11-15):** `Core/Battle/BattleState.cs` 283 (no slice adds to it);
  `Game/Battle/ActionPlayback.cs` 274 (SW changes two lines in place); test files past 240:
  `ContentCatalogLoaderTests` 272, `ItemCatalogValidatorTests` 248, `BattleStatusTests` 246, `CombatantTests` 244,
  `ActionPlaybackTests` 243, `BattleTurnSequenceTests` 240, `GameRulesValidatorTests` 239: new tests go to the
  siblings each slice names.
- **Every switch over a part kind** (Core's `StatusModifiers` and `TurnUpkeep`, Data's `StatusCatalogValidator`, the
  Game's tick tone and sound) gets its arm in the slice that adds the kind; each slice names the helper that keeps its
  method under CA1502.

## Out of scope
- An `effects.json` catalog of named damage records (decision 782 explains; design.md, section 4 prices it).
- Status icons: nothing draws one yet; the field comes with the slice that draws it (decision 795).
- A speed-based turn order, so Slowed can slow turns (decision 807; backlog).
- The status and skill editors (roadmap row 7), enemy tactics forms (row 8), gear's "+X % damage" modifier (row 9).
- The basic attack's formula (weapon damage times Power's percent): unchanged (decision 784).
- A skill's own effect strip (row 2, its own plan; decision 814).

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box when
a slice is done and write its deviations in the slice file. Tunables, design and risks: [design.md](design.md).

### Phase 1: damage tiers
- [ ] ADR 0056, damage tiers. Write it from decisions 782-793 (the options: tiers in `rules.json` named by the skill,
  an `effects.json` catalog of named records, today's flat power; the defaults marked); `/adr`'s question step is
  skipped, the developer confirms at approval.
- [ ] **T1. A tiered skill deals its user's Power times its tier** (Core, about 140 lines) → [slices/T1.md](slices/T1.md)
- [ ] **T2. `rules.json` names the damage tiers and an effect names one by id** (Data + content, about 240 lines) →
  [slices/T2.md](slices/T2.md)
- [ ] **T3. Enemies have a Power for their tiered skills** (Core + Data, about 160 lines) →
  [slices/T3.md](slices/T3.md)
- [ ] **EP. The Enemies window edits an enemy's Power** (Editor, about 100 lines) → [slices/EP.md](slices/EP.md)
- [ ] **T4. Every skill names its damage tier** (content + Data, about 170 lines plus `skills.json`) →
  [slices/T4.md](slices/T4.md)

### Phase 2: statuses as records
- [ ] ADR 0055, statuses as content records (amends ADR 0021). Write it from decisions 795-811 (the options: records
  over a closed part list, today's closed kinds with per-skill numbers, instant effects re-run each turn (refused,
  decision 677)); `/adr`'s question step is skipped, the developer confirms at approval.
- [ ] **SC1. `statuses.json`: the status records, loaded and checked** (Core + Data + content, about 270 lines plus 160
  of JSON) → [slices/SC1.md](slices/SC1.md)
- [ ] **R1. Core tests build statuses through `TestStatuses`** (tests only, about 250 lines, mechanical) →
  [slices/R1.md](slices/R1.md)
- [ ] **R2. Game tests build status events through `TestStatuses`** (tests only, about 90 lines) →
  [slices/R2.md](slices/R2.md)
- [ ] **MS. An effect grants several statuses** (Core + Data + content, about 200 lines) → [slices/MS.md](slices/MS.md)
- [ ] **SW. A grant names a status record; `StatusKind` goes** (Core + Data + Game + content, about 340 lines) →
  [slices/SW.md](slices/SW.md)

### Phase 3: the new status behaviors
- [ ] **TT. A tick may have a damage type: Burning meets fire resistance** (Core + Data + content, about 150 lines) →
  [slices/TT.md](slices/TT.md)
- [ ] **ST. A status acts at the start or the end of its holder's turn** (Core + Data + content, about 240 lines) →
  [slices/ST.md](slices/ST.md)
- [ ] **WK. Weakened: a status lowers the damage its holder deals** (Core + Data + content, about 230 lines) →
  [slices/WK.md](slices/WK.md)
- [ ] **SL. Slowed: a status changes a stat by points** (Core + Data + content, about 230 lines) →
  [slices/SL.md](slices/SL.md)

## Decisions

Numbers 781-815. "(default ...)" marks a game-design choice taken while the developer was away; his to overturn.

Plan:
- **781.** One plan, three phases: damage tiers (T1-T4, EP), statuses as records (SC1, R1, R2, MS, SW), the new status
  behaviors (TT, ST, WK, SL); ADR 0056 before T1, ADR 0055 before SC1.

Damage tiers (ADR 0056):
- **782.** (default taken while the developer was away; his to overturn) The tiers live in `rules.json` `damageTiers`,
  a list of `{ id, perPower }` (his sheet's five: low 2, medium 4, high 9, massive 22, extreme 58); a skill's damage
  or heal effect names its tier by id (`"tier": "low"`) beside its own damage type. His "fire_1 .. fire_5" are "fire
  at low .. extreme"; there is no `effects.json` catalog. Every Low fire, ice or heal reads the same multiplier, so
  decision 703's "everything holding that effect reads the same numbers" holds through the tier. Overturning it costs
  about two slices and one more window in row 7 (design.md, section 4).
- **783.** A tiered amount is the user's Power times the tier's `perPower` times (1 + the sum of the damage modifiers),
  one `long` product rounded down once (`DamageFormula.TieredAmount`); then crit, resistance after penetration,
  Exposed's extra share and block, exactly as today (`DamageFormula.Calculate` unchanged); a dominant force raises the
  result after, as today (`SkillEmpowerment` unchanged).
- **784.** Power's damage percent (`CombatStats.DamagePermille`) doesn't raise a tiered effect: the tier already
  multiplies Power, and the percent would count it twice. It still raises the basic attack (weapon damage times Power's
  percent, decision 696's "weapon gives power" through the wielder's Power) and any flat effect. Crit damage keeps
  Power's share (decision 300).
- **785.** (default taken while the developer was away; his to overturn) Items keep a flat power and name no tier: an
  item isn't raised by its user (decision 321's spirit), so a potion heals its number. A revive skill keeps a flat
  power too (none exists).
- **786.** The modifiers' sum starts with one source, Weakened's damage dealt change (WK); the sheet's "gear 33 %"
  waits for the items plan (row 9), which adds to the same sum. The sum applies to damage only (tiered skills and the
  basic attack), never to heals or items.
- **787.** Enemies get `power` (a whole number, 0 and left out of the file by default), what their tiered skills
  multiply; an enemy whose tactics or phases name a tiered skill needs at least 1 (Data's message names the enemy and
  the skill).
- **788.** `CombatStats.Power` is a member's sheet Power, gear included, or an enemy's content power.
- **789.** A content id resolves to its record at load (`Data/Content/CatalogReferenceConverter<T>`): a skill's tier
  from T2, a grant's status from SW. The battle reads records and never a catalog; the converter writes the id back,
  for row 7's writer. ADR 0002's "straight into Core records" holds, with this one mechanism for references.
- **790.** (default taken while the developer was away; his to overturn) Each skill's flat power becomes the tier whose
  amount at its class's level 1 base Power (an enemy's: its power) is nearest today's, on a ratio scale (design.md,
  section 3.1). Palm Strike and Flurry roughly triple: the sheet has no tier below Low.
- **791.** (default taken while the developer was away; his to overturn) The Goblin Chief gets power 5, so Cleave at
  Low keeps today's 10.
- **792.** A tiered effect keeps `"power": 0` in the file: `Effect`'s power stays a required positional, as
  `grantStatus` effects already write it; Data refuses any other power beside a tier. Row 7's writer may make it
  optional when it takes `skills.json` over.
- **793.** Skill descriptions drop their HP numbers (Heal, Recovery, Heal All): the amount now depends on the user.
- **794.** The Enemies window's Stats tab gets a Power box (EP), so an enemy's skill damage is set without JSON.

Statuses (ADR 0055, amending ADR 0021):
- **795.** A status is a record in `statuses.json`: `id`, `name`, `description` (may be empty), `isHarmful`, `parts`;
  `actsAt` from ST. No icon yet: nothing draws one, and an optional field costs nothing to add with the slice that
  draws it (decision 676's icon, deferred). A record compares its parts in order (as decision 604's tables).
- **796.** The closed part list, one `amount` each whose meaning the kind gives: `damageEachTurn` (HP; optional
  `damageType` from TT), `healEachTurn` (HP), `skipTurn` (none), `drawAttacks` (none), `resistanceChange` (points on
  every resistance), `damageTakenChange` (percent of each hit), `damageDealtChange` (percent, WK), `statChange`
  (points on one `stat`, SL).
- **797.** A status holds at most one per-turn HP part (a tick reports one amount and one tone), and each kind at most
  once, except `statChange` once per stat; each kind's amount range is in design.md, section 2.3.
- **798.** A grant is `{ "status": id, "turns", "chance" }`: turns at least 1, chance a percent from 1 to 100, 100 when
  left out. A grant carries no strength (decision 702: one strength per status); one effect can't grant the same status
  twice.
- **799.** Intelligence's status percent raises every part's amount and the chance, and Intelligence's extra turns
  lengthen every status but one with `skipTurn` (decision 317); a dominant force raises the parts of a status-only
  skill's statuses, as it raises their power today. The landed status is the record with its amounts raised
  (`StatusDefinition.Strengthened`); the set and the events carry it.
- **800.** (default taken while the developer was away; his to overturn) A holder's statuses are kept by id: different
  ids run together (`poison_1` beside `poison_2`, Burning beside Poison, `guard_1` and `guard_2` adding up), and the
  same id refreshes: the new grant's turns, each part's amount the one farther from zero (ADR 0021's "the stronger").
  Today the same kind refreshed, so two Guards no longer cap at the stronger. Overturning it is a `family` field, one
  slice.
- **801.** Every grant rolls its chance, drawing only when uncertain (ADR 0026); crowd control resistance cuts the
  chance only of a status with `skipTurn` (decision 324's Stun rule); `drawAttacks` keeps its per-attack shrug-off in
  `EnemyTargeting`, since a self-granted taunt's own holder would otherwise resist it.
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
- **807.** (default taken while the developer was away; his to overturn) Slowed (`slowed_1`) lowers accuracy by 15 and
  dodge by 10 points, since speed orders no turn yet (`TurnOrder` is a shuffled queue); `statChange` accepts accuracy,
  dodge, crit chance and crowd control resistance, in percentage points, each result kept at 0 or above and dodge
  within its cap.
- **808.** (default taken while the developer was away; his to overturn) Two placeholder skills show the new parts:
  Cone of Cold for the mage (ice at Low, Slowed for 2 turns, 4 mana) and Enfeeble for the warlock (Weakened for 3
  turns, +2 instability).
- **809.** An effect's `statuses` list replaces its `status` (decision 676): each lands in list order on every target
  still standing after the effect landed; an `Effect` compares its statuses in order.
- **810.** The game reads the record: the status's name (the six `status_*` ids leave `ui.json`), the mark's letter
  from the name, marks by status id, the rim from `isHarmful`, a tick's tone and sound from its part (damage: Poison's
  tone and tick sound; heal: Regen's).
- **811.** The test re-pin: `TestStatuses` fixtures in `Jrpg.Core.Tests/Builders/` and `Jrpg.Game.Tests/Battle/`,
  filled in tests-only slices (R1, R2) before the swap; the swap changes their bodies.

Saves, simulations and other rows:
- **812.** No save format change: statuses end with the battle (ADR 0021), skills come from classes (ADR 0020), and
  neither rules, enemies nor statuses are saved; `formatVersion` stays.
- **813.** The simulations run after T4 and SW; their bounds hold by design (the forest's parties attack only and its
  enemies have no skills; the Chief's damage barely moves), and a failing bound is re-pinned only with its reason in
  the slice's Done note.
- **814.** Row 2 (a skill's own effect strip): no change; the strip stays on the skill, since there is no effect record
  to carry it.
- **815.** Row 7 (the editors) takes over `skills.json` and `statuses.json` with the writer: it registers both
  converters for writing, relays both files, and rebuilds the skills' grants when a status record is edited (the draft
  holds resolved records; design.md, section 6).

## Defaults to confirm
Taken while the developer was away, each his to overturn:
1. **782.** Tiers in `rules.json`, named by the skill; no `effects.json` catalog of fire_1 .. fire_5 records.
2. **785.** Items and revives keep a flat power.
3. **790, 791.** The conversion by nearest tier (Palm Strike and Flurry triple); the Goblin Chief's power 5.
4. **800.** Statuses kept by id: tiers of one status and two Guards run together.
5. **805.** Burning is fire and acts at the start of the turn.
6. **806.** Weakened weakens skills and attacks, not heals, by 25 %.
7. **807.** Slowed lowers accuracy and dodge (speed orders no turn yet).
8. **808.** The placeholder skills Cone of Cold and Enfeeble.

## Open design questions (developer decides)
- None blocking: the defaults above are the questions. For later: a speed-based turn order (Slowed's real meaning);
  status icons on the marks.

## Notes for other rows
- **Row 2 (skill effect strips):** the strip stays a skill field (decision 814).
- **Row 7 (editors):** decision 815; the Skills window offers the damage types and `rules.json`'s tiers; the Statuses
  window offers the part kinds with each kind's amount label (design.md, section 2.3).
- **Row 8 (enemy behavior):** the Power box exists (EP); an enemy using a tiered skill needs Power 1 or more.
- **Row 9 (items):** gear's damage modifier joins the sum (decision 786); an item names no tier (decision 785).

## ADRs needed
- **0056, damage tiers** (before T1): decisions 782-793; options and costs in design.md, section 4.
- **0055, statuses as content records** (before SC1), amending ADR 0021: decisions 795-811.
