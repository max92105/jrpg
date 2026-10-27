# Stats and gear slots
Status: approved, in progress

Fifth feature plan after the migration, following the escape menu third pass.
- Renumbered when the two machines' histories were merged (2026-10-27): this plan was drafted on 267-308, which the escape menu third pass had taken, so every number here moved up 24. Its Game slices were drafted against the other machine's menu code (the UI second pass built twice); check each against the third pass's pages before slicing it.
- Decisions 291-302, 316-332 and 337 are recorded below; questions 303-315 are all answered (decisions 320-332). The next free number is in `docs/plans/README.md` (333-336 went to the escape menu third pass's F1).
- This plan writes ADRs 0026 (stat model), 0027 (damage types and resistances) and 0028 (equipment slots); the next ADR is 0029.

Developer's request (2026-10-26, translated): "It's time to start thinking about and implementing stats. Challenge my ideas and complement them. Gear should be more like an ARPG than a classic JRPG: I want to mash the genres, so rarity, random rolls, drop tables, more gear slots (ring, amulet…), salvage, materials and a bit of crafting. But first define enough interesting stats and growth. I haven't thought about scaling." With it, the "Character Stats V2" sheet (damage types, five primaries, derived stats). The session's challenge of the sheet gave decisions 291-302; this plan is the stat half, the loot half is the next plan.

The developer approved the draft on 2026-10-26 with edits: answers to every question (decisions 316-332) and the devil's advocate's review, which moved `Vitals.ResizeMax` into P2 and Intelligence's sheet parts into P3b, added `StatSheet.ValueOf` (P4), made `CombatStats` a sealed record (D1), split G1a in two and W1 in two, added D3 (weakness cues) and I2 (Taunt against crowd control resistance, a status skill per class), set one rounding rule and tenths of a percent (ADR 0026, 0027), and moved S2 right after S1.

## Goal
At the end of this plan:
1. **Every party member has a real stat sheet**: five primaries (Power, Agility, Intelligence, Constitution, Will) from class growth plus worn gear, and the numbers they give: max HP, speed, accuracy, critical chance and damage, dodge, crowd control resistance and eight resistances. Every conversion is a percent per point, its rate in `rules.json`; derived percents are kept in tenths.
2. **Battles use it**: every hit has one of eight damage types and meets the target's resistance to it, with a "Weak" or "Resisted" cue; hits can miss (accuracy against dodge) and crit; Power scales every raw value of a skill or attack, Intelligence what a skill leaves behind (status strength, extra status turns except on Stun, Stun's chance), Will the class resource. Defense and attack leave the game.
3. **Enemies carry derived stats only**: HP, damage with a type, accuracy, crit chance, dodge, crowd control resistance and eight resistances; a weakness is a resistance below 0.
4. **Every class has a skill on each dimension**: the mage, cleric, monk and warlock gain a status on one skill (I2); the rogue's Power acts through its attack (decision 320).
5. **Members wear nine slots**: Weapon, Off-hand, Head, Chest, Hands, Feet, Amulet and two Rings, with fixed-stat items for each in content; saves keep all nine (format 4).
6. **The screens show it**: the Status page shows about twenty numbers, primaries always beside what they give, proven to fit by arithmetic before drawing; the Equipment page lists nine slots and compares the highlighted item's stats; the leader choice shows the class's shape.

## Order, and why
1. **Phase 0: clean-up (C1, C2).** `BattleState` (296 lines) and three test files over or near 300 are split before the slices that grow them.
2. **Phase 1: the stat sheet (ADR 0026, P1, P2, P3a, ADR 0027, P3b, P4).** Core first: primaries on the class, the rules in content, the derived sheet, then gear bonuses feeding it. Nothing in battle changes yet, except max HP from Constitution.
3. **Phase 2: the pages read the sheet (L1, S1, S2, E1).** Before attack and defense leave, so no screen reads them when they go. The Status page's look follows its content at once (decision 316); the Equipment page's look waits for the nine slots.
4. **Phase 3: damage types and resistances (D1, D2, D3).** Every hit typed, then resistances reduce it while defense still subtracts, then the "Weak" and "Resisted" cues.
5. **Phase 4: Power, and the end of attack and defense (M1a, M1b, M2a, M2b-1, M2b-2).** Power scales raw values and weapons deal damage; the test fixtures fold defense into attack (mechanical), then defense leaves and Guarded raises resistances.
6. **Phase 5: hits (H1, H2).** Misses, then crits, each with its battle-screen cue.
7. **Phase 6: Intelligence and Will (I1, I2, W1a, W1b).** Intelligence and Stun's chance, then Taunt against crowd control resistance and a status skill for each class lacking one; Will on battle resources, then on Faith and Instability.
8. **Phase 7: gear slots (ADR 0028, G1a-1, G1a-2, G1b, G1c, G2).** Item kinds (mechanical), nine slots in Core, saves format 4, the Equipment page's nine rows, then the new items.
9. **Phase 8: look (E2).** After the nine slots, so the Equipment page's look is done once.
10. **Phase 9: balance and walk-through (B1, Z).**

