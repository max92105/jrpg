# Enemy skills and bosses
Status: approved, in progress

Battle depth before Act 1 content (developer, 2026-10-29). Today every enemy only attacks (`Battle/EnemyTactics`: a random living member, or a taunting one; question 224). This plan lets enemies use the party's kind of skills, chosen by content-driven tactics, and adds a boss: an enemy the battle treats differently, with phases that change its tactics. One placeholder boss encounter, the Goblin Chief, makes it testable and visible.

- Numbers: decisions 399-414, ADR 0034.
- Approved on 2026-10-29 with the developer's answers (399-410) and the devil's advocate's edits: the Chief's encounter is rolled by no map (ADR 0017's "a lone level 1 leader must win the start area's fights" stays true), as the training dummy's (question 226); the records stay in `Battle`; the attack's target rule moves into `EnemyTargeting` in its own refactor slice; the "nothing eligible" fallback lands with the first pick; phases are entered at one place, the end of `Execute`; the phase cue has its own beat; a boss's size comes from its art; the simulation comes before the boss slices.
- No save format change, no NuGet package, no new map and no story (the developer designs maps and story).

## Goal
1. **Enemy skills**: an enemy uses skills from `skills.json` (the same `SkillDefinition`, with effects, statuses and targeting), landed by the same `BattleState.Execute` path as the party's.
2. **Content-driven tactics**: each enemy lists weighted actions in `enemies.json` (a skill, or the basic attack), each with an optional condition: own HP below a percent, a living ally's HP below a percent, an ally fallen (decision 402). The pick draws from the battle's `IRandom`; an enemy without tactics draws exactly what it draws today.
3. **Bosses**: `isBoss` in `enemies.json` (decision 414). The party can't run from a battle with a boss (decision 407), and HP thresholds switch the boss to another phase's actions, shown as an "Enraged!" cue in its own beat (decisions 404-406). A boss looks bigger only through its art (decision 408).
4. **A placeholder boss encounter**: a Goblin Chief behind two goblins, in `encounters.json` but rolled by no map (decision 399); hand checks add it to the forest's table locally and revert.

## Order, and why
1. **Phase 1: the first pick (ADR step, T0, T1).** The attack's target rule moves out of `EnemyTactics` unchanged, then the weighted pick of skills, tested headless; nothing in the repository's content uses it yet.
2. **Phase 2: the Chief in content (D1, C1).** The validator for actions, then the Chief with Attack and Cleave: from C1 on, every slice is checked by hand in the Chief's fight.
3. **Phase 3: conditions (T2, T3).** The three conditions, then the heal that follows its condition, with Field Dressing.
4. **Phase 4: numbers (S1).** A Chief fight simulation before the boss rules, so the developer tunes the kit with win rates.
5. **Phase 5: the boss (B1, B4, B2, B3).** No escape (visible at once), the Chief's own bigger art, then phases in Core and in content with their cue.

## Guiding rules
- **Mechanisms in code, numbers in content** (ADR 0019's pattern): the closed list of condition kinds and the phase rule are code with tests; weights, percents, thresholds and every stat are `enemies.json` and `skills.json` placeholders (design.md, "Tunables"; decision 410).
- **One path for both sides** (ADR 0004): an enemy's skill is a `SkillCommand` passed to `Execute`, checked by the same `CanUse` and `TargetChoices` as a member's; `EnemyTactics` stays outside `BattleState`.
- **Determinism**: every draw goes through the battle's `IRandom`, only when the outcome is uncertain (ADR 0026): one weighted pick among more than one eligible action, then the target draw the attack already makes. An enemy without tactics keeps today's draws, so every seeded test and simulation keeps its sequence.
- **Enemies have no class resources** (ADR 0019): a skill an enemy lists may neither change nor require one; Data refuses it at load (decision 411).
- **The screen decides nothing**: whether Run is offered is `BattleState.CanFlee`; which phase an enemy is in is Core's, reported as an event (ADR 0013's playback, exhaustive over events).
- **ADR 0017's balance rule holds**: the start area's maps roll no boss; S1 prints a lone level 1 leader against the Chief for the developer, without asserting a win.
- **Slice sizes:** under about 250 lines, tests at about 13 lines each plus the existing tests a change breaks. No slice adds a Game view type, so none takes the 1.6x.
- **Files near 300 lines:** `Battle/BattleState.cs` 269 (B1 +8, B2 +4: about 281; the backlog's "split the per-command bodies" waits for the next command path, which this plan doesn't add), `Battle/ActionPlayback.cs` 252 (B3 +12; `Read` keeps its cases, design.md "Key decisions" 5), `GameSession` 245 (+1). `ContentCatalogLoaderTests` is 272: this plan's repository tests go in a new `RepositoryEncounterTests`. `CommandMenuTests` is 393: B1's tests go in `CommandMenuRunTests`. `EnemyTacticsTests` (135) keeps the attack tests.

## Out of scope (follow-ups)
- **A battle event step** (decision 399): a boss met on a cell or by talking to someone, with its editor support, in a later plan; then a boss beaten once for good (a world flag).
- **More conditions, enemy resources, cooldowns or "once per battle" actions** (decision 402 keeps three).
- **Phases that change stats, resistances or art** (decision 404).
- **A boss name banner or a top HP bar** (decision 408).
- **Skills for the goblin and the stone beetle** (decision 409).
- **Flipping enemy effect strips** (backlog, "From the battle plan"): enemy skills reuse the enemy's attack strip as attacks do today.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: the first pick
- [x] **ADR 0034: enemy tactics and boss phases** (write it from the "ADRs" entry below: the developer's answers 402-405 chose option A)
- [x] **T0. Move the attack's target rule into `EnemyTargeting`** (Core refactor, about 70 lines) → [slices/T0.md](slices/T0.md)
- [x] **T1. Enemies choose a skill from a weighted list; nothing usable means the attack** (Core, about 245 lines) → [slices/T1.md](slices/T1.md)

### Phase 2: the Chief in content
- [x] **D1. `enemies.json` actions, checked against the skills** (Data, about 190 lines) → [slices/D1.md](slices/D1.md)
- [ ] **C1. The Goblin Chief's encounter, with Attack and Cleave, rolled by no map** (content + tests, about 90 lines) → [slices/C1.md](slices/C1.md)

### Phase 3: conditions
- [ ] **T2. Conditions on actions: own HP, a living ally's HP, an ally fallen** (Core + Data, about 220 lines) → [slices/T2.md](slices/T2.md)
- [ ] **T3. A heal aims at the most wounded ally its condition found; the Chief's Field Dressing** (Core + content, about 110 lines) → [slices/T3.md](slices/T3.md)

### Phase 4: numbers
- [ ] **S1. A Goblin Chief fight simulation** (Simulations, about 140 lines) → [slices/S1.md](slices/S1.md)

### Phase 5: the boss
- [ ] **B1. No running from a boss: Core refuses Flee, Run is greyed** (Core + Game + content, about 170 lines) → [slices/B1.md](slices/B1.md)
- [ ] **B4. The Chief's own art, drawn bigger by its size** (content + test, about 40 lines) → [slices/B4.md](slices/B4.md)
- [ ] **B2. Boss phases in Core, entered at the end of `Execute`** (Core, about 230 lines) → [slices/B2.md](slices/B2.md)
- [ ] **B3. Phases in content and on screen: validated, the Chief's second phase, "Enraged!" in its own beat** (Data + Game + content + text + Simulations, about 220 lines) → [slices/B3.md](slices/B3.md)

## Decisions
Developer's answers (2026-10-29):
- **399. The Goblin Chief's encounter exists, and no map rolls it**, as the training dummy's (question 226); hand checks add it to the forest's table locally and revert. A battle event step comes in a later plan. (Q1)
- **400. A single-target enemy skill aims at a random living member, and a taunting member draws it**: the basic attack's rule, crowd control resistance roll included. A skill that takes every target ignores Taunt. (Q2)
- **401. An enemy heal goes to the lowest HP share among the living allies that met its condition** (itself for "own HP below"; every legal target without an HP condition); ties go to the first in battle order; no random draw. (Q3)
- **402. Three conditions**: own HP below a percent, a living ally's HP below a percent, an ally fallen. An ally is another enemy in the same battle, never the enemy itself; "below" is strict. (Q4)
- **403. With nothing eligible, the enemy attacks.** An action is eligible when its condition holds and `BattleState.CanUse` allows its skill (someone to aim at). (Q5)
- **404. A phase changes the action list only.** (Q6)
- **405. A phase is latched**: once the boss's HP falls below a threshold, that phase's actions are its list for the rest of the battle, even if healed above it. (Q7)
- **406. A phase change shows a generic "Enraged!" cue on the boss, in its own beat after the action's.** (Q8)
- **407. No Run while a boss is in the battle, standing or fallen**; Run is greyed, with no notice, as Skills, Items and Move are (decision 105). (Q9)
- **408. Nothing more marks a boss on screen now**: its size comes from its art; a banner or a top HP bar later. (Q10)
- **409. The goblin and the stone beetle get no skills in this plan.** (Q11)
- **410. The Chief's numbers and kit stay placeholders** in design.md's "Tunables". (Q12)

Planner's picks the review accepted (2026-10-29):
- **411. Enemy skills live in `skills.json`** beside the party's (the developer's request), and **an enemy's skill uses no class resource**: Data refuses one that changes or requires a resource (enemies have none, ADR 0019), one aimed `alliesInRange` (the formation is the party's, ADR 0023) and one aimed `otherCombatant` (Ready, a rogue's resource mechanic). Quick skills must spend a resource, so the first rule covers them.
- **412. An action without `skillId` is the basic attack; an enemy without `tactics` only attacks**, exactly as today, with the same draws.
- **413. Enemy skills land as content wrote them**: an enemy's `CombatStats` scale by 1000 permille (no Power, no Intelligence), so a skill's power and grant are its content numbers.
- **414. "Boss" is a flag on the enemy (`isBoss`)**, not on the encounter: the battle reads it from its combatants. The music plan's boss theme goes on the Chief's encounter (an `encounters.json` theme field), not on the training dummy's.

## Open design questions (developer decides)
None open.

## ADRs
- **ADR 0034: enemy tactics and boss phases.** Options: (A) a weighted list of actions with conditions from a closed set, phases that replace the list at HP thresholds, latched; precedent: RPG Maker's action patterns (skill, condition, rating), which pick among the actions whose condition holds by rating; (B) an ordered priority list, the first matching rule wins (Final Fantasy XII's gambits): predictable, no weights; (C) one tactics class per enemy kind, named by content (ADR 0019's option B): free-form, but every change is code and content names a C# type. Decision: A (decisions 402-405); phases are entered at the end of `Execute` (design.md, "Key decisions" 4).
