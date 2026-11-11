# GX1. Game dead code with evidence

Game (and the Game tests a deletion frees), size set by what the listing finds; about 40 lines expected. Builds on decision 558 and the first plan's X1 (its method and fates). After the splits, so the listing reads the files as they stay.

## Goal
Every `public` or `internal` member of `src/Jrpg.Game` with no caller in `src/` outside its own file is listed with its fate, and the ones with no reader are gone.

## The listing (recorded in the Done note)
1. Declarations: `Grep "^\s*(public|internal)\s[^=;]*\b(\w+)\s*(\(|\{|=>|;|=)"` over `src/Jrpg.Game/**/*.cs`: methods, properties, fields, constants and events.
2. For each name, `Grep "\bName\b"` over `src/` (`*.cs`), its own file's hits dropped. Counted as called also when reached without the name:
   - an interface member (`IScene.Update`, `IMenuPage.Draw` and the like) or an override (`Game.Update`, `Draw`, `LoadContent`);
   - a `MessageIds` constant, which `MessageIdsTests` and `ui.json` pair (its readers are the screens; a constant no screen reads is dead like any other, and its `ui.json` entry is content: noted, not removed, since `content/` is out of scope);
   - a `UiArtFiles` or `UiSoundFiles` constant, checked by their tests: same rule.
3. The Done note's table: member, file, callers found (none, tests only), fate.

Fates (the first plan's X1):
- **Deleted here** when no `src/` caller remains and its tests (if any) can move onto what replaced it with a mechanical edit, at most about five sites; a test of nothing else goes with it.
- **A `backlog.md` line** otherwise, naming the member, its test sites and why it waits.
- **Kept, with the reason**, when its reader is real but nameless (step 2).

A member kept only as a test seam (`internal` read by `Jrpg.Game.Tests` through `InternalsVisibleTo`, such as `OptionsPage.TextSpeedRow`) is kept: the design rule asks for such seams over vacuous tests.

## Tests
None new. The runner's totals per project before and after, equal unless a test of a deleted member went with it (named in the note).

## Verify by hand
If anything was deleted from a screen's code: start the game and open that screen once; it behaves as before.
