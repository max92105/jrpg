---
name: greyed-confirm-untested
description: Battle menu "Confirm on a greyed row does nothing" guards ship without a test (seen B6a skills, B6b items)
metadata:
  type: feedback
---

Greyed rows in battle lists (skills, items) rely on a `CanUse` guard before opening `BattleTargetCursor`, which throws on zero candidates. The guard has shipped untested twice (B6a, B6b).

**Why:** removing the guard crashes the battle on a player's Confirm; Core's CanUse test doesn't cover the menu wiring.
**How to apply:** for any new greyed list entry in the Game, look for a menu test that presses Confirm on it and asserts nothing opened; Warning if missing.
