# Battle: each class's resource and core mechanic
Status: approved

Second feature plan after the migration, following the new-game flow.
- Decisions 188-207 and 229 are recorded below. Questions 208-228 are open; the next free number is 230.
- This plan writes ADRs 0019-0024; the next ADR is 0025.
- It answers question 181 (where skills come from) through ADR 0020.

Developer's request (2026-10-24, translated typos only): "The next step I want to do is implement each mechanic for each class. Their resources will all be different and define their way to play their core mechanics. I will need maybe a couple of skill placeholders for each. Don't bother with balancing or making the perfect skill, it is a first draft. The important thing is to introduce the core mechanics / resources and be able to test it."

The pitch, from the developer's spreadsheet, is decisions 189-195. The developer approved the draft on 2026-10-24, answering its questions (decisions 197-207). The devil's advocate's review split the large slices, moved the practice enemy first and folded implementation choices into the ADRs.

## Goal
At the end of this plan, every class fights its own way, with two or three placeholder skills, and the developer can play-test each one:
1. **MP is gone.** Every class has its own resource instead, and no screen, item or save holds MP.
2. **Mage, Mana.**
   - Starts each battle with a little mana and gains some every turn; saving up buys the bigger spell.
   - **Clarity**, a lasting resource, drops with every battle fought and lowers the starting mana until the mage rests.
3. **Fighter (renamed from Warrior, id `fighter`), Stamina.** Regains stamina every turn and spends it on strong blows and on a bash that stuns.
4. **Barbarian, Ferocity.** Gains ferocity from hits taken. Taunts to draw the enemies' attacks, then spends ferocity on a crushing blow.
5. **Paladin, Conviction.**
   - Gains conviction when an enemy falls and when hit hard.
   - An aura guards the allies within its range in the party's **formation**. The party can change places in battle, so the paladin moves to cover who needs it.
6. **Rogue, Readiness.**
   - His **speed** sets how many times per battle he can **Ready**: an extra turn he places right after a combatant of his choice.
   - Expose sets up an enemy, so every hit on it lands harder.
7. **Warlock, Instability.** Casting raises instability. More instability unlocks stronger spells, and every point adds a chance that a spell backfires on the caster.
8. **Druid, Growth and Decay.**
   - Healing-over-time feeds Growth; poison feeds Decay.
   - The dominant force empowers its skills, and dominance only changes hands when the other force goes strictly above.
   - Equilibrium needs both.
9. **Cleric, Faith.**
   - A level, not a pool: the cleric can cast any skill whose faith requirement is at or below the current faith.
   - Faith drops when bad things happen and stays lowered until a rest.
10. **Monk, Chakra.** Many small hits rather than one big one: each hit that lands builds chakra, which is spent on multi-hit skills.
11. **Rest.** An inn (a map event, with a placeholder inn on the start map) and a rest item restore what only a rest restores. Saves keep it.
12. **The battle screen** shows each member's resources and everyone's statuses, first plainly, then in the Octopath style.

## Order, and why
1. **Phase 0: preparation (PT, FR1).** The practice enemy comes first, so every later slice can be tried in a long fight. The class rename is next, small and mechanical, before any slice adds fighter content.
2. **Phase 1: skills from the class, resources and mana (ADR 0019, ADR 0020, CS1, M1a, M1b, M2a, M2b).** Skills move to the class. Then one resource goes all the way through: loaded, carried into battle, gained each turn, spent by skills. After M2b, the mage casts with mana; M3 (Phase 2) shows it on screen once MP's row is gone.
3. **Phase 2: the cleric's faith, then MP leaves (CL1, ADR 0022, MP1, MP2, M3, MP3a, MP3b).** This comes early because it changes the battle panel's layout and the save format.
   - The cleric gets Faith first, so her heals stay gated when MP1 removes their MP cost.
   - Then skills, then the screens. M3 puts the resources where the MP row was, so the developer sees mana and faith from there on.
   - Then saves (the first format change), then members and enemies.
