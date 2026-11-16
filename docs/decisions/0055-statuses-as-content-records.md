# 0055. Statuses as content records

Status: accepted
Date: 2026-11-15

Amends ADR 0021: a status is no longer a closed Core kind with its numbers on each skill's grant.

## Context
ADR 0021 made a status one of six closed kinds (`StatusKind`: Stun, Taunt, Guarded, Exposed, Poison, Regen), its
number carried by each skill's grant (`StatusGrant(kind, power, turns)`). The developer wants statuses designed in the
editor as data, one record per status (decision 676: "one per status"), tiered like the damage records (`poison_1`,
`poison_2`, decision 705), and new behaviors without new code each time (Weakened, Slowed; Burning hurting as fire).
Constraints: the battle never reads a catalog at run time (decision 789); every number stays content; today's numbers
must survive the conversion; no save format change, since statuses end with the battle.

## Options
### A. Records built from a closed list of parts (chosen)
`statuses.json` holds records (`id`, `name`, `description`, `isHarmful`, an optional `family`, `parts`), each part one
of a closed Core list (`damageEachTurn`, `healEachTurn`, `skipTurn`, `drawAttacks`, `resistanceChange`,
`damageTakenChange`, later `damageDealtChange` and `statChange`) with an amount.
- Pros: a new status is content; a new behavior is one part kind with its arm in each switch; the editor (row 7)
  edits records with forms; a grant names a record by id, resolved at load like a tiered effect.
- Cons: a part kind is still code; the parts' rules (one ticking part, each kind once, one family's records alike)
  must be checked by Data.

### B. Today's closed kinds with per-skill numbers
- Pros: no conversion.
- Cons: no record to design or tier; every new behavior is a Core kind and a switch arm everywhere; refused by the
  developer's "one per status".

### C. Instant effects re-run each turn
A status as a list of the effects a skill has, replayed every turn.
- Cons: a status's modifiers (Guard, Exposed) aren't effects; refused (decision 677).

## Decision
Option A (decisions 795-812, 816, 819). A status is a record in `statuses.json` built from the closed part list, one
amount each whose meaning its kind gives. A holder keeps one status per family (its id when it has none): a grant of
a held family refreshes it with the new grant's turns, each part's amount the one farther from zero (ADR 0021's "the
stronger"). A grant is `{ "status": id, "turns", "chance" }`, the chance a percent, 100 when left out; every grant
rolls it, Intelligence raising every part's amount and every chance, crowd control resistance cutting only a status
that skips a turn. A status acts at the start or the end of its holder's turn (end by default); its countdown stays at
the end (ADR 0021). The battle reads records resolved at load, never the catalog; the landed status is the record with
its amounts raised (`StatusDefinition.Strengthened`).

## Consequences
- `StatusKind` goes once grants name records (the plan's SW slice); `skills.json`'s effects list their grants in a
  `statuses` list.
- The game reads a status's name, mark letter and harm from its record; the six `status_*` text ids leave `ui.json`.
- A ticking part may have a damage type and then meets the holder's resistance, never a hit's other rules.
- A status's numbers stay flat on its record, scaled by its granter's Intelligence; a stat-scaled tick is a named
  extension (decision 816).
- No save format change: statuses end with the battle.
