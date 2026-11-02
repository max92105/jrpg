---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-music-and-sfx, slice B2 (every other battle event kind sounds). Round 1.
Verdict: Fix the Criticals, then commit. 1 Critical, 0 Warnings, 1 Suggestion.

- Open (Critical): BattleSounds.cs:49-50 startOf(kind) gives the first beat of a kind, but Tick and Defeat can each have two beats: [Impact, Defeat, Backfire, Defeat] drops the caster's fall (dedup; should be at 3.0 s), [Impact, Tick, Skip, Tick, Defeat] puts the goblin's PoisonTick at 1.0 s, its number at 2.6 s; fixtures in ActionPlaybackFallTests:44 and :105; no test
- Open (Suggestion): BattleSounds.cs:47 OtherSoundOf names nothing; SoundAtItsBeat pairs with SoundAtTheStartOf
- Note: hand check is the developer's; plan checkbox is ticked after review
- Fixed (from B1): Follow_TimeNotYetReached_AsksNothing exists; RestoreHp and Revive named; LostBattle's comment moved onto it; TimedSound File doc says the effects folder
