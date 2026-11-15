# EF6. The swing's placeholder sound from freesound, at the swing's start

Content + Game, about 110 lines (about 50 of them tests), plus one sound file. Needs EF5. Decision 658 (Q9); ADR 0036
(placeholder sounds).

## Goal
A swing plays a short whoosh when it starts (0.47 s into the wind-up), a placeholder found on freesound.org by the
project's sound rules, recorded in `content/audio/sound-sources.json` and credited. An attack without a swing makes no
whoosh.

## Before the code: the sound (the implementer, with the project's scripts)
- Slot brief: **a short sword whoosh, a single fast swing through the air, under 0.4 s, no impact, no voice**.
- `.claude/scripts/find-freesound.pl` for candidates: CC0 first, CC-BY when nothing CC0 fits, never NC, SA or
  Sampling+, never an upload naming or imitating a commercial game or film (`.claude/rules/content.md`, ADR 0036).
- `.claude/scripts/add-sound-source.pl` writes its entry in `content/audio/sound-sources.json` (file
  `effects/Swing.wav`); `.claude/scripts/fetch-placeholder-sounds.ps1` downloads and converts it and regenerates
  `content/audio/CREDITS.md`. Never edit the converted WAV by hand.
- The developer listens at the phase's end; a veto gets a shortlist of three links.

## Changes
- `Game/Ui/UiSoundFiles.Battle.Swing` (`"Swing.wav"`; decision 425: a sound the code plays is a constant its
  existence test covers).
- `ActionWindUp.Sounds`: the swing sound at `BattlePacing.SwingStart` when `SwingFile` isn't null, after the opening
  sound (sorted by time).

## Tests
- `ActionWindUpSoundTests` (TN4's file): `Sounds_AttackWithASwing_WhooshesAtTheSwingStart`;
  `Sounds_AttackWithoutASwing_IsSilent`; `Sounds_SkillWithASoundAndNoSwing_IsItsSoundOnly`.
- `UiSoundFilesTests` (the existence test): passes with the file in place (the constant sits in a checked group).
- `SoundSourcesTests` (the sound pass's P2): passes with the entry and the converted file.
- `BattleSceneSoundTests`: `Update_SwingingAttack_WhooshesBeforeTheHit` (strips built in the test with a swing: the
  whoosh in the update at 0.47 s, the hit at 0.7 s).

## Verify by hand
1. With EF5's local strips file: a goblin's attack whooshes as its swing starts, then the hit sounds on the impact.
2. Without a swing: no whoosh.
3. Listen to the whoosh on its own: short, clean, no click at its end.

- **Note (paused 2026-11-15):** the sound is chosen: freesound 60024, "swosh swinging sword 37" by qubodup, CC0, a
  bamboo stick recorded by its uploader (no game or film named), trimmed to 0.35 s with a 0.05 s fade:
  `perl .claude/scripts/add-sound-source.pl effects/Swing.wav=60024:0.35:0.05`, then
  `fetch-placeholder-sounds.ps1 -Only effects/Swing.wav`. The conversion needs ffmpeg (`winget install Gyan.FFmpeg`,
  decision 428), which this machine lacks; installing it is the developer's call, so the slice waits for it.
- **Done:** as planned, once the developer approved installing ffmpeg (2026-11-15). `effects/Swing.wav` is freesound
  60024 trimmed to 0.35 s (ends at silence: the fetch script's loop jump 0.000). The three wind-up sound tests sit in
  `ActionWindUpEffectTests`, beside the swing's own tests and their swinging strip, not in `ActionWindUpTests`.
