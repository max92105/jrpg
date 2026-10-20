# Z. Hand check

Part of [the menu cleanup plan](../README.md).

- **Z. Hand check** (manual)
  - [x] `verify.ps1` three times in a row: every check passes, with the same test total each time (F1).
  - [x] Open the menu once and, without closing it:
    - use a potion on a hurt member, then look at Party and Status;
    - add a reserve member and reorder, then look at Status and Equipment;
    - equip a sword, then look at Inventory.
    
    Each page starts on its first entry (M1).
  - [x] The leader can't be moved or sent to the reserve, and the hint shows. With the leader at 0 HP, the last member standing can't go to the reserve, and the refusal shows (C2).
  - [x] HP and MP read "45 / 60" on the Party cards, Status and the picker (U3).
  - [x] Party, Equipment, Status and Options look as before. The picker shows the shared cards in two columns (U1-U4, E2, P2).
  - [x] Then fill in "Done, with deviations" on each slice and set this plan's status.
- **Done, with deviations:** as planned. Every hand check passed (developer, 2026-10-20).
