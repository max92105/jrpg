# Design: menu cleanup

Part of [the menu cleanup plan](README.md).

## Tunables and placeholders
Layout constants move with their code, values unchanged, except the picker's (question 112).

| Name | Where | Value | Meaning |
|---|---|---|---|
| `SectionTitle.LabelPixels` | `Ui/SectionTitle` (U1) | 14 | Legacy `SectionHeaderStyle` label size, today in three pages as `LabelPixels`. |
| `SectionTitle.LineGap` | `Ui/SectionTitle` (U1) | 20 | The hairline's offset below the label, today `SectionLineGap` in three pages. |
| `stat_value_of_max` | `content/text/ui.json` (U3) | `{value} / {max}` | The one HP and MP format (decision 110). |
| `party_needs_someone_standing` | `content/text/ui.json` (C2) | "{name} is the only one standing and can't leave the party." | Placeholder wording for the "never all fallen" refusal (decision 107). |
| Picker card columns, card width | `Menus/ItemTargetPickerView` (U4) | 2 columns, 560 px, 16 px apart | Question 112. |

## Design overview

### What exists
- **Fonts.** `Rendering/FontLibrary.Load` reads every `.ttf` in `art/fonts/` and hands the bytes to FontStashSharp's `FontSystem.AddFont`. Its `catch (Exception)` turns the rasterizer's bare exception into `ContentValidationException("art/fonts/<file>: not a readable font.")`.
  - `FontLibraryTests.Load_FileThatIsNotAFont_ThrowsNamingTheFile` writes a 23-byte HTML file as `bad.ttf`.
  - StbTrueTypeSharp's `stbtt__find_table` reads the table directory through unsafe pointers. The directory is 12 bytes, then 16 per table, so it reads past those 23 bytes.
  - Usually it reads garbage and throws, which the test expects. Sometimes the read hits an unmapped page, and the test host dies with 0xC0000005, which can't be caught in .NET.
  - Seen in the M3b, M3c, M4a and S1-2 reviews (code-reviewer memory `flaky-font-test-crash`; the M3b follow-up in `2026-10-18-game-title-menu-movement.md`).
  - A truncated font in `content/` would crash the game the same way.
  - `FontLibraryTests` is also not in the `RepositoryScreenText` collection. It builds a `FontSystem` while other classes use the shared one, which is a second suspect (F1 checks both).
- **Menu pages.** `Flow/SceneFactory.CreateGameMenu` builds every page when the menu opens. `GameMenuScene` sets `_isInPage` on Confirm and calls nothing on the page. Each page formats its state in its constructor, and again only on its own input. So within one opened menu:
  - a potion leaves the Party cards and Status stale;
  - a party change leaves Status's and Equipment's member order stale;
  - equipping leaves the Inventory list stale (ADR 0003).
  
  `SystemPage` already calls `ISaveFilesPage.Open()` each time it shows the save files.
- **Party rules.** `Core/Party/PartyState`:
  - `Leader` is `activeOrder[0]`, fixed;
  - `TryMoveToReserve` refuses the leader;
  - `Reorder` throws unless the leader is first;
  - there is no reserve query and no fallen check.
  
  The battle plan's design notes that the Party page can leave only fallen members active. Decision 107 now closes that path in Core. `PartyPage` repeats the leader rule: `member == _party.Leader` for `PartyLeaderStays`, and `MoveToNextPlaceToSwap` skips `_row == 0`. `Menus/PartyOrderEdit.Swap` is list editing for `Reorder`, not a rule. `Vitals.IsAlive` is `Hp > 0`.
- **Copies.**
  - The section title is in `PartyPage`, `EquipmentPage` and `StatusPage`.
  - "Text and its width" is `PartyMemberCardView.MeasuredText` (also used by `PartyPage._count`), `OptionsPage.ValueText`, and `ItemTargetPickerView.MemberCard`'s `Name` + `NameWidth`.
  - HP and MP bars from vitals are built in `PartyMemberCardView.CardOf`, `ItemTargetPickerView.Show` and `StatusPage.ShowMember`.
  - Two HP formats: `"{v}/{max}"` (card) and `"{v} / {max}"` (Status).
  - Two member cards: `Menus/PartyMemberCardView` (a 120 px row: portrait, name, badge, level, class, bars with values) and `ItemTargetPickerView`'s card (a 112 px column: portrait over name and two bars).
- **`EquipmentPage`** (393 lines): input and commands; the sheet's formatting (`MemberSheet`, `SheetOf`, `WornIn`, which copies Core's `internal PartyMember.EquippedIn`); the preview text (`PreviewText`, `ColorOf`); the labels; all the drawing.
- **`PartyPage`** (372 lines): input; cursor navigation over two columns (`MoveInColumn`, `SwitchColumn`, `KeepCursorOnACard`, `MoveToNextPlaceToSwap`); the action choice; commands to Core; card formatting (`ShowParty`); all the drawing (`DrawCards`, `DrawMovingOutline`, `DrawActionChoice`, `DrawSectionTitle`, the count and notice).

