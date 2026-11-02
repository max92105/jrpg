---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-music-and-sfx, slice B1 (the impact's sounds, each at its beat). Round 1.
Verdict: Fix the Criticals, then commit. 1 Critical, 0 Warnings, 3 Suggestions.

- Open (Critical): PlaybackSoundQueue.cs:21 `At <= Elapsed` has no test; every Follow test uses Hit(), whose sound is at zero; the slice's Follow_TimeNotYetReached_AsksNothing is missing and the Done note doesn't record it; fixture [StatusTicked, EffectApplied] puts the hit at TickHold
- Open (Suggestion): BattleSounds.cs:35 `EffectApplied or LifeStolen` gives any non-damage kind the heal; name RestoreHp and Revive so the rest reach the throw
- Open (Suggestion): BattleSceneSoundTests.cs:13 "A battle lost at the start, so no seed matters" sits over a test of a battle not lost; move it to LostBattle
- Open (Suggestion): TimedSound.cs:4 `File` param doc "The file." restates the name; move "in the effects folder" onto it
- Note: hand check is the developer's
- Fixed (from D1): ExplorationScene AskAll covered by ExplorationSceneSoundTests; `lastMultiple > 0` gone; voice param doc mentions the typing; map's Exploration row lists TypingSound; Menu press's confirm sound tested
- Dropped (from D1, committed): tick spacing at Normal (developer's call on decision 422); TypingSound clock test and IsDue test Suggestions
