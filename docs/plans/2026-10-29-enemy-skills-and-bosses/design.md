# Enemy skills and bosses: design

Part of [the enemy skills and bosses plan](README.md).

## Tunables and placeholders

Every number is a placeholder for the developer to tune (decision 410); S1's simulation prints what they give.

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| Action weight | `enemies.json` `tactics[].weight` | per action, at least 1 | An eligible action's share of the pick: its weight over the eligible total |
| Condition percent | `enemies.json` `tactics[].when.percent` | 50 | "Below half": own HP, or a living ally's, under this share of max HP (1..99) |
| Phase threshold | `enemies.json` `phases[].belowHpPercent` | 50 | The boss enters the phase once its HP falls under this share (1..99, descending) |
| Phase beat | `Game/Battle/BattlePacing.PhaseHold` | 1.0 s | How long the "Enraged!" beat holds, after the action's beats |
| Phase cue | `content/text/ui.json` `battle_enemy_phase` | "Enraged!" | The word on the boss when it enters a phase (decision 406) |
| Goblin Chief | `enemies.json` `goblin_chief` | HP 240, damage 12 piercing, accuracy 80, dodge 0, crit 5, crowd control resistance 50, resistances fire −25, XP 150, drop table `goblin`, `isBoss` (B1) | The placeholder boss |
| Chief's art | `content/art/battlers/GoblinChief.png` (B4) | the Goblin's 256 px art scaled up 2x by whole pixels, 512 px | Drawn at 256 px by today's `BattlerScaling` (pixel art up to 512 px is halved), twice the goblin's 128 |
| Chief's phase 1 | `enemies.json` `goblin_chief.tactics` | attack 4; `cleave` 2 (C1); `field_dressing` 3 when a living ally's HP is below 50 % (T3) | |
| Chief's phase 2 | `enemies.json` `goblin_chief.phases[0]` (B3) | below 50 %: attack 2; `crushing_blow` 3; `cleave` 2; `war_cry` 2 when an ally has fallen | |
| `cleave` | `skills.json` (C1) | damage 10 slashing, `allEnemies` | Hits the whole party |
| `field_dressing` | `skills.json` (T3) | restoreHp 25, `singleAlly` | Heals one enemy, the Chief included |
| `crushing_blow` | `skills.json` (B3) | damage 24 blunt, `singleEnemy`, Stun 40 % for 1 turn | Phase 2's heavy hit |
| `war_cry` | `skills.json` (B3) | grantStatus Guarded 20 for 2 turns, `allAllies` | Phase 2: raises every standing enemy's resistances |
| Boss encounter | `encounters.json` `goblin_chief` (C1) | `["goblin", "goblin", "goblin_chief"]`, `Forest.png` | The Chief in the last slot, the top row, drawn last, so nothing covers it; rolled by no map (decision 399) |

## Design overview

### What exists
- `Battle/EnemyTactics(IRandom)`: one per playthrough (`GameSession.EnemyTactics`), asked by the Game's `BattleTurnSequence` on every enemy turn; returns an `AttackCommand` on a random living member, or a taunting one unless the enemy's crowd control resistance shrugs the taunt off (decision 324). One draw for the target, one more only when a taunt might be resisted.
- `Battle/BattleState.Execute(SkillCommand)` already works for any actor: `CanUse` (knows it, can pay, has targets), `TargetChoices.ThrowIfIllegal`, `EffectScaling` (an enemy's `DamagePermille` and `StatusPermille` are 1000), `SkillEmpowerment` and `SkillBackfire` (no-ops without resources), `EffectLanding`. `TargetRules` works by relation, so an enemy's `singleEnemy` aims at the party and its `singleAlly` at an enemy, itself included.
- `Battle/Combatant.FromEnemy(EnemyDefinition)`: `Skills` empty, `Resources` empty. 58 call sites, almost all tests: its signature stays.
- `Battle/EnemyDefinition`, `EncounterDefinition`; `Data/Battle/EnemyCatalogValidator.Validate(file, enemies, dropTableIds)` (4 test sites).
- `Battle/BattleState.Execute`: `ActionStarted`, the command, then `TurnFlow` (upkeep, the stunned loop, the outcome) unless the command ended the battle. `Flee`: only the party flees; nothing else refuses it. `Game/Battle/CommandMenu.IsEnabled` greys Skills, Items and Move, and its `ListCursor` skips a greyed entry.
- Game playback (`ActionPlayback`) is actor-agnostic: an enemy's `SkillCommand` shows its name in the action window, damage plays the enemy's attack strip, anything else a cue with the skill's name, a status its name. `Read` is at the complexity limit (backlog, stats plan); `AddSkipCue` and `AddBackfire` give their cues a beat of their own after the action's.
- `Game/Battle/BattlerScaling`: art over 512 px is a painting shrunk to the spot's square; pixel art up to 512 px is halved (the goblin's 256 px art draws at 128).
- Encounters come only from a map's random table; the training dummy's encounter is rolled by no map (question 226), pinned by `ContentCatalogLoaderTests.Load_RepositoryContent_HasTrainingDummyEncounter`.

