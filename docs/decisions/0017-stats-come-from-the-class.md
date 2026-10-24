# 0017. Stats come from the class

Status: accepted
Date: 2026-10-24

## Context
A class is "like D&D has class" (decision 164): it decides what a character can do. Everyone starts at level 1 with 0 XP, and the rest depends on the class (decision 168). Today every party member's definition holds its own level, experience, HP, MP, max HP and MP, attack and defense, and a level-up raises only the level: stats never grow.

Constraints:
- Formulas live in one static, pure place per domain, tested (CLAUDE.md).
- Saves keep only what play changes (ADR 0007), and derived values are recomputed on load "until stat growth exists". Old saves must still load.
- Balance numbers are the developer's: the code gives a place for them; content gives the values (question 180).

## Options
### A. The class at the member's level: base plus linear growth
`classes.json` gives each class its stats at level 1 (`base`) and what each level after the first adds (`growth`), for max HP, max MP, attack and defense. One formula, `stat(level) = base + growth × (level − 1)`, in `Party/ClassGrowth`. A member keeps only who they are: name, art, summary, skills.
- Pros: what the developer asked for; nine classes are nine rows of numbers; a level-up has an effect; saves don't change, since max HP and stats are still derived from content at the saved level.
- Cons: every member of a class has the same numbers; linear growth is simple, and a curve may be wanted later.

### B. Per member, as today, plus a per-member growth
- Pros: each character tuned by hand.
- Cons: the opposite of "think of a class like D&D"; nine characters' worth of numbers to keep balanced; the class stays a label.

### C. Class base plus a per-member modifier
- Pros: characters of one class can still differ.
- Cons: more numbers than placeholders need now. A modifier can be added to A later without changing saves.

## Decision
A (developer, 2026-10-24): a member's max HP, max MP, attack and defense are their class's at their level, `base + growth × (level − 1)`, through one formula in `Party/ClassGrowth`.

## Consequences
- `PartyMemberDefinition` loses level, experience, HP, MP and the stats; `ClassDefinition` gains `Base` and `Growth`; `PartyMember` computes its maxima and base stats from its class at its level (C1b), and a level-up raises them by the growth, adding the max HP and MP gained to current HP and MP (C1c).
- Saves are unchanged. An old save's members keep their saved level and get their class's stats at it, and `SaveGameRepair` clamps HP and MP to the class's maxima (ADR 0007, line "recomputed on load", now with growth).
- A curve, a per-member modifier, or skills by class are later extensions of the class record and the one formula.
- Balance: a lone level 1 leader must win the start area's fights (question 184); C1b checks that in simulation and records the numbers.