4. **Phase 3: statuses, fighter, barbarian, paladin, formation (ADR 0021, ST1, ST2, FT1, BB1, PL1, ADR 0023, FM1, FM2, PL2a, PL2b).** The fighter's stun needs statuses; the taunt and the aura reuse them. The aura's range needs the formation, so moving in battle comes before it.
5. **Phase 4: tempo and risk (RG1, RG2, RG3, RG4, WL1, WL2).** Speed and the rogue's extra turn change the turn order, and the warlock's backfire adds a roll.
6. **Phase 5: forces and hits (DR1, DR2, MO1).** Ticking statuses, the druid, then the monk's multi-hit.
7. **Phase 6: run-through (Z1).** The developer plays every class.
8. **Phase 7: lasting resources and rest (ADR 0024, LR1, LR2, LR3, RS1, RS2, RS3).** What a rest restores comes after the run-through, so play can shape it. It is the second save format change.
9. **Phase 8: the look (LK1, LK2, LK3, Z2).** Behavior first, then the look.

## Guiding rules
- **Systems, not balance.** Every number is a placeholder in content (design.md, "Tunables and placeholders"). Every rule the developer hasn't given is a numbered question, with the placeholder used until it's answered. `GoblinTrioSimulation` keeps passing (decision 187).
- **Mechanisms in code, numbers in content** (ADR 0019).
  - Triggers, status kinds, formation, the extra turn and the special rules (backfire, dominance, lasting strain) are code.
  - Amounts, maxima, costs, powers, ranges and turns are content.
  - Code never names a class or a resource id.
- **Rules in Core** (ADR 0001). The Game shows events and reads live state (ADR 0013).
- **Events with their mechanic** (ADR 0004). A new `BattleEvent` may land in a Core slice before the Game can show it only while no content produces it. The content lands with the playback case, because `ActionPlayback` throws on an unknown event.
- **Formulas in one static place each** (`DamageFormula`, `HeavyHitRule`, `BackfireFormula`, `DominanceFormula`, `ReadinessFormula`), with their reasoning and tests.
- **Determinism.** Rolls go through the battle's `IRandom`; tests use `FixedSequenceRandom`.
- **Saves** change twice, each time with a version bump, a round trip and a test that the older format is refused (ADR 0022, decision 229): MP leaves in MP3a (format 1 → 2), and lasting resources arrive in LR2 (2 → 3). No migration before the first release.
- **Slice sizes count tests at about 13 lines each**, plus the existing tests a change breaks. Mechanical slices (FR1, CS1, MP3b) record the solution's test count before and after.
- **Temporary content edits for a hand check are reverted.** No slice commits a content change made only to see a rule.
- **Look** (`.claude/rules/game.md`). Read `docs/ui_ux_exemples/Combat1.webp`, `Combat3.webp` and `Combat5.webp` before LK1-LK3.
- **No NuGet package.**

## Out of scope (follow-ups)
- **Balance**, real skill lists, skill icons, skills learnt by level.
- **Enemy resources and skills** (question 224); enemies only receive statuses.
- **Gold and inn prices** (question 222); the placeholder inn is free.
- **Speed for anything but the rogue's readiness.** Turn order stays a shuffled cycle; speed is only a class stat here.
- **Elemental weaknesses and a break gauge** (question 215).
- **Editor support for classes, skills and items.** These are still edited as JSON. The editor only gets the rest step (RS2), because the editor can't open a map that uses a step it doesn't know.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 0: preparation
- [x] **PT. A sturdy practice enemy, in an encounter of its own** (content + tests, about 40 lines) → [slices/PT.md](slices/PT.md)
- [x] **FR1. Warrior becomes Fighter: the id `fighter` everywhere** (content + tests + docs, about 110 lines, mostly mechanical) → [slices/FR1.md](slices/FR1.md)

