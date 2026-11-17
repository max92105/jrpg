---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-encounters-and-backgrounds, slice BG5 (BattleBackdrop plays animated backgrounds; ExplorationScene
decodes a map's battle backgrounds on entry and releases the last map's via BackgroundRelease/TextureLibrary.Release;
Core EncounterBackground.PossibleOn).
2026-11-17. Round 1. Verdict: Ready to commit (no Critical, no Warning).

- Checked: warp's map change happens in EventPresenter.ShowWarp at FadePhase.Holding, so the next Draw decodes under
  black; only random encounters start battles (ExplorationState.TryStartEncounter), so PossibleOn covers every battle;
  only BattleScene draws non-title/menu backgrounds, fetched per frame, so a release can't dispose a held texture.
- Suggestion (open): ExplorationScene.DecodeBattleBackgrounds also releases; a name like
  SwapDecodedBattleBackgrounds says both.
- Follow-up (open): a new game or load builds a new ExplorationScene with nothing decoded, so the last playthrough's
  map backgrounds stay cached for the session (implementer's note).
- Follow-up (BG7, open): GridProblemOf divides by FrameWidth/FrameHeight and reads Animation; the editor must call it
  only on a record its validator passed.
