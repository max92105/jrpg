# Enemy skills and bosses
Status: draft

Battle depth before Act 1 content (developer, 2026-10-29). Today every enemy only attacks (`Battle/EnemyTactics`: a random living member, or a taunting one; question 224). This plan lets enemies use the party's kind of skills, chosen by content-driven tactics, and adds a boss: an enemy the battle and the screen treat differently, with phases that change its tactics. One placeholder boss fight in the forest makes it testable and visible.

- Numbers: none yet. Decisions below are "proposed" and questions are Q1, Q2...; both get real numbers at approval (`docs/plans/README.md`, "Next free numbers").
- No save format change, no NuGet package, no new map and no story (the developer designs maps and story).

## Goal
1. **Enemy skills**: an enemy uses skills from `skills.json` (the same `SkillDefinition`, with effects, statuses and targeting), landed by the same `BattleState.Execute` path as the party's.
2. **Content-driven tactics**: each enemy lists weighted actions in `enemies.json` (a skill, or the basic attack), each with an optional condition ("own HP below 50 %", "an ally's HP below 50 %", "an ally has fallen"); the choice draws from the battle's `IRandom`, and an enemy without tactics draws exactly what it draws today.
3. **Bosses**: `isBoss` in `enemies.json`. The party can't run from a battle with a boss (Run is greyed), the boss is drawn bigger, and HP thresholds switch it to another phase's actions, shown as a cue on the boss.
4. **A placeholder boss fight**: a Goblin Chief with two goblins, in the forest's random encounter table (Q1), every number a placeholder.

