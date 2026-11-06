---
name: stale-size-comments
description: Comments and docs that keep an old rule, name, number or owner after a change the code-quality grep rule doesn't list (second jobs, planned names in later slices, a parent's arithmetic, a removed member's noun)
metadata:
  type: feedback
---

The code-quality rule already says to grep the old value, wording or file name after a resize, a move, a conditional
call or a rename. These cases slipped past it:

- A type gaining a second job kept a summary naming only the first (Forge M2/M3a: `GameRules` without crafting,
  `GearRoll` "Rolls a dropped base item" after `AddAffix`; M3b: the Forge's `gearRoll` param "so an upgrade draws" once
  reroll used it too). Now a code-quality line; still reread the class summary of every type that gains a public member.
- A Done note that pulls work forward or renames a planned API (loot S1a: FromSaved -> PieceFrom) leaves the later slice
  files and design.md citing the old name; grep the next slices and design.md for the planned name. Same after a test
  split (C2): a pending slice still cited "BattleStateTests (lines 225-354)".
- A renamed id (M1b: StatAttack -> StatWeaponDamage) leaves test names and locals with the old word; grep it in the tests.
  Same when tests switch their data source (mana-seed T2a: content -> test-world left `EveryRepositoryTile`,
  `RepositoryTree`, `RepositoryIntroSkipped`): grep test names for the old source's word, not only comments. A shared
  fixture switching source (T2c: `SaveGameFixtures.Catalog` -> test-world) leaves its users' unchanged summaries saying
  "against the repository content": grep the fixture's users, not only the diff.
- A record gaining a field (M2b-2: StatSheet.AttackDamage) is missed by its owner's "X, Y and Z" summary; a mechanic no
  longer read (M2b-1) turns "while the battle uses it" docs false; grep "while ... uses" and the slice id.
- A responsibility moved to a new type (C1) leaves a neighbour's param doc naming the old owner; C2 left the backlog with
  `EquipmentPage.Apply` after it moved: grep `<OldOwner>.<MovedMember>` across docs/.
- A removed member leaves its noun in the type's own param docs (G1b-2: "the take hint and the refusal").
- A child's spacing change (ST1a) leaves the parent's reserved-height arithmetic on the old rows; grep the parent for the
  child's old numbers. Sizes and file counts a doc states (C3: "split in two files" with four present) need rechecking.
- A docs/map.md entry edited by inserting a new parenthetical keeps the old one beside it (gold S2: `AffixDefinition`
  with two field lists); verify.ps1's two-summaries check only reads code, so read every changed map.md line whole. E1 (gold)
  added `EventScreenTransitions` to the Flow line but left the deleted `ForgeTransitions` in its key types.
- A part that starts writing into its host's list (sound SH3: ShopCounter asking coins into ShopScene's AskedSounds) leaves the
  host's `Sounds` summary listing only the host's own asks; reread the host's summary.
- A check moved out of one layer (mana-seed X2: the writer's art check now only at game startup) leaves comments
  elsewhere saying where it refuses ("refuses it only on save" in TileImages) and summaries describing the old fallback
  (`Select`: "nothing when the sheet isn't in the box"); grep the check's name and "on save" across the feature folders.

- A removed question or refusal (map flows MS1: switching no longer asks) leaves callers' summaries and window
  comments naming it ("or the user kept the open map"); grep the old outcome's wording across the views. MS2 (one open
  map became many) left "the open map may have changed" in TileBrowserViewModel.ApplyCut and CatalogDraft.Rewriting's
  "the open map is replaced": grep the singular noun when a single becomes a collection.

- Deleted content files leave text a script generates (music change, decision 496: fetch-placeholder-sounds.ps1 writing
  "The music ... rendered by new-placeholder-tune.pl" into CREDITS.md) and the generator's now-unused subs; grep
  `.claude/scripts/` too, not only comments and docs.

**Why:** a comment or doc stating a wrong rule, name or number misleads the next reader.

**How to apply:** after the rule's grep, run the list above against the diff's renames, moves, new members and removed
members. Related: [[tautological-tests]].
