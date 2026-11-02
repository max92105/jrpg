---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-music-and-sfx, slice E1 (the map's track in the editor). Round 1.
Verdict: Ready to commit. No Critical, no Warning.

- Open (Suggestion): MainViewModel.cs:286 and SetMapSettingsEdit.cs:5 list the name and random battles but not the track
- Open (Suggestion): AudioFoldersTests sort test passes without Order on NTFS; a temp folder with a.wav, B.wav, notes.txt pins sort and filter
- Open (Suggestion): no test that StartEditingSettings passes the music folder's files to the dialog
- Note: first verify run failed the tests step (total 3486, error 1, exit -1, no named test); 9 later runs passed 3487; cause unknown
- Fixed (from P3, Warning): SoundLibrary summary now says kept until released
- Fixed (from P3, Suggestion): MusicPlayer no longer has the `_mix.Paused != wasPaused` clause
- Fixed (from P3, Suggestion): MusicPlayer foreach now uses TracksLeavingTheMix
- Fixed (from P3, Suggestion): JrpgGame passes one themes list to MusicPlayer
