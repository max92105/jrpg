# T2a. Game tests on the test world

Builds on T1 and design.md, "Key decisions" 1.

## Goal
Every Game test that walks a map, starts a session or reads tiles, sheets or objects loads `test-world`; the repository's maps are free to change for the Game tests. A mechanical change: the Game test count is the same before and after, and every test passes unchanged.

## Changes
- `tests/Jrpg.Game.Tests/Exploration/StartMapScenarios`: `RepositoryContent` becomes `TestWorld`, loaded from `Path.Combine(AppContext.BaseDirectory, "test-world")`; its summary and the scenarios' summaries say "the test world's start map" where they said "the repository's". Its about 10 call sites (`EventPresenterTests`, `EventPresenterFadeSoundTests` and others: grep `StartMapScenarios.RepositoryContent`) follow the rename.
- The other Game helpers and tests that load `content` and start a session or read the map side: `Battle/BattleScenarios`, `Flow/GameFlowTests`, `Rendering/TileAtlasTests`, `Rendering/ObjectSpriteBakerTests` and any other the grep `BaseDirectory, "content"` finds that does; each switches its path. Tests that only read text, fonts, UI art or sound files (`RepositoryScreenText`, `MessageIdsTests`, `FontLibraryTests`, `UiArtFilesTests`, `UiSoundFilesTests`, `BattlerArtTests`) stay on `content`.
- Comments in the moved files that say "the repository's start map" or "the repository's content" for a map read "the test world's".

## New types
None.

## Tests
None new: the guard is the Game test count before and after (state both in the Done note) and every existing test passing.

## Verify by hand
- Temporarily move a placed object in `content/map-layouts/start.json` (the shopkeeper), run the Game tests: they pass. Revert.

- **Done, with deviations:** 1390 Game tests before and after, every one passing. `Battle/BattleScenarios` stays on `content`: it reads enemies, skills, rules and members, never the map side, so the battle screen's tests keep following the live balance. The leader choice, inventory filter and quest detail tests stay for the same reason. Switched: `StartMapScenarios` (renamed `TestWorld`, 8 call sites), `GameFlowTests`, `TileAtlasTests`, `ObjectSpriteBakerTests`.
