# SI5. New NPC picks a set-up sprite; Change NPC sprite

Editor, about 190 lines (90 of them tests). Builds on SI4. Folds the backlog line "The New NPC dialog doesn't check that the chosen sprite holds the walk frames" (only set-up sprites are offered, and SI1's import refuses a too-small image).

## Goal
New NPC... offers only sprites with a record, the placeholder first, and shows the chosen one walking down; with none set up it says where to set one up. **Change NPC sprite...** (Objects menu and the Objects panel's button row, greyed unless the chosen object is an NPC) opens the same dialog on the chosen NPC object with its id fixed, and OK swaps its sprite as one undo step (ADR 0015's "a one-field change per NPC object", now a click).

## New types
- `Editor/Objects/NpcSpriteChange` (internal sealed): the Change NPC sprite flow: `CanChange` (the Objects panel's chosen object has a field sprite), `Start()` (the dialog's state on that object), `Apply(dialog)` (`CatalogDraft.SetObject` with the new `FieldSpriteFile`, nothing when unchanged).

## Changes
- `Editor/Objects/NewNpcViewModel`: its sprite list is the draft's records' files, sorted, the placeholder first (`PlaceholderSprite`); `SpriteProblem` says "Set up a character sprite first: Objects > Character sprites..." when the list is empty; `ForChange(ObjectDefinition npc, IReadOnlyList<string> spriteFiles)` builds it with the id fixed (`IsIdFixed`, the id box read-only, `IdProblem` null); `ToDefinition()` in change mode keeps the object's outline and category.
- `Editor/Objects/NewNpcWindow`: the preview walks down at the editor's time (as SI4's preview, one facing), title "Change NPC sprite" in change mode.
- `Editor/Shell/MainViewModel.FieldSprites.cs`: `StartNewNpc` reads the records; `NpcSpriteChange`.
- `Editor/Shell/MainWindow.xaml` (284 after SI4): one line under Objects, `Change NPC sprite...`, and one beside the Objects tab's `New NPC...` button, `Change sprite...`: 286.

## Tests
- `NewNpcViewModelTests` (42 lines): `SpriteFiles_RecordsOnly_PlaceholderFirst`; `SpriteProblem_NoRecord_PointsToCharacterSprites`; `ForChange_Npc_FixesIdAndPicksItsSprite`; `ToDefinition_ChangeMode_KeepsOutlineAndCategory`.
- `NpcSpriteChangeTests` (new): `CanChange_ChosenTileObject_IsFalse`; `Apply_OtherSprite_IsOneUndoStep`; `Apply_SameSprite_RecordsNothing`; `CanChange_AfterUndoRemovesTheNpc_IsFalse` (the editor rule: the chosen object re-checked on a catalog change).

## Verify by hand
1. Objects > New NPC...: the list shows the placeholder first, then the bought sheet from SI4; choose it: the preview walks down. Id `villager_anna`, OK: the NPC is chosen in the Objects panel; place it on the start map: it stands on its cell at the game's size.
2. Choose the placeholder NPC object, Objects > Change NPC sprite...: the id is greyed; pick the bought sheet, OK: every placed copy on the map shows it. Ctrl+Z: back; Ctrl+Y.
3. Ctrl+S, `dotnet run --project src/Jrpg.Game`: the NPC stands on the map as in the editor, and the Talk prompt sits above its head.
4. Revert with `git checkout -- content` if this was only a check.

- **Done, with deviations:** the dialog's preview walks all four facings through `WalkPreview` (shared with the Character sprites window), the image read through `SpriteImageFile` (shared too); `NpcSpriteChange` is an observable object re-raising `CanChange` on the browser's choice and the catalog; `Apply` returns false on a sprite problem, as `CreateNpc` does, so one dialog serves both; the backlog line on the dialog's sprite check is folded (only set-up sprites are offered). The review: the dialog orders the files itself, so the shell and the change flow pass the records as they are.