### Core changes (all in `Core/Battle`)
- **Content records** beside `EnemyDefinition` (no new folder):
  - `EnemyAction` (T1): one weighted choice in an enemy's tactics. `public sealed record`: `string? SkillId { get; init; }` (null: the basic attack), `required int Weight { get; init; }`, `TacticCondition? When { get; init; }` (T2).
  - `TacticCondition` (T2): when an action may be chosen. `public sealed record TacticCondition(TacticConditionKind Kind, int Percent = 0)`.
  - `TacticConditionKind` (T2): the closed list. `OwnHpBelow`, `AllyHpBelow`, `AllyFallen`.
  - `EnemyPhase` (B2): the actions an enemy switches to below a share of its HP. `public sealed record`: `required int BelowHpPercent`, `required IReadOnlyList<EnemyAction> Tactics`.
- `EnemyDefinition` gains `IReadOnlyList<EnemyAction> Tactics { get; init; } = []` (T1), `bool NamesSkill(string skillId)` (T1; B2 adds the phases' actions; a method, so the content JSON never sees it), `bool IsBoss { get; init; }` (B1), `IReadOnlyList<EnemyPhase> Phases { get; init; } = []` (B2).
- `EnemyTargeting` (T0, internal): whom an enemy aims a chosen action at. T0 moves the attack's rule into it unchanged; T1 adds skills (decision 400); T3 adds the heal (decision 401).
- `EnemyTactics(IRandom random, IReadOnlyDictionary<string, SkillDefinition> skills)` (T1): the enemy's current actions (its phase's from B2, else its own), the eligible ones, one weighted pick, nothing eligible → the attack (decision 403), then the targets from `EnemyTargeting`.
- `TacticConditions` (T2, internal static): who meets a condition for an enemy now (itself, the living allies below the share, the fallen allies), and whether anyone does.
- `Combatant.Knows(SkillDefinition skill)` (T1): a member's `Skills` contain it; an enemy's definition names it. `BattleState.CanUse` calls it.
- `Combatant.EnemyPhase` (B2): the phase reached, null before any; set only by `EnemyPhases`.
- `EnemyPhases` (B2, internal static): enters each standing enemy's deepest phase its HP has fallen below, reporting `EnemyPhaseEntered(Combatant Enemy, EnemyPhase Phase)` (new event). Called once, at the end of `Execute`, while the battle isn't over.
- `BattleState.CanFlee` (B1): the actor is a party member and no enemy in the battle is a boss. `Flee` throws when it's false.
- `Session/GameSession`: `new EnemyTactics(context.Random, context.Catalog.Skills)` (T1).

**Data** (`Data/Battle`): `EnemyReferences(IReadOnlySet<string> DropTableIds, IReadOnlyDictionary<string, SkillDefinition> Skills)` replaces the `dropTableIds` parameter, as `ClassReferences` does for classes (D1); `EnemyTacticsValidator` checks an enemy's actions (D1), their conditions (T2) and its phases (B3).

**Game** (`Game/Battle`): `CommandMenu` greys Run when `!battle.CanFlee` (B1); `ActionPlayback` shows `EnemyPhaseEntered` as a cue in a `PlaybackBeatKind.Phase` beat (B3). No size rule: the Chief's art is bigger (B4).

### Structure and folder dependencies

| From | To | Why | New? |
|---|---|---|---|
| Core/Battle | Core/Skills | `EnemyTactics` resolves a skill id | exists |
| Core/Session | Core/Battle | builds `EnemyTactics` with the catalog's skills | exists |
| Data/Battle | Core/Battle, Core/Skills | the validator checks actions against the skills | exists (Battle), new (Skills) |
| Game/Battle | Core/Battle | `CanFlee`, `EnemyPhaseEntered` | exists |
| tests/Jrpg.Simulations/Battle | Core/Battle, Data/Content | `BossFightSimulation` on the repository's content | exists |

No new folder; no Content method (a method taking `ContentCatalog` lives in Content or Session, never in a folder Content references: loot retro).

