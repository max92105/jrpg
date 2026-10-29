---
name: stale-size-comments
description: Sizing, drawing moves, behavior removals or renamed files leave comments/docs stating the old size, place, rule or file name (SZ1, SZ2, M4, M2c, F1, S1a map.md)
metadata:
  type: feedback
---

Values-only sizing passes update the comments next to the changed constants but miss comments in neighbouring types
that quote the same size (SZ1: UiTheme row/inset, SquareShrink icon sizes; SZ2: FloatingNumber.StackSpacing "40 px").
Same when a drawing moves: M4 moved the description from under the list to under the selected row, but
InventoryPage's class summary still said "description under the list". Same when a call becomes conditional: M2c made
the sidebar refresh only on pages that show it, but PartySidebar's rewritten summary kept "refreshed every update a page
is open". Same for files: F1 replaced docs/ui-reference/menu-status.webp with .png, leaving the ui-reference README,
export-index and pending slices (Z, the stats plan's L1) pointing at the deleted file.

**Why:** a comment or doc stating a wrong size, place, rule or file misleads the next reader.

**How to apply:** for each changed constant's old value, grep `"<old> px"` and `<old>` in comments across the feature
folder and `Ui/`, not only the edited lines; when something is drawn somewhere new, grep the old place's wording
("under the list"); when a call site gains a condition, reread the callee's summary for "every"/"always"; when a file
is deleted or renamed (`git status` D/??), grep its old name across docs/. Same when tests split (C2): a pending
slice (M2a) still cited "BattleStateTests (lines 225-354)"; grep pending slices for "<File> (line" after a split.
Same when a type gains a new trigger (P4b): PartyMember.Sheet still said "Rebuilt on level-up" once Wear/TakeOff rebuilt it
too; reread the summary of every property a new call site writes.
Same for docs/map.md (S1a): adding a type to a feature line left the neighbouring type's description listing the removed sections; reread the whole edited map line.
Loot S1a: a Done note that pulls work forward from a later slice or renames a planned API (FromSaved -> PieceFrom) left
the later slice file and design.md planning/citing the old; grep the next slices and design.md for the planned name.
Same for a renamed id (M1b: StatAttack -> StatWeaponDamage): test names (`Of_AttackUp_...`) and locals (`var attack`)
kept the old word; grep the old word in the tests that use the new id.
Same when a mechanic stops being read (M2b-1: battle stops reading defense): "until the plan's X" docs and "while the
battle uses it" (CombatStats, StatComparison, StatusSheet) turn false; grep the slice id and "while ... uses" wording.
Same when a record gains a field (M2b-2: StatSheet.AttackDamage): the summary listing what it holds on its owner
(PartyMember.Sheet "primaries, max HP, speed and the derived percents") misses it; reread "X, Y and Z" summaries.
Same when a responsibility moves to a new type (C1: the list builds rows now): a neighbour's param doc still said "which the page
measures"; grep the old owner's name in the neighbours' docs. C2 again: docs/plans/backlog.md kept `EquipmentPage.Apply`
after Apply moved to SlotChoice; grep `<OldOwner>.<MovedMember>` across docs/ for each moved member.
C3: a new fixture summary said "split in two files" with four repair files present; backlog kept "EquipmentPage.cs (281)" after C2 cut it to 228. Recheck sizes and file counts a doc states.
G1b-2: removing SlotChoice.Refusal left the constructor's `<param name="messages">The take hint and the refusal.`;
when a member goes, grep its noun ("refusal", "notice") in the type's own param docs and summaries.
ST1a: a decision changing one view's spacing (StatusSkillsView rows) left the parent's reserved height (StatusSheetView
SkillsHeight "3 x 70", 10 px under band) on the old rows; grep the parent's arithmetic for the child's old numbers.
Related: [[tautological-tests]].
