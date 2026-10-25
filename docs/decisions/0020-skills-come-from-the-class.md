# 0020. Skills come from the class

Status: accepted
Date: 2026-10-24

## Context
Question 181 left skills on the member (`party-members.json` `skillIds`) "until a class-skills plan". This plan gives each class its own battle resource (ADR 0019), and skills spend, gain or require those resources. Today only Sarxian has skills, and they are the cleric's and the mage's spells.

Constraints:
- A class is like a D&D class: it decides what a character can do (decision 164).
- A skill must never refer to a resource its user's class lacks, and Data should catch that at load time, not in battle.
- Saves never held skills (ADR 0007), so this change doesn't touch saves either way.

## Options
### A. The class lists its skills
`classes.json` gains `skillIds`, and every member of the class has those skills in that order. `party-members.json` loses `skillIds`.
- Pros:
  - It matches decision 164: the class sets the stats (ADR 0017) and now the kit.
  - Data can check every skill against the class's resources, naming both the class and the skill.
  - Nine short lists, one per class.
- Cons:
  - Two characters of one class can't differ in skills.
  - Sarxian, the one member with skills today, has none until WL1 gives the warlock his.

### B. The member keeps `skillIds`, as today
- Pros: no change now, and each character can be tuned by hand.
- Cons:
  - A skill could name a resource its member's class lacks, so the check has to go through the member to the class.
  - The class stays a label for skills.

### C. Both: the class's skills plus a member's own
- Pros: shared kits plus a signature skill per character.
- Cons:
  - Nothing needs per-character skills yet.
  - C can be added to A later without changing saves.

## Decision
A (developer, 2026-10-24): a class lists its skills in `classes.json` `skillIds`, and every member of the class has them, in that order. This answers question 181.

## Consequences
- `ClassDefinition` gains `SkillIds`, which are empty when absent, and `PartyMemberDefinition` loses them (CS1). A member's skills are their class's, in the class's order.
- Data checks that every id in a class's `skillIds` is a known skill, listed once. From M2a it also checks that each resource the skill changes or requires is one of that class's resources, naming the class and the skill.
- Saves are unchanged.
- The class stays the main source of skills, but it won't be the only one. The developer expects a skill tree per class and skills granted by items (2026-10-24, not settled; question 228). So code that asks for a member's skills should go through one place (`PartyMember.Skills`), never read `Class.SkillIds` directly, so that more sources can join that list later. Learning skills by level and C's per-member extras would extend the same list.
