# 0035. Music and sound effects

Status: accepted
Date: 2026-11-01

Extends [ADR 0018](0018-audio-files-and-playback.md), which left music and effects to their own plan.

## Context
Voices already reach the device through a channel a presenter writes and the host plays. Music and effects differ: music
must follow the whole flow (title, map, battle), including while the scenes wait for the encounter's flash, and almost
every presenter asks for effects, most of them already at four constructor parameters. The developer's answers are
decisions 415-425 in the music and sound effects plan.

## Options
### A. Music chosen by the flow from its state; effects reported by scenes as values
- Pros: no scene knows what plays; the music follows the flash, when no scene updates; effects are asserted on a scene as
  its requests are, with no new constructor parameter.
- Cons: one more member on the scene contract, and a clear order to keep in `GameFlow.Update`.

### B. Every scene writes the channels, as the event presenter writes the voice
- Pros: one mechanism for all three channels.
- Cons: a channel in nearly every constructor; a scene that doesn't update can't change the music; each new screen must
  remember the track.

### C. `Song` and `MediaPlayer` for music
- Pros: streams long tracks and reads compressed files.
- Cons: one global song, so no resume where a track paused and no two tracks at once; loses what decisions 416 and 417 ask
  for.

## Decision
A, played through `SoundEffect`.
- **Music is chosen by the flow from its state** (`Flow/MusicSelection`: no session, a pending encounter, the battle
  screen's music cue, the current map) and written into a `MusicChannel` after every update; no scene writes it. A future
  `playMusic` event step would set Core state (a track override in `WorldState`, so the save keeps it) that the selection
  reads; never a scene writing the channel.
- **Effects are reported by scenes as values** (`IScene.Sounds`, `IMenuPage.Sounds`: the files the last update asks for).
  `GameFlow.Update` clears the effect channel first, and copies the top scene's right after `_scenes.Update`, before it
  applies a request. The voice stays injected: it is one long-lived sound a single presenter starts and stops.
- **Playback stays WAV through `SoundEffect`**: music loops on a `SoundEffectInstance` (`IsLooped`), paused and resumed;
  volumes are set per instance, never through the global `SoundEffect.MasterVolume`.
- **A track is released when it leaves the mix** (its instance and its loaded sound); the title and battle themes stay
  loaded, preloaded in `JrpgGame.StartAudio` so the flash doesn't stall on a load.

## Consequences
- Core gains only content names (a map's track, an encounter's theme, a skill's sound); determinism and saves are
  untouched.
- `SoundEffect.FromFile` holds a whole track in memory, so real tracks that make WAV too big move to streaming through
  `DynamicSoundEffectInstance`, which changes only `MusicPlayer` and `SoundLibrary`; not to `Song`.
- A looped instance loops the whole file, so a track whose end doesn't meet its start clicks: real tracks must be cut to
  loop.

## Amendment (2026-11-05): the developer's tracks, with fades (decision 496)
The placeholder tunes are replaced by the developer's own tracks, one folder each under `music/`: the content names the
loop (`"musicFile": "town/loop.wav"`, the folder part of the name checked case and all), and an `intro.wav` or an
`outro.wav` beside the loop is found by the player (`Audio/MusicTrack`), never named by content. Only the title theme
has them: it opens with the intro, loops, and ends with the outro when the game leaves the title, with no fade and no
pause, since the outro ends it; wanted again it starts over from the intro. Every other track is a bare loop, so the
mix fades it in over one second and fades it out when it leaves (a cross-fade with the track coming in); the change
that was a cut at full gain (decision 416) is now that fade. The tracks are 48 kHz WAV, 6 to 23 MB each, held whole in
memory as before: streaming is the follow-up above if that weighs. The intro hands over to the loop when the player sees
its instance stopped, once per update: up to a frame of silence at the join, plus the device's start delay. If the
developer hears it, a `DynamicSoundEffectInstance` fed the intro's samples then the loop's closes the gap.
