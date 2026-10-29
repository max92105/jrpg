# Enemy skills and bosses: design

Part of [the enemy skills and bosses plan](README.md).

## Tunables and placeholders

Every number is a placeholder for the developer to tune (README, "Guiding rules"); S1's simulation prints what they give.

| Name | Where | Placeholder | Meaning |
|---|---|---|---|
| Action weight | `enemies.json` `tactics[].weight` | per action, at least 1 | An eligible action's share of the pick: its weight over the eligible total |
| Condition percent | `enemies.json` `tactics[].when.percent` | 50 | "Below half": own HP, or an ally's, under this share of max HP (1..99) |
| Phase threshold | `enemies.json` `phases[].belowHpPercent` | 50 | The boss enters the phase once its HP falls under this share (1..99, descending) |
| Boss size factor | `Game/Battle/BattlerArt.BossSizeFactor` | 2 | A boss is drawn this many times a regular battler's size (a whole factor: pixel art stays crisp) |
| Phase cue | `content/text/ui.json` `battle_enemy_phase` | "Enraged!" | The word on the boss when it enters a phase (Q8) |
| Goblin Chief | `enemies.json` `goblin_chief` | HP 240, damage 12 piercing, accuracy 80, dodge 0, crit 5, crowd control resistance 50, resistances fire −25, XP 150, drop table `goblin`, `isBoss` | The placeholder boss (Q12) |
| Chief's phase 1 | `enemies.json` `goblin_chief.tactics` | attack 4; `cleave` 2; `field_dressing` 3 when an ally's HP is below 50 % | |
| Chief's phase 2 | `enemies.json` `goblin_chief.phases[0]` | below 50 %: attack 2; `crushing_blow` 3; `cleave` 2; `war_cry` 2 when an ally has fallen | |
| `cleave` | `skills.json` | damage 10 slashing, `allEnemies` | Hits the whole party |
| `field_dressing` | `skills.json` | restoreHp 25, `singleAlly` | Heals one enemy (the Chief included) |
| `crushing_blow` | `skills.json` | damage 24 blunt, `singleEnemy`, Stun 40 % for 1 turn | Phase 2's heavy hit |
| `war_cry` | `skills.json` | grantStatus Guarded 20 for 2 turns, `allAllies` | Phase 2: raises every standing enemy's resistances |
| Boss encounter | `encounters.json` `goblin_chief` | `["goblin", "goblin_chief", "goblin"]`, `Forest.png` | The Chief in the middle slot |
| Forest table | `content/maps/forest.json` | `goblin_chief` added (one encounter in three) | Q1 |

## Design overview

