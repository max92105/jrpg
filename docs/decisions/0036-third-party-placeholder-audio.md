# 0036. Third-party placeholder audio

Status: accepted
Date: 2026-11-02

Extends [ADR 0018](0018-audio-files-and-playback.md) (the WAV encodings the game reads) and
[ADR 0035](0035-music-and-sound-effects.md) (how effects reach the device).

## Context
The music plan's sound effects were sine tones from `new-placeholder-tone.ps1`: painful to hear and telling nothing
apart. The developer asked for placeholders found on freesound.org instead, and for every future placeholder sound to
come from there (the sound pass plan, decisions 426 and 427). Freesound serves a sound's page and its 128 kbps `-hq.mp3`
preview without a login; the original file needs an account, and the API a key. The game plays WAV only, and the
developer works on two PCs, so whatever converts a preview must be reproducible on both.

## Options
### A. A manifest of sources, a script that fetches the previews and converts them with ffmpeg
- Pros: swapping a sound is one manifest line and a rerun; the credits are written from the same file, so they can't
  drift; no account, no secret; a Data test keeps the manifest, the effects folder and the credits in step.
- Cons: ffmpeg is a tool to install once per PC; preview quality (128 kbps MP3) is enough for placeholders, not for
  release.

### B. The original files through freesound's OAuth2 API
- Pros: full quality.
- Cons: an account, a token flow and a secret on two PCs, for sounds that will be replaced.

### C. Downloading and converting by hand, with a credits file kept by hand
- Pros: nothing to write.
- Cons: nothing reproducible; a swap on one PC is a mystery on the other; the credits drift from the files.

### D. An API key for searching, or a NuGet decoder (NAudio) instead of ffmpeg
- Pros: structured search results; no tool outside the repository.
- Cons: the key isn't needed once the manifest stores the preview URL, since the public site searches; NAudio needs a
  package approval and its MP3 decoding leans on Windows codecs.

## Decision
A (developer, 2026-11-02).
- **The manifest** `content/audio/sound-sources.json` names, per placeholder file (a path under `content/audio/`), the
  freesound id, the sound's name, its page as `https://freesound.org/s/<id>/`, its author, its license as an SPDX id,
  its preview URL as the page serves it, the date it was fetched, and the conversion's start, length, fade and gain; a
  `tunes` list names the effects `new-placeholder-tune.pl` renders. The key is (id, start): one take can feed several
  slots from different starts, never the same slot twice. The game never reads the manifest, so it has no Data loader.
- **The conversion** (`.claude/scripts/fetch-placeholder-sounds.ps1`, `-Only <file>` for one entry) downloads the
  preview with curl into a cache and converts it with ffmpeg to 16-bit PCM mono 44 100 Hz (about 2 MB for every effect;
  the tunes stay 22 050 Hz). ffmpeg drops the MP3's encoder delay when the header carries it, so the trim is for an
  uploader's lead-in: silence below the measured peak minus 40 dB (`detection=peak`) is cut from the start; a 1 ms fade
  in (5 ms when the start offset cuts into the sound) and a short fade out against clicks; the peak set to the
  manifest's level plus the entry's gain; bitexact output, a plain RIFF with only `fmt` and `data`. The script refuses
  an entry with a disallowed license or a repeated key, since it is the only writer of the converted WAVs, which are
  committed.
- **The credits** `content/audio/CREDITS.md` are rewritten from the whole manifest on every run (UTF-8 without BOM,
  CRLF): per entry, the file, the sound's name linked to its page, the author, the license linked to its deed,
  "modified: trimmed, resampled to mono, level-adjusted", and the fetch date. A Data test checks every entry is in it, every effect
  is sourced or a tune, and every license is allowed.
- **Licenses:** CC0 preferred; CC-BY 3.0 or 4.0 when nothing CC0 fits; never NC, SA or Sampling+; never an upload that
  names or imitates a commercial game or film, whatever its license (decision 427).

## Consequences
- Preview quality is enough for placeholders, not for release; real audio replaces manifest entries one by one, and a
  recorded or bought file leaves the manifest and its credit with it.
- CC-BY sounds oblige the credits file to ship, which `content/` publishing already does.
- A sound removed from freesound, or a preview URL changed, only stops its regeneration: the committed WAV stays.
- ffmpeg is a per-PC tool (`winget install Gyan.FFmpeg`, decision 428), needed only to regenerate a sound; the game, the
  build and the tests never need it.
- A later placeholder (a voice, an ambient loop) is a manifest entry, not a new pipeline.