### Phase 1: skills from the class, resources and mana
- [x] **ADR 0019: Class battle resources** (before M1a) → [slices/ADR-0019.md](slices/ADR-0019.md)
- [x] **ADR 0020: Skills come from the class** (before CS1; answers question 181) → [slices/ADR-0020.md](slices/ADR-0020.md)
- [x] **CS1. Skills come from the class; members lose `skillIds`** (Core + Data + content, about 240 lines, about 60 of them test-site edits) → [slices/CS1.md](slices/CS1.md)
- [x] **M1a. Combatants carry their class's resources and gain at the end of each of their turns** (Core, about 230 lines) → [slices/M1a.md](slices/M1a.md)
- [x] **M1b. Resources load from `classes.json`; the mage has mana; the screen accepts the new event** (Data + content + Game, about 160 lines) → [slices/M1b.md](slices/M1b.md)
- [x] **M2a. Skills can spend a class resource** (Core + Data, about 250 lines) → [slices/M2a.md](slices/M2a.md)
- [x] **M2b. The mage's spells cost mana; the skill list shows it** (content + Game, about 220 lines) → [slices/M2b.md](slices/M2b.md)

### Phase 2: the cleric's faith; MP leaves
- [x] **CL1. The cleric: Faith, a level that skills require; it drops when an ally falls** (Core + Data + content + Game text, about 270 lines) → [slices/CL1.md](slices/CL1.md)
- [x] **ADR 0022: MP leaves the game; no save migrations before release** (before MP1) → [slices/ADR-0022.md](slices/ADR-0022.md)
- [x] **MP1. Skills and effects stop using MP** (Core + Data + content + Game, about 200 lines) → [slices/MP1.md](slices/MP1.md)
- [x] **MP2. No screen shows MP** (Game, about 230 lines) → [slices/MP2.md](slices/MP2.md)
- [x] **M3. The battle panel shows each member's resources where MP was, plainly** (Game, about 170 lines) → [slices/M3.md](slices/M3.md)
- [x] **MP3a. Saves stop keeping MP: format 2, older saves refused** (Core + Data, about 150 lines) → [slices/MP3a.md](slices/MP3a.md)
- [x] **MP3b. Members and enemies lose MP** (Core + Data + content, about 260 lines, mostly deletions and test-site edits) → [slices/MP3b.md](slices/MP3b.md)

### Phase 3: statuses, the fighter, the barbarian, the paladin and the formation
- [x] **ADR 0021: Status effects** (before ST1) → [slices/ADR-0021.md](slices/ADR-0021.md)
- [x] **ST1. Statuses in Core: granted by effects, counted down from the holder's next turn, cleared on a fall** (Core + Data, about 260 lines) → [slices/ST1.md](slices/ST1.md)
- [x] **ST2. A stunned combatant loses its turn** (Core, about 150 lines) → [slices/ST2.md](slices/ST2.md)
- [x] **FT1. The fighter: stamina and a stunning bash; the screen shows statuses plainly** (content + Game, about 250 lines) → [slices/FT1.md](slices/FT1.md)
- [x] **BB1. The barbarian: ferocity from hits taken; Taunt draws the enemies' attacks** (Core + Data + content + Game text, about 260 lines) → [slices/BB1.md](slices/BB1.md)
- [x] **PL1. The paladin: conviction from fallen enemies and heavy hits; Smite; the cleric loses faith on heavy hits** (Core + Data + content, about 230 lines) → [slices/PL1.md](slices/PL1.md)
- [x] **ADR 0023: The battle formation** (before FM1) → [slices/ADR-0023.md](slices/ADR-0023.md)
- [x] **FM1. The party's formation in battle; Move swaps places with a neighbor** (Core, about 220 lines) → [slices/FM1.md](slices/FM1.md)
- [x] **FM2. The Move command on screen; battlers stand where the formation says** (Game, about 240 lines) → [slices/FM2.md](slices/FM2.md)
- [x] **PL2a. Guarded raises defense; combatants have current stats** (Core, about 170 lines) → [slices/PL2a.md](slices/PL2a.md)
- [x] **PL2b. The paladin's aura reaches the allies within its range in the formation** (Core + Data + content + Game, about 250 lines) → [slices/PL2b.md](slices/PL2b.md)

