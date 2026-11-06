# CP2. Copy, cut and paste placed objects

Builds on design.md, "Key decisions" 8, Risks (text boxes, warps to their own map); decision 512; B1. Needs CP1, MO1 (Ctrl + click selection) and SN1 (`GridSnap`, `IsSnapping`).

## Goal
In Objects mode, Ctrl+C copies the selected placed objects with their events; Ctrl+X copies and removes them (one undo step); Ctrl+V pastes them centered on the mouse when it is over the map, else on the middle of the view (Edit > Paste: the mouse is on the menu), snapped with Alt or the Snap to grid box as placing is, as one undo step, and selects the pasted objects so arrows or Shift + drag adjust them at once. The copies stay while the editor runs: open another map and Ctrl+V pastes them there. Paste is greyed while a copied object's id is missing from the catalog (its composing undone, or the object removed) and comes back if a redo brings it back. The Edit menu has Copy, Cut and Paste with their keys. In a text box, the three keys stay the text box's.

## New types
- `Editor/Objects/ObjectCopying` (`internal sealed partial class : ObservableObject`): the objects copied and pasting them on the open map.
  - `ObjectCopying(CatalogDraft catalog, ObjectPlacingViewModel placing)` (the catalog's objects; the shown map, the selection and the object sizes through the placing view model).
  - `IReadOnlyList<PlacedObject> Copied` (private set; empty at the start).
  - `[RelayCommand(CanExecute = nameof(HasSelection))] void Copy()`; `[RelayCommand(CanExecute = nameof(HasSelection))] void Cut()` (copy, then `DeleteSelectedObjects`, one step).
  - `public bool CanPaste`: something copied, a map shown, and every copied object id in `catalog.Current.Objects` (B1); raised (`PropertyChanged`, and the window's `CommandManager.InvalidateRequerySuggested`) on every `CatalogDraft.Current` change (`.claude/rules/editor.md`), on a copy and on a map shown. `public void PasteCenteredAt(Point2D center)` and `public void PasteOnGridCenteredAt(Point2D center)`: the group's bounds (`PlacedObjectCopies.TopLeftOf` and the objects' sizes) centered on the point, the top-left through `GridSnap.TopLeft` for the second; each makes the copies with `PlacedObjectCopies.Make`, does one `PlaceObjectsEdit`, then selects the pasted uids. Nothing while `CanPaste` is false.
  - `CanExecute`s re-raised when the selection or the open map changes.

## Changes
- `Objects/ObjectPlacingViewModel` (about 240 after SN1): `public ObjectCopying Copying { get; }` built in the constructor with its catalog; `public void Select(IReadOnlyList<string> uids)` (the pasted ones). About +8, ends about 248.
- New partial `Shell/MainWindow.ObjectCopying.cs` (summary: "Copy, Cut and Paste in Objects mode: the window's command bindings over `ObjectCopying`."): handlers for `ApplicationCommands.Copy`, `Cut` and `Paste` (`CanExecute` false outside Objects mode or with no map; a focused `TextBox` handles its own first); Paste centers on `_mapPoint`, or on the map point at the view's center when the mouse isn't over the map, snapped while `IsSnapping`.
- `Shell/MainWindow.xaml`: `Window.CommandBindings` gains the three bindings (+3); the Edit menu gains Copy, Cut and Paste (`Command="Copy"` etc.; WPF shows Ctrl+C, Ctrl+X, Ctrl+V) (+3). Ends about 286.
- `Shell/ControlsHelpWindow.xaml`, Objects: "Ctrl+C / Ctrl+X: copy / cut the selected objects with their events. Ctrl+V: paste them centered on the mouse, or on the middle of the view from the Edit menu (Alt snaps), on this map or another; the pasted ones are selected."

## Tests (new `Objects/ObjectCopyingTests`, the test world's forest and start maps)
- `ObjectCopyingTests: CopyCommand_NothingSelected_CannotExecute`; `CopyCommand_SelectionMade_CanExecuteAndIsRaised`.
- `ObjectCopyingTests: Copy_ThenPasteCenteredAt_AddsCopiesWithTheirEventsInOneStep` (the test world's chest with its event: one undo removes the copy).
- `ObjectCopyingTests: PasteCenteredAt_Group_CentersItsBoundsOnThePoint`.
- `ObjectCopyingTests: PasteOnGridCenteredAt_FreePoint_PutsTheGroupsTopLeftOnWholeCells`.
- `ObjectCopyingTests: PasteCenteredAt_SelectsThePastedObjects`.
- `ObjectCopyingTests: PasteCenteredAt_AnotherMapOpened_PastesThereWithFreshIds` (copy on forest, `TryOpenMap("start")` through `MainViewModel`, paste: the uid and event ids are new on start, and `Save` on a content copy leaves `SaveError` null).
- `ObjectCopyingTests: Cut_RemovesTheSelectionAndKeepsTheCopy` (one undo puts them back; the copy stays pasteable).
- `ObjectCopyingTests: PasteCenteredAt_NothingCopied_RecordsNoStep`.
- `ObjectCopyingTests: CanPaste_CopiedObjectsComposingUndone_IsFalseAndRaised` (B1: compose an object, place it, copy it, remove the placement, undo the composing: `CanPaste` false, `PropertyChanged` raised; Ctrl+Y: true again).
- `ObjectCopyingTests: PasteCenteredAt_ObjectEditedAfterTheCopy_PastesItAsCopied` (an event changed after Ctrl+C: the paste has the old one).

Size: about 78 lines for `ObjectCopying`, 8 in the placing view model, 35 in the window partial, 7 in XAML and F1; 11 tests (about 135 with a shared arrange helper): about 225. The tests are short; the window partial is command plumbing.

## Map
`docs/map.md`, Editor `Tiles, Objects` row: `ObjectCopying` (Ctrl+C, Ctrl+X and Ctrl+V of placed objects with their events, across maps).

## Hand check (yours)
1. Start the editor (Visual Studio's "Game and editor" profile, F5); Objects tab.
2. Ctrl + click an NPC that has a Talk event; press Ctrl+C. Move the mouse elsewhere on the map, press Ctrl+V: a copy appears centered on the mouse, outlined (selected). Click the **Events** tab, click the copy: its Talk event is there, with the same lines.
3. Ctrl+Z: the copy goes in one step.
4. Open another map from the list, Ctrl+V: the NPC lands there. Ctrl+S saves without an error.
5. Start the game (same F5) and talk to the copy: it says its lines. A copied chest opens and gives its item even if you already opened the original.
6. Ctrl+X on a selected object: it goes; Ctrl+V puts it back where the mouse is.
7. Edit > Copy, Cut and Paste show their keys; Edit > Paste pastes in the middle of the view.
8. Click in the map list's filter box, type some text, Ctrl+C then Ctrl+V: the text is copied and pasted in the box; no object changes.

- **Done, with deviations:** the Edit menu entries read "Copy objects", "Cut objects" and "Paste objects", so they aren't taken for text editing; the window relies on WPF's own requery after each key or click for Paste's greying, no `InvalidateRequerySuggested`. The undo checks are their own tests (`PasteCenteredAt_Undo_...`, `Cut_Undo_...`, `CanPaste_ComposingUndoneThenRedone_...`), and the editor-level ones are in `Shell/MainViewModelObjectCopyingTests`. Review: the Edit menu's three entries aim at the map (`CommandTarget`), since a menu hands its command to the focused text box first; a paste centered off the map does nothing, as placing; Paste is greyed too while a copied event swaps to an object the catalog lacks; each with a test.