## Guiding rules
- **Mechanisms in code, numbers in content** (ADR 0019): conversion rates and bases are `rules.json` `stats`; class primaries and growth are `classes.json`; enemy stats `enemies.json`. The battle's fixed limits (hit floor and ceiling, dodge cap, resistance cap and floor, the "Resisted" threshold, minimum damage, base crit damage 150 % by decision 300) are named constants beside their formulas, as `DamageFormula.MinimumDamage` is today (ADR 0026).
- **One formula place per domain** (CLAUDE.md): `Stats/StatFormula` (primaries to derived), `Effects/DamageFormula` (a hit's damage), `Effects/HitFormula` (hit chance), `Statuses/GrantScaling` (a grant under Intelligence), `Resources/WillFormula`. Each states its source or reasoning and is unit-tested.
- **One rounding rule** (ADR 0027): a hit's damage is computed in a `long` and rounded down once, at the end; derived percents are held in tenths of a percent (permille) and shown as whole percents (ADR 0026).
- **Every number is a placeholder** (design.md, "Tunables"). Balance is not this plan's goal; the goblin trio simulation (a lone level 1 leader wins every fight, ADR 0017) must stay green after every slice that changes numbers, and the slice records its table.
- **Rolls draw only when the outcome is uncertain** (ADR 0026): a chance of 0 or 100 % never calls `IRandom`. Core test fixtures fight under `TestStatRules.Neutral` (every rate 0, base accuracy 100, base crit 0) with certain hits and no crits, so seeded tests keep their sequences; the Game's `BattleScenarios` use a copy of the same neutral rules; tests of a roll set its chance.
- **Status page shows results** (decision 296): a primary is never shown alone, always beside what it gives.
- **Pure presenter + thin view** (`docs/map.md`). Every Game slice writes its raw estimate and the 1.6x factor (class mechanics plan). One selection look (decision 252).
- **Slice sizes:** under about 250 lines, tests at about 13 lines each plus the existing tests a change breaks; each slice file counts its call sites. New tests go in new files when the old one is near 300 (each slice names the file).
- **Files near 300 lines split right before the slice that grows them** (verify fails a changed file crossing 300). Watched: `Battle/BattleState.cs` 296 (C1), `Party/PartyMember.cs` 241 (P2-P4, W1b, G1a-2), `Menus/StatusSheetView.cs` 255 (S1), `Menus/EquipmentPage.cs` 255 and `EquipmentSheetView.cs` 197 (E1, G1c), `Battle/ActionPlayback.cs` 234 (D3, H1, H2, I1), `Data/Saving/SaveGameRepair.cs` 234 (P4, G1b); tests `ClassCatalogValidatorTests` 292, `BattleSkillAndItemTests` 282, `BattleStatusTests` 280, `FieldItemUseTests` 276, `GameSessionSaveTests` 272, `SaveGameRepairTests` 266.
- **Saves:** one format change (G1b, 3 to 4), refused when older (ADR 0022). Derived stats are never saved.
- **No NuGet package. No editor change**: the editor edits maps, tiles, objects and events, never items, classes, skills or enemies; it loads them through Data, so it follows the new shapes without code.

## Out of scope (follow-ups)
- **The next plan, loot:** rarity, random affix rolls, drop tables, salvage, materials, crafting; lifesteal, armor penetration, block and drop luck as affixes (decision 294).
- **Speed and turns** (decision 301): how many times a combatant acts in a round, and an enemy speed field (nothing reads it until then).
- **Free points or an allocation screen** (decision 293: may come later without breaking saves).
- **Enemy skills** (question 224): until then no enemy stuns or taunts a member, so a member's crowd control resistance has nothing to resist; an enemy's does (decision 324).
- **New status kinds** (a Burn or a Chill): I2 uses ADR 0021's list (a burn is a Poison for now).
- **Two-handed weapons, dual wield** (decision 322).
- **Gear art**: the new items reuse today's icons.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 0: clean-up
- [x] **C1. `BattleState`'s target choices move to `Battle/TargetChoices`** (Core, mechanical, about 120 lines moved) → [slices/C1.md](slices/C1.md)
- [x] **C2. Test files split: `BattleStateTests` by command, `PartyMemberTests` by behavior, `ClassCatalogValidatorTests` stats apart** (tests, mechanical, about 200 lines moved) → [slices/C2.md](slices/C2.md)

### Phase 1: the stat sheet
- [x] **ADR 0026: The stat model** (before P1) → [slices/ADR-0026.md](slices/ADR-0026.md)
- [x] **P1. Five primaries and accuracy on the class, grown by level** (Core + Data + content, about 150 lines) → [slices/P1.md](slices/P1.md)
- [x] **P2. Stat rules in content; Constitution raises max HP; members are built with the rules; `Vitals.ResizeMax`** (Core + Data + content, about 250 lines) → [slices/P2.md](slices/P2.md)
- [x] **P3a. The member's sheet: accuracy, crit chance and damage, dodge, crowd control resistance, Power's percent, in tenths** (Core + Data + content, about 200 lines) → [slices/P3a.md](slices/P3a.md)
- [x] **ADR 0027: Damage types and resistances** (before P3b) → [slices/ADR-0027.md](slices/ADR-0027.md)
- [x] **P3b. Eight resistances from Constitution and Will; Intelligence's status percent and extra turns** (Core + Data + content, about 230 lines) → [slices/P3b.md](slices/P3b.md)
- [x] **P4a. Gear bonuses in content; `StatSheet.ValueOf`** (Core + Data + content, about 220 lines; P4 split in two on 2026-10-27) → [slices/P4a.md](slices/P4a.md)
- [x] **P4b. Worn gear adds to the sheet; max HP and speed follow it** (Core + Data, about 210 lines) → [slices/P4b.md](slices/P4b.md)

### Phase 2: the pages read the sheet
- [x] **L1. Stat names and number formats; the leader choice shows HP, speed and the five primaries** (Game + text, about 200 lines) → [slices/L1.md](slices/L1.md)
- [x] **S1a. The Status page's Combat, Resistances and Resources** (Game + text, about 230 lines; S1 revised against F1's page and split on 2026-10-27) → [slices/S1a.md](slices/S1a.md)
- [x] **S1b. Attributes beside the profile; weaknesses and capped resistances tinted** (Game + text, about 170 lines; holds what was S2) → [slices/S1b.md](slices/S1b.md)
- [x] **E1. The Equipment page compares sheets: what the highlighted item changes, or what the worn one gives** (Core + Game, about 240 lines) → [slices/E1.md](slices/E1.md)

