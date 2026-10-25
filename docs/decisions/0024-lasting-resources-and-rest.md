# 0024. Lasting resources and rest

Status: accepted
Date: 2026-10-25

## Context
Every class should need a rest (decision 198). The cleric's faith stays lowered until a rest (decision 197). The mage's starting mana drops as the mage tires (question 219). Today every resource is battle-only: `ResourceMeters` is built new each battle from `ResourceStarts`, and nothing reaches the next battle.

The questions are:
- where the lasting state lives between battles;
- how it reaches a battle and comes back;
- how saves keep it;
- what restores it.

Constraints:
- **Mechanisms in code, numbers in content** (ADR 0019): code never names a class or a resource.
- **Rules in Core** (ADR 0001).
- **Saves refuse older formats** until the first release (ADR 0022).

## Options
### A. A lasting resource keeps its amount on the member (recommended)
A resource marked `isLasting` keeps its amount on the `PartyMember` between battles:
- A battle starts from the member's amount and writes it back when the battle ends.
- A new `perBattleFought` gain applies when a battle ends.
- An optional `strain` rule lets a lasting resource lower another resource's start. For example, each missing Clarity point lowers starting mana by 1.
- A rest refills each lasting resource to its starting amount.

Faith simply becomes lasting, and the mage gets a lasting Clarity (max 3, −1 per battle fought).
- Pros:
  - One meter model: same triggers, same panel rows, same validation. The other classes' lasting costs (question 220) are content, plus a trigger only if one is new.
- Cons:
  - `PartyMember` gains a mutable dictionary.
  - The battle end must write back. `GameSession.FinishBattle` already applies the outcome, so it is one more step there.

### B. A separate strain counter per member, outside resources
- Pros: the battle meters stay battle-only.
- Cons:
  - A second model for the same idea, with its own triggers, save field, validation and display.
  - Faith, which is already a resource, would have to be duplicated or moved.

### C. Lasting statuses that survive the battle
- Pros: reuses the status list.
- Cons:
  - Statuses are battle-only by ADR 0021, and they count turns, not battles.
  - Faith is an amount that skills read, not a status with a power.

## Decision
A (developer, 2026-10-25): a resource marked `isLasting` keeps its amount on the member between battles, gains `perBattleFought` when a battle ends, may lower another resource's start through a `strain` rule, is saved by id, and is refilled by a rest. A rest restores full HP, revives the fallen and refills every lasting resource, from the inn and the rest item alike (decision 230).

## Consequences
With A:
- **Saves** gain `lastingResources` per member: resource id → amount. The format goes from 2 to 3, and older saves are refused (ADR 0022). `SaveGameRepair` drops unknown ids and clamps each amount to 0..max.
- **Rest.** An inn step and a rest item both call one Core method, `PartyMember.Rest()`. It restores full HP, revives the fallen and refills the lasting resources (decision 230).
- **Only lasting resources carry over** (question 223). Every other resource starts each battle afresh.
- **Validation.** A strain names another resource of the same class, and its source must be lasting.
- **Watch:** a lasting resource that a battle drains to 0 must not leave a member unable to fight. Faith at 0 only greys the cleric's skills, and missing Clarity floors mana's start at 0, never below.
