# 0034. Enemy tactics and boss phases

Status: accepted
Date: 2026-11-01

## Context
Every enemy only attacks (`Battle/EnemyTactics`: a random living member, or a taunting one; question 224). Battles need
enemies that use the party's kind of skills, and a boss whose behavior changes as it is hurt, all written in content.
The developer answered the plan's questions on 2026-10-29 (decisions 402-405).

## Options
### A. A weighted list of actions with conditions from a closed set, and phases that replace the list
- Pros: one list per enemy in `enemies.json`; a change of kit is content only; the conditions are a small closed list
  with tests, as ADR 0019's mechanisms are. Precedent: RPG Maker's action patterns (skill, condition, rating), which pick
  among the actions whose condition holds by rating.
- Cons: one more draw on a turn with a real choice; a new kind of condition is code.

### B. An ordered priority list, the first matching rule wins (Final Fantasy XII's gambits)
- Pros: predictable, no weights to tune.
- Cons: an enemy with no condition left to tell its rules apart does the same thing every turn; variety needs more rules.

### C. One tactics class per enemy kind, named by content (ADR 0019's option B)
- Pros: free-form behavior.
- Cons: every change is code, and content names a C# type.

## Decision
A.
- **An action** is a skill id, or none for the basic attack, with a weight and at most one condition: own HP below a
  percent, a living ally's HP below a percent, or an ally fallen (decision 402). An ally is another enemy in the same
  battle, never the enemy itself; "below" is strict.
- **The pick** is one weighted draw among the eligible actions, through the battle's `IRandom`, and only when the outcome
  is uncertain (ADR 0026). An action is eligible when its condition holds and `BattleState.CanUse` allows its skill; with
  nothing eligible the enemy attacks (decision 403). An enemy without tactics draws exactly what it drew before.
- **A phase** replaces the action list only (decision 404), below a share of the boss's HP, and is latched: healing above
  the threshold doesn't leave it (decision 405).
- **Phases are entered at one place**, the end of `BattleState.Execute`, while the battle isn't over. By then every HP
  loss of the call has happened (hits, backfire, poison ticks, the stunned loop's ticks, a kept turn), so one call sees
  them all and `TurnFlow` stays as it is. A battle the action ended enters nothing; a fallen boss enters nothing.

## Consequences
- An enemy's skill lands through the same `Execute` path as a member's (ADR 0004); `EnemyTactics` stays outside
  `BattleState`.
- A seeded test using an enemy with tactics pins one more draw whenever more than one action is eligible; enemies without
  tactics keep every seeded result.
- A new condition is a new kind, its arm, its Data rule and a test. Cooldowns, "once per battle" actions and phases that
  change stats, resistances or art are later work.