### Core changes
| Member | Why | Slice |
|---|---|---|
| `PartyState.ReserveMembers` (recruitment order, a kept list) | The Party page and `MemberOrder` stop working the reserve out themselves (decision 108). | C1 |
| `PartyState.CanMoveToReserve(PartyMember)`: active, not the leader, and another active member standing | The leader rule and "never all fallen" (decision 107). `TryMoveToReserve` uses it. | C2 |
| `PartyState.CanChangePlace(PartyMember)`: active and not the leader; `Reorder` checks through it | The leader keeps first place (decision 107). | C2 |
| `PartyMember.EquippedIn(EquipmentSlot)` becomes `public` | `EquipmentSheet` reads what's worn without copying the switch. | E1 |

No new Core folder edges (`Party` already uses `Stats.Vitals`). No save format change.

### Game structure added by this plan
```
Jrpg.Game/
  Rendering/ FontFileHeader (F1)
  Ui/        SectionTitle (U1); MeasuredText (U2); VitalMeter, MemberVitalBars (U3);
             MemberCardView (U4, from Menus/PartyMemberCardView)
  Menus/     IMenuPage.Open (M1); EquipmentSheet, StatPreviewText (E1); EquipmentSheetView (E2);
             PartyCursor (P1); PartyColumns, PartyColumnsView, PartyActionChoiceView (P2)
Removed: Menus/PartyMemberCardView (U4, moved); PartyMemberCardView.MeasuredText, OptionsPage.ValueText (U2);
         ItemTargetPickerView's MemberCard (U4); EquipmentPage's nested records (E1, E2); three DrawSectionTitle copies (U1)
```

**Folder edges (Game).** None points back.

| From | To | For | New? |
|---|---|---|---|
| Menus | Ui | `SectionTitle`, `MeasuredText`, `MemberCardView`, `MemberVitalBars`, `ListCursor` | no |
| Ui | Rendering | `TextureLibrary`, `FontLibrary` through `ScreenText` | no |
| Ui | Jrpg.Core.Stats | `VitalMeter`, `MemberVitalBars` read `Vitals` | yes, checked: Core has no Game reference |
| Ui | Jrpg.Core.Party, Content | `MemberCardView` reads `PartyMember` and class names from `ContentCatalog` | yes, same |
| Rendering | Jrpg.Data.Content | `ContentValidationException` | no |

Shared parts are in `Ui/`, not `Menus/`, so the future `Battle/VictoryScreenView` (battle plan B7b) can use the card and meters without a `Battle -> Menus` edge.

