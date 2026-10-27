# 0027. Damage types and resistances

Status: accepted
Date: 2026-10-27

## Context
Today a hit's damage is its power less the target's defense (the placeholder `DamageFormula`, plan 2026-10-17), with a
floor of 1. The stat model (ADR 0026) gives Constitution and Will a resistance role, and the developer wants gear that
adds "+10 % Fire" and enemies with weaknesses (decisions 292, 299). Resistances must be numbers the Status page shows
and gear compares, and must keep seeded battles deterministic.

## Options
### A. A closed list of eight types in Core; one type per hit from content; percent resistances replacing defense
- Pros: one mitigation to balance; a resistance is a readable percent on the Status page and on gear; a weakness is
  just a resistance below 0; Constitution and Will each raise a named group.
- Cons: every hit in content must name a type; defense and attack leave every test fixture (M2a, M2b).

### B. Types as content (`damageTypes.json`, resistances keyed by id)
- Pros: open-ended.
- Cons: Constitution's physical group and Will's magical group are rules, so code would still name the types.

### C. Defense stays, types add weakness tiers (weak, normal, resist, immune as multipliers)
- Pros: simple to read, as Octopath's weakness break or Pokémon.
- Cons: a tier can't be a gear stat that adds 5 %.

### D. Defense stays and subtracts, resistances on top
- Pros: nothing removed.
- Cons: two mitigations to balance; a flat defense makes small hits vanish and big ones barely move.

### E. Diminishing returns (`armor / (armor + K)`)
- Pros: no cap needed.
- Cons: a rating isn't a percent the player can read or compare on gear, and a weakness (below 0) has no natural place
  in the curve.

## Decision
A. `Stats/DamageType` is Blunt, Piercing and Slashing (physical) and Fire, Ice, Lightning, Light and Dark (magical).
Every hit has exactly one type, from content: a skill's or item's damage effect names it; a basic attack takes its
weapon's, the unarmed type from `rules.json`, or the enemy's `damageType` (decision 292). Poison ticks and backfire are
not hits and meet no resistance.

`Stats/Resistances` holds one percent per type, kept within −100..75 %: a weakness at most doubles a hit, and nothing is
immune. The 75 % cap matches Path of Exile's default maximum resistance. A member's resistances come from Constitution
(physical), Will (magical) and gear; an enemy's from content, absent meaning 0; a weakness is a resistance below 0
(decision 299).

Damage is `max(1, power × crit × (1000 − resistance) × (1000 + exposed) ÷ 1000³)`, every percent in permille (ADR 0026).
**One rounding rule:** the product is computed in a `long` and rounded down once, at the end; no intermediate step
rounds. A test stacks power 999, a crit, Exposed and a −100 % weakness to prove it doesn't overflow (H2).

Feedback (decision 323): a hit a negative resistance raised reads "Weak"; one a resistance at or above 25 %
(`DamageFormula.ResistedThreshold`) lowered reads "Resisted". Defense and attack leave the game (M2b); Guarded raises all
eight resistances instead (decision 331).

## Consequences
- A ninth type is an enum member, a resistance field, a `StatKind`, a name in `ui.json` and a choice of group.
- Content gains a `damageType` on every damage effect, weapon and enemy; the validator refuses a damage effect without
  one.
- Until M2b, defense still subtracts beside resistances (D2): two mitigations for a few slices, retuned in M2b.
- Watch: a −100 % weakness doubles a hit, so content weaknesses below −50 % need the simulation's numbers.