### Phase 3: damage types and resistances
- [x] **D1. Every hit has a damage type; `CombatStats` becomes a sealed record** (Core + Data + content, about 245 lines) → [slices/D1.md](slices/D1.md)
- [x] **D2. Resistances reduce damage of their type; enemy weaknesses in content** (Core + Data + content, about 200 lines) → [slices/D2.md](slices/D2.md)
- [x] **D3. "Weak" and "Resisted" cues by the floating number** (Core + Game, about 150 lines) → [slices/D3.md](slices/D3.md)

### Phase 4: Power, and the end of attack and defense
- [x] **M1a. Power scales attacks and skills; weapons deal damage; attack leaves classes and items** (Core + Data + content + tests, about 270 lines; M1 split on 2026-10-27) → [slices/M1a.md](slices/M1a.md)
- [x] **M1b. "Weapon damage" on the Equipment comparison and the Status Combat column** (Game + a small Core query, about 90 lines; decision 337) → [slices/M1b.md](slices/M1b.md)
- [x] **M2a. Test fixtures fold defense into attack** (tests, mechanical, about 140 lines) → [slices/M2a.md](slices/M2a.md)
- [x] **M2b-1. Hits ignore defense; Guarded raises resistances; enemies deal damage, the goblin retuned** (Core + Data + content + tests, about 250 lines; M2b split on 2026-10-27) → [slices/M2b-1.md](slices/M2b-1.md)
- [x] **M2b-2. Defense leaves members, items and the pages** (Core + Data + content + Game + tests, about 230 lines) → [slices/M2b-2.md](slices/M2b-2.md)