### Phase 4: tempo and risk
- [x] **RG1. Speed: a class stat; the rogue's readiness charges come from it** (Core + Data + content, about 240 lines) → [slices/RG1.md](slices/RG1.md)
- [x] **RG2. A quick skill doesn't end the actor's turn** (Core + Data, about 140 lines) → [slices/RG2.md](slices/RG2.md)
- [x] **RG3. Ready: an extra turn right after a chosen combatant** (Core + Data, about 260 lines) → [slices/RG3.md](slices/RG3.md)
- [ ] **RG4. The rogue's kit: Ready and Expose on screen; Exposed makes every hit land harder** (Core + content + Game, about 270 lines) → [slices/RG4.md](slices/RG4.md)
- [ ] **WL1. The warlock: casting builds instability, and stronger spells need more** (content + Game + tests, about 150 lines) → [slices/WL1.md](slices/WL1.md)
- [ ] **WL2. Instability can backfire on the caster** (Core + Data + content + Game playback, about 230 lines) → [slices/WL2.md](slices/WL2.md)

### Phase 5: forces and hits
- [ ] **DR1. Statuses that tick: poison and regeneration at the end of the holder's turn** (Core + Game playback, about 250 lines) → [slices/DR1.md](slices/DR1.md)
- [ ] **DR2. The druid: Growth and Decay with sticky dominance, and Equilibrium** (Core + Data + content, about 280 lines) → [slices/DR2.md](slices/DR2.md)
- [ ] **MO1. The monk: multi-hit skills and chakra from each hit landed** (Core + Data + content + Game, about 270 lines) → [slices/MO1.md](slices/MO1.md)

### Phase 6: play-test
- [ ] **Z1. Run-through: every class's mechanic** (manual) → [slices/Z1.md](slices/Z1.md)

### Phase 7: lasting resources and rest
- [ ] **ADR 0024: Lasting resources and rest** (before LR1) → [slices/ADR-0024.md](slices/ADR-0024.md)
- [ ] **LR1. Lasting resources carry from battle to battle on the member** (Core, about 240 lines) → [slices/LR1.md](slices/LR1.md)
- [ ] **LR2. Saves keep lasting resources: format 3** (Core + Data, about 220 lines) → [slices/LR2.md](slices/LR2.md)
- [ ] **LR3. The mage's Clarity lowers starting mana; the cleric's faith lasts** (Core + Data + content, about 200 lines) → [slices/LR3.md](slices/LR3.md)
- [ ] **RS1. The rest step: an event rests the party** (Core + Data + Game, about 240 lines) → [slices/RS1.md](slices/RS1.md)
- [ ] **RS2. The editor authors the rest step; a placeholder inn on the start map** (Editor + content, about 160 lines) → [slices/RS2.md](slices/RS2.md)
- [ ] **RS3. A rest item, used from the menu** (Core + Data + content + Game, about 200 lines) → [slices/RS3.md](slices/RS3.md)

### Phase 8: the look
- [ ] **LK1. Resource look: pips or a thin bar in the resource's color, spaced-caps label** (Game + Data + content, about 240 lines) → [slices/LK1.md](slices/LK1.md)
- [ ] **LK2. Status look: small marks with turns left; tick and backfire pops** (Game, about 220 lines) → [slices/LK2.md](slices/LK2.md)
- [ ] **LK3. Formation look: battlers slide when they swap; the aura's reach and the readied turn are shown** (Game, about 200 lines) → [slices/LK3.md](slices/LK3.md)
- [ ] **Z2. Play-through check** (manual) → [slices/Z2.md](slices/Z2.md)

## Decisions
Questions and decisions share numbers.

