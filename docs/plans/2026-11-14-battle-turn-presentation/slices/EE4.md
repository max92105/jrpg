# EE4. The Effect strips window and Database > Effect strips...

Editor, about 170 lines (XAML about 120, code-behind and the menu about 50). Needs EE3. Decision 659; ADR 0045.

## Goal
**Database > Effect strips...** opens a window shaped like the other Database windows: the effects images on the left
(an unlisted one greyed, a missing one marked), the chosen strip on the right: its file, **Frame time (ms)**, **Hit
frame**, **Swing** (an editable box over the effects images, empty for none), its length line, **Reset to unlisted**,
and the problem line; **OK** greyed by `CanApply`, **Cancel**. OK applies the session as one undo step; **Ctrl+S** in
the main window writes `effect-strips.json`.

## New types
- `Editor/Battle/EffectStripCatalogWindow` (XAML + code-behind): the Effect strips window. Its window style
  (`EditorWindow`), the theme's `EnabledWhenChosen` for the right panel, list rows by `ItemContainerStyle`; code-behind
  only for view concerns (closing on OK or Cancel).

## Changes
- `Shell/MainWindow.Database` (the partial that opens the Database windows): **Effect strips...** under **Database**,
  after **Drop tables...**; opens the window with `EffectStripEditingViewModel.Start()`, applies on OK.
- `Shell/MainWindow.xaml`: the menu entry.
- `docs/map.md`: the Editor `Battle` row names the window and its view models.

## Tests
- None new: the window is view code over EE3's tested rules (it imports nothing, so its image list is read once as it
  opens); the hand check carries it.

## Verify by hand
1. Run the editor, **Database > Effect strips...**: every image in `content/art/effects/` is listed, unlisted ones
   greyed; choose `AttackEnemy.png`: 80, 0, an empty swing, "N frames, ...".
2. Type **50** in **Frame time (ms)**: the row is no longer greyed, the length line changes; type **abc**: the problem
   line names the box, **OK** greys; type **50** again.
3. Pick `AttackPlayer.png` in **Swing**, **OK**, **Ctrl+S**: `content/effect-strips.json` holds one record with
   `"frameMilliseconds": 50` and `"swingFile": "AttackPlayer.png"`, no `hitFrame` line. **Ctrl+Z** in the main window:
   the record goes; **Ctrl+S** writes `[]` back.
4. Reopen, choose the strip again after redoing, **Reset to unlisted**, **OK**, **Ctrl+S**: the record is gone.
5. Revert `content/effect-strips.json` if anything is left.
