# 0004. How a battle turn is driven and reported

Status: accepted
Date: 2026-10-17

## Context
Slice 5b builds `Battle/BattleState`: whose turn it is, applying commands (attack, defend, flee; skills and items in slice 6), and victory, defeat or escape. The same API serves three callers: the MonoGame battle screen (part 3), which animates each action before showing the next turn; unit tests; and headless simulations running thousands of seeded battles.

The legacy demo let the UI drive every step: `ExecuteCurrentTurnAction(CombatAction, targets, object param)`, then the UI called `AdvanceTurn()`, then `IsBattleOver()`, a query that also changed the state. The UI picked enemy targets with `new Random()`. Forgetting one call, or calling them in the wrong order, left the battle stuck or let a dead unit act.

Constraints: rules live in Core, never in the Game (CLAUDE.md); Core prefers returning results over side effects so the renderer can animate them (core.md); randomness only through `IRandom`.

## Options
### A. One `Execute` call per turn, returning what happened
`TurnResult Execute(BattleCommand command)` checks the command is legal for the current actor, applies it, moves to the next living combatant, and updates `Phase` (`PartyTurn`, `EnemyTurn`, `Victory`, `Defeat`, `Escaped`). `TurnResult(Combatant Actor, IReadOnlyList<EffectOutcome> Outcomes)` tells the renderer who acted and what each effect actually did. Enemy turns go through the same call: `EnemyTactics.ChooseCommand(battle)` returns a command, the caller passes it to `Execute`.
- Pros: a battle can't be left half-advanced; the only way to change it is a legal command. Tests and simulations are a simple loop. The renderer animates `TurnResult`, then reads `Phase` and `CurrentActor` for the next turn. One code path for both sides.
- Cons: the renderer must keep the `TurnResult` while it animates, because `CurrentActor` has already moved on. Effects that happen between turns (poison ticks, counters) would need new outcome kinds later.

### B. Separate apply and advance steps, driven by the UI (legacy shape)
`Apply(command)` returns outcomes; the UI calls `EndTurn()` once its animation finishes.
- Pros: the renderer controls timing exactly; `CurrentActor` stays the acting combatant during the animation.
- Cons: Core trusts the UI to call steps in order, the way legacy went wrong. Every caller (tests, simulations) repeats the sequence. Battle state can be observed half-done.

### C. Execute returns a stream of battle events
Like A, but `Execute` returns ordered events (`ActionStarted`, `DamageDealt`, `CombatantDefeated`, `TurnStarted`, `BattleWon`…) instead of one `TurnResult`.
- Pros: most expressive for animation; between-turn effects fit naturally.
- Cons: many event types before there's a renderer to need them; speculative for a slice whose only mechanics are attack, defend and flee.

## Decision
**C, kept minimal** (developer, 2026-10-17), with A's driving rule: one `IReadOnlyList<BattleEvent> Execute(BattleCommand command)` per turn, and `EnemyTactics` outside `BattleState` choosing enemy commands. C is the shape a JRPG battle grows into, and starting there avoids changing the renderer's contract later. Only events that current mechanics produce exist:
- `ActionStarted(Combatant Actor, BattleCommand Command)`
- `EffectApplied(Combatant Target, EffectKind Kind, int Amount)`: the amount actually applied.
- `CombatantDefeated(Combatant Combatant)`
- `BattleEnded(BattlePhase Outcome)`

New event types are added with the mechanic that needs them (status ticks, counters), never ahead of it.

## Consequences
- Events come in the order they happen: an attack that fells the last enemy gives `ActionStarted`, `EffectApplied`, `CombatantDefeated`, `BattleEnded`. Defend gives only `ActionStarted`; a successful flee gives `ActionStarted`, `BattleEnded(Escaped)`.
- `Execute` throws `InvalidOperationException` once the battle is over or for a command the current actor can't give (e.g. fleeing on an enemy turn), and `ArgumentException` for an illegal target; the UI only offers legal choices.
- A test or simulation runs a battle as: while not over, choose a command (player policy or `EnemyTactics`), `Execute` it. Tests assert on the event list.
- The battle screen plays the events back, then reads `Phase` and `CurrentActor`; it never advances turns itself.