From the request and the pitch (2026-10-24):
- **188. Class mechanics:** each class gets its own battle resource, which defines how it plays its core mechanic, plus a couple of placeholder skills. It's a first draft: no balancing, no perfect skills. What matters is introducing the mechanics and being able to test them.
- **189. Mage, Mana; magic damage.** The mage starts each battle with some mana and gains mana every round. Saving up means casting a bigger, stronger spell. Mana goes back to its base level at the start of the next battle. The mage should need a rest: something lowers the regeneration or the starting mana ("instability, tiredness").
- **190. Barbarian, Ferocity; heavy damage, aggro, tank.** Ferocity builds up when the barbarian takes hits.
- **191. Fighter, Stamina; damage and control.** Stamina regenerates every round.
- **192. Paladin, Conviction; aura.** Conviction is gained when monsters die and when the paladin takes a heavy hit.
- **193. Rogue, actions based on speed; debuff, setup, weakness exploit.**
- **194. Warlock, Arcane Instability.** Instead of spending mana, the warlock accumulates instability when casting. More instability unlocks stronger magic but becomes increasingly dangerous.
- **195. Druid, Growth and Decay.**
  - Growth rises by healing, shielding and strengthening allies; Decay rises by damage over time, weakening enemies and consuming life.
  - Abilities grow stronger with the dominant force, and some powerful abilities need both in balance.
- **196. Numbers are the assistant's placeholders** ("don't bother with balancing"). Rules for how a resource behaves are the developer's.

The developer's answers to the draft (2026-10-24):
- **197. MP goes away.** No member uses MP any more: every class has its own resource. MP leaves members, enemies, skills, the battle panel, the menus and saves (ADR 0022). The mage's Mana replaces MP for the mage.
  - *Checked:* no item restores MP (`EffectKind.RestoreMp` is used by no content), so no item retires; the effect kind goes.
- **198. Rest is in this plan.** "There will be inns where they can rest and items that allow a rest or something like that, and I need the other classes to also NEED a rest somehow, like faith is permanently lowered until rest."
  - A lasting per-member resource that only a rest restores (ADR 0024): the mage's Clarity, the cleric's Faith.
  - Other classes' lasting costs are question 220.
  - Rest comes from an inn (a map event step, RS1-RS2) and a rest item (RS3).
- **199. Barbarian.** The ferocity gain amount is open ("will depend on how much an ability costs to cast"; question 208). Taking aggro is part of the class's skill kit: the Taunt skill.
- **200. Fighter control** means skills that control monsters; Stun is a good start.
- **201. The paladin's aura has a range in the party formation.** "If the paladin is in the middle of the party and has an aura with 1 range, it affects him and the allies immediately beside him. Some will have like 2 left, 2 right. I want the paladin to have to move around in the formation depending on aura use and the role he wants to play." The active party's order is a line in battle (ADR 0023); the party can change places in battle (FM1, FM2).
- **202. Rogue.** "Depending on his speed, he has an ability he can trigger that makes him play again, and he chooses when in the turn he will play again. He has this ability X times a battle depending on speed. Not sure about it, but start with something like that; don't hesitate if you have an idea."
  - Placeholder design: speed is a class stat, and the rogue's Readiness charges per battle come from it (RG1).
  - Ready spends a charge, doesn't end his turn, and gives him an extra turn right after a combatant he chooses (RG2).
- **203. Warlock:** "yes that's what I had in mind". Instability builds when casting, unlocks stronger spells and can backfire. Levels and balance are placeholders (question 216).
- **204. Druid dominance is sticky:** "A tie doesn't replace the dominant one until the other reaches over." The dominant force stays dominant on a tie and changes only when the other goes strictly above it. Equilibrium skills, which need both, stay.
- **205. Cleric, Faith:** "they can always cast skills that have their level of faith or under, and they lose faith more and more when bad things happen, until rest." Faith is a level, never spent. A skill requires a faith; faith drops on bad events (question 217) and stays lowered until a rest.
- **206. Monk, Chakra:** many small hits instead of one big hit. A skill's effect can hit several times. Chakra is gained per hit landed and spent on multi-hit skills (question 218).
- **207. Warrior becomes Fighter, id included:** "everything needs to correlate and be clean, no shortcut." The class id is `fighter` everywhere, with no alias (FR1). Saves hold no class id, so no save changes.
- **229. No save migrations before the first release** (ADR 0022): "delete existing saves, I don't care, they are test saves". Until the first release, a format change bumps `formatVersion` and refuses older saves. The developer's test save was deleted on 2026-10-24.