### What exists
- `Battle/EnemyTactics(IRandom)`: one per playthrough (`GameSession.EnemyTactics`), asked by the Game's `BattleTurnSequence` on every enemy turn; returns an `AttackCommand` on a random living member, or a taunting one unless the enemy's crowd control resistance shrugs the taunt off (decision 324). One draw for the target, one more only when a taunt might be resisted.
- `Battle/BattleState.Execute(SkillCommand)` already works for any actor: `CanUse` (knows it, can pay, has targets), `TargetChoices.ThrowIfIllegal`, `EffectScaling` (an enemy's `DamagePermille` and `StatusPermille` are 1000), `SkillEmpowerment` and `SkillBackfire` (no-ops without resources), `EffectLanding`. `TargetRules` works by relation, so an enemy's `singleEnemy` aims at the party and its `singleAlly` at an enemy (itself included).
- `Battle/Combatant.FromEnemy(EnemyDefinition)`: `Skills` empty, `Resources` empty. 58 call sites, almost all tests: its signature stays.
- `Battle/EnemyDefinition`, `EncounterDefinition`; `Data/Battle/EnemyCatalogValidator.Validate(file, enemies, dropTableIds)` (4 test sites).
- `Battle/TurnFlow`: upkeep, then `OutcomeOf` (three call sites: `EndTurn`, `KeepTurn`, the stunned loop), then the advance.
- `Battle/BattleState.Flee`: only the party flees; nothing refuses it otherwise. `Game/Battle/CommandMenu.IsEnabled` greys Skills, Items and Move.
- Game playback (`ActionPlayback`) is actor-agnostic: an enemy's `SkillCommand` shows its name in the action window, damage plays the enemy's attack strip, anything else a cue with the skill's name, a status its name.
- No event step starts a battle: encounters come only from a map's random table (`RandomEncounters`, a uniform pick).

### Core changes

**New leaf folder `Core/Tactics`** (content records only; references nothing in Core):
- `EnemyAction`: one weighted choice in an enemy's tactics. `public sealed record`: `string? SkillId { get; init; }` (null is the basic attack), `required int Weight`, `TacticCondition? When { get; init; }`.
- `TacticCondition`: when an action may be chosen. `public sealed record TacticCondition(TacticConditionKind Kind, int Percent = 0)`.
- `TacticConditionKind`: the closed list. `OwnHpBelow`, `AllyHpBelow`, `AllyFallen` ("ally" is another enemy in the same battle).
- `EnemyPhase` (B2): the actions an enemy switches to below a share of its HP. `public sealed record`: `required int BelowHpPercent`, `required IReadOnlyList<EnemyAction> Tactics`.

**`Core/Battle`:**
- `EnemyDefinition` gains `IReadOnlyList<EnemyAction> Tactics { get; init; } = []` (T1), `bool IsBoss { get; init; }` (B1), `IReadOnlyList<EnemyPhase> Phases { get; init; } = []` (B2), and `bool NamesSkill(string skillId)` (T1: its tactics name the skill; B2 adds its phases'). A method, not a computed property, so the content JSON never sees it.
- `EnemyTactics(IRandom random, IReadOnlyDictionary<string, SkillDefinition> skills)`: chooses the command: the enemy's current actions (its phase's, else its own), the eligible ones, one weighted pick, then the targets from `EnemyTargeting`. With no tactics it attacks exactly as today.
- `EnemyTargeting` (new, internal): whom an enemy aims a chosen action at. The attack and single-target skills aimed at the party: a taunting member unless resisted, else a random living one (today's rule, moved); a skill that takes all: every candidate; a heal on one ally (T2): the lowest HP share; any other single target: random.
- `TacticConditions` (new, internal static, T2): whether a condition holds for an enemy now.
- `Combatant.Knows(SkillDefinition skill)` (T1): a member's `Skills` contain it; an enemy's definition names it (`Enemy.NamesSkill(skill.Id)`). `BattleState.CanUse` calls it.
- `Combatant.EnemyPhase` (B2): the phase reached, null before any; set only by `EnemyPhases`.
- `EnemyPhases` (new, internal static, B2): enters each standing enemy's deepest phase its HP has fallen below, reporting `EnemyPhaseEntered(Combatant Enemy, EnemyPhase Phase)` (new event).
- `TurnFlow`: `EnemyPhases.EnterReached(combatants, events)` right before each `OutcomeOf` (B2).
- `BattleState.CanFlee` (B1): the actor is a party member and no enemy in the battle is a boss. `Flee` throws when it's false.
- `Session/GameSession`: `new EnemyTactics(context.Random, context.Catalog.Skills)`.

**Data** (`Data/Battle`): `EnemyReferences(IReadOnlySet<string> DropTableIds, IReadOnlyDictionary<string, SkillDefinition> Skills)` replaces the `dropTableIds` parameter (as `ClassReferences` does for classes); `EnemyTacticsValidator` checks an enemy's actions (D1) and phases (B3).

**Game** (`Game/Battle`): `CommandMenu` greys Run when `!battle.CanFlee` (B1); `ActionPlayback.AddImpactCue` shows `EnemyPhaseEntered` as a cue on the enemy (B3); `BattlerArt` gains `SizeFactor` and `BattlerScaling.Of(sourceSize, sizeFactor)` scales by it (B4).

### Structure and folder dependencies

| From | To | Why | New? |
|---|---|---|---|
| Core/Battle | Core/Tactics | `EnemyDefinition` holds the actions and phases; `EnemyTactics` reads them | new |
| Core/Battle | Core/Skills | `EnemyTactics` resolves a skill id | exists |
| Core/Tactics | (none) | ids and numbers only, so no cycle with Battle | new |
| Core/Session | Core/Battle | builds `EnemyTactics` with the catalog's skills | exists |
| Data/Battle | Core/Tactics, Core/Skills | the validator checks actions against the skills | new (Tactics) |
| Game/Battle | Core/Battle | `CanFlee`, `EnemyPhaseEntered`, `Enemy.IsBoss` | exists |

No Content method is added (a method taking `ContentCatalog` lives in Content or Session, never in a folder Content references: loot retro).

### Key decisions
1. **Weighted actions with conditions, phases that replace them** (ADR needed: enemy tactics and boss phases). The developer's example; the ADR weighs it against a priority list (gambits) and per-enemy tactics classes.
2. **Ids in content, resolved by the tactics; `Combatant.Knows`.** The enemy's actions name skills by id (as a class's `skillIds` do, ADR 0020). `EnemyTactics` holds the catalog's skills and builds the `SkillCommand`; `BattleState.CanUse` asks `Combatant.Knows`, which asks the enemy's definition. Alternatives rejected: `Combatant.FromEnemy(enemy, skills)` (58 call sites, and `BattleState.Start` would take a fifth parameter); a Data DTO or a converter resolving ids at load (no precedent, and the shared read-only `ContentJsonOptions` can't hold a lookup).
3. **Draw only when uncertain.** The weighted pick goes through `Randomness/WeightedPick` (no draw when one eligible action holds the whole weight); conditions draw nothing; the target draws as the attack does today. An enemy without tactics never calls the pick.
4. **Phases are entered in `TurnFlow`, before the outcome.** One place sees every HP loss (hits, backfire, poison ticks, a stunned holder's ticks), and a boss that falls enters nothing (only standing enemies). The event comes after the action's own events, so the cue shows at the impact. Latched (Q7): `Combatant.EnemyPhase` only moves deeper.
5. **Phase cue through `AddImpactCue`.** `ActionPlayback.Read` is at the complexity limit (backlog, stats plan); `EnemyPhaseEntered` goes to the default branch's `AddImpactCue` switch, one arm, so `Read` keeps its cases.
6. **No escape is a Core query** (`CanFlee`), read by the menu; `Execute(FleeCommand)` throws without it, as every illegal command does (ADR 0004).
7. **The boss's size is the Game's** (`BattlerArt.SizeFactor` from `Enemy.IsBoss`): Core says "boss", the shell decides how big.

### Extension points
- A new condition: a `TacticConditionKind` member, its arm in `TacticConditions.Holds` (the default arm throws), its Data rule and a test (Q4).
- A battle event step (Q1) would set `ExplorationState.PendingEncounter` from a step; `GameSession.StartPendingBattle` needs no change.
- Phase effects beyond the action list (Q6) would hang off `EnemyPhases.EnterReached`.

### Replacement mapping
| Existing | Becomes |
|---|---|
| `EnemyTactics.ChooseCommand`'s target rule (taunt, resist roll, random member) | `EnemyTargeting.ForAttack` (moved, same draws), called by `EnemyTactics` |
| `BattleState.CanUse`'s `CurrentActor.Skills.Contains(skill)` | `CurrentActor.Knows(skill)` |
| `EnemyCatalogValidator.Validate(file, enemies, dropTableIds)` | `Validate(file, enemies, EnemyReferences)` |
| `new EnemyTactics(random)` (18 sites) | `new EnemyTactics(random, skills)`; tests pass an empty dictionary unless they use skills |

## Risks
- **Determinism:** an enemy with tactics draws once more per turn (the pick), so seeded tests that use such an enemy pin the pick's draw. No repository enemy has tactics before C1; the Chief is only met through the forest's table, and repository Game tests that walk the forest replace its table (`StartMapScenarios`), so none meets it. Check `ForestFightsSimulation` and `LootSimulation`: they name their encounters, so the new table entry doesn't move them.
- **The forest gets harder:** one encounter in three is the Chief, with no escape (Q1, Q9). A level 1 party may lose it; S1 shows by how much. Placeholder until the developer moves the boss behind an event step.
- **A fallen enemy revived by an enemy skill:** `TargetRules` allows `singleFallenAlly` for enemies, and the screen fades the fallen out. No placeholder skill revives; if the developer writes one, the hand check must look at its battler and tag coming back (not planned).
- **Overlap on screen:** a boss at twice the size in the middle slot may cover the goblin above it or reach the turn order strip; B4's hand check looks for it. Spots stay where they are; only the drawn size grows.
- **Coupling:** `EnemyTactics` now needs the skill catalog; it stays one per playthrough. `Combatant.Knows` walks the enemy's few actions on each `CanUse` (a handful per turn, never per frame).
- **File sizes:** `BattleState` 269 → about 277 (B1); `ActionPlayback` 252 → about 255 (B3); `CommandMenuTests` is 393, so B1's tests go in `CommandMenuRunTests`. `EnemyTacticsTests` (135) keeps the attack tests; T1 and T2 add `EnemySkillTacticsTests` and `EnemyTacticConditionTests`.
- **Save format:** unchanged. Phases and tactics are battle state and content; nothing new is saved.
