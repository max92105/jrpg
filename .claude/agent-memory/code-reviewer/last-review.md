---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

No plan slice: hand-check fix after the goblin got an animated gnoll strip (384 x 128, 3-frame idle loop).
BattlerScaling.OfImage (frame at own size while looping, else Of) now shared by BattlersView.ScalingOf and the
repository chief/goblin test; BattlerArtTests expected record copies IdleAnimation. 2026-11-15. Round 1.
Verdict: ready to commit. No Critical, no Warning.

- Suggestion (open): BattlerScalingTests.Of_RepositoryChiefArt_IsTwiceTheGoblinsDrawnSize now goes through OfImage;
  rename to OfImage_RepositoryChiefArt_IsTwiceTheGoblinsDrawnSize.
- Suggestion (open): BattlerScaling.AtOwnSize's only production caller is now OfImage; could go private (its own
  test folded into OfImage's looping test).