Earlier decisions that apply:
- 13: experience goes to the members still standing.
- 97: a battle with nobody standing starts lost.
- 101: only damage has an animation; anything else shows a cue with the action's name.
- 105: a greyed skill can still be selected and says why it can't be used.
- 164, 168: a class is like a D&D class; everyone starts at level 1.
- 187: the start fight is balanced on the goblin.

## Open design questions (developer decides)
Each slice uses the placeholder until answered.
- **208. Ferocity per hit.** Placeholder: +2 per hit taken, whatever the damage; the developer's direction is that it will depend on what abilities cost. Alternatives: by the share of max HP lost; also on dealing damage.
- **209. Aggro.** Placeholder: while a member has Taunt, every enemy attack goes to a taunting member. Alternatives: taunt weighs the odds rather than forcing them; a passive threat in which the barbarian is always likelier to be hit.
- **210. When the aura applies.** Placeholder: at the cast, to the allies within range then (Guarded for 3 of their turns). The alternative is a lasting area that follows positions, covering whoever stands in range each turn.
- **211. Moving in battle.** Placeholders:
  - Move swaps with an adjacent ally and costs the mover's turn; the ally loses nothing.
  - The battle's formation doesn't change the party order after the battle.

  Alternatives: a free move once per turn; any ally, not only a neighbor; the order kept after the battle.
- **212. What a heavy hit is.** Placeholder: a hit that takes at least 20 % of the target's max HP. No goblin or practice-dummy hit reaches that at level 1, so the hand checks lower the threshold locally for one run and revert it; the tests pin the rule.
- **213. Speed and readiness.** Placeholders: each class has a speed (design.md); the rogue's charges are speed ÷ 10, rounded down, at least 1 (3 at level 1). Other classes' speed does nothing yet.
- **214. Ready.** Placeholders:
  - Ready spends a charge and keeps the rogue's turn.
  - He picks any other living combatant; his extra turn comes right after that combatant's next turn.
  - The extra turn has no upkeep (no per-turn gains, no countdown), and one readied turn is pending at a time.
