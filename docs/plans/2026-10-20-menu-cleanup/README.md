# Cleanup: font test crash, menu pages, party rules, shared menu pieces
Status: approved

A cleanup plan between part 3b's menu plan (`2026-10-18-game-title-menu-movement.md`, every slice done) and the battle plan (`2026-10-20-battle/`, not started). It closes follow-ups those slices left open:
- the M3b test-host crash;
- the P4-1 "pages keep stale state" known gap and its `PartyState.ReserveMembers` follow-up;
- the P4-2 "the Game repeats Core's leader rule" follow-up;
- copied menu pieces (the section title, measured text, vital bars, two member cards);
- `EquipmentPage` (393 lines) and `PartyPage` (372 lines).

Developer's request (2026-10-20): "cleanup: font test crash, menu pages rebuilt when shown, PartyState.ReserveMembers and a Core leader rule, shared section title and member card, split EquipmentPage". The developer answered questions 107-111 and approved the devil's-advocate edits (2026-10-20). They added the `PartyPage` split and the "never all fallen" rule, and asked for the UI to be built from as many reusable components as possible.

## Goal
At the end of this plan:
1. **Tests run without the host crashing.** A file in `art/fonts/` that isn't a whole TrueType font is refused by `FontLibrary` with its usual "not a readable font" message, before the rasterizer reads past its end.
2. **Menu pages show the current state.** Entering a page from its tab rebuilds it from Core and puts its cursor on the first entry (decision 109). Within one opened menu:
   - a potion used on Inventory shows on Party and Status;
   - a party change shows on Status and Equipment;
   - equipping shows on Inventory.
3. **Core owns the party rules.**
   - `PartyState.ReserveMembers` lists the reserve.
   - `CanMoveToReserve` and `CanChangePlace` own the leader rule (decision 107), and the new rule that the active party is never entirely fallen.
   - The Party page asks Core. A refused "To reserve" is greyed and, on Confirm, says why.
