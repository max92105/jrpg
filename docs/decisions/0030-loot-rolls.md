# 0030. Loot rolls

Status: accepted
Date: 2026-10-28

## Context
Gear is owned and worn as pieces with a rarity and rolled affixes (ADR 0029). The loot plan makes defeated enemies drop
items, and dropped gear roll a rarity and affixes, as in an action RPG (decision 338). Numbers belong in content
(ADR 0019) and rolls draw only when the outcome is uncertain, so seeded tests keep their sequences (ADR 0026). The
battle's draws are pinned by many seeded tests, and `BattleState` is near its size limit. The developer chose five
tiers (decision 339): Common, Magic and Rare roll; Unique is hand-made and later; Material is the materials' color.
Rarity gives the affix count only (decision 340), and only dropped gear rolls (decision 344).

## Options
### A. A closed rarity enum with numbers in rules; an affix pool and drop tables in content; loot rolls after the battle
- Pros: the screens and the save name each rarity with a compiler behind them; every balance number is content; no
  battle draw changes, and `Battle` stays unaware of loot.
- Cons: a new rarity is a code change (an enum member, a color, a name).

### B. Rarities as content (a list with names and colors)
- Pros: open-ended.
- Cons: the screens' colors and names, and the save shape, name each rarity anyway, as ADR 0027 found for damage types.

### C. Affixes in code (an enum of affixes with ranges as constants)
- Pros: type-checked.
- Cons: every balance change is a rebuild, against ADR 0019.

### D. Loot rolled inside `BattleState`, on each enemy's fall
- Pros: drops could show mid-battle.
- Cons: battle draws would shift every seeded battle test, and `BattleState` grows a loot rule.

## Decision
A.
- `Items/Rarity` is a closed list (decision 339). `rules.json` `loot.rarities` gives each rolled tier (Common, Magic,
  Rare) a weight and an affix count range (Rare 2-3, decision 340). Unique is reserved and never rolls.
- `affixes.json` is the affix pool: an id, a stat kind, a whole-number range, the equipment kinds it rolls on and a
  weight. A piece never rolls one affix twice; a pool too small for the count gives what it has (decision 359).
- `drop-tables.json` holds the drop tables. An enemy names one (`dropTableId`), and several enemies may share it. Each
  entry rolls on its own, with a chance and a quantity range (decision 343).
- Loot rolls in `GameSession.FinishBattle`, after the experience, with the session's `IRandom`. Chances go through
  `ChanceRolls` and weighted choices through `WeightedPick`, both drawing only when uncertain. The drops return beside
  the rewards (`Session/BattleSpoils`), so `Battle` knows nothing of loot.
- Only dropped gear rolls (decision 344): starting items and map gifts stay plain.
- Lifesteal, armor penetration, block and drop luck are stat kinds that only affixes give (decision 294, the affix
  stats plan); drop luck raises drop chances and the weights above Common (decision 349).

## Consequences
- No battle's draws change; a seeded run that fights and then walks into the next encounter sees its later draws
  shift once an enemy has a table.
- A new rarity is an enum member, a rules row, a color and a name.
- A new affix on an existing stat is one content line.
