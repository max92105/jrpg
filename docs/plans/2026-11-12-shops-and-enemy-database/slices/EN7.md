# EN7. The Enemies window: the list, Stats and Art tabs

Editor, about 220 lines (window code and XAML). Builds on EN3 and EN6. Decision 583.

## Goal
Database > **Enemies...** opens a window: enemies on the left (Add, Duplicate, Remove), the chosen enemy on the right in two tabs. **Stats**: id, name, the numbers in two columns, damage type, the resistances grid (type and percent), gold (a box and min / max), drop table, boss, and a line "Tactics and phases: edited in enemies.json (n actions, m phases)". **Art**: the three pickers with Import... beside each, the flip box, and the battle sprite previewed (mirrored when flipped, fitted to the box, its pixel size written under it; the battle's own scaling rule, `Game/Battle/BattlerScaling`, isn't reachable from the editor and isn't copied). The problem line under the tabs; OK greyed while it shows.

## New types
- `Editor/Battle/EnemyCatalogWindow` (`.xaml` + `.xaml.cs`, `EditorWindow` style): the window above; its battler preview loads the image through a lookup given by the shell (never the view model).

## Changes
- `Editor/Shell/MainWindow.Database.cs`: `OnEditEnemiesClicked` and the art Import... file dialogs.
- `Editor/Shell/MainViewModel.Database.cs`: `EnemyEditing`, `HasEnemyEditing`.
- `Editor/Battle/EnemyViewModel`: `TacticsSummary` (the line above, read-only).
- `Shell/MainWindow.xaml` (287 after SH3): `Enemies...` under Database (288).

## Tests
- `EnemyViewModelTests`: `TacticsSummary_GoblinChief_CountsActionsAndPhases`. The rest is window code; EN2, EN3 and EN6 test the rules.

## Verify by hand
1. Database > Enemies...: goblin, stone_beetle, training_dummy, goblin_chief; goblin chosen: its numbers, Fire -25 in the grid, gold 6-12, drops `goblin`.
2. Choose goblin_chief, **Duplicate**: id `wolf_chief`, name `Wolf Chief`, max HP 300; its tactics line shows the chief's counts. Art tab: **Import...** a battler PNG of your own, it is picked and previewed; tick Flip: the preview mirrors.
3. Set Accuracy 250: the problem line shows Data's words, OK greys; set 80, OK.
4. Ctrl+Z removes the wolf chief; Ctrl+Y. Ctrl+S: `git diff content/enemies.json` shows only the new enemy (its tactics copied).
5. To fight it now: add an encounter naming it to `content/encounters.json` by hand (with the editor closed), reopen the editor, Map > Settings... on a map with random battles, pick the encounter, Ctrl+S, play the map.
6. Add the imported image's line to `content/art/art-sources.json` and `CREDITS.md` (ADR 0038), or revert with `git checkout -- content` and delete the PNG.

- **Done, with deviations:** an Import... picks the copied file in its box through `EnemyCatalogViewModel.TakeImport` (folders read again, `EnemyArtViewModel.Pick` by folder), tested, rather than in the window's code; the battle sprite preview reads through `SpriteImageFile.LoadFrom` (the loader given a folder), handed to the window by the shell; the missing-picture marks show as one line under the boxes; the shell's `ImportArtImages` asks before a replacement and writes the status line, shared with the Character sprites window (the review's third copy); the loader returns no picture for an unreadable PNG (the review: a broken battler named by an enemy crashed the preview); the preview reads the picture only when the chosen enemy's art changes. The look is the developer's hand check (no capture).
