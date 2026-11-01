---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan: 2026-10-29-music-and-sfx, slice C1 (map tracks in content). Round 1.
Verdict: Ready to commit. No Critical, one Warning.

- Open (Warning): MapSettingsViewModel.ToSettings builds new settings without MusicFile, so a name or battle edit plus Ctrl+S drops the map's track; keep MusicFile = _currentSettings.MusicFile and test it
- Open (Suggestion): Done note says "as planned" but the slice's AddMissingByExactName became an iterator MissingByExactName; record it
- Open (Follow-up, from V1, still in the tree): older OptionsPageTests reach Text speed by one Down press (lines 53-88); PageOn(TextSpeedRow) exists
