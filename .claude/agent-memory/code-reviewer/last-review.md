---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-22-demo-retirement/slices/W4.md, slice W4 (remove "legacy" from Game comments outside Ui/ and Battle/)
Round: 1 (verdict: Ready to commit)

- warning (open): TitleScene.cs:36 glow comment says "when hovered"; there is no mouse (decision 50), code applies SelectedGlowBlur/SelectedScale to _menu.Selected; say "on the selected entry".
- warning (open): one-word tails left by the 120 rewrap: ExplorationScene.cs:22 "decision 86).", MapDrawList.cs:10 "grown.", InventoryPage.cs:20 "(P2b).", PartyColumnsView.cs:138 "moving.", StatusPage.cs:27 "section.", MapDrawListTests.cs:78 "all." and :136 "behind)."; trim a word so each fits one line.
- suggestion (open): SavePage.cs:21 "the page's 28 x 20 right margin" -> "the page's 28 px right margin" (NoticeMargin = 28).
- suggestion (open): PartyActionChoiceView.cs:21 "glass panel" vs its summary's "translucent panel"; ItemTargetPickerView.cs:19 and MenuHeader.cs:18 "Placeholder layout:" without "(decision 82)" unlike siblings.
- follow-up: decision 86 text in docs/plans/2026-10-18-game-title-menu-movement.md:777 still says "legacy camera easing" (outside slice).
