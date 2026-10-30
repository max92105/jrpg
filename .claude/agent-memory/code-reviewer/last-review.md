---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-gold-and-shops, slice T3 (selling: stacks via picker, pieces via priced ConfirmPrompt; Purchase became ShopCounter; Ui/PieceName).
Round 1. Verdict: Ready to commit. verify.ps1 all pass (3117 tests).

- Suggestion, open: a sold piece's notice uses the row's base name ("Sold: Bronze Sword.") while the question names it with PieceName.Of ("Magic Bronze Sword"); use PieceName.Of in ShopCounter.Sell's notice
- Suggestion, open: ShopCounter keeps Picker/Prompt and _settle as fields that must agree (`_settle ?? throw` guard); hold the open deal as one value
- Follow-up: ShopScene at 296 lines; the next slice touching it crosses 300
- Follow-up (carried): ShopListView.Draw 5 parameters; private Draw helpers (ShopDetailView.DrawRow 6, DrawPreview/DrawPicker 5) exceed max 4
- T2 carried: notice-priority Warning fixed (deal's line first); Left-in-picker scene test fixed (Left_InPicker_StepsTheCountDownNotTheMember); Purchase _picked/Picker pairing superseded by the ShopCounter suggestion above