- **215. Weakness exploit.** Placeholder: Exposed (+50 % damage taken, from everyone, for 3 of the target's turns) is the setup, and every party member exploits it. Alternatives: a bonus for the rogue only; elemental weaknesses and breaks, Octopath-style.
- **216. The warlock's numbers.** Placeholders:
  - each point of instability is a 5 % chance per cast of a backfire costing the caster 10 % of max HP;
  - instability falls by 1 at the end of each of his turns;
  - Void Rift needs 4, Chaos Lance 7.
- **217. What lowers faith.** Placeholders: −1 when an ally falls, −1 when the cleric takes a heavy hit. Max 5, so Heal needs 1, Recovery 2 and Heal All 4.
- **218. Chakra.** Placeholders: +1 per hit landed by the monk (attacks included), max 10. Flurry spends 3 for 4 hits of power 8; Palm Strike is a free 2-hit skill.
- **219. The mage's lasting cost.** Placeholder: Clarity (max 3) drops by 1 for each battle fought, and each missing point lowers the starting mana by 1. Alternatives: it lowers the per-turn gain; it drops on casting the top spell.
- **220. A lasting cost for the other classes.** Proposals, none built until chosen. Each would be a content change, plus a trigger if it needs a new one:
  - Fighter: Fatigue: each battle lowers the max stamina by 1 until rest.
  - Barbarian: Scars: each fall lowers starting ferocity's cap.
  - Paladin: Oath: conviction spent on auras wears down the starting conviction.
  - Rogue: readiness charges per rest instead of per battle.
  - Warlock: Corruption: each backfire raises the starting instability until rest.
  - Druid: Attunement: each battle narrows the forces' max.
  - Monk: Balance: chakra's max lowered after each battle.
- **221. What a rest restores.** Placeholder: an inn and a rest item both restore full HP, revive the fallen and refill every lasting resource. Alternatives: the item only refills lasting resources; the item can't revive.
- **222. Paying for rest.** Placeholder: the inn is free (no gold exists). The rest item is found or given like any item.
- **223. What carries between battles.** Placeholder: only lasting resources (Clarity, Faith); every other resource starts each battle at its starting amount.
- **224. Enemies.** Placeholder: enemies have no resources and no skills; they only receive statuses.
- **225. Magic damage.** Placeholder: spells use the same formula as attacks (power minus defense). Should magic have its own stat, or ignore defense?
- **226. A practice fight.** Placeholder (PT): a Training Dummy encounter that no map uses; the developer adds it to the forest for a test session and reverts it. Should a map offer it for good?
- **227. The druid's first dominant force.** Placeholder: at the start, neither force dominates, so neither has the bonus. The first force to go strictly above the other becomes dominant.
- **228. Other skill sources.** The developer's direction (2026-10-24, not settled): the class is the main source, and a skill tree per class and skills granted by items may come later. Placeholder for this plan: the class's list only (ADR 0020), read through `PartyMember.Skills` so that other sources can be added to it.

Still open from earlier plans: 92, 112, 147, 151-155, 179, 180, 182, 183.

## ADRs
Each is a checkbox step right before the first slice that needs it.
- **0019 Class battle resources** (before M1a).
  - **A. Mechanisms in code, numbers in content (recommended).** A class lists its resources in `classes.json`: max, starting amount, and gains on a closed set of triggers. Skills spend, gain or require them. Special rules (backfire, dominance, lasting strain) are switched on by optional fields.
  - **B. A shared meter plus per-class rule hooks.** One meter type, and one small rule class per class, chosen by a content id, with battle hooks (turn ended, hit taken, combatant fell). Each class's rule reads in one file. A still wins: seven of nine resources differ only in numbers on the same four triggers, so B's hooks would repeat each other. A trigger change in A is a content edit the developer can make while play-testing; in B it's code.
  - **C. Scripted triggers in content.** Speculative.
  - Also records: per-turn things happen at the end of the holder's own turn, which is once per round in today's cycle; resources live for one battle unless lasting (ADR 0024).
- **0020 Skills come from the class** (before CS1; answers question 181). A: the class lists them (recommended). B: the member keeps them. C: both.
- **0021 Status effects** (before ST1).
  - A: a closed list of kinds in code, granted by an effect with a power and a number of turns from content (recommended).
  - B: statuses as content definitions.
  - C: stat modifiers only.
  - Also records:
    - a status counts down at the end of its holder's turns, starting from the turn after the one it was granted on;
    - re-applying refreshes the turns and keeps the stronger power, never stacking;
    - a fall clears a combatant's statuses;
    - all statuses end with the battle.
- **0022 MP leaves the game; no save migrations before release** (before MP1). Accepted 2026-10-24.
  - The MP part follows decision 197.
  - The save part is decision 229 (option D). It was chosen over a migration chain (A), tolerant reading (B) and DTOs per format (C).
- **0023 The battle formation** (before FM1).
  - **A. The active party order is a line; Move swaps with a neighbor (recommended).** It reuses the order the party page already sets.
  - **B. Front and back rows**, Octopath-like. A second concept that ranges don't need.
  - **C. Free positions on a grid.** Speculative.
- **0024 Lasting resources and rest** (before LR1).
  - **A. A resource marked lasting carries its amount on the member between battles and is saved; a rest refills it; an optional rule lets one lasting resource lower another's start (recommended).**
  - **B. A separate per-member "strain" counter per class**, outside resources. A second meter model for the same idea.
  - **C. Lasting effects as statuses that survive the battle.** Statuses are battle-only by ADR 0021.
- **None needed for:**
  - the class rename (decision 207);
  - multi-hit effects (a field on `Effect`);
  - taunt targeting (a rule in `EnemyTactics`);
  - Ready (a skill effect plus a turn-order insertion, under ADR 0004's one-call rule).
