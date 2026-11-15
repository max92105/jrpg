# CE7. Each still's voice length beside its Seconds box

Data + Editor, about 150 lines (about 80 of them tests). Needs CE6. Decision 620.

## Goal
Beside a still's **Seconds** box, the window shows its voice line's length ("voice 5.3 s"), or "voice not found" when
the voices folder lacks it, or nothing without a voice, so stills can be timed to the voice without audio in the editor.
A still shorter than its voice shows the length in the warning color (a voice runs over the next still by design,
decision 613; the color only says so).

## New types
- Data `Audio/WavDuration` (public static): a WAV file's playing length from its header: the `fmt ` chunk's byte rate and
  the `data` chunk's size, walking the RIFF chunks (others skipped). `TimeSpan? Read(string path)`: null for a file that
  isn't a readable RIFF WAVE with both chunks (never a throw on the developer's file; I/O errors read as null too, the
  editor only shows text). Summary: "for the editor's timing line; the game never reads it".

## Changes
- `Editor/Cinematics/CinematicMediaChoices`: `Func<string, TimeSpan?> VoiceLengthOf` (a lookup the shell builds over
  `WavDuration.Read` in the voices folder, cached per file while the window is open).
- `Editor/Cinematics/CinematicStillRow`: `string VoiceLengthText` and `bool IsShorterThanVoice`, recomputed when the
  voice box or the seconds box changes (a read of the lookup, never a throw); the row takes the lookup from its
  cinematic when built. Not relayed to the list's problem (not a typed field).
- `CinematicCatalogWindow.xaml`: the text beside the Seconds box, in the theme's warning brush when shorter.
- `Shell/MainViewModel.Database.cs`: the lookup.

## Tests
- `WavDurationTests` (new, `tests/Jrpg.Data.Tests/Audio/`), WAV bytes written by the test (a header-only helper, as
  `PngHeaderFile` writes PNG headers):
  - `Read_OneSecondAt44100Mono16Bit_IsOneSecond` (byte rate 88200, data 88200).
  - `Read_ListChunkBeforeData_SkipsIt`.
  - `Read_NotRiff_IsNull`; `Read_NoDataChunk_IsNull`; `Read_MissingFile_IsNull`.
  - `Read_RepositoryPlaceholderVoice_IsAboveZero` (`content/audio/voices/Placeholder.wav`, read from the source tree).
- `CinematicStillRowTests`: `VoiceLengthText_VoiceFound_ShowsSecondsToATenth`; `VoiceLengthText_VoiceMissing_SaysNotFound`;
  `VoiceLengthText_NoVoice_IsEmpty`; `IsShorterThanVoice_SecondsBelowLength_IsTrue` and `..._SecondsAtLength_IsFalse`
  (both sides); `VoiceLengthText_VoiceChanged_IsRecomputed` (from a length that differs).

## Verify by hand
1. Database > Cinematics..., `ragnir_intro`, still 1: pick `Placeholder.wav` in **Voice**: "voice N.N s" appears beside
   Seconds; type a shorter time: it turns to the warning color; a longer one: back to normal.
2. Type a voice name the folder lacks: "voice not found". Clear the voice: nothing shows.

- **Done, with deviations:** the row's lookup is a `VoiceLengthOf` property its cinematic hands it (as it sets `Number`), and the cinematic's comes from the window's choices, rather than a constructor parameter through every `From` and `New`. The shell caches each voice's length per window. Tests: `CinematicStillRowVoiceTests` (a sibling; the row's tests file is near 200 lines), with the rule's two sides, the notices, and the lookup reaching a held row and an added one; `WavDurationTests` as listed.
  CE7's review: the lookup reads a voice only when the folder holds its name exactly (Windows would open another case, which the game's start refuses), and the shown length rounds up to a tenth, so typing it never warns.
