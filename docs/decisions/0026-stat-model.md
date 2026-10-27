# 0026. The stat model

Status: accepted
Date: 2026-10-27

## Context
Members have four numbers today (max HP, attack, defense, speed), from their class at their level (ADR 0017), plus what a
weapon or armor adds. The developer wants enough interesting stats for ARPG-style gear (the next plan: rarity, rolled
affixes, more slots), and wants each stat to be a dimension a build can lean on (decision 296). Constraints: one formula
place per domain, numbers in content (ADR 0019), deterministic seeded battles whose test fixtures keep their sequences,
and a Status page that can show every number (decision 316).

## Options
### A. Primaries, percent-per-point conversions, rates in `rules.json`, limits in code, percents in tenths
Five primaries (Power, Agility, Intelligence, Constitution, Will) from class growth and gear; derived stats convert at a
fixed percent per point with no level divisor, applied to bases that grow (class HP, skill and weapon power).
- Pros: a point is worth the same at every level, so a fixed-stat item keeps meaning something for many levels; every
  primary is a dimension (Power scales hits and healing, Agility crit and dodge, Intelligence statuses, Constitution HP
  and physical resistances, Will the class resource and magical resistances); every rate is tunable in content.
- Cons: members are built with the rules (max HP depends on Constitution); more numbers to show and balance.

### B. Derived stats straight on the class, no primaries
Today's model, widened (crit, dodge, resistances on the class and on gear).
- Pros: fewer numbers; nothing to convert.
- Cons: the dimensions disappear, and gear can't shape a build: a ring adds a number, not a direction.

### C. Ratings divided by level
MMO-style: crit chance = rating ÷ (k × level).
- Pros: keeps old gear from staying strong.
- Cons: gear must be replaced as levels rise, which suits rolled loot but not fixed-stat items; a rating is hard to read
  on the Status page.

### D. Conversion rates as code constants
- Pros: nothing to thread into members.
- Cons: every tuning is a rebuild, against ADR 0019.

## Decision
A. A primary is the class's base + growth × (level − 1), ADR 0017's one formula, plus worn gear, with no free points
(decision 293). Derived stats (the plan's design.md, "The stat sheet") convert at a percent per point; rates and bases
are `rules.json` `stats`, loaded into `Stats/StatRules`. Speed and accuracy are their own stats from class and gear
(decisions 295, 297). Enemies carry derived stats only, crowd control resistance included (decisions 299, 324).

Also decided:
- **Limits in code.** The battle's limits (hit floor and ceiling, dodge cap, resistance cap and floor, minimum damage,
  base crit damage 150 %, decision 300) are named constants beside their formulas: members and enemies share them, and
  the battle takes no rules. Moving one to content later is a field and a parameter.
- **Tenths of a percent.** Content gives whole percents; Core holds every derived percent in permille
  (`CritChancePermille` 90 is 9 %) and screens show it rounded down to a whole percent, so a small bonus (+5 Agility,
  +1.25 % crit) moves a stored number instead of vanishing in rounding.
- **Who scales what** (decision 296): skills and attacks scale with their user; consumables land their own numbers
  (decision 321). Intelligence never adds turns to Stun (decision 317: `TurnFlow` skips a turn per Stun turn left, so
  extra turns would stun-lock). Every class has mid-to-high Power (decision 318).
- **Rolls draw only when the outcome is uncertain**: a chance of 0 or 100 % calls no random, so certain test fixtures
  keep their seeded sequences.
- **The sheet is never saved.** A member is built with the rules (`PartyMember.Recruit(…, StatRules)`); its sheet is
  rebuilt on level-up and on equipment changes.

## Consequences
- A new stat is a `StatKind` member, a sheet field, a formula and its rate; the Status page lists it.
- The loot plan's affixes are `StatBonuses` with rolled numbers.
- Every member construction, in code and tests, takes the rules; test fixtures use neutral rules (every rate 0, base
  accuracy 100, base crit 0) so battles stay certain.
- Watch: percents that only grow (Power) need the simulation's numbers recorded per slice; balance is a later pass.
