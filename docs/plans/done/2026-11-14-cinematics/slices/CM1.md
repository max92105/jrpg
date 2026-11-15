# CM1. `cinematics.json` loads into `Cinematics/` records

Core + Data, about 250 lines (about 120 of them tests). Needs CA0 and ADR 0046 written. Decisions 609, 610, 614,
617. Design: "Core changes",
"Content format", "Structure and folder dependencies", tunable "Zoom bounds".

## Goal
`content/cinematics.json` loads into `ContentCatalog.Cinematics`, each record checked by its own validator with
messages naming the cinematic and the still. The file starts empty: the developer fills it in the window (CE6).

## New types
- Core `Cinematics/CinematicDefinition` (public sealed record): one cinematic as content: its stills in order and the
  track under them. `required string Id`, `string? MusicFile` (a track in the music folder, `town/loop.wav` style; null
  keeps the map's, decision 614), `required IReadOnlyList<CinematicStill> Stills`.
- Core `Cinematics/CinematicStill` (public sealed record): one still as content: its picture, time, subtitle, voice
  line and zoom. `required string ImageFile` (under the cinematics art folder, its cinematic's folder included, decision 610),
  `required double Seconds` (above 0), `string? Caption`, `string? VoiceFile` (in the voices folder), `StillZoom? Zoom`;
  `TimeSpan Duration => TimeSpan.FromSeconds(Seconds)`.
- Core `Cinematics/StillZoom` (public sealed record): a still's slow zoom: its scale at the start and at the end of its
  time. `required int StartPercent`, `required int EndPercent`; `public const int MinPercent = 100`, `MaxPercent = 150`
  (decision 617). `ScaleAt` comes with its first caller (CG6).
- Data `Cinematics/CinematicCatalogValidator` (internal static): checks `cinematics.json`: through
  `ContentListValidator` (ids present and unique, no null record, as the other catalogs), then per cinematic: `stills`
  present, not empty, no null entry; per still (named `stills[i]`): `imageFile` named; `seconds` above 0 and finite;
  the cinematic's total seconds a time the game can count (below `TimeSpan.MaxValue.TotalSeconds`); `caption` null or
  text; `voiceFile` null or named; `zoom` percents each within `MinPercent..MaxPercent`; `musicFile` null or named.
  Message shape: `cinematics.json: cinematic 'ragnir_intro': stills[2]: seconds must be above 0 (was 0).`

## Changes
- Core `Content/ContentCatalog`: `IReadOnlyDictionary<string, CinematicDefinition> Cinematics { get; init; }`
  defaulting to an empty ordinal dictionary (as `FieldSprites`), summary "The cinematics an event's `cinematic` step
  plays, by id (ADR 0046)."
- Data `Content/ContentCatalogLoader`: `internal const string CinematicsFile = "cinematics.json"`; read and validated
  after the quests (it names no other catalog), set on the catalog.
- Data `Content/AbsentWhenNullFilter`: four lines, comment "ADR 0046: a cinematic without a track keeps the map's; a
  still may have no subtitle, voice or zoom": `(CinematicDefinition, "musicFile")`, `(CinematicStill, "caption")`,
  `(CinematicStill, "voiceFile")`, `(CinematicStill, "zoom")`.
- `content/cinematics.json`: `[]` with a CRLF and a trailing newline (the writer's empty list; CE1's identical-bytes test
  confirms).
- `docs/map.md`: a Core row for `Cinematics` (leaf; the records), the Data `Content` row names `cinematics.json`.

## Tests
- `CinematicCatalogValidatorTests` (new, `tests/Jrpg.Data.Tests/Cinematics/`), fixtures built in the file
  (`CinematicFixtures.TwoStills` with `with` for each case, never the repository's file):
  - `Validate_InvalidCinematics_ThrowsNamingTheField` (theory, one row each): stills empty; stills holding null; image
    blank; seconds 0; seconds NaN; seconds infinite; total seconds past `TimeSpan.MaxValue` (two stills of
    `TimeSpan.MaxValue.TotalSeconds / 1.5`); caption blank; voice blank; music blank; zoom start 99; zoom end 151; id
    twice.
  - `Validate_ZoomAtBothBounds_Passes` (100 to 150, and 150 to 100).
  - `Validate_NoOptionalFields_Passes` (no caption, voice, zoom or music).
  - `Validate_SmallestSeconds_Passes` (0.01).
- `ContentCatalogLoaderCinematicTests` (new sibling: `ContentCatalogLoaderTests` is at 272), on a copied content root:
  - `Load_CinematicsFile_ReadsStillsInOrder` (a file written by the test with two stills `b.png` then `a.png`: the
    record lists `b.png` first, each still's fields read, zoom included).
  - `Load_RepositoryContent_HasCinematicsCatalog` (loads; no id pinned: the file is the developer's).
  - `Load_StillWithoutOptionalFields_ReadsThemAsNull`.

## Verify by hand
None: nothing shows a cinematic yet. The game and editor start as before (`dotnet run --project src/Jrpg.Game`).

- **Done, with deviations:** `CinematicStill.Duration` waits for its first caller, CG1 (no member without a user). The loader tests replace `Load_RepositoryContent_HasCinematicsCatalog`, which an empty catalog passed whatever the loader did, with `Load_NoCinematicsFile_ThrowsNamingIt` and `Load_InvalidStill_ThrowsNamingTheFile`; the validator tests add one per side of each zoom bound and a negative time.
