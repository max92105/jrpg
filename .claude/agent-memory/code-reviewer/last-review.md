---
name: last-review
description: Slice, round and finding status of the latest review; read first to apply re-review rules
metadata:
  type: project
---

Plan 2026-11-15-skill-and-status-editors, slice AN6 (Animation/StripFromCellsWindow, PreviewFrames.OfCells,
SpriteSheetMarks.RowAt, the Effect strips page's button, the script's nested entry). 2026-11-16. Round 1.
Verdict: Ready to commit (no Critical).

- Warning (open): StripFromCellsWindow.Decoded (xaml.cs:42-57) copies SpriteImageFile.Decode line for line (IsReadable,
  BitmapImage OnLoad + IgnoreImageCache + Freeze); TileImages.LoadImage is the third copy of the block. Fix: a public
  path-taking SpriteImageFile read returning null, the window turning null into "not a PNG image.".
- Warning (open): open-editor-windows.ps1 grew 290 -> 318 lines (verify's 300-line check covers .cs and .xaml only).
  Fix: Open-EventWindows (or Open-Nested) into a dot-sourced file.
- Suggestion (open): RowAt's partial-row guard and the X side of the number box (a point right of the number at its
  height) have no test.
- Follow-up: TileImages.LoadImage onto the shared read once it exists.
- Checked, no finding: decode runs in EndInit inside the try (OnLoad closes the file); catch list = GifReader's plus IO;
  Pick off guarded in the VM (PickCellAt:132, PickRow:145) and tested; no-button path returns and the loop closes the
  owner dialog; ControlsHelpWindow 129 lines; FieldLabel without BasedOn harmless (the theme has no TextBlock style).
- AN5 carried: its two suggestions dropped (committed slice).
