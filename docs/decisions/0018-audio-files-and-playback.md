# 0018. Audio files and playback

Status: accepted
Date: 2026-10-24

## Context
The intros are spoken (ADR 0016, decision 172): a voice step starts a voice-over while the stills show. The game plays no sound yet. Whatever plays it must:
- add no NuGet package (CLAUDE.md);
- keep Core free of audio: Core only says a voice step started;
- keep the update path free of devices (ADR 0008): scenes return what they want, and the host acts on it;
- run on a machine with no sound device, and in tests, which have none;
- let the developer drop in recorded files later without code changes, as art works (ADR 0009, 0011).

MonoGame 3.8.6 DesktopGL plays sound through OpenAL, bundled with it. `SoundEffect.FromFile` reads a RIFF WAV file in one of these encodings: 8-bit unsigned PCM, 16-bit signed PCM, 24-bit signed PCM, 32-bit IEEE float, MS-ADPCM 4-bit, or IMA-ADPCM (IMA4) 4-bit, mono or stereo (from the package's documentation). Whoever replaces the placeholder voice should export 16-bit PCM WAV, the safest of these.

## Options
### A. WAV through `SoundEffect.FromFile`
Voice files are WAV, loaded whole once and played with a `SoundEffectInstance`, which can stop when the intro is skipped.
- Pros: no package, no content pipeline. Files load from `content/` like PNGs. Several sounds can overlap, which SFX will need later. Stop and volume work per instance.
- Cons: WAV is large, about 10 MB a minute at 16-bit 44.1 kHz stereo, and git keeps every version. It's fine for short voice lines, but not for music.

### B. OGG through `Song` and `MediaPlayer`
- Pros: small files; it streams, so it suits long tracks.
- Cons: `MediaPlayer` plays one song at a time, globally, which is a poor fit for a voice over music later. `Song.FromUri` loading outside the content pipeline is less tried on DesktopGL. Its static state is harder to keep out of tests.

### C. A package (NAudio, a Vorbis decoder, FMOD)
- Pros: any format, mixing, streaming.
- Cons: a package needs the developer's approval. It's more to learn and ship for one voice line, and some packages are Windows-only or have licence terms.

## Decision
A (developer, 2026-10-24). Voices are 16-bit PCM WAV files (any encoding listed above loads) in `content/audio/voices/`, played through MonoGame's `SoundEffect`.

## Consequences
- **Files.** Files live in `content/audio/<kind>/`, starting with `voices/`, and are named by events, by exact name. Data's `AudioReferenceCheck` checks at startup that each named file exists, as `ArtReferenceCheck` does for art (ADR 0009). The encoding isn't checked: a file that won't load is reported once, by name, and plays silent.
- **Playback.** Scenes write the wanted voice into a plain `VoiceChannel` (play this file, or stop). After each update, the host's `VoicePlayer` makes the sound match the channel. Only `SoundLibrary` and `VoicePlayer` touch MonoGame audio, so presenters stay testable without a device.
- **No device.** The audio system is touched once at load. A missing device is reported once at startup, and the game runs silent; it never stops the game.
- **Size.** Voice files stay short. The placeholder is one shared, generated file.
- **Later.** Music and SFX extend this in their own plan. That plan may pick `Song` for streaming music, with its own ADR.
