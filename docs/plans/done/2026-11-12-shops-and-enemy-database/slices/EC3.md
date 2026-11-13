# EC3. The Encounters window

**Later: not in the approved plan** (README, "Later"); a draft kept with EC1. Its `MainWindow.xaml` line counts are from the draft and are stale.

Editor, about 160 lines (window code and XAML). Builds on EC2.

## Goal
Database > **Encounters...** opens the window: encounters on the left (Add, Remove), the chosen one on the right: id, its enemies in slot order (Add from a box, Remove, Up, Down), the background box and the music box; the problem line; OK greyed while it shows. With it, a new monster reaches a map: Map > Settings... already picks the encounters a map's random battles roll.

## New types
- `Editor/Battle/EncounterCatalogWindow` (`.xaml` + `.xaml.cs`, `EditorWindow` style).

## Changes
- `Editor/Shell/MainWindow.Database.cs`: `OnEditEncountersClicked`.
- `Editor/Shell/MainViewModel.Database.cs`: `EncounterEditing`, `HasEncounterEditing`.
- `Shell/MainWindow.xaml` (289 after EN7): `Encounters...` under Database (290).

## Tests
None new: EC2 tests the rules.

## Verify by hand
1. Database > Encounters...: Add: id `wolf_pack`, add `wolf` (EN7) twice, background from the list, music "(battle theme)"; OK.
2. Map > Settings... on a map with random battles: add `wolf_pack` to its encounters, OK; Ctrl+S.
3. Play: walk that map until a battle: two wolves, their art (flipped if set), their numbers and tactics as set.
4. Ctrl+Z on the map setting, then on the encounter: the encounter undoes; with the map setting kept, its undo is refused naming the map.
5. Revert with `git checkout -- content` if this was only a check (and the imported battler, or add its art source line).
