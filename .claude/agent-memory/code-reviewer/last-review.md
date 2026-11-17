---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-encounters-and-backgrounds, slice BG7c (Backgrounds/BattleBackgroundCatalogViewModel, the extracted
Catalogs/ArtImageRowProblem used by the Effect strips and Character sprites pages too).
2026-11-17. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Suggestion (open): Problem_RecordListedTwice_IsDatasLine's expected value comes from OfAnimatedBackgrounds itself,
  so it passes null == null if Data stops refusing a duplicate; add Assert.NotNull or pin the message's start.
- Suggestion (open): ArtImageRowProblem.FirstOf's `chosen` param doc ("null with none, or with none whose problem
  counts") reads awkwardly; name the case (an unedited Character sprites setup).
- Not a finding (drop): the `!` in BattleBackgroundViewModel.WithTypedTime is needed; without it the build fails
  with CS8629, as in AffixViewModel.ToAffix (BG7b).
- BG7b's non-square grid test suggestion: fixed (BattleBackgroundViewModelTests, "3 x 2 grid").
