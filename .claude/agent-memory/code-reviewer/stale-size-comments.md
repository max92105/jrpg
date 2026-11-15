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
  Also a table type gaining rows for a new reason (shops EN1: AbsentWhenNull/ZeroFilter "added after the content was
  written" + `_addedLater`, gaining decision 586's default-valued enemy fields): reread the summary and the array name.
- A Done note that pulls work forward or renames a planned API (loot S1a: FromSaved -> PieceFrom) leaves the later slice
  files and design.md citing the old name; grep the next slices and design.md for the planned name. Same after a test
  split (C2): a pending slice still cited "BattleStateTests (lines 225-354)".
- A renamed id (M1b: StatAttack -> StatWeaponDamage) leaves test names and locals with the old word; grep it in the tests.
  Same when tests switch their data source (mana-seed T2a: content -> test-world left `EveryRepositoryTile`,
  `RepositoryTree`, `RepositoryIntroSkipped`): grep test names for the old source's word, not only comments. A shared
  fixture switching source (T2c: `SaveGameFixtures.Catalog` -> test-world) leaves its users' unchanged summaries saying
  "against the repository content": grep the fixture's users, not only the diff. A test fixture's step type swapped
  (cinematics CR2: still -> cinematic) left ExplorationArrivalTests' `IntroOfOnlyStills`, `stillsOnly` and "plays two
  stills"; grep the old type's noun in the edited test files' names, locals and fixture comments.
- A record gaining a field (M2b-2: StatSheet.AttackDamage; AL2: MapSettings.ShowsBanner left MainViewModel.
  TryApplySettings's "the name, the track, the area level and the random battles", and docs/map.md's Content line naming
  only the null and zero filters) is missed by every "X, Y and Z" summary of it; grep the last listed field. A mechanic no
  longer read (M2b-1) turns "while the battle uses it" docs false; grep "while ... uses" and the slice id.
- A responsibility moved to a new type (C1) leaves a neighbour's param doc naming the old owner; C2 left the backlog with
  `EquipmentPage.Apply` after it moved: grep `<OldOwner>.<MovedMember>` across docs/. ML0 (map list moved into
  `MapListPanel`) added the panel to map.md's Shell row but left `MainWindow.MapSwitching` described as "the map list,
  its filter": reread the old owner's own parenthetical in the same row.
- A file split (cleanup S3: MainViewModelTests 260 -> 194) leaves a sibling's "X is over 300 lines, so this is tested
  here" comment (`MainViewModelMusicTests`); grep the split file's name across its test folder.
- A removed member leaves its noun in the type's own param docs (G1b-2: "the take hint and the refusal").
- A switch case gaining a value (battle turns TN3: AttackCommand null -> "Attack") leaves a consumer's unchanged
  comment naming the old null case (ActionPlayback.AddLanding "An action without one, an attack"); grep the old case's
  noun near every caller.
- A child's spacing change (ST1a) leaves the parent's reserved-height arithmetic on the old rows; grep the parent for the
  child's old numbers. Sizes and file counts a doc states (C3: "split in two files" with four present) need rechecking.
- A docs/map.md entry edited by inserting a new parenthetical keeps the old one beside it (gold S2: `AffixDefinition`
  with two field lists); verify.ps1's two-summaries check only reads code, so read every changed map.md line whole. E1 (gold)
  added `EventScreenTransitions` to the Flow line but left the deleted `ForgeTransitions` in its key types. SI2 broke the
  tables themselves (a header merged with its `|---|` row; a key type appended as a 4th cell): count the `|` per changed row.
- A part that starts writing into its host's list (sound SH3: ShopCounter asking coins into ShopScene's AskedSounds) leaves the
  host's `Sounds` summary listing only the host's own asks; reread the host's summary.
- A check moved out of one layer (mana-seed X2: the writer's art check now only at game startup) leaves comments
  elsewhere saying where it refuses ("refuses it only on save" in TileImages) and summaries describing the old fallback
  (`Select`: "nothing when the sheet isn't in the box"); grep the check's name and "on save" across the feature folders.

- A removed question or refusal (map flows MS1: switching no longer asks) leaves callers' summaries and window
  comments naming it ("or the user kept the open map"); grep the old outcome's wording across the views. MS2 (one open
  map became many) left "the open map may have changed" in TileBrowserViewModel.ApplyCut and CatalogDraft.Rewriting's
  "the open map is replaced": grep the singular noun when a single becomes a collection. MS3 (switching keeps steps)
  left a test comment "the switch drops forest's steps" and a param doc "the open map's": grep tests' comments too.

- Deleted content files leave text a script generates (music change, decision 496: fetch-placeholder-sounds.ps1 writing
  "The music ... rendered by new-placeholder-tune.pl" into CREDITS.md) and the generator's now-unused subs; grep
  `.claude/scripts/` too, not only comments and docs.

- A guard widened (map flows PO3: key guards `is TextBox` -> `is TextBox or ComboBox or ComboBoxItem`) leaves the
  method's comment "Anywhere but a text box"; reread the comment above every changed condition. Cinematics CE2: a
  refusal chained into `RefusalOfCatalogUndo` left its "In the order checked" list without it.

- A Done note saying "every count matches its file" (game cleanup GR1) checked design.md, not the backlog
  line it corrects: EventPresenter stayed 293 vs 294. wc -l each file a changed backlog group names. DT1 (drop
  tables) prefixed the real count but kept the approval's "takes it to about 293": read the whole updated line.

- A write made conditional (CW1: only changed catalogs, a stamped one refused) leaves an unchanged helper comment
  promising the old recovery (WriteAll: "the next save writes them all again"); reread comments in the callees.

- A retuned timing constant (battle turns TN2: ImpactHold 1.0 -> 0.85 s, a new 0.7 s wind-up) re-pinned to
  `BattlePacing.X` in the asserts left the seconds in the test's own comment and the class header's turn timeline; grep
  the old seconds ("1.0 s") in every re-pinned test file.

- A caller switching what it feeds a helper (NU3: font advances -> digit strip advances into `DigitRow.Of`) leaves the
  helper's summary and param doc naming the old source ("at the font's own size", "width in the font"); reread the
  docs of every callee whose argument changed origin.

**Why:** a comment or doc stating a wrong rule, name or number misleads the next reader.

**How to apply:** after the rule's grep, run the list above against the diff's renames, moves, new members and removed
members. Related: [[tautological-tests]].