4. **The menu is built from shared parts.** These live in `Ui/`, so the battle's victory screen can reuse them:
   - `SectionTitle`;
   - `MeasuredText`;
   - `VitalMeter` (one bar with its "45 / 60" text, the single format, decision 110);
   - `MemberVitalBars` (a member's HP and MP meters);
   - `MemberCardView` (the one member card), used by the Party page and the item target picker (decision 111).
5. **`EquipmentPage` and `PartyPage` are split by responsibility.** A pure part is tested on its own, drawing goes in views, and the page keeps input and commands to Core.

Nothing else changes on screen, apart from:
- the stale values (goal 2);
- the greyed "To reserve" (goal 3);
- the Party cards' "45/60", which becomes "45 / 60";
- the item target picker, which shows the shared card (question 112).

## Order, and why
1. **The crash first (F1).** Every later slice runs the Game tests, and a crashed host hides failures in whichever slice runs next.
2. **Core party rules (C1, C2)**, each with its first Game user.
3. **Pages rebuilt when shown (M1)**, before the refactors, so they move code that already rebuilds.
4. **Shared parts, smallest first (U1-U4).**
   - U1 is the section title.
   - U2 is measured text, used by U3 and U4.
   - U3 is the vital meter with the single format.
   - U4 is the shared member card, built from U2 and U3.
5. **Split `EquipmentPage` (E1, E2).** Pure parts first, then the draw part.
6. **Split `PartyPage` (P1, P2)**, after C2 and M1 (its rules and `Open()`) and after U4. The shared card left `Menus/` in U4, so P2's view draws through `Ui/MemberCardView`, and the card isn't moved twice.
7. **Hand check (Z).**

## Guiding rules
- **No behavior change in the refactor slices** (U1, U2, E1, E2, P1, P2). Existing tests pass unchanged. A test that has to change is a requested change, noted in the slice's deviations.
- **Core owns every rule** (ADR 0001). The Party page asks Core what a member may do.
  - It compares with `Leader` only to pick which notice to show.
  - It never skips row 0, and never checks who is fallen.
- **Reusable components** (developer, 2026-10-20). UI shared by more than one screen is a `Ui/` type with one responsibility: the section title, measured text, the vital meter and its format, member vitals, the member card. A page composes them; it doesn't copy them.
- **Text in content** (ADR 0010). The vital format (`stat_value_of_max`) and the refusal notice (`party_needs_someone_standing`) are messages in `content/text/ui.json`, each with a `MessageIds` constant checked by `MessageIdsTests`.
- **Rebuild on change, never per frame.** `Open()` and the `Show...` methods format text; `Draw` only reads it.
- **Pure presenter + thin view.** The pure types are tested in `tests/Jrpg.Game.Tests`:
  - `EquipmentSheet`, `StatPreviewText`;
  - `MemberVitalBars`, `VitalMeter`;
  - `PartyCursor`.
  
  Views are checked by hand.
- **Look unchanged** except where a decision changes it. The constants move with the code. No type is named after its folder (CA1724).

## Out of scope (follow-ups)
- **Choosing who the leader is** (decision 107). It comes with a character-selection feature, which isn't coded yet. Today the leader is the first member of the starting active party.
- **Swapping an active member with a reserve member** in one move. The Party page has no such action: a reserve member joins, and an active member leaves. If it's added, it goes through the same "never all fallen" query (C2).
- **Column titles over a hairline**, copied in `InventoryPage.DrawColumnTitles`, `EquipmentChoiceView` and `Saving/SaveSlotListView`. They put several labels on one line; a shared `Ui/ColumnTitles` would be its own slice.
- **`StatusPage`** is 286 lines and has no split here.
- **Why Core refused an equipment change.** `EquipmentPage.Apply` works out "no room in the bag" itself, and "Remove all" loops over the slots in the Game. A Core result would own both.
- **`StatusPage`'s own slot labels** could use `EquipmentSheet.Slots` after E1.
- **`InventoryLabels`' measured texts** could use `MeasuredText` after U2.
- **The battle HUD's party status** (battle plan B3a) has its own layout. It could use `VitalMeter`'s format when built.

## Slices

One line per slice. Each slice's goal, types, tests and hand checks are in its file under `slices/`. Tick the box here when a slice is done, and write its deviations in the slice file. Tunables, design overview and risks: [design.md](design.md).

### Phase 1: the crash

- [x] **F1. Only whole TrueType fonts reach the rasterizer** (Game, about 150 lines) → [slices/F1.md](slices/F1.md)

### Phase 2: party rules in Core

- [x] **C1. `PartyState.ReserveMembers`** (Core + Game, about 90 lines; decision 108) → [slices/C1.md](slices/C1.md)
- [x] **C2. The leader rule and "never all fallen" in Core** (Core + content + Game, about 230 lines; decision 107) → [slices/C2.md](slices/C2.md)

### Phase 3: pages rebuilt when shown

- [x] **M1. Entering a page rebuilds it from Core and starts on the first entry** (Game, about 260 lines; decision 109) → [slices/M1.md](slices/M1.md)

### Phase 4: shared parts

- [x] **U1. One section title** (Game, about 130 lines) → [slices/U1.md](slices/U1.md)
- [x] **U2. Measured text** (Game, about 70 lines) → [slices/U2.md](slices/U2.md)
- [x] **U3. The vital meter, one HP and MP format** (content + Game, about 200 lines; decision 110) → [slices/U3.md](slices/U3.md)
- [ ] **U4. One member card for the Party page and the item target picker** (Game, about 280 lines; decision 111, question 112) → [slices/U4.md](slices/U4.md)

### Phase 5: split EquipmentPage

- [ ] **E1. The equipment sheet and preview text as pure types** (Core + Game, about 200 lines) → [slices/E1.md](slices/E1.md)
- [ ] **E2. The equipment sheet view** (Game, about 260 lines, mostly moved) → [slices/E2.md](slices/E2.md)

### Phase 6: split PartyPage

- [ ] **P1. The party cursor as a pure type** (Game, about 220 lines) → [slices/P1.md](slices/P1.md)
- [ ] **P2. The party page views** (Game, about 250 lines, mostly moved) → [slices/P2.md](slices/P2.md)
- [ ] **Z. Hand check** (manual) → [slices/Z.md](slices/Z.md)

## Decisions (developer, 2026-10-20)
Questions and decisions share numbers.
- **107. Leader rule:** (a), a fixed leader.
  - The leader is always first and never goes to the reserve; nobody takes their place.
  - Choosing who the leader is comes later, with character selection (out of scope).
  - **New rule:** the leader may be fallen, but the active party is never entirely fallen. Core refuses any move that would leave every active member fallen, and the page says so (`party_needs_someone_standing`, placeholder wording to review).
- **108. Reserve order:** recruitment order. No save format change.
- **109. Coming back to a page:** the cursor goes back to the first entry and notices are cleared. There is no restoring of the previous member.
- **110. HP and MP text:** one format everywhere, **"45 / 60"**. It is the message `stat_value_of_max` = `{value} / {max}`, the same spacing as the existing `party_count` ("{count} / {max}") and the Status page. It is owned by `Ui/VitalMeter` (U3).
- **111. Member card:** one shared card. The Party card and the item target picker's card become `Ui/MemberCardView` (later reused by the victory screen), built from `MeasuredText` and `MemberVitalBars`.
- **Scope:** `PartyPage` is split in this plan (P1, P2), after C2 and M1, the same way as `EquipmentPage`.

Earlier decisions that apply:
- 61: party order, pick a member and then the place to swap with.
- 82: look.
- 92: tab and member switching.
- 97: no Party-page guard against a whole party down. It is refined by 107: Core now refuses leaving only fallen members active. A party that ends up all fallen in battle is still a defeat (battle plan C4).
- 105: greyed entries say why on Confirm.

## Open design questions (developer decides)
Each slice uses the placeholder until answered.
- **107 follow-up:** review the wording of `party_needs_someone_standing`. Placeholder: "{name} is the only one standing and can't leave the party."
- **112. The item target picker with the shared card.** The Party card is a 120 px tall row, so seven members no longer fit in one centered row of small cards.
  - Placeholder: the picker band shows the cards in two columns, 560 px wide each, filled in member order. Navigation stays as today (`ItemTargetPicker` unchanged: every direction steps through the members in order).
  - A member the item can't be used on is dimmed (as today), and the selected one gets the card's selected-row glow.
  - Or a different arrangement?

## ADRs needed
- **None.** `IMenuPage.Open()` follows the existing `ISaveFilesPage.Open()`; the alternatives are in [design.md](design.md), key decision 2. The "never all fallen" rule is a Core query with tests and changes no format.
