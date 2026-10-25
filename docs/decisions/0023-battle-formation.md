# 0023. The battle formation

Status: accepted
Date: 2026-10-24

## Context
The paladin's aura reaches the allies within a number of places of the paladin (decision 201). The developer's own words: "if the paladin is in the middle of the party and has an aura with 1 range, it affects him and the allies immediately beside him. Some will have like 2 left, 2 right." The paladin has to move around the formation to cover who needs it, so the party must be able to change places in battle.

Two questions follow: what a position in battle is, and how it changes.

Constraints:
- **Rules in Core:** positions are a battle rule, so they live in Core (ADR 0001), and every change comes out of one `Execute` call as events (ADR 0004).
- **Existing order:** the party page already sets an active order. The battle screen lists party panels in that order and places battlers by it.
- **Minimal:** no shape beyond what ranges need yet. Enemies have no formation (question 224).

## Options
### A. The active party order is a line
`BattleState` keeps the party's line as a `Formation`, starting from the active order. The distance between two members is the difference in their places. A Move command swaps the actor with a neighbor.
- Pros:
  - It is what the developer described ("2 left, 2 right").
  - It reuses the order the party page already sets.
  - A range is one subtraction.
- Cons:
  - A line has no depth, so there's no front or back row. Rows could come later as another idea.

### B. Front and back rows, as in Octopath
- Pros: a familiar shape that also explains who gets hit.
- Cons:
  - It adds a second concept that ranges along a line don't need.
  - The developer described a line, not rows.

### C. Free positions on a grid
- Pros: the most freedom.
- Cons:
  - Speculative: nothing needs two dimensions.
  - Every range and every Move becomes harder to explain and to test.

## Decision
A (developer, 2026-10-25): the active party order is a line in battle, kept by `BattleState` as a `Formation`; distance is the difference in places, and Move swaps the actor with a neighbor.

## Consequences
- **Battle state:** the formation is battle state. `Combatants` stays the fixed roster, so events and the HUD's lists keep their order, and positions are read from `Formation`.
- **Fallen allies:** a fallen ally keeps their place and can be swapped with, so a body never blocks the line.
- **Placeholders (question 211):** a Move costs the mover's turn. The ally swapped with loses nothing. The line doesn't change the party order after the battle.
- **Events:** `PlacesSwapped(Combatant Mover, Combatant Other)`. The battle screen places battlers by their place in the formation (FM2), and LK3 slides them.
- **Enemies** have no formation (question 224).
