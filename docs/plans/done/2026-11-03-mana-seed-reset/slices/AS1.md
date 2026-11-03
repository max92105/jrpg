# AS1. Art sources manifest and credits

Builds on decision 440, ADR 0038 (written before this slice) and design.md, "Tunables". Needs S0 (the pack out of `content/`) and the developer's answer to Q8.

## Goal
Every art file under `content/art/` has a source, author and license; placeholders are flagged; the credits list every source; a Data test reading the repository's folder keeps all of it in step, so a sheet imported later fails the tests until its line is added (decision 448).

## Before writing
Ask the developer where each folder's files came from (Q8): `backgrounds/`, `battlers/`, `portraits/`, `icons/`, `effects/`, `cinematics/`, `field-sprites/`, the old tilesheets (`City.png`, `FG_Forest_Fall.png`, `Grass1.png`, `Grass2.png`, `Ground.png`, `RA_Ground_Tiles.png`, `RA_Jungle.png`, `RA_Jungle_Animations.png`, `tree01_s_01_animation.png`, `Water1.png`, `Water2.png`). The fonts are known: Lato and Cormorant Garamond, `OFL-1.1`, with `fonts/OFL.txt` and `fonts/titles/OFL.txt` as their `licenseFile`. Anything without an answer goes under an `unrecorded` source (`LicenseRef-Unrecorded`, `isPlaceholder: true`).

## Files
- `content/art/art-sources.json` (new): `sources` sorted by id, `files` (a path and a `sourceId`) sorted by path, laid out as `ContentWriter` writes (one field per line in nested objects, CRLF, trailing newline). About 55 file entries.
- `content/art/CREDITS.md` (new): one section per non-placeholder source (name linked to its url, author, license), then "Placeholders (replace before release)" listing the placeholder sources and how many files each holds.
- `tests/Jrpg.Data.Tests/Jrpg.Data.Tests.csproj`: `<AssemblyMetadata Include="ContentRoot" Value="$([System.IO.Path]::GetFullPath('$(MSBuildThisFileDirectory)..\..\content'))" />`, as `Jrpg.Game.csproj` and `Jrpg.Editor.csproj` stamp it (unconditional: tests always run from the repository).
- `.claude/rules/content.md`, three lines: every file under `content/art/` has an entry in `content/art/art-sources.json` and its source a CREDITS section (ADR 0038); bought packs stay outside the repository and only the sheets used are imported (decision 439); **agents never open a Mana Seed image, or a capture or screenshot showing one: the Mana Seed license refuses AI-related use, the developer accepted the risk (decision 438) and this is the free mitigation; work from file names, sizes and the developer's descriptions.**
- `docs/map.md`: the Data `Art` line names the manifest and its test, as the `Audio` line does.

## New types
- `Data.Tests/Art/ArtSourcesManifest`: the manifest as the tests read it (`ArtSourcesManifest`, `ArtSource`, `ArtFile` records), read once with `System.Text.Json`, and the repository's art folder from the assembly's `ContentRoot` metadata. Test-only: the game never reads it (ADR 0036's rule).
- `Data.Tests/Art/ArtSourcesTests`: files, entries, sources, licenses and credits in step.

## Tests
- `ArtSourcesTests: Manifest_EveryImageAndFont_HasAnEntry` (every `*.png` and `*.ttf` under the repository's `content/art/`, by exact relative path).
- `ArtSourcesTests: Manifest_EveryEntry_NamesAFile` (exact case, as `ExactFileName` does).
- `ArtSourcesTests: Manifest_EveryEntry_NamesAKnownSource`.
- `ArtSourcesTests: ArtFolder_EveryOtherFile_IsALicenseTextOrTheManifest` (anything not PNG or TTF is a source's `licenseFile`, the manifest or the credits: a pack's `.tmx`, `.tsx` or readme fails here).
- `ArtSourcesTests: Manifest_EveryLicense_IsAllowed`.
- `ArtSourcesTests: Manifest_UnrecordedOrGeneratedLicense_OnlyOnPlaceholderSources`.
- `ArtSourcesTests: Credits_EverySource_IsListedWithItsAuthor`.

## Verify by hand
- Copy any non-Mana Seed PNG into `content/art/tilesheets/` and run the Data tests (no rebuild needed: the test reads the source tree): `Manifest_EveryImageAndFont_HasAnEntry` fails naming it. Delete it.

- **Done, with deviations:** 56 file entries under five sources (Lato, Cormorant Garamond, the three Rogue Adventure sheets by ELV Games, the AI-generated placeholders, and seven old tilesheets of unrecorded origin; decision 452). `LicenseRef-ElvGames` joins the allowed licenses, since those sheets were bought. Stamping `ContentRoot` into the test assembly moved `ContentRootLocatorTests`' no-metadata case onto the Data assembly. Checked by hand: a stray PNG in `tilesheets/` fails `Manifest_EveryImageAndFont_HasAnEntry` by name without a rebuild.
