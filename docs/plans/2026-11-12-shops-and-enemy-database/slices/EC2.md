# EC2. The Encounters window's rules

**Later: not in the approved plan** (README, "Later"); a draft kept with EC1.

Editor, about 180 lines (90 of them tests). Builds on EC1 and NP2 (`ArtFolders.ImagesIn`).

## Goal
The Encounters window's state, headless: every encounter in id order, new ones after; an encounter's id (typed while new), its enemies in slot order (the screen's slots follow the order: add from the draft's enemies, remove, move), its background picked from `content/art/backgrounds/` (editable list) and its music: "(battle theme)" or a track folder from `content/audio/music/` (`AudioFolders.FilesIn`); the problem line; Add, Remove only while new.

## New types
- `Editor/Battle/EncounterViewModel` (internal sealed partial, `ObservableObject`): one encounter as edited: `Id`, `EnemyIds` (`ObservableCollection<string>`), `SelectedEnemy`, `EnemyToAdd`, Add enemy, Remove enemy, Move up, Move down (`Events/ListOrder`), `BackgroundFile`, `MusicFile` (null for the battle theme); `Problem` (the new id); `ToEncounter()`.
- `Editor/Battle/EncounterCatalogViewModel` (internal sealed partial, `ObservableObject`): `Encounters`, `SelectedEncounter`, Add, Remove (new only), the enemy, background and music choices (lookups given), `Problem` (then `CatalogProblems.OfEncounters`), `ToEncounters()`.
- `Editor/Battle/EncounterEditingViewModel` (internal sealed): `Start()` / `Apply(edited)` (`CatalogDraft.SetEncounters`).

## Changes
None outside the folder.

## Tests
- `EncounterViewModelTests` (new): `From_Encounter_ReadsEnemiesInSlotOrder`; `AddEnemy_SameEnemyTwice_IsAllowed` (two goblins in one fight); `MoveUp_SecondEnemy_SwapsSlots`; `ToEncounter_BattleThemeChosen_HasNoMusic`.
- `EncounterCatalogViewModelTests` (new): `Remove_SavedEncounter_IsGreyed`; `Problem_NoEnemies_IsDataMessage`; `MusicChoices_BattleThemeFirst`.
- `EncounterEditingViewModelTests` (new): `Apply_NewEncounter_IsOneUndoStep`.

## Verify by hand
None: the window is EC3.