### Key decisions
1. **F1 fixes the product, not only the test.**
   - `Rendering/FontFileHeader.IsComplete` checks three things before `AddFont`:
     - the sfnt version is TrueType `0x00010000`, the only kind in the content (devil's advocate 4; `true`, `OTTO` and `ttcf` are refused);
     - the table directory fits in the file;
     - every table record lies inside the file.
   - It is `internal static` beside its only user, tested with byte arrays.
   - `FontLibraryTests` joins the `RepositoryScreenText` collection, so it can't build a `FontSystem` in parallel with the shared one.
   - Before the fix, the class runs alone in a loop to confirm the out-of-bounds read.
   - Rejected: only rewriting the test's fake file. The game would still crash on a bad font.
2. **Pages rebuild through `IMenuPage.Open()`**, called by `GameMenuScene` when Confirm enters a page.
   - **A. An `Open()` hook (chosen).** One call per entry, matching `ISaveFilesPage.Open()`.
   - **B. Change stamps in Core**, compared each update. It adds a Core API only the UI needs.
   - **C. Fresh pages on each entry**, through the factory. It needs the factory inside the menu scene.
   
   `Open()` puts each cursor on the first entry (decision 109). Cursor fields stay `readonly` and use `Select(0)`. Pages whose list can change length (`InventoryPage` rows) rebuild their cursor as today. Constructors call the same private build method, so tests that never call `Open()` stay valid.
3. **The party rules as two member queries, not a swap command.**
   - `CanMoveToReserve(member)`: active, not the leader, and at least one other active member standing.
   - `CanChangePlace(member)`: active and not the leader. A swap is allowed when both members can change place. Reordering never changes who is active, so it can't leave everyone fallen.
   - `TryAddToActive` can't leave the party all fallen either: it only adds.
   - `PartyOrderEdit.Swap` stays in the Game. It builds a list, and `Reorder` still checks it. Moving the swap into Core (`TrySwapPlaces`) would duplicate `Reorder` with no new rule.
   - **Cost of changing the leader rule later.** Making the leader whoever stands first, or letting them go to the reserve, changes more than these two bodies. `Leader` becomes derived from the active order instead of fixed, and its readers follow:
     - `FieldItemUse`, where the leader stands in for "all" items;
     - the card's Leader badge;
     - the `PartyLeaderStays` notice;
     - `Reorder`'s check.
     
     Character selection (out of scope) is where that would be decided.
   - **On the page**, an action Core refuses is drawn greyed and stays selectable. On Confirm it shows why, as greyed battle skills do (battle plan decision 105):
     - under the rules, only "To reserve" can be refused for a non-leader (they're the last one standing);
     - for the leader, both queries are false, and Confirm on the card shows `PartyLeaderStays` without opening the choice. The page compares with `Leader` only to pick that message.
4. **`ReserveMembers` is a kept list**, rebuilt in the constructor and after a successful `TryAddToActive` or `TryMoveToReserve`, so reading it never allocates (as `ActiveMembers`).
5. **`SectionTitle` holds its title**: one instance per section, from a message id. `Draw(batch, textures, section, opacity)` stays at four parameters.
6. **One vital meter owns the format.** `Ui/VitalMeter` is one vital's bar and its value text. It formats `stat_value_of_max` only when the value or max changed. `MemberVitalBars` pairs an HP and an MP meter for a member. Every screen showing HP or MP text in the menu goes through them:
   - the card;
   - Status;
   - the picker, through the card.
7. **The shared member card is the Party card's look**, moved to `Ui/MemberCardView`. It is the richer layout, legacy `PartyMemberCard`, and the one the victory screen's member cards resemble. It takes its width from the place it's drawn in. The picker lays these cards out in two columns (question 112) and keeps its pure `ItemTargetPicker` unchanged. The card is built from `MeasuredText` and `MemberVitalBars`. It has no section title of its own; `SectionTitle` stays a page-level part.
8. **Both page splits follow the existing pattern** (map: "Pure presenter + thin view"), as `EquipmentChoice` / `EquipmentChoiceView` already do:
   - **Equipment:**
     - `EquipmentSheet`: pure formatting;
     - `StatPreviewText`: pure preview values and colors;
     - `EquipmentSheetView`: drawing.
   - **Party:**
     - `PartyCursor`: pure navigation over the two columns, with the swap's place-skipping through `CanChangePlace`;
     - `PartyColumnsView`: titles, count, cards, the moving outline, the notice;
     - `PartyActionChoiceView`: the glass action panel.
   
   Each page keeps input, its sub-states and commands to Core.

### Extension points
- Character selection (decision 107, out of scope) changes `PartyState.Leader` and the readers listed in key decision 3.
- A new page implements `Open()` and resets its cursor.
- Another vital (EXP, a future SP) is a `VitalMeter` with its own bar gradient. The format stays in one message.
- The victory screen (battle plan B7b) draws `MemberCardView`. Its experience bar can be a `VitalMeter` over `UiTheme.XpBar`.
- A third equipment slot is one entry in `EquipmentSheet.Slots` plus its label id.

### Legacy mapping
Nothing new is ported. The extracted pieces keep their legacy sources in comments:
- `SectionTitle`: `SectionHeaderStyle`;
- `MemberCardView`: `PartyMemberCard`;
- `VitalMeter`: `PartyMemberCard`'s bar rows;
- `EquipmentSheetView`: `EquipmentView` and `StatsPanel`;
- `PartyColumnsView`: `PartyView`;
- `PartyActionChoiceView`: `GlassPanelStyle` and `ListRow`.

## Risks
- **F1 may not catch every bad font.** A file with a complete directory but corrupt table contents can still send the rasterizer out of bounds; only a hand-damaged file does that. If the 20-run loop still crashes after both changes, the parallel run was the cause (devil's advocate 3). Record which one it was in the slice.
- **"Never all fallen" and existing saves.** A save made before C2 could hold an active party that is all fallen (possible through the Party page today). C2 refuses only moves; the constructor accepts such a party. Such a save loads, and the next battle is the battle plan's C4 defeat. Making the constructor refuse it would make old saves fail to load (ADR 0007).
- **The picker's new look (U4).** Seven members in two columns of 120 px cards make a band about 600 px tall; 13 members (the most P2b-2 allowed for) would make it about 950 px of 1080. Question 112 may change the layout. `ItemTargetPicker` is unchanged, so its tests guard the behavior.
- **Battle plan overlap.**
  - B3a changes `StatBar` (track color). After U3 that touches `VitalMeter` once.
  - B7b can reuse `MemberCardView`.
  
  The battle plan hasn't started, so this plan lands first.
- **Refactor size.** E2 and P2 move about 200 lines each, and a diff counts them twice. Review them as moves: same constants, same order.
- **Determinism and save format:** unaffected.