### Phase 5: hits
- [x] **H1. Accuracy against dodge: hits can miss, and the battle screen says so** (Core + Data + content + Game, about 240 lines) → [slices/H1.md](slices/H1.md)
- [x] **H2. Critical hits; a critical number stands out; the overflow test** (Core + Data + content + Game, about 215 lines) → [slices/H2.md](slices/H2.md)

### Phase 6: Intelligence and Will
- [x] **I1. Intelligence shapes statuses (never Stun's turns); Stun's chance against crowd control resistance; enemies get it** (Core + Data + content + Game, about 245 lines) → [slices/I1.md](slices/I1.md)
- [x] **I2. Enemies resist Taunt; a status skill for the mage, cleric, monk and warlock** (Core + content, about 170 lines) → [slices/I2.md](slices/I2.md)
- [x] **W1a. Will raises a battle resource's start or max** (Core + Data + content + Game, about 180 lines) → [slices/W1a.md](slices/W1a.md)
- [ ] **W1b. Will raises Faith's max and shrinks Instability's gains** (Core + Data + content, about 170 lines) → [slices/W1b.md](slices/W1b.md)

### Phase 7: gear slots
- [ ] **ADR 0028: Equipment slots** (before G1a-1) → [slices/ADR-0028.md](slices/ADR-0028.md)
- [ ] **G1a-1. Items have a kind; armor becomes Chest; `"slot"` becomes `"kind"`** (Core + Data + content + Game, mechanical, about 150 lines) → [slices/G1a-1.md](slices/G1a-1.md)
- [ ] **G1a-2. Members wear nine slots, a ring kind in two; `TryEquip` names the slot** (Core + Game, about 240 lines) → [slices/G1a-2.md](slices/G1a-2.md)
- [ ] **G1b. Saves keep all nine slots: format 4** (Core + Data, about 200 lines) → [slices/G1b.md](slices/G1b.md)
- [ ] **G1c. The Equipment page lists nine slots; the Armor tab lists every piece that isn't a weapon** (Game, about 150 lines) → [slices/G1c.md](slices/G1c.md)
- [ ] **G2. Fixed-stat gear for the new slots, in the new game's starting items** (content + tests, about 110 lines) → [slices/G2.md](slices/G2.md)

### Phase 8: look
- [ ] **E2. Equipment page look: the Status row look, the comparison in two columns** (Game, about 210 lines) → [slices/E2.md](slices/E2.md)

### Phase 9: balance and walk-through
- [ ] **B1. Goblin trio simulation with hits and crits counted; numbers recorded per class** (Simulations, about 120 lines) → [slices/B1.md](slices/B1.md)
- [ ] **Z. Walk-through: stats, battles, gear and pages by hand** (manual) → [slices/Z.md](slices/Z.md)

## Decisions
Questions and decisions share numbers.

Taken in the planning session (2026-10-26), after a challenge of the developer's "Character Stats V2" sheet:
- **291. No Energy stat.** The per-class battle resources stay (ADR 0019, 0022, 0024). Will scales a class's resource instead: each class declares in content what Will does for it (`willScaling`, W1a and W1b; widened by decision 319).
- **292. All eight resistances on a party member**: Blunt, Piercing, Slashing, Fire, Ice, Lightning, Light, Dark, each a percent that reduces damage of its type. Every hit (attack, skill, item) has exactly one damage type, declared in content: a weapon's type for basic attacks, the skill's for skills.
- **293. Primaries come from class growth plus gear**: class base + growth × (level − 1) as ADR 0017 does, extended to the five primaries, plus what worn gear adds. No free points, no allocation screen (may come later without breaking saves).
- **294. This plan's scope**: the stat sheet, formulas, growth, enemy stats, damage types and resistances, the Status page, the Equipment page with the new slots, and fixed-stat items for them. **Not here** (the next plan): rarity, random affix rolls, drop tables, salvage, materials, crafting; lifesteal, armor penetration, block and drop luck (gear affixes then, not base stats).
- **295. Agility feeds crit chance and dodge only.** Speed stays its own stat from class growth and gear, not fed by Agility. Dodge is hard-capped (placeholder value).
- **296. Scaling dimensions, on every skill and attack** ("Power is magnitude, the others are shape"): Power scales every raw value, hit damage and healing alike, and the critical damage modifier; Intelligence scales what a skill leaves behind: a granted status's duration in turns and its strength (Poison and Regen ticks, Exposed %, Guarded amount, Stun chance); Agility: crit chance, dodge; Constitution: max HP and the three physical resistances; Will: the class resource and the five magical resistances. Durations are whole turns, so Intelligence works in breakpoints; the Status page shows resulting values, never raw primaries alone.
- **297. Accuracy is a derived stat opposed to Dodge**: hit chance = base + accuracy − dodge, clamped to a floor and ceiling (placeholders). Accuracy grows with level and gear.
- **298. Gear slots**: Weapon, Off-hand, Head, Chest, Hands, Feet, Amulet, Ring × 2 (the two rings are one slot kind worn twice). Existing armor becomes Chest; saves bump `formatVersion` and refuse the old format (ADR 0022), no migration code.
- **299. Enemies carry derived stats only**: HP, damage (with a type), accuracy, crit chance, dodge, speed and the eight resistances; no primaries. A weakness is a resistance below 0. (Speed waits for the turn plan: see "Contradictions" in design.md; crowd control resistance is added by decision 324.)
- **300. Base critical damage is 150 %**, raised by Power.
- **301. Deferred to its own later plan**: Speed deciding how many times a combatant acts in a round. This plan only makes Speed a real stat with gear on it.
- **302. Crowd Control Resistance Chance % stays on the sheet**: a chance to resist Stun and Taunt; placeholder base value.

The developer's answers to the draft (2026-10-26):
- **316. The Status page layout** (resolves the devil's advocate's blocker): a smaller portrait (160 px) in the profile band with the attribute column beside it; combat numbers and resistances below; skills keep their rows. S1 writes the arithmetic that proves the page fits before anything is drawn; S2 runs right after S1.
- **317. Stun never gains turns from Intelligence**: Intelligence raises Stun's chance only. Otherwise a stun-lock: `TurnFlow` skips a turn per remaining Stun turn (I1).
- **318. Every class has mid-to-high Power**: Power is the universal damage stat, so no class starts low in it; Agility, Intelligence, Will and Constitution set classes apart (design.md, class primaries table).
- **319. Will may also lower a harmful resource's gain or raise a lasting one's max**: `willScaling` covers Faith (lasting: raise max) and Instability (harmful: shrink its gains); design.md names each class's choice (W1a, W1b).
- **320. Dead stats: this plan adds one status effect per class that lacks one** (answers 310): the mage, cleric, monk and warlock each get a status on an existing skill from ADR 0021's list (I2, the exact skill and status there); the rogue's Power acts through its basic attack only, which is accepted.
- **321. Consumables don't scale** with the user's stats (answers 303).
- **322. The off-hand holds stats-only items** (a shield, a focus); no second weapon, no dual wield (answers 309).
- **323. Resistance feedback** (answers 315): the battle shows a "Weak" cue by the floating number when a resistance below 0 raised the hit, and "Resisted" when a resistance at or above a threshold (placeholder 25 %) lowered it (D3).
- **324. Enemies get crowd control resistance in this plan** (answers 311): Stun rolls against the target's (I1); an enemy rolls its own to ignore a member's Taunt when choosing its target (I2). With the monk's new Stun (I2) and the fighter's Shield Bash, `CombatStats.CrowdControlResistance` has live callers.

Placeholders the developer accepted (2026-10-26; may be overturned later):
- **325. Only damaging hits roll** to hit and to crit, each hit of a multi-hit on its own; heals, revives and status-only effects never miss or crit; Stun rolls its own chance (answers 304).
- **326. A miss is not a hit**: it sets off no "hit taken" or "hit landed" gain, and a fully missed damage effect grants no status (answers 305).
- **327. The worn list leaves the Status page**; the Equipment page shows it (answers 306).
- **328. Gear that changes max HP keeps current HP**, clamped to the new max; a living member keeps at least 1 (answers 307).
- **329. The same ring twice on one member is allowed** (answers 308).
- **330. One unarmed attack rule for everyone**, in `rules.json` (18 Blunt) (answers 312).
- **331. Guarded raises all eight resistances** by its power in points (answers 313).
- **332. The new gear comes from the new game's starting items** until the loot plan (answers 314).

Taken on 2026-10-27, while slicing E1:
- **337. Weapons and armor stay comparable on the Equipment page.** E1's comparison keeps Attack and Defense lines, read from what the battle uses, until they leave the game; M1 turns Attack into **Weapon damage** (the weapon's damage raised by Power, the unarmed damage without one), shown on the Equipment comparison and in the Status page's Combat column; M2b removes the Defense line. Without them a sword could not be told from another (a weapon's damage is not a stat kind).

