---
name: last-review
description: The most recent review: slice, round and finding status. Overwritten every review; read first to detect a re-review.
metadata:
  type: project
---

Slice: docs/plans/2026-10-23-new-game-flow/slices/A1b.md, slice A1b (the intro speaks; silent without a sound device)
Round: 1 (verdict: Fix the Criticals, then commit)

- Critical: JrpgGame.StartAudio sets SoundEffect.MasterVolume = 1f, a no-op in MonoGame 3.8.6 (setter returns early when equal; device init only in SoundEffect ctors); with no device NoAudioHardwareException comes from SoundEffect.FromFile in SoundLibrary.TryLoad, uncaught -> crash at the intro. Fix: probe with a tiny `new SoundEffect(new byte[2], 22050, AudioChannels.Mono)` disposed at once — open
- Suggestion: EventPresenterTests loads the repository catalog a second time; StartMapScenarios already holds one (_repositoryContent), expose it — open
