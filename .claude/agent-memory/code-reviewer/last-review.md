---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-music-and-sfx, slice P3 (the map's track pauses and resumes). Round 1.
Verdict: Ready to commit. No Critical, one Warning.

- Open (Warning): SoundLibrary.cs:7 summary says each sound is "loaded on first use and kept"; Release makes that false
- Open (Suggestion): MusicPlayer.Follow's `_mix.Paused != wasPaused` clause can't decide anything: Paused only changes with Playing
- Open (Suggestion): MusicPlayer.cs:50 foreach header is 140 columns; extract the dropped tracks to a named method
- Open (Suggestion): JrpgGame.StartAudio lists the two themes twice, as keptLoaded and as preloads; share one list
- Fixed (from P2, Suggestion): JrpgGame is now PlayWhatTheChannelsAsk
- Fixed (from P2, Suggestion): the comment over _sounds now names voices and music
