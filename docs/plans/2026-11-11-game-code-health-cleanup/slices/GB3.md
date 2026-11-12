# GB3. The UI art check compares exact file names

Game tests, about 15 lines. Builds on decision 555.

## Goal
`UiArtFilesTests` fails when a constant of `UiArtFiles` names a file in another case than the one on disk, as `UiSoundFilesTests` does for sounds. Today it uses `File.Exists`, which ignores case on Windows, while a release on another system may not.

## Changes
- `tests/Jrpg.Game.Tests/Ui/UiArtFilesTests.cs` (30 → about 35): `EveryConstant_ExistsInItsFolder` lists the folder's file names (`Directory.GetFiles(Path.Combine(_artRoot, folder)).Select(Path.GetFileName)`, compared with `StringComparer.Ordinal`) and asserts each constant is among them, with the same message; the comment says why, as `UiSoundFilesTests`' does.
- `docs/plans/backlog.md`: delete the P1 line.

## Tests
The changed theory. To show it can fail (decision 538): change one `UiArtFiles` constant's case for one run (`UiArtFiles.Backgrounds.Menu`, say), see the old test pass and the new one fail naming the file, then restore it. The Done note records that run and the runner's totals per project (equal).

## Verify by hand
None.

- **Done:** as planned; the folder's names in a set compared with `StringComparer.Ordinal`, the comment as the sound test's. With `UiArtFiles.Backgrounds.Menu` as "gamemenu.png" for one run, the theory failed naming the file ("content/art/backgrounds has no 'gamemenu.png'."), where `File.Exists` passed it; restored, it passes. The runner's total is 4610 before and after. The P1 line went.
