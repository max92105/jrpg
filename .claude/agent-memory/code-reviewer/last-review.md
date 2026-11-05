---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

No plan: the music tracks change (decision 496, ADR 0035 amended), one commit, round 1 (2026-11-05). Verdict: ready to commit.

- Open (Warning): editor map music choices (AudioFolders.FilesIn, MainViewModel.cs:287) now list titlescreen/intro.wav and outro.wav as tracks.
- Open (Warning): intro to loop handover polled once per update in MusicPlayer.FollowIntros: up to a frame of silence between them.
- Open (Warning): MusicMix.Want resets _sinceStart to zero when the wanted track is still Leaving: its volume drops to 0 then fades in.
- Open (Suggestion): fetch-placeholder-sounds.ps1:119 and content/audio/CREDITS.md still say the music is script-rendered.
- Open (Suggestion): new-placeholder-tune.pl keeps dead subs arp and repeat_bar; its line 1 is 122 columns.
- Open (Suggestion): MusicPlayer.Begin plays nothing when an intro exists but fails to load; could fall back to the loop.
- Checked: no per-frame allocation in Follow; double Settle of a track in both wasLeaving and wasPaused is safe (State check, Remove guard).
