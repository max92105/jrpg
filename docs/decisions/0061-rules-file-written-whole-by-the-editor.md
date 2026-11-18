# 0061. `rules.json` written whole by the editor

Status: Accepted (default taken while the developer was away; his to overturn)
Date: 2026-11-17

## Context
The classes and progression plan (`docs/plans/2026-11-15-classes-and-progression/`) gives the experience table a page
of its own in its phase XP (the Experience page, XP2-XP4). The table is `rules.json` `experience`, one of seven
sections beside party, exploration, stats, loot, crafting and trade, and the file is laid out by hand today (about 40
lines). For the page to save, the editor has to write that file, and ADR 0011 lets it write content only through
Data's `ContentWriter`.

Constraints:
- One way to write content (ADR 0011): `ContentWriter`, the loader's validators first, the loader's JSON options.
- No hand edit lost silently: the sections no page edits yet stay the developer's to edit by hand.
- A file the editor takes over is relaid once through the writer, with a test that what loads before and after is the
  same (ADR 0045).
- No save format change (decision 943); nothing random, no wall clock.

The developer delegated the plan's approval, so this choice was taken while he was away; it is item 3 of the plan's
"Defaults to confirm".

## Options
The plan's design.md, section 4, holds the options and their costs.

### A. The whole file through the writer (chosen)
`GameRules` joins the writer's `SerializedCatalogs`; `rules.json` is relaid once in the writer's layout, one field per
line (about 40 lines become about 110).
- Pros: one write path, the catalogs' own. Safe because of the stamps (decision 559): the file is written only when the
  draft's rules differ from the bytes stamped at load, so a session that never touches the table never rewrites it;
  when one does, a hand edit made on disk since the load makes Save all refuse with the file named
  (`CatalogFileStamps.IsEditedOnDisk`), so no hand edit is lost silently. The content rule already asks every content
  file for the writer's layout. Every later rules page costs only its boxes.
- Cons: the relay's layout diff; the round trip must hold for every section, not only the table (design.md, risks 1
  and 2: the rarity-keyed dictionaries of `loot` and `crafting` may write an enum key as `Common`, and
  `exploration.walkSpeed` 3.0 writes as `3`). One slice (XP1, about 140 lines plus the relaid file; about 40 more if
  the rarity keys need their converter).

### B. The table in its own file, `experience.json`
- Pros: `rules.json` keeps its hand layout; the editor writes a file holding only what it edits.
- Cons: a content split (the loader, the test world, `GameRules` built from two files), and the same question again
  for the next rules page. About one and a half slices.

### C. A patch of the `experience` node, the rest of the text kept
- Pros: no layout change; the other sections keep their hand layout.
- Cons: a second write path beside the writer's, outside the stamps' compare-the-bytes rule. Two ways to write content
  is what ADR 0011 avoids, so refused.

## Decision
Option A (decisions 943 and 955; options B and C set aside in design.md, section 4).
- **The whole file (955, default taken while the developer was away; his to overturn).** `rules.json` is
  editor-written whole: `GameRules` joins `SerializedCatalogs`, every section in the writer's layout and in `GameRules`'
  declaration order (party, experience, exploration, stats, loot, crafting, trade). XP1 relays the file once, with a
  test that the rules loaded before and after are the same; its check joins the writer's catalog check
  (`CatalogProblems.OfRules` and `CheckRules`, the `CatalogProblems.Rules` partial).
- **Why it is safe (559).** A draft whose rules are unchanged never rewrites the file; a hand edit made while the editor
  holds a changed table makes Save all refuse, naming the file (`CatalogFileStamps.IsEditedOnDisk`).
- **What this plan edits (955).** Only the experience table, through the Experience page as one
  `CatalogDraft.SetExperienceTable` step (XP2-XP4). The other six sections are written back as loaded and stay edited
  by hand, in the writer's layout, until a later page takes them.
- **Saves (943).** No change.

## Consequences
- **Save format:** no change, `formatVersion` stays (decision 943). Saves keep a member's level and experience, never
  the table, which is read at load; a lowered max level lowers saved members at their next load
  (`SaveGameRepair.RepairLevel`), and the Experience page says so while its max is below the saved one (XP2).
- **Easier:** a later rules page (row 9's sale share in `trade`, party size, stat conversions, walking, loot rarities,
  crafting) costs only its boxes: the file is already the writer's.
- **Harder:** one relay commit with a layout-only diff (XP1, "relay" in its message); a hand edit to `rules.json` while
  the editor holds a changed table is refused at Save all, so the developer reloads or saves before editing by hand.
- **Watch for:** every section's round trip. A rarity key is fixed with a `Rarity` property-name converter, never
  `DictionaryKeyPolicy`, which would rename every string key (design.md, risk 1); a field added to any rules section
  is covered by the repository test that the writer writes the file back unchanged.
- If the developer overturns the choice: option B moves the table to `experience.json` and changes XP1 (the loader,
  the test world and the writer's list), while XP2-XP4 stay, since they edit the draft's table, not the file.
- Applies ADR 0045's takeover rule (relaid once, then the identical-bytes test) to `rules.json`; amends no earlier ADR.