Earlier decisions that apply: 164 and 168 (classes, level 1 start), 165 (anyone wears anything), 137 (taking a worn item from another member), 196 (every resource number is a placeholder), 252 (one selection look), 260 (the Status page shows level, class, resources, skills), 261 ("HP" everywhere), 266 (member pages hide the sidebar).

## Open design questions (developer decides)
None open: 303-315 are answered (decisions 320-332).

Still open from earlier plans: 92, 112, 147, 151-155, 179, 180, 182, 183, 208-220, 222-228.

## ADRs
Each is a checkbox step right before the first slice that needs it. The developer chose each direction in the session; the ADR records it with its alternatives.
- **0026 The stat model** (before P1).
  - **A. Five primaries from class growth and gear; derived stats by percent-per-point conversions; rates and bases in `rules.json`; battle limits as constants beside their formulas; derived percents in tenths (decided).** Each primary converts at a fixed percent per point, with no level divisor, so a point is worth the same at every level, and the percents land on raw values that grow (class HP, skill and weapon power). Members are built with the rules; the battle reads only combatants' numbers.
  - **B. Derived stats straight on the class, no primaries** (today's model, widened). Fewer numbers; but the dimensions (decision 296) disappear and gear can't shape a build.
  - **C. Ratings divided by level** (MMO-style: crit chance = rating ÷ (k × level)). Gear must be replaced as levels rise; worse for fixed-stat items and harder to read on the Status page.
  - **D. Conversion rates as code constants.** No rules threaded into members; but every tuning is a rebuild, against ADR 0019's "numbers in content".
  - Also records: rolls draw only when uncertain; skills and attacks scale with their user, consumables don't (decision 321); a dimension per primary (decision 296); Stun gains no turns (decision 317); derived percents held in tenths of a percent and shown whole, so a small bonus still moves a stored number.
- **0027 Damage types and resistances** (before P3b).
  - **A. A closed list of eight types in Core; one type per hit from content; one percent resistance per type, capped at 75 % (Path of Exile's default cap), below 0 a weakness; resistances replace defense (decided).**
  - **B. Types as content** (`damageTypes.json`, resistances keyed by id). Open-ended; but Constitution and Will's physical and magical groups are rules, so code would still name them.
  - **C. Defense stays, types add weakness tiers** (weak, normal, resist, immune as multipliers, Pokémon or Octopath's weakness break). Simple to read; but a tier can't be a gear stat that adds 5 %.
  - **D. Defense stays and subtracts, resistances on top.** Two mitigations to balance; a flat defense makes small hits vanish and big ones barely move.
  - **E. Diminishing returns** (`armor / (armor + K)`, as many ARPGs and MOBAs do). No cap needed and stacking always helps a little; but a rating isn't a percent the player can read, and a weakness (below 0) has no natural place.
  - Also records the one rounding rule: a `long` intermediate, rounded down once.
- **0028 Equipment slots** (before G1a-1).
  - **A. Nine slots as a closed Core enum; an item declares a kind; the ring kind fits two slots (recommended).** Saves key worn items by slot.
  - **B. Slots defined in content** (`rules.json` list of slots and kinds). Flexible; but the Equipment page's layout, the save shape and the rules (one weapon gives the attack) name slots anyway.
  - **C. Kind only; a member holds up to two rings as a list.** No ring slots to name; but "which ring do I replace" needs a list index, the page's rows and the save need an order anyway, and taking from another member must say which ring.
- **None needed for:** the page layouts (Game look, named constants), the stat names (text), the save bump (ADR 0022 already decides it), the test splits.
