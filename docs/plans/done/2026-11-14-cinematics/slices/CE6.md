# CE6. The Cinematics window and Database > Cinematics...

Editor, about 200 lines (mostly XAML; about 30 of tests). Needs CE5. Design: tunable "Preview size", risk "Editor
decoding". The first hand check of the whole tool.

## Goal
Database > **Cinematics...** opens the window: cinematics on the left (Add with an id box, Remove while new); on the
right the chosen cinematic's id, its **Music** box (none first) and its stills (numbered, with **Add still**, **Remove**,
**Up**, **Down**), the chosen still's **Picture**, **Seconds**, **Subtitle**, **Voice**, **Zoom from %** and **Zoom to
%** boxes, and the chosen still's picture in a 480 x 270 preview (covering it, as the game's screen does); a note naming
what the folders lack; the problem line; OK greyed through `CanApply`. OK applies one undo step; Ctrl+S saves it.

## New types
- `Editor/Cinematics/CinematicCatalogWindow` (XAML + code-behind): the window. Code-behind only shows the chosen
  still's picture: decoded through a `Func<string, BitmapSource?>` lookup (null when missing or unreadable:
  `PngSize.IsReadable` first, `.claude/rules/editor.md`), at the preview's width (`DecodePixelWidth = 480`), cached per
  file while the window is open; a missing picture shows the missing-art mark's text. The window sets the
  `EditorWindow` style; its rows choose themselves through a `PreviewGotKeyboardFocus` setter (as the drop table rows).

## Changes
- `Shell/MainViewModel.Database.cs`: `CinematicEditing` (`{ get; }`, declared here) and the media lookups (`ArtRoot`,
  the audio root); `Shell/MainViewModel.cs` sets it in the constructor beside `DropTableEditing` (282 → 283).
- `Shell/MainWindow.Database.cs`: `OnEditCinematicsClicked` (as `OnEditShopsClicked`, passing the picture lookup
  `file => SpriteImageFile.LoadFrom(_viewModel.ArtRoot, ArtFolders.Cinematics, file)` or a decoding helper at a width).
- `Shell/MainWindow.xaml` (291 → 292): Database > **Cinematics...** after Drop tables.
- `docs/map.md`: the Editor `Cinematics` row names the window and the menu.

## Tests
- `CinematicEditingViewModelTests`: `Start_AfterSetCinematics_ListsTheNewOne` (the window opened after a step shows it).
- None for the window (view only).

## Verify by hand
Ragnir's intro, click by click:
1. Run the editor. **Database > Cinematics...**: the window opens with an empty list.
2. Type `ragnir_intro` in the id box under the list, click **Add**: it appears, chosen, with the problem "Add at least
   one still." and OK greyed.
3. In **Picture**, pick `ragnir_intro/1_happy_life.png`; click **Add still**: still 1 appears, 4 s, and its picture
   shows in the preview. Repeat for `2_training_son.png` to `6_i_will_get_him_back.png`, each added after the chosen
   one.
4. Choose still 3; click **Up**: it becomes still 2 and the numbers follow; click **Down** to put it back.
5. Choose still 1: type `6` in **Seconds**, a line in **Subtitle**, pick a voice in **Voice** if one is in
   `content/audio/voices/` (only `Placeholder.wav` today), `100` and `108` in the zoom boxes. Type `x` in **Seconds**:
   the problem names still 1's seconds and OK greys; type `6` again.
6. Type `151` in **Zoom to %**: the problem line gives Data's message naming still 1; type `108` again.
7. Pick a track in **Music** (for example `town/loop.wav`), or leave none.
8. Click **OK**, then **Ctrl+S**: `content/cinematics.json` holds `ragnir_intro` with six stills in order, the optional
   fields only where set. **Ctrl+Z** takes the whole session back; **Ctrl+Y** brings it again; save.
9. CE2's refusal: open the intro map, Events mode, click cell (0, 1); add a **Cinematic** step choosing `ragnir_intro`;
   OK. Open another map in the list and press **Ctrl+Z** until the catalog step is next: the status says "Can't undo:
   cinematic ragnir_intro is played on map intro." Undo the event step on the intro map instead (or don't save it: CG3
   switches Ragnir's intro in content).
10. Rename a picture file in the folder by hand while the window is closed, reopen it: the note names the missing
    picture and OK still applies (decision 442); rename it back.

- **Done, with deviations:** the preview's picture comes through a `CinematicPictureConverter` (the lookup, cached per file while the window is open) bound to the chosen still's `ImageFile`, so the code-behind only builds it; `SpriteImageFile.LoadAtWidth` decodes at 480 px. The picture and voice boxes are editable combo boxes over the folders' files, so a name the folders lack can still be typed (decision 442). Hand check is the developer's.
  CE6's review: the preview's picture is centered (a UniformToFill image pins top-left otherwise), and its size is one pair of constants on the window, read by the XAML and the shell's decode.