## Order, and why
1. **Phase 1: tactics in Core (ADR step, T1, T2).** The chooser and its conditions, tested headless with test catalogs; nothing in the repository's content uses them yet, so play doesn't change.
2. **Phase 2: tactics in content (D1, C1).** The validator, then the placeholder Chief and its skills in the forest: from C1 on, every slice is checked by hand in a forest fight (the forge review's "a hand check exists early").
3. **Phase 3: bosses (B1, B2, B3, B4).** No escape first (smallest, visible at once), then phases in Core, then phases in content with their cue, then the bigger battler.
4. **Phase 4: numbers (S1).** A boss fight simulation, so the developer tunes the placeholders with win rates rather than guesses.

## Guiding rules
- **Mechanisms in code, numbers in content** (ADR 0019's pattern): the closed list of condition kinds and the phase rule are code with tests; weights, percents, thresholds and every stat are `enemies.json` and `skills.json` placeholders (design.md, "Tunables").
- **One path for both sides** (ADR 0004): an enemy's skill is a `SkillCommand` passed to `Execute`, checked by the same `CanUse` and `TargetChoices` as a member's; `EnemyTactics` stays outside `BattleState`.
- **Determinism**: every draw goes through the battle's `IRandom`; a draw happens only when the outcome is uncertain (ADR 0026): one weighted pick among more than one eligible action, then the target draw the attack already makes. An enemy without tactics keeps today's draws, so every seeded test and simulation keeps its sequence.
- **Enemies have no class resources** (ADR 0019): a skill an enemy lists may neither change nor require one; Data refuses it at load.
- **The screen decides nothing**: whether Run is offered is `BattleState.CanFlee`; which phase an enemy is in is Core's, reported as an event (ADR 0013's playback, exhaustive over events).
- **Slice sizes:** under about 250 lines, tests at about 13 lines each plus the existing tests a change breaks. A Game slice that adds a view type gives its raw estimate and the 1.6x; none here adds one (B4 changes `BattlerArt` and `BattlerScaling`).
- **Files near 300 lines:** `Battle/BattleState.cs` 269 (B1 adds about 8; the backlog's "split the per-command bodies" waits for the next command path, which this plan doesn't add), `Battle/ActionPlayback.cs` 252 (B3 adds one switch arm in `AddImpactCue`, not a case in `Read`, which is at the complexity limit), `GameSession` 245 (+1). New tests go in new files beside `EnemyTacticsTests` and `CommandMenuTests` (393).

## Out of scope (follow-ups)
- **A battle event step** (Q1): a boss met on a cell or by talking to someone, with its editor support. The Act 1 content will need it; the placeholder boss uses the forest's random table.
- **A boss beaten once for good** (a world flag after the victory): needs the event step.
- **Enemy resources, cooldowns or "once per battle" actions** (Q4).
- **Phases that change stats, resistances or art** (Q6).
- **A boss name banner or a top HP bar** (Q10): planned only if the developer picks it.
- **Skills for the goblin and the stone beetle** (Q11): the developer's design.
- **Flipping enemy effect strips** (backlog, "From the battle plan"): enemy skills reuse the enemy's attack strip as attacks do today.

## Slices

One line per slice; each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: tactics in Core
- [ ] **ADR: enemy tactics and boss phases** (with `/adr`, before T1; options in design.md, "Key decisions" 1 and 5)
- [ ] **T1. Enemies choose a skill from a weighted list** (Core, about 240 lines) → [slices/T1.md](slices/T1.md)
- [ ] **T2. Conditions on actions; a heal aims at the most wounded** (Core, about 210 lines) → [slices/T2.md](slices/T2.md)

### Phase 2: tactics in content
- [ ] **D1. `enemies.json` tactics, checked against the skills** (Data, about 230 lines) → [slices/D1.md](slices/D1.md)
- [ ] **C1. A placeholder Goblin Chief fight in the forest, with its skills** (content + tests, about 90 lines) → [slices/C1.md](slices/C1.md)

### Phase 3: bosses
- [ ] **B1. No running from a boss: Core refuses Flee, Run is greyed** (Core + Game + content, about 170 lines) → [slices/B1.md](slices/B1.md)
- [ ] **B2. Boss phases in Core: HP thresholds switch the actions, reported as an event** (Core, about 230 lines) → [slices/B2.md](slices/B2.md)
- [ ] **B3. Phases in content and on screen: validated, the Chief's second phase, a cue on the boss** (Data + Game + content + text, about 200 lines) → [slices/B3.md](slices/B3.md)
- [ ] **B4. A boss is drawn bigger** (Game, about 140 lines; no new view type) → [slices/B4.md](slices/B4.md)

### Phase 4: numbers
- [ ] **S1. A boss fight simulation** (Simulations, about 110 lines) → [slices/S1.md](slices/S1.md)

## Decisions (proposed, numbered at approval)
- **Enemy skills live in `skills.json`** beside the party's (the developer's request); no separate file. A skill no class lists is legal content.
- **An enemy's skill uses no class resource and isn't quick**: Data refuses one that changes or requires a resource (enemies have none, ADR 0019), and one whose targeting is `alliesInRange` (the formation is the party's, ADR 0023) or `otherCombatant` (Ready, a rogue's resource mechanic). Quick skills are already required to spend a resource, so they fall under the first rule.
- **An action without `skillId` is the basic attack**; an enemy without `tactics` only attacks, exactly as today (same draws).
- **An action is eligible when its condition holds and `BattleState.CanUse` allows its skill** (someone to aim at: a revive with nobody fallen isn't). With no eligible action the enemy attacks (Q5).
- **Enemy skills land as the content wrote them**: an enemy's `CombatStats` scale by 1000 permille (no Power, no Intelligence), so a skill's power and grant are its content numbers.
- **Taunt draws single-target skills aimed at the party**, as it draws attacks, with the same crowd control resistance roll (Q2); skills that hit every member ignore it.
- **The enemy knows a skill when its tactics name it**: `Combatant.Knows(skill)` replaces `Skills.Contains` in `BattleState.CanUse`; enemies keep an empty `Skills` (design.md, "Key decisions" 2).
- **A phase replaces the enemy's actions, latched**: once the boss's HP falls below a phase's threshold, that phase's actions are its list for the rest of the battle, even if healed above it (Q6, Q7). Phases are checked at the end of each action, before the outcome, so a poison tick crosses one too.
- **No escape when a boss is in the battle, standing or fallen** (Q9); Run is greyed like Skills, Items and Move are (decision 105), with no notice.
- **The placeholder boss is reached through the forest's random table** (Q1), one encounter in three.

## Open design questions (developer decides)
- **Q1. How does a boss fight start?** Proposed for this plan: the Chief's encounter joins the forest's random table, so it can be met and tested now. Alternatives: a battle event step (a boss on a cell or behind a talk, with editor support; a later plan, likely with Act 1), or a Debug-only command line start (`--battle <encounter>`). Which one do you want for real bosses?
- **Q2. Whom does an enemy's single-target skill aim at?** Proposed: at random among the legal targets, like the attack, and a taunting member draws it. Alternatives: the lowest HP, the front of the formation, a per-action rule in content.
- **Q3. Whom does an enemy heal?** Proposed: the living ally with the lowest HP share (ties by encounter order), no draw. Alternative: at random.
- **Q4. Which conditions?** Proposed set: own HP below a percent, an ally's HP below a percent, an ally has fallen. Others to consider: a target without a status (no Poison on the already poisoned), every Nth turn, once per battle, the party's size, a cooldown per action.
- **Q5. When no action is eligible**, should the enemy attack (proposed), or must content always give an action without a condition (a load error otherwise)?
- **Q6. What does a phase change?** Proposed: the action list only. Also stats, resistances, art, or a heal at the threshold?
- **Q7. Is a phase latched** (proposed: never goes back), or does it follow the HP, so a boss healed above the threshold returns to its earlier actions?
- **Q8. How is a phase change shown?** Proposed placeholder: a one-word cue on the boss from `ui.json` ("Enraged!"), at the impact. Alternatives: a line per phase in `enemies.json`, the action-name window, a banner, or nothing.
- **Q9. Escape rule.** Proposed: no Run while a boss is in the battle, even fallen, and Run greyed with no notice. Alternatives: a flag on the encounter (`canEscape`), Run allowed once the boss falls, a "Can't escape!" notice on Confirm.
- **Q10. How else does a boss look different?** Planned: drawn at twice the size. Also: its name in a banner as the battle opens, or a long HP bar at the top of the screen instead of its tag?
- **Q11. Do the goblin and the stone beetle get skills now?** Proposed: no; only the Chief has tactics, and the two goblins beside it stay plain goblins.
- **Q12. The Chief's numbers and kit** (design.md, "Tunables"): 240 HP, Cleave on the whole party, Field Dressing on a wounded ally, Crushing Blow with a chance to stun and War Cry in phase 2. Are these the shape you want for a first boss?

## ADRs needed
- **ADR needed: enemy tactics and boss phases.** Options: (A) a weighted list of actions with conditions from a closed set, phases that replace the list at HP thresholds (proposed, the developer's example); (B) an ordered priority list, the first matching rule wins (Final Fantasy XII's gambits): predictable, no weights; (C) one tactics class per enemy kind, named by content (ADR 0019's option B): free-form, but every change is code and content names a C# type. Includes the latched-phase rule and when a phase is entered.
