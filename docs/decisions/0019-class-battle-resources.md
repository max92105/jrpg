# 0019. Class battle resources

Status: accepted
Date: 2026-10-24

## Context
Every class gets its own battle resource, and that resource sets how the class plays (decisions 188-206). Mana grows every turn. Ferocity builds when the barbarian is hit, and conviction when an enemy falls. Faith is a level that skills require. Instability adds a chance to backfire. Growth and Decay compete for dominance. MP goes away (decision 197).

Constraints:
- The developer will keep changing the rules while play-testing. Every number is a placeholder (decision 196).
- No content in C# (CLAUDE.md), so code never names a class or a resource id.
- The rules live in Core and are deterministic (ADR 0001). Battle events come out of one `Execute` call (ADR 0004), and the HUD reads live state (ADR 0013).

## Options
### A. Mechanisms in code, numbers in content
Each class lists its resources in `classes.json`, with a max, a starting amount and gains. A gain is attached to one trigger from a closed set: per turn, per hit taken, per hit landed, per heavy hit, per enemy fallen and per ally fallen (per battle fought arrives with ADR 0024). Skills spend, gain or require a resource. Optional fields switch on the special rules (backfire, dominance, max from speed, strain), and each rule is a small record with its own pure formula.
- Pros:
  - Seven of the nine resources are the same meter on the same few triggers with different numbers, so one model covers them.
  - Changing a trigger amount while play-testing is a JSON edit.
  - The Data validator can check a class's resources.
- Cons:
  - An unusual rule needs an optional field and a formula, not a free-form class.
  - A new trigger is a code change.

### B. A shared meter plus per-class rule hooks
There is one meter type, plus one small rule class per class, picked by a content id. Each rule class reacts to battle hooks: turn ended, hit taken, combatant fell.
- Pros:
  - Each class's rules read in one file.
  - Odd rules such as backfire and sticky dominance fit naturally.
- Cons:
  - Most of the nine rule classes would repeat each other.
  - Every tuning change is code.
  - Content would name a C# type.

### C. Scripted triggers in content
- Pros: anything can be expressed without a build.
- Cons:
  - It needs a script language and an interpreter, which are speculative for nine resources.
  - It's harder to validate and to test deterministically.

## Decision
A (developer, 2026-10-24): mechanisms in code, numbers in content. B's one strength, odd rules, is kept in A by small rule records with their own formulas.

## Consequences
- **New code:** a `Core/Resources` folder with `ResourceDefinition`, `ResourceGains`, `ResourceChange`, `ResourceRequirement` and `ResourceMeters`. It references neither Battle, Party nor Skills. `ClassDefinition` gains `Resources`.
- **A new trigger** is a `ResourceGains` field, plus one call in `ResourceTriggers`, plus a test.
- **Per-turn gains** happen at the end of the holder's own turn, once per round in today's turn order. The battle's constructor raises no events.
- **Lifetime:** resources live in `ResourceMeters` on the `Combatant`, which is built new for every battle. Lasting resources (ADR 0024) are the exception.
- **Events:** `ResourceChanged(Combatant Holder, string ResourceId, int Change)` reports every change. The HUD reads the live amounts and plays no beat.
- **To watch:** `BattleState` must not grow a branch per rule. Payment, triggers and upkeep go to their own types.