### Key decisions
1. **Weighted actions with conditions, phases that replace them, latched** (ADR 0034; decisions 402-405). RPG Maker's action patterns are the precedent.
2. **Ids in content, resolved by the tactics; `Combatant.Knows`.** An enemy's actions name skills by id (as a class's `skillIds` do, ADR 0020). `EnemyTactics` holds the catalog's skills and builds the `SkillCommand`; `BattleState.CanUse` asks `Combatant.Knows`, which asks the enemy's definition. Rejected: `Combatant.FromEnemy(enemy, skills)` (58 call sites, and `BattleState.Start` would take a fifth parameter); a Data DTO or a converter resolving ids at load (no precedent, and the shared read-only `ContentJsonOptions` can't hold a lookup).
3. **Draw only when uncertain.** The weighted pick goes through `Randomness/WeightedPick` (no draw when one eligible action holds the whole weight; never called with nothing eligible, since that is the attack); conditions draw nothing; the target draws as the attack does today. An enemy without tactics never calls the pick.
4. **Phases are entered at one place: the end of `BattleState.Execute`, while the battle isn't over.** By then every HP loss of the call has happened (hits, backfire, poison ticks, the stunned loop's ticks, a kept turn), so one call sees them all and `TurnFlow` stays as it is. `EnemyPhaseEntered` is the call's last event. A battle the action ended enters nothing; a fallen boss enters nothing.
5. **The phase cue has its own beat, without a new case in `Read`.** `Read`'s default arm calls `AddImpactCue` today; B3 renames it `AddCue` and sends `EnemyPhaseEntered` to a new `AddPhaseCue` (a `Phase` beat after every beat before it, the cue showing when it starts, as `AddSkipCue` does), and the other four events to the impact cue as now. `Read` keeps its case count (CA1502).
6. **No escape is a Core query** (`CanFlee`), read by the menu; `Execute(FleeCommand)` throws without it, as every illegal command does (ADR 0004).
7. **"Boss" is an enemy flag; its size is its art.** Core reads `isBoss` for the escape rule (decision 414); the screen draws every battler by its image's size (decision 408), so a bigger boss is bigger art, and no `IsBoss` branch exists in the Game. The music plan's boss theme belongs on the Chief's encounter (`encounters.json`), not on the training dummy's.

### Extension points
- A new condition: a `TacticConditionKind` member, its arm in `TacticConditions` (the default arm throws), its Data rule and a test.
- A battle event step (decision 399's later plan) would set `ExplorationState.PendingEncounter` from a step; `GameSession.StartPendingBattle` needs no change.
- Phase effects beyond the action list would hang off `EnemyPhases.EnterReached`.

### Replacement mapping
| Existing | Becomes |
|---|---|
| `EnemyTactics.ChooseCommand`'s target rule (taunt, resist roll, random member) and `ResistsTaunt` | `EnemyTargeting.ForAttack` (T0, moved, same draws) |
| `BattleState.CanUse`'s `CurrentActor.Skills.Contains(skill)` | `CurrentActor.Knows(skill)` (T1) |
| `EnemyCatalogValidator.Validate(file, enemies, dropTableIds)` | `Validate(file, enemies, EnemyReferences)` (D1) |
| `new EnemyTactics(random)` (18 sites) | `new EnemyTactics(random, skills)` (T1); tests pass an empty dictionary unless they use skills |
| `ActionPlayback.AddImpactCue` | `AddCue` (B3): the phase cue to `AddPhaseCue`, the rest as before |

## Risks
- **Determinism:** an enemy with tactics draws once more per turn (the pick) when more than one action is eligible, so seeded tests that use such an enemy pin the pick's draw. No repository encounter a map rolls holds an enemy with tactics, so no Game test, `ForestFightsSimulation` or `LootSimulation` moves.
- **A local forest table left in a commit:** hand checks add `goblin_chief` to `content/maps/forest.json` and revert. C1's repository test (no map rolls it) fails the verify run if it stays.
- **A fallen enemy revived by an enemy skill:** `TargetRules` allows `singleFallenAlly` for enemies, and the screen fades the fallen out. No placeholder skill revives; if the developer writes one, its battler and tag coming back need a hand check (not planned).
- **Coupling:** `EnemyTactics` now needs the skill catalog; it stays one per playthrough. `Combatant.Knows` walks the enemy's few actions on each `CanUse` (a handful per turn, never per frame).
- **The Chief's placeholder art** is a scaled copy of the goblin's: made with PowerShell's `System.Drawing` (nearest neighbor; no Python here), replaced by the developer's art later. `ArtReferenceCheck` checks the name's case.
- **File sizes:** `BattleState` 269 → about 281 (B1, B2); `ActionPlayback` 252 → about 264 (B3); `EnemyTactics` about 34 → about 75 (T0 to T2): if its eligibility grows past that, it moves into `TacticConditions`.
- **Save format:** unchanged. Phases and tactics are battle state and content; nothing new is saved.
