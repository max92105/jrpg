---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-26-stats-and-gear-slots/slices/S1b.md, slice S1b (Attributes column beside the Status profile with AttributeCaptions; ResistanceTone weak/capped tints; label/value font consts shared from StatusSheetView)
Round: 1 (verdict: Ready to commit; verify all pass, 2430 tests)

- W1 src/Jrpg.Game/Menus/StatusSheet.cs:112 + StatusLabels.cs:30 + AttributeCaptions.Of: the five-primary order now spelled a third time (LeaderProfile._shownKinds is the first) and matched by index across three files -> one AttributeKinds list, labels via StatNames.IdOf; open
- W2 content/text/ui.json:39-43 "+{x} %" hardcodes the plus: a negative primary (gear taking a primary below 0, which MemberSheet supports and the validator allows) reads "+-1 % magic res." -> sign in code or a signed number argument; open
- W3 StatusSheet.ToneOf `permille < 0`: the 0 boundary is untested (fire test's Ice is positive); `<=` would tint every 0 resistance red -> a test with a resistance at exactly 0 reading Plain; open
- S1 StatusSheetView.cs:9-13 summary and DrawProfile comment (~line 144) don't mention the Attributes column; open (suggestion)
- S2 StatusPageSheetTests WearTestArmor one ~170-column line -> local for chain_mail; open (suggestion)
