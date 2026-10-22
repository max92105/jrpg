# E7b. Importing a sheet image in the editor

Part of [the editor plan](../README.md). Design: [design.md](../design.md); decision 125. Builds on [E7a](E7a.md)'s writer. Split from E7 (developer, 2026-10-21).

- **E7b. Importing a sheet image in the editor** (Editor, about 210 lines)
  - **Goal:**
    - Tiles > **Import sheet image...** (and a button beside the sheet box) opens a file dialog for PNGs, several at once.
    - Each is copied into `content/art/tilesheets/` through E7a's writer. A name already there asks before replacing (decision 125); a refused file (not a PNG, or too small for the tiles cut from it) is reported with E7a's message, and the old file stays.
    - The last imported image is shown, "(not set up)" if it has no sheet, ready for E5b's Set up.
    - An import is a file copy, not a catalog step: Ctrl+Z doesn't undo it (the status line says "Imported Forest2.png").
    - A replaced file's cached image and size are forgotten, so the sheet and the map show the new art.
  - **New types:**
    - `Tiles/SheetImport`: importing images into the tilesheet art through Data, asking about taken names, and reporting what was refused.
      - `SheetImport(ContentWriter writer, CatalogDraft catalog)`.
      - `TryImport(IReadOnlyList<string> paths, Func<string, bool> confirmReplace)`: returns `SheetImportResult` (record: the last imported file name, or null; the refused files' messages).
  - **Changes:**
    - `Tiles/SheetImages`: the file list is read from the art folder when asked, so an import shows without restarting.
    - `Tiles/TileBrowserViewModel.Show(string fileName)`: refreshes `SheetChoices` from the art folder and shows that image.
    - `Shell/MainViewModel`: owns `Import`; forgets a replaced file's cached size.
    - `Shell/MainWindow`: the menu entry, the file dialog, the replace question; `TileImages` forgets a replaced file's image.
  - **Tests:**
    - `SheetImportTests`: `TryImport_NewImage_ReturnsItsName`, `TryImport_TakenNameDeclined_KeepsTheOldFileAndAsksNothingElse`, `TryImport_RefusedReplacement_ReportsTheMessage`.
    - `TileBrowserViewModelTests`: `Show_NewImageFile_ListsItNotSetUpAndShowsIt`.
  - **Verify by hand:**
    - [ ] Import a PNG from Downloads: it appears "(not set up)", shown; set it up and paint.
    - [ ] Import a `.txt` renamed `.png`: refused with its name.
    - [ ] Import a name that exists, decline: nothing changes.
    - [ ] On a `--content` copy, import a smaller image named `Ground.png`, accept: refused, naming a tile; `Ground.png` is unchanged. A same-size image: accepted, the map shows the new art.
- **Done, with deviations:** split from E7 (developer). `SheetImport.Import` (not `TryImport`: it returns a result, and `Try` methods return bool). `SheetImportResult` lists every file copied (`LastImported` is the last), so the shell forgets each one's cached size and the window each one's picture (`MainViewModel.SheetImagesImported`, `TileImages.Forget`, `TileBrowserView.RedrawSheet`); WPF's own image cache is skipped (`IgnoreImageCache`) so a replaced file is read again. The status bar's import line keeps the last import's result. The Import button sits in the Tiles tab beside Mark blocked. Review: shell tests for showing the import and reading a replaced image's new size (`MainViewModelTests`); the window redraws only when something was copied; `PngHeaderFile` shared by the Editor tests.
